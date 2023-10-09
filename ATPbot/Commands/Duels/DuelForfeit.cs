using System;
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
            await RespondAsync("That duel does not exist!", ephemeral: true);
            return;
        }

        var curUser = userManager.GetUserWithDiscordId(Context.Interaction.User.Id);
        if (curUser == null)
        {
            await RespondAsync("You must link your Quaver account first!", ephemeral: true);
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
            await RespondAsync("You are not a part of that duel!", ephemeral: true);
            return;
        }

        var success = duelManager.EndDuel(duel.Id);

        if (!success)
        {
            await RespondAsync("Something went wrong!", ephemeral: true);
            return;
        }

        await RespondAsync("You have forfeited the duel!", ephemeral: true);
    }
}
