using System;
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

        duelManager.RemoveDuel(duel);
        await RespondAsync($"<@{challenger.DiscordId}>, your duel with <@{challengee.DiscordId}> has been declined.");
    }
}
