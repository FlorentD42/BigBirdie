using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace BigBirdie.Models
{
	public class QuizItem2
	{
		[JsonProperty(PropertyName = "id")]
		public int Id { get; set; }

		[JsonProperty(PropertyName = "question")]
		public string? Question { get; set; }

		[JsonProperty(PropertyName = "propositions")]
		public List<string> Propositions { get; set; } = new List<string>();

		[JsonProperty(PropertyName = "réponse")]
		public string? Reponse { get; set; }

		[JsonProperty(PropertyName = "anecdote")]
		public string? Anecdote { get; set; }

		[JsonProperty(PropertyName = "difficulté")]
		public string? Difficulte { get; set; }

	}
}
