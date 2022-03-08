using BigBirdie.Hubs;
using Microsoft.AspNetCore.SignalR;
using System.Collections.ObjectModel;
using System.Timers;

namespace BigBirdie.Models
{
    /// <summary>
    /// Classe Singleton gérant toutes les sessions de l’app
    /// </summary>
    public class QuizService
    {
        private readonly IHubContext<QuizHub, IQuizHub> HubContext;
        private List<QuizSession> Sessions { get; set; }
        private List<QuizUser> Users { get; set; }

        public QuizService(IHubContext<QuizHub, IQuizHub> hubContext)
        {
            this.HubContext = hubContext;
            this.Sessions = new List<QuizSession>();
            this.Users = new List<QuizUser>();
        }

        /// <summary>
        /// Création d’une session
        /// </summary>
        /// <param name="code">Code de la session</param>
        /// <param name="owner">Nom du créateur</param>
        /// <returns>true si ajouté avec succès</returns>
        public bool AddSession(string code, string owner)
        {
            if (string.IsNullOrEmpty(code))
                return false;

            if (this.SessionExists(code))
                return false;

            this.Sessions.Add(new QuizSession(code, owner));


            return true;
        }

        public bool SessionExists(string code)
        {
            return this.Sessions.Any(session => session.Code == code);
        }

		public void StartSession(string code, string username)
		{
            QuizSession? session = this.GetSession(code);
            QuizUser? user = this.GetUser(username);

            if (session == null || user == null || !IsSessionOwner(code, username))
                return;

            session.InitQuiz();
            this.SendQuestion(session);
        }

		public void NextQuestion(string code, string username)
		{
            QuizSession? session = this.GetSession(code);
            QuizUser? user = this.GetUser(username);

            if (session == null || user == null || !IsSessionOwner(code, username))
                return;

            if (session.NextQuestion())
                this.SendQuestion(session);
            else
			{
                // todo afficher les scores
                this.HubContext.Clients.Group(session.Code).SessionUpdate(session.Serialize());
            }
        }

        private void SendQuestion(QuizSession session, bool resend = true)
		{
            if (resend)
            {
                session.Start();
                session.TimedOut += QuestionTimeOut;
            }
            string question = session.GetQuestionJson();

            this.HubContext.Clients.Group(session.Code).SessionUpdate(session.Serialize());
        }

		private void QuestionTimeOut(object? sender, EventArgs e)
		{
            QuizSession? session = sender as QuizSession;

            if (session == null)
                return;

            session.TimedOut -= QuestionTimeOut;

            int answer = session.GetAnswer();

            this.HubContext.Clients.Group(session.Code).SendAnswer(answer);
        }

		public void SendAnswer(string code, string username, int answer)
		{
            QuizSession? session = this.GetSession(code);
            QuizUser? user = this.GetUser(username);

            if (session == null || user == null)
                return;

            if (session.State == SessionState.QUESTION)
                user.SetAnswer(session.Code, answer);
        }

		/// <summary>
		/// Connecte un utilisateur à l’app
		/// </summary>
		/// <param name="username">Nom de l’utilisateur</param>
		public void ConnectUser(string username)
		{
            if (!this.Users.Any(user => user.UserName == username))
                this.Users.Add(new QuizUser(username));

            QuizUser? user = this.GetUser(username);
            if (user == null) return;

            user.StopTimer();
            user.TimedOut -= UserTimedOut;
        }

        /// <summary>
        /// Ajoute un utilisateur à une session
        /// </summary>
        /// <param name="code">Code de session</param>
        /// <param name="username">Nom de l’utilisateur</param>
        /// <returns>ajout avec succès ou non</returns>
        public bool AddUserToSession(string code, string username)
        {
            QuizSession? session = this.GetSession(code);
            QuizUser? user = this.GetUser(username);

            if (session == null || user == null)
                return false;

            bool res = session.TryAddUser(user);

            // supprime l’utilisateur d’éventuelles autres sessions.
            string[] codes = this.Sessions
                .Where(s => s.HasUser(user) && s.Code != session.Code)
                .Select(s => s.Code).ToArray();
            foreach (string otherCode in codes)
                this.RemoveUserFromSession(otherCode, user.UserName);

            return res;
        }

		private QuizUser? GetUser(string username)
		{
            return this.Users.FirstOrDefault(user => user.UserName == username);
		}

		internal QuizSession? GetSession(string code)
        {
            return this.Sessions.FirstOrDefault(session => session.Code == code);
        }

        public bool IsSessionOwner(string code, string username)
        {
            QuizSession? session = this.GetSession(code);

            if (session == null)
                return false;

            QuizUser? user = this.GetUser(username);

            if (user == null)
                return false;

            return session.Owner == user.UserName;
        }

        public void RemoveSession(string code)
        {
            this.Sessions.RemoveAll(session => session.Code == code);
        }

        /// <summary>
        /// Enclenche le timer de time out pour l’utilisateur en cas de perte de connexion
        /// S’il ne se reconnecte pas d’ici la fin du timer, il est retiré des salons
        /// </summary>
        /// <param name="username"></param>
		public void TimeoutUser(string username)
		{
            QuizUser? user = this.GetUser(username);

            if (user == null)
                return;

            if (!this.Sessions.Any(s => s.HasUser(user)))
                return;

            Console.WriteLine("starting timer for " + user.UserName);

            // début du timer
            user.StartTimer();

            // abonnement au time out du timer
            user.TimedOut += UserTimedOut;
        }

        /// <summary>
        /// Callback du timer de time out de l’utilisateur
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
		private void UserTimedOut(object? sender, EventArgs e)
		{
            QuizUser? user = sender as QuizUser;

            if (user == null)
                return;

            user.TimedOut -= UserTimedOut;

            Console.WriteLine("User " + user.UserName + " timed out.");

			string[] codes = this.Sessions.Where(s => s.HasUser(user)).Select(s => s.Code).ToArray();
            foreach (string code in codes)
                this.RemoveUserFromSession(code, user.UserName);
        }

        /// <summary>
        /// Supprime l’utilisateur d’une session. 
        /// S’il en est le créateur, yeet tous le monde hors de la session.
        /// </summary>
        /// <param name="code"></param>
        /// <param name="username"></param>
		public void RemoveUserFromSession(string code, string username)
        {
            QuizSession? session = this.GetSession(code);
            QuizUser? user = this.GetUser(username);

            if (session == null || user == null)
                return;

            if (!session.HasUser(user))
                return;

            user.RemoveSessions(session.Code);
            session.RemoveUser(user);

            if (session.Owner == user.UserName)
			{
                this.RemoveSession(session.Code);
                this.HubContext.Clients.Group(session.Code).Error("L’hôte n’est plus connecté au salon !");
            }
            else
                HubContext.Clients.Group(code).SessionUpdate(session.Serialize());

            // si l’utilisateur n’est plus dans aucun salon, on le supprime
            if (!this.Sessions.Any(s => s.HasUser(user)))
                this.Users.RemoveAll(u => u.UserName == user.UserName);
            
            return;
        }
    }
}
