using BigBirdie.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace BigBirdie.Hubs
{
    [Authorize]
    public class QuizHub : Hub<IQuizHub>
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
        public async Task JoinSession(string code)
        {
            Console.WriteLine(Username + " joined session " + code);
            if (!this.QuizService.SessionExists(code))
            {
                await Clients.Caller.Error("Le salon n’existe pas.");
                return;
            }

            if (!this.QuizService.AddUserToSession(code, Username))
			{
                await Clients.Caller.Error("Impossible de rejoindre le salon.");
                return;
			}
            
            if (this.QuizService.IsSessionOwner(code, Username))
                await Clients.Caller.IsOwner();

            await Groups.AddToGroupAsync(Context.ConnectionId, code);

            await Clients.Group(code).SessionUpdate(this.QuizService.GetSession(code)?.Serialize());
        }

        public void StartSession(string code)
		{
            this.QuizService.StartSession(code, Username);
		}

        public void SendAnswer(string code, string answer)
		{
            try
            {
                int ans = Convert.ToInt32(answer);
                this.QuizService.SendAnswer(code, Username, ans);
            }
            catch
			{

			}
		}

        public void NextQuestion(string code)
		{
            this.QuizService.NextQuestion(code, Username);
		}

        /// <summary>
        /// Déconnexion d’un salon
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public async Task LeaveSession(string code)
        {
            Console.WriteLine(Username + " logged out");
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, code);
            this.QuizService.RemoveUserFromSession(code, Username);
        }

        /// <summary>
        /// Déconnexion du Client
        /// </summary>
        /// <param name="exception"></param>
        /// <returns></returns>
        public override Task OnDisconnectedAsync(Exception? exception)
        {
            Console.WriteLine(Username + " lost connection");
            this.QuizService.TimeoutUser(Username);
            return base.OnDisconnectedAsync(exception);
        }

        /// <summary>
        /// Connexion du Client
        /// </summary>
        /// <returns></returns>
        public override Task OnConnectedAsync()
		{
            Console.WriteLine(Username + " connected");
            this.QuizService.ConnectUser(Username);
			return base.OnConnectedAsync();
		}
	}
}
