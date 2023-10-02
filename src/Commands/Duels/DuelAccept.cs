using System;
using System.Net.Http;
using System.Threading.Tasks;
using ATPbot.Duels;
using Discord.Interactions;

namespace ATPbot.Commands.Duels
{
    public partial class Duels : InteractionModuleBase<SocketInteractionContext>
    {
        [ComponentInteraction($"{DUEL_ACCEPT}:*", true)]
        public async Task DuelAccept(string guid)
        {
            var duel = duelManager.GetDuel(new Guid(guid));
            if (duel == null)
            {
                await RespondAsync("That duel does not exist!", ephemeral: true);
                return;
            }

            var challenger = duel.Challenger;
            if (challenger == null)
            {
                await RespondAsync("You must link your Quaver account first!", ephemeral: true);
                return;
            }
            var challengee = userManager.GetUserWithDiscordId(Context.Interaction.User.Id);
            if (challengee == null)
            {
                await RespondAsync("That user has not linked their Quaver account!", ephemeral: true);
                return;
            }

            if (duel.Challengee != challengee)
            {
                await RespondAsync("You are not the challengee of that duel!", ephemeral: true);
                return;
            }

            if (duel.Accepted)
            {
                await RespondAsync("You have already accepted that duel!", ephemeral: true);
                return;
            }

            duelManager.AcceptDuel(duel.Id);
            // var mapsetIds = await quaverWebApi.Endpoints.GetRankedMapsets();
            // duel.SetRandomMap(mapsetIds, quaverWebApi);

            var map = await quaverWebApi.Endpoints.GetMap(duel.MapId);

            await RespondAsync(
                $"<@{challenger.DiscordId}>, your duel with <@{challengee.DiscordId}> has been accepted.\n" +
                $"The map is: [{map.Artist} - {map.Title}](https://quavergame.com/mapset/map/{map.Id}); <t:{((DateTimeOffset)duel.EndAt!).ToUnixTimeSeconds()}:R>.");
        }
    }
}