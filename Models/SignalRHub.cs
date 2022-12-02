using Microsoft.AspNetCore.SignalR;

namespace StudentsForStudentsAPI.Models
{
    public class SignalRHub : Hub
    {
        public async Task SendMessage(string user, string message)
        {
            await Clients.All.SendAsync("messageReceived", user, message);
        }
    }
}
