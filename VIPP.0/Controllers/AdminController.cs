using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using VIPP.Models;
using Microsoft.EntityFrameworkCore;

namespace VIPP.Controllers
{
	public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

		public AdminController(ApplicationDbContext context)
		{
			_context = context;
        }

        [Authorize(Roles = "admin")]
		public async Task<ActionResult> Index()
		{
			if (ModelState.IsValid)
			{
				var users = await _context.Users.ToListAsync();
				return View(users);
            }
            return View();
		}

		[HttpGet]
		[Authorize(Roles = "admin")]
		public ActionResult AddMarathon()
		{
			return PartialView();
		}

		[HttpPost]
		public async Task<ActionResult> AddMarathon(Marathon marathon)
		{
			if(ModelState.IsValid)
			{
				marathon.Id = Guid.NewGuid();
				_context.Marathons.Add(marathon);
				await _context.SaveChangesAsync();
			}
			return RedirectToAction("Index");
		}

		[HttpGet]
		[Authorize(Roles = "admin")]
		public async Task<ActionResult> SetMarathonDate()
		{
			ViewBag.Marathons = new SelectList(await _context.Marathons.ToListAsync(), "Id", "Name");
			return PartialView();
		}

		[HttpPost]
		public async Task<ActionResult> SetMarathonDate(MarathonDate _marathonDate)
		{
			if(ModelState.IsValid)
			{
				var id = Guid.NewGuid();
				DateTime date = new DateTime(_marathonDate.Year, _marathonDate.Month, _marathonDate.Day);
				MarathonDate marathonDate = new MarathonDate { Id = id, MarathonId = _marathonDate.MarathonId, StartDate = date };
				_context.MarathonDates.Add(marathonDate);
				await _context.SaveChangesAsync();
			}
			return RedirectToAction("Index");
		}

		[HttpGet]
		[Authorize(Roles = "admin")]
		public async Task<ActionResult> AddParticipants()
		{
			ViewBag.MarathonDates = new SelectList(await _context.MarathonDates.ToListAsync(), "Id", "StartDate");
			ViewBag.Users = new SelectList(await _context.Users.ToListAsync(), "Id", "UserName");
			return PartialView();
		}

		[HttpPost]
		public async Task<ActionResult> AddParticipants(Participant _participant)
		{
			if(ModelState.IsValid)
			{
				Participant participant = new Participant { Id = Guid.NewGuid(), UserId = _participant.UserId, MarathonDateId = _participant.MarathonDateId };
				_context.Participants.Add(participant);
				await _context.SaveChangesAsync();
			}
			return RedirectToAction("Index");
		}

		[HttpPost]
		public async Task<ActionResult> ClearSelfEstimationCheckList()
		{
			_context.Achievements.RemoveRange(await _context.Achievements.ToListAsync());
			_context.Resumes.RemoveRange(await _context.Resumes.ToListAsync());
			_context.Feedbacks.RemoveRange(await _context.Feedbacks.ToListAsync());
			await _context.SaveChangesAsync();

			return RedirectToAction("Index");
		}
	}
}