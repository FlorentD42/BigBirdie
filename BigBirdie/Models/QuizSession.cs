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
        QUESTION_RUNNING,
        QUESTION_TIMED_OUT
	}

    /// <summary>
    /// Classe gérant une session de quiz
    /// </summary>
    public class QuizSession
    {
        public List<string> Users { get; private set; } = new List<string>();
        public string Code { get; private set; }
        public string Owner { get; private set; }
        public int MaxSize { get; private set; }
        public int NumberQuestions { get; private set; }
        private List<QuizItem> Questions { get; set; }
        public QuizItem? CurrentQuestion { get => Questions.ElementAtOrDefault(QuestionIndex); }
        private int QuestionIndex { get; set; }
        public int QuestionTimer { get; private set; }
        private Timer Timer { get; set; }
        public EventHandler? TimedOut;
        public SessionState State { get; private set; }

        public QuizSession(string code, string owner)
        {
            this.Code = code;
            this.Owner = owner;
            this.MaxSize = 20;
            this.Questions = new List<QuizItem>();
            this.QuestionIndex = -1;
            this.NumberQuestions = 3;
            this.QuestionTimer = 10;
            this.Timer = new Timer(this.QuestionTimer * 1000);
            this.Timer.AutoReset = false;
            this.Timer.Elapsed += Timer_Elapsed;
            this.State = SessionState.LOBBY;
            this.LoadQuiz();
        }

		private void LoadQuiz()
		{
            string filepath = @"Resources/quiz.json";
            Utf8JsonReader reader = new Utf8JsonReader(File.ReadAllBytes(filepath));

            List<QuizItem> items = JsonSerializer.Deserialize<List<QuizItem>>(ref reader)!;

            this.NumberQuestions = Math.Min(this.NumberQuestions, items.Count);

            // shuffle
            Random random = new Random();
            this.Questions = items.OrderBy(item => random.Next()).Take(this.NumberQuestions).ToList();
        }

        public void Start()
		{
            this.QuestionIndex = 0;
            this.Timer.Start();
            this.State = SessionState.QUESTION_RUNNING;
		}
        private void Timer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            this.State = SessionState.QUESTION_TIMED_OUT;
            this.TimedOut?.Invoke(this, e);
        }

        private QuizItem? GetQuestion()
		{
            return this.Questions.ElementAtOrDefault(this.QuestionIndex);
		}

        public string GetQuestionJson()
		{
            QuizItem? question = this.GetQuestion();
            if (question == null) return string.Empty;
            return question.GetPublicJson();
		}

        public int GetAnswer()
		{
            QuizItem? question = this.GetQuestion();
            if (question == null) return -1;
            return question.Propositions.IndexOf(question.Reponse!);
        }

        public bool NextQuestion()
		{
            this.QuestionIndex++;
            return this.QuestionIndex < this.NumberQuestions;
		}

		public bool HasUser(string user)
        {
            return Users.Contains(user);
        }

        private void AddUser(string user)
        {
            this.Users.Add(user);
        }

        public ReadOnlyCollection<string> GetUsers()
        {
            return this.Users.AsReadOnly();
        }

        public void RemoveUser(string user)
        {
            this.Users.Remove(user);
        }

        public bool TryAddUser(string user)
        {
            if (this.Users.Contains(user))
                return true;
            if (this.Users.Count >= MaxSize)
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
