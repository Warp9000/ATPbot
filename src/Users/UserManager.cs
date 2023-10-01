using System.Collections.Generic;
using ATPbot.Logging;

namespace ATPbot.Users
{
    public class UserManager
    {
        private Logger Logger { get; set; }

        private List<User> Users { get; set; }

        public UserManager(Logger logger)
        {
            Logger = logger;
            Users = DataManager.Get<List<User>>(this, "users") ?? new List<User>();
        }

        ~UserManager()
        {
            Save();
        }

        private void Save()
        {
            DataManager.Set(this, "users", Users);
        }

        public User? GetUserWithDiscordId(ulong id)
        {
            foreach (User user in Users)
            {
                if (user.DiscordId == id)
                {
                    return user;
                }
            }
            return null;
        }

        public User? GetUserWithQuaverId(int id)
        {
            foreach (User user in Users)
            {
                if (user.QuaverId == id)
                {
                    return user;
                }
            }
            return null;
        }

        public void AddUser(User user)
        {
            Users.Add(user);
            Save();
        }

        public void CreateUser(ulong discordId, int quaverId)
        {
            var user = new User(discordId, quaverId);
            AddUser(user);
        }

        public void RemoveUser(User user)
        {
            Users.Remove(user);
            Save();
        }
    }
}