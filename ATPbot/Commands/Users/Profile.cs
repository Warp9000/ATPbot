using System.Threading.Tasks;
using Discord;
using Discord.Interactions;

namespace ATPbot.Commands.Users;

public partial class Users : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("profile", "View your profile")]
    public async Task Profile(IGuildUser? user = null)
    {
        var atpUser = userManager.GetUserWithDiscordId(user == null ? Context.User.Id : user.Id);

        if (atpUser == null)
        {
            var embed = Defaults.WarningEmbed("You/that user must link your Quaver account first!");
            await RespondAsync(embed: embed, ephemeral: true);
            return;
        }

        var discordUser = Context.Guild.GetUser(atpUser.DiscordId);

        var quaverUser = await _quaverWebApi.Endpoints.GetFullUserAsync(atpUser.QuaverId);

        var stats = userManager.GetStats(atpUser);

        string statsStr = 
            $"Wins: {stats.Wins}\n" +
            $"Losses: {stats.Losses}\n" +
            $"Ties: {stats.Ties}\n" +
            $"Total Games: {stats.TotalGames}\n" +
            $"Win/Loss Ratio: {stats.WinLossRatio}";

        var embedBuilder = Defaults.DefaultEmbedBuilder
            .WithTitle($"{discordUser.Username} - {quaverUser.Info.Username}")
            .WithThumbnailUrl(discordUser.GetAvatarUrl())
            .AddField("Stats", statsStr);

        await RespondAsync(embed: embedBuilder.Build());
    }
}
