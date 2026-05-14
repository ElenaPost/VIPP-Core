using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNetCore.SignalR;

namespace VIPP.Hubs
{
	public class FeedbackHub : Hub
	{
		public async Task AddFeedback(string text, string userId)
		{
			await Clients.Client(userId).SendAsync("addFeedback", text);
		}
		public async Task AddFinalFeedback(string text, string userId)
		{
			await Clients.Client(userId).SendAsync("addFinalFeedback", text);
		}
	}
}