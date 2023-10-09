using System.Threading.Tasks;
using ATPbot.Filtering;
using Discord;
using Discord.Interactions;

namespace ATPbot.Commands.Duels;

public partial class Duels : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("challenge", "Challenge a user to a duel")]
    public async Task Challenge(IGuildUser user, string? filter = null, int maxRerolls = 1)
    {
        var challenger = userManager.GetUserWithDiscordId(Context.User.Id);
        if (challenger == null)
        {
            var embed = Defaults.WarningEmbed("You must link your Quaver account first!");
            await RespondAsync(embed: embed, ephemeral: true);
            return;
        }
        var challengee = userManager.GetUserWithDiscordId(user.Id);
        if (challengee == null)
        {
            var embed = Defaults.WarningEmbed("That user has not linked their Quaver account!");
            await RespondAsync(embed: embed, ephemeral: true);
            return;
        }

        if (challenger == challengee)
        {
            var embed = Defaults.WarningEmbed("You cannot duel yourself!");
            await RespondAsync(embed: embed, ephemeral: true);
            return;
        }

        if (duelManager.HasDuel(challenger, challengee) || duelManager.HasDuel(challengee, challenger))
        {
            var embed = Defaults.WarningEmbed("You already have a duel with that user!");
            await RespondAsync(embed: embed, ephemeral: true);
            return;
        }

        if (filter != null)
        {
            var (success, errMessage) = FilterManager.Verify(filter);
            if (!success)
            {
                var embed = Defaults.WarningEmbed($"Failed to parse filter: {errMessage}");
                await RespondAsync(embed: embed, ephemeral: true);
                return;
            }
        }

        var duel = duelManager.CreateDuel(challenger, challengee, Context.Channel.Id, true, filter, maxRerolls);

        var embedBuilder = Defaults.DefaultEmbedBuilder
            .WithTitle("Duel Challenge");

        embedBuilder.WithDescription($"{user.Mention}, you have been challenged to a duel by {Context.User.Mention}!");

        if (duel.Filter != null)
        {
            var possibleMaps = FilterManager.GetMapsFromFilter(duel.Filter, mapsManager);
            embedBuilder.AddField("Filter", $"`{duel.Filter}` ({possibleMaps.Length} maps)");
        }

        embedBuilder.AddField("Max Rerolls", duel.MaxRerolls.ToString());

        MessageComponent comp = new ComponentBuilder()
            .WithButton("Accept", $"{DUEL_ACCEPT}:{duel.Id}", ButtonStyle.Success)
            .WithButton("Decline", $"{DUEL_DECLINE}:{duel.Id}", ButtonStyle.Danger)
            .Build();

        await RespondAsync(embed: embedBuilder.Build(), components: comp);
    }
}
