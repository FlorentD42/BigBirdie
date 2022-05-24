using System.Text.Json;
using System.Text.Json.Serialization;

namespace BigBirdie.Models
{
	public class OpenQuizItem
    {
		[JsonPropertyName("id")]
		public int Id { get; set; }

		[JsonPropertyName("question")]
		public string? Question { get; set; }

		[JsonPropertyName("propositions")]
		public List<string> Propositions { get; set; } = new List<string>();

		[JsonPropertyName("reponse")]
		public string? Reponse { get; set; }

		public string? ReponseSetter
		{
			set => Reponse = value;
		}

		[JsonPropertyName("anecdote")]
		public string? Anecdote { get; set; }

		[JsonPropertyName("difficulte")]
		public string? Difficulte { get; set; }
	}
	public class OpenQuiz
	{
		[JsonPropertyName("fournisseur")]
		public string? Fournisseur { get; set; }

		[JsonPropertyName("rédacteur")]
		public string? Redacteur { get; set; }
		[JsonPropertyName("difficulté")]
		public string? Difficulté { get; set; }
		[JsonPropertyName("version")]
		public string? Version { get; set; }
		[JsonPropertyName("mise-à-jour")]
		public string? MiseAJour { get; set; }
		[JsonPropertyName("catégorie-nom-slogan")]
		public string? Cat { get; set; }



		public string GetPublicJson()
		{
			return JsonSerializer.Serialize(this);
		}
	}
}
