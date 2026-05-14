using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace VIPP.Models
{
    // В профиль пользователя можно добавить дополнительные данные, если указать больше свойств для класса ApplicationUser. Подробности см. на странице https://go.microsoft.com/fwlink/?LinkID=317594.
    public class ApplicationUser : IdentityUser
    {
        public DateTime DateOfBirth { get; set; }
        public string Gender { get; set; }
    }

    public class Marathon
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Cost { get; set; }
        public int Count { get; set; }
        public int CountDays { get; set; }
    }

    public class MarathonDate
    {
        public Guid Id { get; set; }
        public Guid MarathonId { get; set; }

        [NotMapped]
        public int Year { get; set; }

        [NotMapped]
        public int Month { get; set; }

        [NotMapped]
        public int Day { get; set; }

        public DateTime StartDate { get; set; }
    }

    public class Participant

    {
        public Guid Id { get; set; }
        public string UserId { get; set; }
        public Guid MarathonDateId { get; set; }
    }
    public class SelfEstimationCheckList
    {
        public Guid Id { get; set; }
        public string UserId { get; set; }
        public int Day { get; set; }

        public int SerialNumber { get; set; }
        public string Achievement { get; set; }
    }

    public class SelfEstimationResumeFromUser
    {
        public Guid Id { get; set; }
        public string UserId { get; set; }
        public int Day { get; set; }
        public string Resume { get; set; }
    }

    public class SelfEstimationFeedbackToUser
    {
        public Guid Id { get; set; }
        public string UserId { get; set; }
        public int Day { get; set; }
        public string Feedback { get; set; }
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Marathon> Marathons { get; set; }
        public DbSet<MarathonDate> MarathonDates { get; set; }
        public DbSet<Participant> Participants { get; set; }

        public DbSet<SelfEstimationCheckList> Achievements { get; set; }
        public DbSet<SelfEstimationResumeFromUser> Resumes { get; set; }
        public DbSet<SelfEstimationFeedbackToUser> Feedbacks { get; set; }
    }
}