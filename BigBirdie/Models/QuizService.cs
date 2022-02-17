using System.Collections.ObjectModel;

namespace BigBirdie.Models
{
    /// <summary>
    /// Classe Singleton gérant toutes les sessions de l’app
    /// </summary>
    public class QuizService
    {
        private Dictionary<string, QuizSession> Sessions { get; set; }

        public QuizService()
        {
            this.Sessions = new Dictionary<string, QuizSession>();
        }

        public bool AddSession(string code, string owner)
        {
            if (string.IsNullOrEmpty(code))
                return false;

            if (this.SessionExists(code))
                return false;

            this.Sessions[code] = new QuizSession(code, owner);

            return true;
        }

        public bool SessionExists(string code)
        {
            return this.Sessions.ContainsKey(code);
        }

        public bool AddUser(string code, string user)
        {
            if (string.IsNullOrEmpty(code) || string.IsNullOrEmpty(user))
                return false;

            if (!this.SessionExists(code))
                return false;

            return this.Sessions[code].TryAddUser(user);
        }

        internal QuizSession? GetSession(string code)
        {
            if (string.IsNullOrEmpty(code))
                return null;

            if (!this.SessionExists(code))
                return null;

            return this.Sessions[code];
        }

        public bool IsSessionOwner(string code, string user)
        {
            if (string.IsNullOrEmpty(code) || string.IsNullOrEmpty(user))
                return false;

            if (!this.SessionExists(code))
                return false;

            return this.Sessions[code].Owner == user;
        }

        public void RemoveSession(string code)
        {
            this.Sessions.Remove(code);
        }

        public bool RemoveUserFromSession(string code, string user)
        {
            if (string.IsNullOrEmpty(code) || string.IsNullOrEmpty(user))
                return false;

            if (!this.SessionExists(code))
                return false;

            if (this.Sessions[code].HasUser(user))
            {
                this.Sessions[code].RemoveUser(user);
                return true;
            }

            return false;
        }

        public IEnumerable<string> RemoveUser(string username)
        {
            List<string> codes = new List<string>();

            foreach (string code in this.Sessions.Keys)
                if (this.RemoveUserFromSession(code, username))
                    codes.Add(code);

            return codes;
        }

        public ReadOnlyCollection<string> GetSessionUsers(string code)
        {
            ReadOnlyCollection<string> list = new List<string>().AsReadOnly();

            if (string.IsNullOrEmpty(code))
                return list;

            if (!this.SessionExists(code))
                return list;

            return this.Sessions[code].GetUsers();
        }
    }
}
