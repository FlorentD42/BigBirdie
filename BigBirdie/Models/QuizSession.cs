using System.Collections.ObjectModel;
using System.Text.Json;

namespace BigBirdie.Models
{
    /// <summary>
    /// Classe gérant une session de quiz
    /// </summary>
    public class QuizSession
    {
        public List<string> Users { get; private set; } = new List<string>();
        public string Code { get; private set; }
        public string Owner { get; private set; }
        public int MaxSize { get; private set; }

        public QuizSession(string code, string owner)
        {
            this.Code = code;
            this.Owner = owner;
            this.MaxSize = 20;
        }

        public bool HasUser(string user)
        {
            return Users.Contains(user);
        }

        public void AddUser(string user)
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
            this.AddUser(user);
            return true;
        }

        public string Serialize()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
