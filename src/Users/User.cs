using System;

namespace ATPbot.Users
{
    public class User
    {
        public ulong DiscordId { get; private set; }
        public int QuaverId { get; private set; }

        public User(ulong discordId, int quaverId)
        {
            DiscordId = discordId;
            QuaverId = quaverId;
        }

        public override bool Equals(object? obj)
        {
            if (obj is User user)
            {
                return user.DiscordId == DiscordId && user.QuaverId == QuaverId;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(DiscordId, QuaverId);
        }
    }
}