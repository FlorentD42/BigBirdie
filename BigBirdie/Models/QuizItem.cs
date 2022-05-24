using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace BigBirdie.Models
{
    // Root myDeserializedClass = JsonSerializer.Deserialize<Root>(myJsonResponse);
    public class CategorieNomSlogan
    {
        [JsonProperty(PropertyName = "fr")]
        public CategorieNomSloganLang Fr { get; set; }

        [JsonProperty(PropertyName = "en")]
        public CategorieNomSloganLang En { get; set; }

        [JsonProperty(PropertyName = "es")]
        public CategorieNomSloganLang Es { get; set; }

        [JsonProperty(PropertyName = "it")]
        public CategorieNomSloganLang It { get; set; }

        [JsonProperty(PropertyName = "de")]
        public CategorieNomSloganLang De { get; set; }

        [JsonProperty(PropertyName = "nl")]
        public CategorieNomSloganLang Nl { get; set; }
    }

    public class CategorieNomSloganLang
    {
        [JsonProperty(PropertyName = "catégorie")]
        public string Categorie { get; set; }

        [JsonProperty(PropertyName = "nom")]
        public string Nom { get; set; }

        [JsonProperty(PropertyName = "slogan")]
        public string Slogan { get; set; }
    }

    public class QuizzLang
    {

        [JsonProperty(PropertyName = "débutant")]
        public List<Item> Debutant { get; set; }

        [JsonProperty(PropertyName = "confirmé")]
        public List<Item> Confirme { get; set; }

        [JsonProperty(PropertyName = "expert")]
        public List<Item> Expert { get; set; }
    }

    public class Item
    {
        [JsonProperty(PropertyName = "id")]
        public int Id { get; set; }

        [JsonProperty(PropertyName = "question")]
        public string Question { get; set; }

        [JsonProperty(PropertyName = "propositions")]
        public List<string> Propositions { get; set; }

        [JsonProperty(PropertyName = "réponse")]
        public string Reponse { get; set; }

        [JsonProperty(PropertyName = "anecdote")]
        public string Anecdote { get; set; }
    }

    public class Quizz
    {
        [JsonProperty(PropertyName = "fr")]
        public QuizzLang Fr { get; set; }

        [JsonProperty(PropertyName = "en")]
        public QuizzLang En { get; set; }

        [JsonProperty(PropertyName = "de")]
        public QuizzLang De { get; set; }

        [JsonProperty(PropertyName = "es")]
        public QuizzLang Es { get; set; }

        [JsonProperty(PropertyName = "it")]
        public QuizzLang It { get; set; }

        [JsonProperty(PropertyName = "nl")]
        public QuizzLang Nl { get; set; }
    }

    public class QuizItem
    {
        [JsonProperty(PropertyName = "fournisseur")]
        public string Fournisseur { get; set; }

        [JsonProperty(PropertyName = "rédacteur")]
        public string Redacteur { get; set; }

        [JsonProperty(PropertyName = "difficulté")]
        public string Difficulte { get; set; }

        [JsonProperty(PropertyName = "version")]
        public int Version { get; set; }

        [JsonProperty(PropertyName = "mise-à-jour")]
        public string MiseÀJour { get; set; }

        [JsonProperty(PropertyName = "catégorie-nom-slogan")]
        public CategorieNomSlogan CategorieNomSlogan { get; set; }

        [JsonProperty(PropertyName = "quizz")]
        public Quizz Quizz { get; set; }
    }


}
