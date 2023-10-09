using System.Collections.Generic;
using ATPbot.Logging;

namespace ATPbot.Users;

public class UserManager
{
    private Logger Logger { get; set; }

    private List<User> Users { get; set; }

    private Dictionary<int, UserStats> Stats { get; set; }

    public UserManager(Logger logger)
    {
        Logger = logger;
        Users = DataManager.Get<List<User>>(this, "users") ?? new List<User>();
        Stats = DataManager.Get<Dictionary<int, UserStats>>(this, "stats") ?? new Dictionary<int, UserStats>();
    }

    ~UserManager()
    {
        Save();
    }

    private void Save()
    {
        DataManager.Set(this, "users", Users);
        DataManager.Set(this, "stats", Stats);
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

    public UserStats GetStats(User user)
    {
        if (Stats.ContainsKey(user.QuaverId))
        {
            return Stats[user.QuaverId];
        }
        else
        {
            var stats = new UserStats();
            Stats.Add(user.QuaverId, stats);
            return stats;
        }
    }

    public void SetStats(User user, UserStats stats)
    {
        if (Stats.ContainsKey(user.QuaverId))
        {
            Stats[user.QuaverId] = stats;
        }
        else
        {
            Stats.Add(user.QuaverId, stats);
        }
        Save();
    }
}
