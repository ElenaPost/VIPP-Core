using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using VIPP.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using VIPP.Hubs;
using Microsoft.IdentityModel.Tokens;

namespace VIPP.Controllers
{
	public class HomeController : Controller
	{
		private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ActionResult> Index()
		{
			if (ModelState.IsValid)
			{
				if(User.Identity != null && User.Identity.IsAuthenticated)
				{
					string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
					DateTime currDate = DateTime.Now;
					List<MarathonLink> marathonLinks = new List<MarathonLink>();
					List<string> upcomingMarathons = new List<string>();
					IEnumerable<Participant> marathonsForCurrUser = await _context.Participants.Where(p => p.UserId == userId).ToListAsync();
					foreach (Participant marathonForCurrUser in marathonsForCurrUser)
					{
						try
						{
							MarathonDate marathonDate = await _context.MarathonDates.FindAsync(marathonForCurrUser.MarathonDateId);
							if (marathonDate != null)
							{
								Guid marathonId = marathonDate.MarathonId;
								DateTime startDate = marathonDate.StartDate;
								Marathon marathon = await _context.Marathons.FindAsync(marathonId);
								if (marathon != null)
								{
									int countDays = marathon.CountDays;
									DateTime dateAfterEnd = startDate.AddDays(countDays);
									if (currDate < dateAfterEnd)
									{
										if (currDate < startDate)
										{
											upcomingMarathons.Add($"{marathon.Name}, который стартует {startDate}");
										}
										else
										{
											marathonLinks.Add(new MarathonLink { LinkText = $"{marathon.Name}, который стартовал {startDate.ToShortDateString()}", ActionName = "AddAchievement", Day = currDate.Subtract(startDate).Days + 1, CurrUserId = userId });
										}
									}
								}
							}
						}
						catch (InvalidOperationException e)
						{
							Console.WriteLine(e.Message);
						}
					}
					ViewBag.MarathonLinks = marathonLinks;
					ViewBag.UpcomingMarathons = upcomingMarathons;
					if (User.IsInRole("admin"))
					{
						ViewBag.MarathonSeeAchievementsLinks = await indexForAdmin();
					}
				}
			}
			return View();
		}

		[HttpGet]
		private async Task<List<MarathonLink>> indexForAdmin()
		{
			List<MarathonLink> marathonLinks = new List<MarathonLink>();
			DateTime currDate = DateTime.Now;
			foreach (var marathonDate in _context.MarathonDates.ToList())
			{
				try
				{
					Guid marathonId = marathonDate.MarathonId;
					DateTime startDate = marathonDate.StartDate;
					Marathon marathon = await _context.Marathons.FindAsync(marathonId);
					if (marathon != null)
					{
						int countDays = marathon.CountDays;
						DateTime dateAfterEnd = startDate.AddDays(countDays);
						if (currDate < dateAfterEnd && currDate >= startDate)
						{
							marathonLinks.Add(new MarathonLink { LinkText = $"{marathon.Name}, который стартовал {startDate.ToShortDateString()}. Просмотреть успехи.", ActionName = "SeeAchievements", Day = currDate.Subtract(startDate).Days + 1, MarathonDateId = marathonDate.Id });
						}
					}
				}
				catch(Exception exc)
				{
					HttpContext.Response.WriteAsync(exc.Message);
				}
			}
			return marathonLinks;
		}

		[HttpGet]
		public async Task<ActionResult> AddAchievement(int activeDay, int currDay, string userId)
		{
			ViewBag.ActiveDay = activeDay;
			ViewBag.CurrDay = currDay;
			ViewBag.UserId = userId;
			ViewBag.currUserAchievements = await _context.Achievements.Where(ach => ach.UserId == userId && ach.Day == activeDay).OrderBy(ach => ach.SerialNumber).ToListAsync();
			//var resume = await _context.Resumes.Where(r => r.UserId == userId && r.Day == activeDay).FirstOrDefaultAsync();

			var (id, resume) = GetResume(userId, activeDay, _context);
			ViewBag.Id = id;
			ViewBag.Resume = resume;

			var (id_, feedbackText) = GetFeedback(userId, activeDay, _context);
			ViewBag.Feedback = feedbackText;

			if (activeDay == 7)
			{
				var (finalId, finalFeedbackText) = GetFeedback(userId, 8, _context);
				ViewBag.FinalFeedback = finalFeedbackText;
			}
			return View();
		}

		[HttpPost]
		public async Task<JsonResult> AddAchievement(string userId, int day, int serialNumber, string achievement)
		{
			Guid achievementId = Guid.NewGuid();
			SelfEstimationCheckList selfEstimationCheckList = new SelfEstimationCheckList { Id = achievementId, UserId = userId, Day = day, SerialNumber = serialNumber, Achievement = achievement };
			if (ModelState.IsValid)
			{
				_context.Achievements.Add(selfEstimationCheckList);
			}
			await _context.SaveChangesAsync();
			return Json(selfEstimationCheckList);
		}

		[HttpPost]
		public async Task<ActionResult> EditAchievement(string userId, int day, string achievement, string? id)
		{
			if (id == null)
			{
				return NotFound();
			}
			Guid achievementId = Guid.Parse(id);
			//SelfEstimationCheckList selfEstimationCheckList = new SelfEstimationCheckList { Id = achievementId, UserId = userId, Day = day, SerialNumber = serialNumber, Achievement = achievement};
			SelfEstimationCheckList selfEstimationCheckList = await _context.Achievements.FindAsync(achievementId);
			selfEstimationCheckList.Achievement = achievement;
            _context.Entry(selfEstimationCheckList).State = EntityState.Modified;
			await _context.SaveChangesAsync();
			return RedirectToAction("Index");
		}

		[HttpPost]
		public async Task<ActionResult> SendResume(string? id, string userId, int day, string resume)
		{
			try
			{
				if (id == null)
				{
					SelfEstimationResumeFromUser selfEstimationResumeFromUser = new SelfEstimationResumeFromUser { Id = Guid.NewGuid(), UserId = userId, Day = day, Resume = resume };
					if (ModelState.IsValid)
					{
						_context.Resumes.Add(selfEstimationResumeFromUser);
					}
					await _context.SaveChangesAsync();
					return Json(selfEstimationResumeFromUser);
				}
				else
				{
					Guid id_ = Guid.Parse(id);
					SelfEstimationResumeFromUser selfEstimationResumeFromUser = await _context.Resumes.FindAsync(id_);
					selfEstimationResumeFromUser.Resume = resume;
					_context.Entry(selfEstimationResumeFromUser).State = EntityState.Modified;
					await _context.SaveChangesAsync();
				}
			}
			catch (Exception exc)
			{
				HttpContext.Response.WriteAsync(exc.Message);
			}
			return Json(null);
		}

		[HttpGet]
		[Authorize(Roles = "admin")]
		public ActionResult SeeAchievements(int currDay, Guid marathonDateId)
		{
			List<UserDone> usersDone = new List<UserDone>();
			List<MarathonParticipant> marathonParticipants = new List<MarathonParticipant>();
			var participants = _context.Participants.Where(p => p.MarathonDateId == marathonDateId).ToList();
			var currUsers = new List<ApplicationUser>();
			foreach(var participant in participants)
			{
				var userId = participant.UserId;
				var user = _context.Users.Find(userId);
				marathonParticipants.Add(new MarathonParticipant { UserId = user.Id, UserName = user.UserName });
				int completeAch = _context.Achievements.Count(ach => ach.UserId == userId && ach.Day == currDay);
				int completeR = _context.Resumes.Count(r => r.UserId == userId && r.Day == currDay);
				bool complete = (completeAch == 100 && completeR == 1) ? true : false;
				if(complete)
				{
					usersDone.Add(new UserDone { UserId = user.Id, UserName = user.UserName });
				}
			}
			ViewBag.UsersDone = usersDone;
			ViewBag.MarathonParticipants = marathonParticipants;
			ViewBag.CurrDay = currDay;
			return View();
		}

		[HttpGet]
		[Authorize(Roles = "admin")]
		public ActionResult SeeAchievementsOfUser(int activeDay, int currDay, string userId, string name)
		{
			ViewBag.ActiveDay = activeDay;
			ViewBag.CurrDay = currDay;
			ViewBag.UserId = userId;
			ViewBag.Name = name;
			if(activeDay == currDay)
			{
				List<SelfEstimationCheckList> achievements = _context.Achievements.Where(ach => ach.UserId == userId && ach.Day == currDay).OrderBy(ach => ach.SerialNumber).ToList();
				ViewBag.Achievements = achievements;
			}

			var resume = _context.Resumes.Where(r => r.UserId == userId && r.Day == activeDay).FirstOrDefault();
			if(resume != null)
			{
				ViewBag.Resume = resume.Resume;
			}

			var (id, feedback) = GetFeedback(userId, activeDay, _context);
			ViewBag.Id = id;
			ViewBag.Feedback = feedback;

			var (finalId, finalFeedback) = GetFeedback(userId, 8, _context);
			ViewBag.FinalId = finalId;
			ViewBag.FinalFeedback = finalFeedback;
			return View();
		}

		private (Guid, string) GetFeedback(string userId, int day, ApplicationDbContext _context)
		{
			var feedback = _context.Feedbacks.Where(r => r.UserId == userId && r.Day == day).FirstOrDefault();
			var result = (id: Guid.Empty, feedback: "");
			if (feedback != null)
			{
				result.id = feedback.Id;
				result.feedback = feedback.Feedback;
			}
			return result;
		}

		private (Guid, string) GetResume(string userId, int day, ApplicationDbContext _context)
		{
			var resume = _context.Resumes.Where(r => r.UserId == userId && r.Day == day).FirstOrDefault();
			var result = (id: Guid.Empty, resume: "");
			if (resume != null)
			{
				result.id = resume.Id;
				result.resume = resume.Resume;
			}
			return result;
		}

		[HttpPost]
		[Authorize(Roles = "admin")]
		public async Task<JsonResult> SeeAchievementsOfUser(int day, string userId, string feedback, string? id)
		{
			try
			{
				if(id.IsNullOrEmpty())
				{
					SelfEstimationFeedbackToUser selfEstimationFeedbackToUser = new SelfEstimationFeedbackToUser { Id = Guid.NewGuid(), UserId = userId, Day = day, Feedback = feedback };
					_context.Feedbacks.Add(selfEstimationFeedbackToUser);
					await _context.SaveChangesAsync();
                    return Json(selfEstimationFeedbackToUser);
				}
				else
				{
					Guid id_ = Guid.Parse(id);
					SelfEstimationFeedbackToUser selfEstimationFeedbackToUser = await _context.Feedbacks.FindAsync(id_);
					selfEstimationFeedbackToUser.Feedback = feedback;
					_context.Entry(selfEstimationFeedbackToUser).State = EntityState.Modified;
					await _context.SaveChangesAsync();
                }
			}
			catch (Exception exc)
			{
				HttpContext.Response.WriteAsync(exc.Message);
			}

			return Json(null);
		}
	}
}