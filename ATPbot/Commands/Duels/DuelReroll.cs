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
            var embed = Defaults.WarningEmbed("That duel does not exist!");
            await RespondAsync(embed: embed, ephemeral: true);
            return;
        }

        var curUser = userManager.GetUserWithDiscordId(Context.Interaction.User.Id);
        if (curUser == null)
        {
            var embed = Defaults.WarningEmbed("You must link your Quaver account first!");
            await RespondAsync(embed: embed, ephemeral: true);
            return;
        }

        if (duel.Challenger != curUser && duel.Challengee != curUser)
        {
            var embed = Defaults.WarningEmbed("You are not a part of that duel");
            await RespondAsync(embed: embed, ephemeral: true);
            return;
        }

        if (duel.RerollCount >= duel.MaxRerolls)
        {
            var embed = Defaults.WarningEmbed("You have already rerolled the duel the maximum amount of times!");
            await RespondAsync(embed: embed, ephemeral: true);
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

            var embedBuilder = Defaults.DefaultEmbedBuilder
                .WithTitle("Duel Rerolled")
                .WithDescription($"The map is: [{map.Artist} - {map.Title} [{map.DifficultyName}]](https://quavergame.com/mapset/map/{map.Id})");

            await RespondAsync($"<@{duel.Challenger.DiscordId}> <@{duel.Challengee.DiscordId}>", embed: embedBuilder.Build(), components: comp);
        }
        else if (result.voteSuccess)
        {
            var embed = Defaults.SuccessEmbed("Reroll", $"<@{curUser.DiscordId}> has voted to reroll the duel!");
            await RespondAsync(embed: embed);
            return;
        }
        else
        {
            var embed = Defaults.WarningEmbed("You have already voted to reroll the duel!");
            await RespondAsync(embed: embed, ephemeral: true);
            return;
        }
    }
}
