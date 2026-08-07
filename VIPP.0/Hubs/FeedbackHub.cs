using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNetCore.SignalR;

namespace VIPP.Hubs
{
	public class FeedbackHub : Hub
	{
		private static Dictionary<string, string> _connections = new();
		public async Task RegisterUser(string userId)
		{
			await Groups.AddToGroupAsync(Context.ConnectionId, userId);
        }

        public async Task AddFeedback(string text, string userId)
		{
			await Clients.Group(userId).SendAsync("addFeedback", text);
		}
		public async Task AddFinalFeedback(string text, string userId)
		{
			await Clients.Group(userId).SendAsync("addFinalFeedback", text);
		}
	}
}