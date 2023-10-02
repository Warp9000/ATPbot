using System.Threading.Tasks;
using Discord;
using Discord.Interactions;

namespace ATPbot.Commands.Duels;

public partial class Duels : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("challenge", "Challenge a user to a duel")]
    public async Task Ping(IGuildUser user, string? filter = null)
    {
        var challenger = userManager.GetUserWithDiscordId(Context.User.Id);
        if (challenger == null)
        {
            await RespondAsync("You must link your Quaver account first!");
            return;
        }
        var challengee = userManager.GetUserWithDiscordId(user.Id);
        if (challengee == null)
        {
            await RespondAsync("That user has not linked their Quaver account!");
            return;
        }

        if (challenger == challengee)
        {
            await RespondAsync("You cannot duel yourself!");
            return;
        }

        if (duelManager.HasDuel(challenger, challengee) || duelManager.HasDuel(challengee, challenger))
        {
            await RespondAsync("You already have a duel with that user!");
            return;
        }

        var duel = duelManager.CreateDuel(challenger, challengee, Context.Channel.Id);

        string msg = $"{user.Mention}, you have been challenged to a duel by {Context.User.Mention}!";
        MessageComponent comp = new ComponentBuilder()
            .WithButton("Accept", $"{DUEL_ACCEPT}:{duel.Id}", ButtonStyle.Success)
            .WithButton("Decline", $"{DUEL_DECLINE}:{duel.Id}", ButtonStyle.Danger)
            .Build();

        await RespondAsync(msg, components: comp);
    }
}
