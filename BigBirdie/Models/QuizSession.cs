using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Timers;
using Timer = System.Timers.Timer;

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
        public int MaxSize { get; private set; }
        public int NumberQuestions { get; private set; }
        private List<QuizItem> Questions { get; set; }
        public QuizItem? CurrentQuestion { get => Questions.ElementAtOrDefault(QuestionIndex); }
        private int QuestionIndex { get; set; }
        public int QuestionTimer { get; private set; }
        private Timer Timer { get; set; }
        private int TimerCounter;
        public EventHandler? TimedOut;
        public EventHandler? UpdateTimer;
        public SessionState State { get; private set; }

        public QuizSession(string code, string owner)
        {
            this.Code = code;
            this.Owner = owner;
            this.MaxSize = 20;
            this.Questions = new List<QuizItem>();
            this.QuestionIndex = -1;
            this.NumberQuestions = 10;
            this.QuestionTimer = 10;
            this.Timer = new Timer(100);
            this.Timer.Elapsed += Timer_Elapsed;
            this.State = SessionState.LOBBY;
            this.LoadQuiz();
        }

		private void LoadQuiz()
		{
            string filepath = @"Resources/quiz-easy.json";
            Utf8JsonReader reader = new Utf8JsonReader(File.ReadAllBytes(filepath));

            List<QuizItem> items = JsonSerializer.Deserialize<List<QuizItem>>(ref reader)!;

            this.NumberQuestions = Math.Min(this.NumberQuestions, items.Count);

            // shuffle
            Random random = new Random();
            this.Questions = items.OrderBy(item => random.Next()).Take(this.NumberQuestions).ToList();
        }

		public void InitQuiz()
        {
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
        private QuizItem? GetQuestion()
		{
            return this.Questions.ElementAtOrDefault(this.QuestionIndex);
		}

        /// <summary>
        /// Retourne l’index de la réponse
        /// </summary>
        public int GetAnswer()
		{
            QuizItem? question = this.GetQuestion();
            if (question == null) return -1;
            return question.Propositions.IndexOf(question.Reponse!);
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

		public bool HasUser(QuizUser user)
        {
            return QuizUsers.Any(u => u.UserName == user.UserName);
        }

        private void AddUser(QuizUser user)
        {
            user.AddSession(this.Code);
            this.QuizUsers.Add(user);
        }

        private List<object> GetUsers()
        {
            return this.QuizUsers
                .Select(u => new { Name = u.UserName, Score = this.State == SessionState.SCORE ? u.GetScore(this.Code) : -1 })
                .OrderByDescending(u => u.Score)
                .ToList<object>();
        }

        public void RemoveUser(QuizUser user)
        {
            this.QuizUsers.RemoveAll(u => u.UserName == user.UserName);
        }

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
