using System;
using System.Diagnostics;
using System.Threading.Tasks;
using ATPbot.Duels;
using Discord.Interactions;

namespace ATPbot.Commands.Duels;

public partial class Duels : InteractionModuleBase<SocketInteractionContext>
{
    [ComponentInteraction($"{DUEL_DECLINE}:*", true)]
    public async Task DuelDecline(string guid)
    {
        var duel = duelManager.GetDuel(new Guid(guid));
        if (duel == null)
        {
            var embed = Defaults.WarningEmbed("That duel does not exist!");
            await RespondAsync(embed: embed, ephemeral: true);
            return;
        }

        var challenger = duel.Challenger;
        if (challenger == null)
        {
            var embed = Defaults.ErrorEmbed(new StackFrame());
            await RespondAsync(embed: embed);
            return;
        }
        var challengee = userManager.GetUserWithDiscordId(Context.Interaction.User.Id);
        if (challengee == null)
        {
            var embed = Defaults.WarningEmbed("You must link your Quaver account first!");
            await RespondAsync(embed: embed, ephemeral: true);
            return;
        }

        if (duel.Challengee != challengee)
        {
            var embed = Defaults.WarningEmbed("You are not the challengee of that duel!");
            await RespondAsync(embed: embed, ephemeral: true);
            return;
        }

        if (duel.Accepted)
        {
            var embed = Defaults.WarningEmbed("You have already accepted that duel!");
            await RespondAsync(embed: embed, ephemeral: true);
            return;
        }

        duelManager.RemoveDuel(duel);

        var embedBuilder = Defaults.DefaultEmbedBuilder
            .WithTitle("Duel Declined")
            .WithDescription($"<@{challengee.DiscordId}> has declined your duel request.");

        await RespondAsync(embed: embedBuilder.Build());
    }
}
