using System.Timers;
using Timer = System.Timers.Timer;

namespace BigBirdie.Models
{
	public class QuizUser
	{
		public EventHandler? TimedOut;
		public string UserName { get; private set; }
		private Timer Timer { get; set; }

		public bool IsOnline { get; set; }

		public QuizUser(string username)
		{
			this.UserName = username;
			this.Timer = new Timer(5000);
			this.Timer.AutoReset = false;
		}

		public void StartTimer()
		{
			Timer.Stop();
			Timer.Start();
			Timer.Elapsed += Timer_Elapsed;
		}

		private void Timer_Elapsed(object? sender, ElapsedEventArgs e)
		{
			TimedOut?.Invoke(this, e);
		}

		public void StopTimer()
		{
			Timer.Stop();
		}
	}
}
