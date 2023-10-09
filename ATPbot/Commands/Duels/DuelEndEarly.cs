using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using ATPbot.Duels;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;

namespace ATPbot.Commands.Duels;

public partial class Duels : InteractionModuleBase<SocketInteractionContext>
{
    [ComponentInteraction($"{DUEL_END_EARLY}:*", true)]
    public async Task DuelEndEarly(string guid)
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

        var result = duelManager.VoteEndEarly(duel.Id, curUser);

        if (result.Ended)
        {
            var embed = Defaults.SuccessEmbed("Duel Ended Early", $"The duel has been ended early");
            await RespondAsync(embed: embed);
            return;
        }
        else if (result.voteSuccess)
        {
            var embed = Defaults.SuccessEmbed("End Duel Early", $"<@{curUser.DiscordId}> has voted to end the duel early");
            await RespondAsync(embed: embed);
            return;
        }
        else
        {
            var embed = Defaults.WarningEmbed("You have already voted to end the duel early!");
            await RespondAsync(embed: embed, ephemeral: true);
            return;
        }
    }
}
