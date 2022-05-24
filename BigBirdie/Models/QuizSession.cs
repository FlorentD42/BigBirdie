using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Timers;
using Timer = System.Timers.Timer;
using BigBirdie.QuizzDB;
using Microsoft.EntityFrameworkCore;

namespace BigBirdie.Models
{
    public enum SessionState
	{
        LOBBY,
        QUESTION,
        ANSWER,
        SCORE
	}

    /// <summary>
    /// Classe gérant une session de quiz
    /// </summary>
    public class QuizSession
    {
        public List<object> Users => this.GetUsers();
        private List<QuizUser> QuizUsers { get; set; } = new List<QuizUser>();
        public string Code { get; private set; }
        public string Owner { get; private set; }
        public int MaxSize { get; set; }
        public int NumberQuestions { get; set; }
        private List<QuizzItem> Questions { get; set; }
        public QuizzItem? CurrentQuestion { get => Questions.ElementAtOrDefault(QuestionIndex); }
        private int QuestionIndex { get; set; }
        public int QuestionTimer { get; set; }
        private Timer Timer { get; set; }
        private int TimerCounter;
        public EventHandler? TimedOut;
        public EventHandler? UpdateTimer;
        public string Lang { get; set; }
        public SessionState State { get; private set; }

        public QuizzDbContext QuizzDB;

        public QuizSession(string code, string owner)
        {
            this.Code = code;
            this.Owner = owner;
            this.MaxSize = 20;
            this.Questions = new List<QuizzItem>();
            this.QuestionIndex = -1;
            this.NumberQuestions = 10;
            this.QuestionTimer = 10;
            this.Lang = "Fr";
            this.Timer = new Timer(100);
            this.Timer.Elapsed += Timer_Elapsed;
            this.State = SessionState.LOBBY;
            this.QuizzDB = new QuizzDbContext();
        }

        /// <summary>
        /// Charge le fichier de Quiz pour la session
        /// </summary>
		private void LoadQuiz()
		{
            // introduction de la langue à faire
            List<QuizzItem> items = this.QuizzDB.quizzItem.Where(e => e.Lang==this.Lang).ToList();

            this.NumberQuestions = Math.Min(this.NumberQuestions, items.Count);

            // shuffle
            Random random = new Random();
            this.Questions = items.OrderBy(item => random.Next()).Take(this.NumberQuestions).ToList();
        }

		public void InitQuiz()
        {
            this.LoadQuiz();
            this.QuestionIndex = 0;
            this.TimerCounter = 0;
        }

        /// <summary>
        /// début d’une question, démarre le timer
        /// </summary>
		public void Start()
		{
            this.Timer.Start();
            this.State = SessionState.QUESTION;
		}

        /// <summary>
        /// Callback du timer de question, appelé tous les 100ms 
        /// </summary>
        private void Timer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            this.TimerCounter++;
            if (this.TimerCounter == this.QuestionTimer * 10)
            {
                this.Timer.Stop();
                this.State = SessionState.ANSWER;
                this.QuizUsers.ForEach(u => u.ValidateAnswer(this.Code, this.GetAnswer()));
                this.TimedOut?.Invoke(this, e);
                this.TimerCounter = 0;
            }
                
            this.UpdateTimer?.Invoke(this, e);
        }

        /// <summary>
        /// Retourne le temps restant en secondes pour la question actuelle
        /// </summary>
        public double GetTimeLeft()
        {
            return Math.Round(this.QuestionTimer - this.TimerCounter / 10.0, 2);
        }

        /// <summary>
        /// Retourne la question actuelle ou null
        /// </summary>
        private QuizzItem? GetQuestion()
		{
            return this.Questions.ElementAtOrDefault(this.QuestionIndex);
		}

        /// <summary>
        /// Retourne l’index de la réponse
        /// </summary>
        public string GetAnswer()
		{
            QuizzItem? question = this.GetQuestion();
            if (question == null) return "";
            return question.Rep;
        }

        /// <summary>
        /// Passe à la question suivante
        /// </summary>
        /// <returns>false si fin du questionnaire</returns>
        public bool NextQuestion()
		{
            this.QuestionIndex++;
            bool res = this.QuestionIndex < this.NumberQuestions;
            if (!res)
                this.State = SessionState.SCORE;
            return res;
		}

        /// <summary>
        /// Renvoie true si "user" est présent dans la session
        /// </summary>
		public bool HasUser(QuizUser user)
        {
            return QuizUsers.Any(u => u.UserName == user.UserName);
        }

        /// <summary>
        /// Ajoute un utilisateur à la session
        /// </summary>
        /// <param name="user"></param>
        private void AddUser(QuizUser user)
        {
            user.AddSession(this.Code);
            this.QuizUsers.Add(user);
        }

        /// <summary>
        /// Retourne une liste d’utilisateurs avec comme attributs le nom et score si disponible
        /// </summary>
        /// <returns></returns>
        private List<object> GetUsers()
        {
            return this.QuizUsers
                .Select(u => new { Name = u.UserName, Score = this.State == SessionState.SCORE ? u.GetScore(this.Code) : -1 })
                .OrderByDescending(u => u.Score)
                .ToList<object>();
        }

        /// <summary>
        /// Enlève un utilisateur de la session
        /// </summary>
        /// <param name="user"></param>
        public void RemoveUser(QuizUser user)
        {
            this.QuizUsers.RemoveAll(u => u.UserName == user.UserName);
        }

        /// <summary>
        /// Essaye d’ajouter un utilisateur
        /// </summary>
        /// <param name="user"></param>
        /// <returns>vrai si succès (déjà présent ou ajout ok), faux sinon (session démarrée ou pleine)</returns>
        public bool TryAddUser(QuizUser user)
        {
            if (this.QuizUsers.Any(u => u.UserName == user.UserName))
                return true;
            if (this.QuizUsers.Count >= MaxSize)
                return false;
            if (this.State != SessionState.LOBBY)
                return false;
            this.AddUser(user);
            return true;
        }

        /// <summary>
        /// Renvoie l’état de la session sous format JSON
        /// </summary>
        /// <returns></returns>
        public string Serialize()
        {
            JsonSerializerOptions options = new JsonSerializerOptions
            {
                Converters = { new JsonStringEnumConverter() }
            };
            return JsonSerializer.Serialize(this, options);
        }
    }
}
