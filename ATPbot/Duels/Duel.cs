using System;
using ATPbot.Users;
using Newtonsoft.Json;

namespace ATPbot.Duels;

public class Duel
{
    public Guid Id { get; private set; }
    public User Challenger { get; private set; }
    public User Challengee { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? AcceptedAt { get; set; }
    public DateTime? EndAt { get; set; }
    public int MapId { get; set; }
    public ulong ChannelId { get; set; }
    public ulong MessageId { get; set; }
    public bool Accepted { get; set; }
    public string? Filter { get; set; }

    public int RerollCount { get; set; }
    public int MaxRerolls { get; set; }
    public bool ChallengerVoteReroll { get; set; }
    public bool ChallengeeVoteReroll { get; set; }

    public bool ChallengerForfeited { get; set; }
    public bool ChallengeeForfeited { get; set; }

    [JsonIgnore]
    public bool ToBeRemoved { get; set; }

    public Duel(User challenger, User challengee, ulong channelId, bool waitForAccept = true, string? filter = null, int maxRerolls = 1)
    {
        Id = Guid.NewGuid();
        Challenger = challenger;
        Challengee = challengee;
        CreatedAt = DateTime.UtcNow;
        ChannelId = channelId;
        Accepted = !waitForAccept;
        if (Accepted)
            AcceptedAt = DateTime.UtcNow;
        Filter = filter;
        MaxRerolls = maxRerolls;
    }
}
