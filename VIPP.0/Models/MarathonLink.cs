using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VIPP.Models
{
	public struct MarathonLink
	{
		public string LinkText { get; set; }
		public string ActionName { get; set; }
		public int Day { get; set; }
		public string? CurrUserId { get; set; }
		public Guid? MarathonDateId { get; set; }
	}
}