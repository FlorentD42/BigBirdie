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
		private Dictionary<string, int> Answers { get; set; }
		private Dictionary<string, int> Scores { get; set; }

		public QuizUser(string username)
		{
			this.UserName = username;
			this.Timer = new Timer(5000);
			this.Timer.AutoReset = false;
			this.Timer.Elapsed += Timer_Elapsed;
			this.Answers = new Dictionary<string, int>();
			this.Scores = new Dictionary<string, int>();
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

		public void SetAnswer(string code, int answer)
		{
			this.Answers[code] = answer;
		}

		public void ValidateAnswer(string code, int answer)
		{
			if (!this.Scores.ContainsKey(code))
				this.Scores[code] = 0;

			if (this.Answers.ContainsKey(code) && this.Answers[code] == answer)
				this.Scores[code]++;
		}

		internal void RemoveSessions(string code)
		{
			this.Scores.Remove(code);
			this.Answers.Remove(code);
		}
	}
}
