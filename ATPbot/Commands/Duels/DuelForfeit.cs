using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using ATPbot.Duels;
using Discord;
using Discord.Interactions;

namespace ATPbot.Commands.Duels;

public partial class Duels : InteractionModuleBase<SocketInteractionContext>
{
    [ComponentInteraction($"{DUEL_FORFEIT}:*", true)]
    public async Task DuelForfeit(string guid)
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

        if (duel.Challenger == curUser)
        {
            duel.ChallengerForfeited = true;
        }
        else if (duel.Challengee == curUser)
        {
            duel.ChallengeeForfeited = true;
        }
        else
        {
            var embed = Defaults.WarningEmbed("You are not a part of that duel");
            await RespondAsync(embed: embed, ephemeral: true);
            return;
        }

        var success = duelManager.EndDuel(duel.Id);

        if (!success)
        {
            var embed = Defaults.ErrorEmbed(new StackFrame());
            await RespondAsync(embed: embed);
            return;
        }

        var embedBuilder = Defaults.DefaultEmbedBuilder
            .WithTitle("Duel Forfeited")
            .WithDescription($"<@{curUser.DiscordId}> has forfeited the duel.");

        await DisableOldButtons(duel);

        await RespondAsync($"<@{duel.Challenger.DiscordId}>", embed: embedBuilder.Build());
    }
}
