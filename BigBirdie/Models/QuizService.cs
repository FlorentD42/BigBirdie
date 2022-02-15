namespace BigBirdie.Models
{
    public class QuizService
    {
        public List<string> Sessions { get; set; }

        public QuizService()
        {
            this.Sessions = new List<string>();
        }
    }
}
