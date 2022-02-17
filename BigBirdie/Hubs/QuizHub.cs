using BigBirdie.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using System.Security.Principal;

namespace BigBirdie.Hubs
{
    [Authorize]
    public class QuizHub : Hub
    {
        private readonly QuizService QuizService;
        private string Username => Context.User?.Identity?.Name ?? string.Empty;

        public QuizHub(QuizService quizService)
        {
            this.QuizService = quizService;
        }

        /// <summary>
        /// Connexion à un salon
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public async Task AddToGroup(string code)
        {
            if(!this.QuizService.SessionExists(code))
            {
                await Clients.Caller.SendAsync("Error", $"Code {code} invalide.");
                return;
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, code);

            await Clients.Group(code).SendAsync("SessionUpdate", this.QuizService.GetSession(code)?.Serialize());
        }

        /// <summary>
        /// Déconnexion d’un salon
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public async Task Logout(string code)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, code);

            if (this.QuizService.IsSessionOwner(code, Username))
            {
                this.QuizService.RemoveSession(code);

                await Clients.Group(code).SendAsync("SessionEnded");
            }
            else
            {
                this.QuizService.RemoveUserFromSession(code, Username);

                await Clients.Group(code).SendAsync("SessionUpdate", this.QuizService.GetSession(code)?.Serialize());
            }
        }

        /// <summary>
        /// Déconnexion du Client de tous les salons
        /// </summary>
        /// <param name="exception"></param>
        /// <returns></returns>
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            IEnumerable<string>? codes = this.QuizService.RemoveUser(Username);
            foreach(string code in codes)
                await this.Logout(code);
        }
    }
}
