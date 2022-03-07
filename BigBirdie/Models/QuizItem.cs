using System.Text.Json;
using System.Text.Json.Serialization;

namespace BigBirdie.Models
{
	public class QuizItem
	{
		[JsonPropertyName("id")]
		public int Id { get; set; }

		[JsonPropertyName("question")]
		public string? Question { get; set; }

		[JsonPropertyName("propositions")]
		public List<string> Propositions { get; set; } = new List<string>();

		[JsonIgnore]
		public string? Reponse { get; set; }

		[JsonPropertyName("réponse")]
		public string? ReponseSetter
		{
			set => Reponse = value;
		}

		[JsonIgnore]
		[JsonPropertyName("anecdote")]
		public string? Anecdote { get; set; }

		[JsonPropertyName("difficulté")]
		public string? Difficulte { get; set; }

		public string GetPublicJson()
		{
			return JsonSerializer.Serialize(this);
		}
	}
}
