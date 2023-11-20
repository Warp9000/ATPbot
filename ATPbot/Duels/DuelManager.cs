using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using ATPbot.Filtering;
using ATPbot.Logging;
using ATPbot.Maps;
using ATPbot.Users;
using Discord.WebSocket;
using QuaverWebApi.v1.Structures.Enums;

namespace ATPbot.Duels;

public class DuelManager
{
    private Logger Logger { get; set; }
    private DiscordSocketClient Client { get; set; }
    private QuaverWebApi.Wrapper quaverWebApi { get; set; }
    private MapsManager mapsManager { get; set; }
    private UserManager userManager { get; set; }
    private Timer Timer { get; set; }
#if DEBUG
    private TimeSpan DuelTime { get; set; } = new TimeSpan(0, 20, 0);
#else
    private TimeSpan DuelTime { get; set; } = new TimeSpan(3, 0, 0, 0);
#endif
    private TimeSpan ExpireTime { get; set; } = new TimeSpan(1, 0, 0);

    /// <summary>
    /// {0} = winner<br/>
    /// {1} = loser
    /// </summary>
    const string text_forfeit = "{0} forfeited the duel against {1}!";
    /// <summary>
    /// {0} = winner<br/>
    /// {1} = loser<br/>
    /// {2} = winner accuracy<br/>
    /// {3} = loser accuracy
    /// </summary>
    const string text_won = "{0} won the duel against {1} with {2:0.00}% accuracy! ({2:0.00}% vs {3:0.00}%)";
    /// <summary>
    /// {0} = player 1<br/>
    /// {1} = player 2<br/>
    /// {2} = accuracy
    /// </summary>
    const string text_tied = "{0} and {1} tied with {2:0.00}% accuracy!";

    public List<Duel> Duels { get; private set; }

    public DuelManager(Logger logger, DiscordSocketClient client, QuaverWebApi.Wrapper quaverWebApi, MapsManager mapsManager, UserManager userManager)
    {
        Logger = logger;
        Client = client;
        this.quaverWebApi = quaverWebApi;
        this.mapsManager = mapsManager;
        this.userManager = userManager;
        Duels = DataManager.Get<List<Duel>>(this, "duels") ?? new List<Duel>();
#if DEBUG
        Timer = new Timer(1000 * 15);
#else
        Timer = new Timer(1000 * 60);
#endif
        Timer.Elapsed += ValidateDuels;
        Timer.Start();
    }

    ~DuelManager()
    {
        Timer.Stop();
        Save();
    }

    private void Save()
    {
        DataManager.Set(this, "duels", Duels);
    }

    private void ValidateDuels(object? sender, ElapsedEventArgs? e)
    {
        foreach (var duel in Duels)
        {
            Logger.Log($"{duel.Challenger.QuaverId} vs {duel.Challengee.QuaverId}, accepted: {duel.Accepted}, time left: {duel.TimeLeft()}", this, Severity.Debug);
            if (duel.HasEnded())
            {
                EndDuel(duel);
            }
            else if (duel.HasExpired(ExpireTime))
            {
                duel.ToBeRemoved = true;
            }
        }
        int i = Duels.RemoveAll(x => x.ToBeRemoved);
        Save();
    }

    private void AddDuel(Duel duel)
    {
        Duels.Add(duel);
        Save();
    }

    public Duel CreateDuel(User challenger, User challengee, ulong channelId, bool waitForAccept = true, string? filter = null, int maxRerolls = 1)
    {
        var duel = new Duel(challenger, challengee, channelId, waitForAccept, filter, maxRerolls);
        AddDuel(duel);
        return duel;
    }

    public bool AcceptDuel(Guid duel)
    {
        var duelObj = Duels.Find(x => x.Id == duel);
        if (duelObj == null)
            return false;
        duelObj.Accepted = true;
        duelObj.AcceptedAt = DateTime.UtcNow;
        duelObj.EndAt = DateTime.UtcNow + DuelTime;
        var mapIds = FilterManager.GetMapIdsFromFilter(duelObj.Filter ?? "", mapsManager);
        duelObj.SetRandomMap(mapIds, quaverWebApi);
        Save();
        return true;
    }

    public bool RerollDuel(Guid duel)
    {
        var duelObj = Duels.Find(x => x.Id == duel);
        if (duelObj == null)
            return false;
        if (duelObj.RerollCount >= duelObj.MaxRerolls)
            return false;
        duelObj.RerollCount++;
        duelObj.ChallengerVoteReroll = false;
        duelObj.ChallengeeVoteReroll = false;
        var mapIds = FilterManager.GetMapIdsFromFilter(duelObj.Filter ?? "", mapsManager);
        duelObj.SetRandomMap(mapIds, quaverWebApi);
        Save();
        return true;
    }

    public (bool voteSuccess, bool rerolled) VoteRerollDuel(Guid duel, User user)
    {
        var duelObj = Duels.Find(x => x.Id == duel);
        if (duelObj == null)
            return (false, false);

        if (duelObj.RerollCount >= duelObj.MaxRerolls)
            return (false, false);

        if (duelObj.Challenger == user)
            duelObj.ChallengerVoteReroll = true;
        else if (duelObj.Challengee == user)
            duelObj.ChallengeeVoteReroll = true;
        else
            return (false, false);

        if (duelObj.ChallengerVoteReroll && duelObj.ChallengeeVoteReroll)
            return (true, RerollDuel(duel));

        Save();
        return (true, false);
    }

    public (bool voteSuccess, bool Ended) VoteEndEarly(Guid duel, User user)
    {
        var duelObj = Duels.Find(x => x.Id == duel);
        if (duelObj == null)
            return (false, false);

        if (duelObj.Challenger == user)
            duelObj.ChallengerVoteEndEarly = true;
        else if (duelObj.Challengee == user)
            duelObj.ChallengeeVoteEndEarly = true;
        else
            return (false, false);

        if (duelObj.ChallengerVoteEndEarly && duelObj.ChallengeeVoteEndEarly)
            return (true, EndDuel(duelObj));

        Save();
        return (true, false);
    }

    public void RemoveDuel(Duel duel)
    {
        Duels.Remove(duel);
        Save();
    }

    public bool EndDuel(Guid duel)
    {
        var duelObj = Duels.Find(x => x.Id.Equals(duel));
        if (duelObj == null)
            return false;
        return EndDuel(duelObj);
    }

    public bool EndDuel(Duel duel)
    {
        ModIdentifier allowedMods = ModIdentifier.Mirror;
        var challengerScore = GetBestScore(duel.Challenger, allowedMods, duel);
        var challengeeScore = GetBestScore(duel.Challengee, allowedMods, duel);

        var channel = Client.GetChannel(duel.ChannelId) as SocketTextChannel;
        if (channel == null)
        {
            Logger.Log("Could not find channel with id " + duel.ChannelId, this, Severity.Warning);
            return false;
        }
        var challengerUser = channel.GetUser(duel.Challenger.DiscordId);
        var challengeeUser = channel.GetUser(duel.Challengee.DiscordId);

        var challengerStats = userManager.GetStats(duel.Challenger);
        var challengeeStats = userManager.GetStats(duel.Challengee);

        string msg = "";
        if (duel.ChallengerForfeited)
        {
            msg = string.Format(text_forfeit, challengerUser.Mention, challengeeUser.Mention);
            challengerStats.Losses++;
            challengeeStats.Wins++;
        }
        else if (duel.ChallengeeForfeited)
        {
            msg = string.Format(text_forfeit, challengeeUser.Mention, challengerUser.Mention);
            challengeeStats.Losses++;
            challengerStats.Wins++;
        }
        else if (challengerScore.Accuracy > challengeeScore.Accuracy)
        {
            msg = string.Format(text_won, challengerUser.Mention, challengeeUser.Mention, challengerScore.Accuracy, challengeeScore.Accuracy);
            challengerStats.Wins++;
            challengeeStats.Losses++;
        }
        else if (challengerScore.Accuracy < challengeeScore.Accuracy)
        {
            msg = string.Format(text_won, challengeeUser.Mention, challengerUser.Mention, challengeeScore.Accuracy, challengerScore.Accuracy);
            challengeeStats.Wins++;
            challengerStats.Losses++;
        }
        else
        {
            msg = string.Format(text_tied, challengerUser.Mention, challengeeUser.Mention, challengerScore.Accuracy);
            challengerStats.Ties++;
            challengeeStats.Ties++;
        }

        userManager.SetStats(duel.Challenger, challengerStats);
        userManager.SetStats(duel.Challengee, challengeeStats);

        channel.SendMessageAsync(msg);

        duel.ToBeRemoved = true;

        return true;
    }

    private QuaverWebApi.v1.Structures.UserScore[] GetRecentUserScores(User user, Duel duel)
    {
        var map = quaverWebApi.Endpoints.GetMap(duel.MapId).Result;
        List<QuaverWebApi.v1.Structures.UserScore> scores = new List<QuaverWebApi.v1.Structures.UserScore>();
        int page = 0;
        // get scores until the oldest score is older than the duel
        while (true)
        {
            var newScores = quaverWebApi.Endpoints.GetUserScoresRecentAsync(user.QuaverId, map.GameMode, 50, page).Result;
            if (newScores.Length == 0)
                break;
            if (newScores.Max(x => x.Time) < duel.AcceptedAt!.Value)
                break;
            scores.AddRange(newScores);
            page++;
        }
        return scores.ToArray();
    }

    private QuaverWebApi.v1.Structures.UserScore GetBestScore(User user, ModIdentifier allowedMods, Duel duel)
    {
        IEnumerable<QuaverWebApi.v1.Structures.UserScore> scores = GetRecentUserScores(user, duel);
        scores = scores.Where(x => x.Time > duel.AcceptedAt && x.Time < duel.EndAt);
        scores = scores.Where(x => x.Map.Id == duel.MapId);
        scores = scores.Where(x => x.Mods == ModIdentifier.None || (x.Mods & allowedMods) == x.Mods);
        return scores.OrderByDescending(x => x.Accuracy).FirstOrDefault();
    }


    public Duel? GetDuel(User challenger, User challengee)
    {
        foreach (var duel in Duels)
        {
            if (duel.Challenger.Equals(challenger) && duel.Challengee.Equals(challengee))
            {
                return duel;
            }
        }

        return null;
    }

    public Duel? GetDuel(Guid guid)
    {
        foreach (var duel in Duels)
        {
            if (duel.Id == guid)
            {
                return duel;
            }
        }

        return null;
    }

    public bool HasDuel(User challenger, User challengee)
    {
        return GetDuel(challenger, challengee) != null;
    }

    public Duel[] GetDuels(User user)
    {
        List<Duel> duels = new List<Duel>();
        foreach (var duel in Duels)
        {
            if (duel.Challenger.Equals(user) || duel.Challengee.Equals(user))
            {
                duels.Add(duel);
            }
        }

        return duels.ToArray();
    }
}
