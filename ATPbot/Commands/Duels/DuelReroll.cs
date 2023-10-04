using System;
using System.Net.Http;
using System.Threading.Tasks;
using ATPbot.Duels;
using Discord;
using Discord.Interactions;

namespace ATPbot.Commands.Duels;

public partial class Duels : InteractionModuleBase<SocketInteractionContext>
{
    [ComponentInteraction($"{DUEL_REROLL}:*", true)]
    public async Task DuelReroll(string guid)
    {
        var duel = duelManager.GetDuel(new Guid(guid));
        if (duel == null)
        {
            await RespondAsync("That duel does not exist!", ephemeral: true);
            return;
        }

        var curUser = userManager.GetUserWithDiscordId(Context.Interaction.User.Id);
        if (curUser == null)
        {
            await RespondAsync("You must link your Quaver account first!", ephemeral: true);
            return;
        }

        if (duel.Challenger != curUser && duel.Challengee != curUser)
        {
            await RespondAsync("You are not a part of that duel!", ephemeral: true);
            return;
        }

        if (duel.RerollCount >= duel.MaxRerolls)
        {
            await RespondAsync("You have already rerolled that duel!", ephemeral: true);
            return;
        }

        var result = duelManager.VoteRerollDuel(duel.Id, curUser);

        if (result.rerolled)
        {
            var map = await quaverWebApi.Endpoints.GetMap(duel.MapId);

            MessageComponent comp = new ComponentBuilder()
                .WithButton("Forfeit", $"{DUEL_FORFEIT}:{duel.Id}", ButtonStyle.Danger)
                .WithButton("Reroll", $"{DUEL_REROLL}:{duel.Id}", ButtonStyle.Secondary, disabled: !duel.CanReroll())
                .Build();

            await RespondAsync(
                $"<@{duel.Challenger.DiscordId}> and <@{duel.Challengee.DiscordId}>, your duel has been rerolled!\n" +
                $"The map is: [{map.Artist} - {map.Title} [{map.DifficultyName}]](https://quavergame.com/mapset/map/{map.Id}); <t:{((DateTimeOffset)duel.EndAt!).ToUnixTimeSeconds()}:R>.",
                components: comp);
        }
        else if (result.voteSuccess)
        {
            await RespondAsync("You have voted to reroll the duel!", ephemeral: true);
        }
        else
        {
            await RespondAsync("You have already voted to reroll the duel!", ephemeral: true);
        }
    }
}
