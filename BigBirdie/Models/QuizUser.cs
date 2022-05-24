using System.Timers;
using Timer = System.Timers.Timer;

namespace BigBirdie.Models
{
	public class QuizUser
	{
		public string UserName { get; private set; }
		private Timer Timer { get; set; }
		public EventHandler? TimedOut;
		public bool IsOnline { get; set; }
		private Dictionary<string, string> Answers { get; set; }
		private Dictionary<string, int> Scores { get; set; }

		public QuizUser(string username)
		{
			this.UserName = username;
			this.Timer = new Timer(5000);
			this.Timer.AutoReset = false;
			this.Timer.Elapsed += Timer_Elapsed;
			this.Answers = new Dictionary<string, string>();
			this.Scores = new Dictionary<string, int>();
		}

        public int GetScore(string code)
        {
			return this.Scores.ContainsKey(code) ? this.Scores[code] : 0;
        }

        public void StartTimer()
		{
			this.Timer.Stop();
			this.Timer.Start();
		}

		private void Timer_Elapsed(object? sender, ElapsedEventArgs e)
		{
			this.TimedOut?.Invoke(this, e);
		}

		public void StopTimer()
		{
			Timer.Stop();
		}

		public void SetAnswer(string code, string answer)
		{
			this.Answers[code] = answer;
		}

		public void ValidateAnswer(string code, string answer)
		{
			if (!this.Scores.ContainsKey(code))
				this.Scores[code] = 0;

			if (this.Answers.ContainsKey(code) && this.Answers[code] == answer)
				this.Scores[code]++;

			this.Answers[code] = "";
		}

		public void AddSession(string code)
        {
			this.Scores[code] = 0;
			this.Answers[code] = "";
        }
		public void RemoveSessions(string code)
		{
			this.Scores.Remove(code);
			this.Answers.Remove(code);
		}
	}
}
