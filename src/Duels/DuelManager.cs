using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using ATPbot.Logging;
using ATPbot.Users;
using Discord.WebSocket;
using QuaverWebApi.v1.Structures.Enums;

namespace ATPbot.Duels
{
    public class DuelManager
    {
        private Logger Logger { get; set; }
        private DiscordSocketClient Client { get; set; }
        private QuaverWebApi.Wrapper quaverWebApi { get; set; }
        private Timer Timer { get; set; }
        // private TimeSpan MaxTime { get; set; } = new TimeSpan(3, 0, 0, 0);
        private TimeSpan DuelTime { get; set; } = new TimeSpan(0, 15, 0);

        private List<Duel> Duels { get; set; }

        public DuelManager(Logger logger, DiscordSocketClient client, QuaverWebApi.Wrapper quaverWebApi)
        {
            Logger = logger;
            Client = client;
            this.quaverWebApi = quaverWebApi;
            Duels = DataManager.Get<List<Duel>>(this, "duels") ?? new List<Duel>();
            // Timer = new Timer(1000 * 60);
            Timer = new Timer(1000 * 15);
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
                if (duel.IsExpired())
                {
                    EndDuel(duel);
                }
            }
            int i = Duels.RemoveAll(x => x.ToBeRemoved);
            if (i > 0)
                Save();
        }

        private void AddDuel(Duel duel)
        {
            Duels.Add(duel);
            Save();
        }

        public Duel CreateDuel(User challenger, User challengee, ulong channelId, bool waitForAccept = true)
        {
            var duel = new Duel(challenger, challengee, channelId, waitForAccept);
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
            var mapsetIds = quaverWebApi.Endpoints.GetRankedMapsets().Result;
            duelObj.SetRandomMap(mapsetIds, quaverWebApi);
            Save();
            return true;
        }

        public void RemoveDuel(Duel duel)
        {
            Duels.Remove(duel);
            Save();
        }

        public void EndDuel(Duel duel)
        {
            ModIdentifier allowedMods = ModIdentifier.Mirror | ModIdentifier.NoFail;
            var challengerScore = GetBestScore(duel.Challenger, allowedMods, duel);
            var challengeeScore = GetBestScore(duel.Challengee, allowedMods, duel);

            var channel = Client.GetChannel(duel.ChannelId) as SocketTextChannel;
            if (channel == null)
            {
                Logger.Log("Could not find channel with id " + duel.ChannelId, this, Severity.Warning);
                return;
            }
            var challengerUser = channel.GetUser(duel.Challenger.DiscordId);
            var challengeeUser = channel.GetUser(duel.Challengee.DiscordId);

            string msg = "";
            if (challengerScore.Accuracy > challengeeScore.Accuracy)
            {
                msg = $"{challengerUser.Mention} won the duel against {challengeeUser.Mention} with {challengerScore.Accuracy:0.00}% accuracy! ({challengerScore.Accuracy:0.00}% vs {challengeeScore.Accuracy:0.00}%)";
            }
            else if (challengerScore.Accuracy < challengeeScore.Accuracy)
            {
                msg = $"{challengeeUser.Mention} won the duel against {challengerUser.Mention} with {challengeeScore.Accuracy:0.00}% accuracy! ({challengeeScore.Accuracy:0.00}% vs {challengerScore.Accuracy :0.00}%)";
            }
            else
            {
                msg = $"{challengerUser.Mention} and {challengeeUser.Mention} tied with {challengerScore.Accuracy:0.00}% accuracy!";
            }

            channel.SendMessageAsync(msg);

            duel.ToBeRemoved = true;
        }

        private QuaverWebApi.v1.Structures.UserScore[] GetRecentUserScores(User user, Duel duel)
        {
            List<QuaverWebApi.v1.Structures.UserScore> scores = new List<QuaverWebApi.v1.Structures.UserScore>();
            int page = 0;
            // get scores until the oldest score is older than the duel
            while (true)
            {
                var newScores = quaverWebApi.Endpoints.GetUserScoresRecentAsync(user.QuaverId, GameMode.Keys4, 50, page).Result;
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
    }
}