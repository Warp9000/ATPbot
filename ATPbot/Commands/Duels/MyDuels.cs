using System;
using System.Threading.Tasks;
using Discord.Interactions;

namespace ATPbot.Commands.Duels;

public partial class Duels : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("myduels", "List your duels")]
    public async Task MyDuels()
    {
        var curUser = userManager.GetUserWithDiscordId(Context.User.Id);

        if (curUser == null)
        {
            var embed = Defaults.WarningEmbed("You must link your Quaver account first!");
            await RespondAsync(embed: embed, ephemeral: true);
            return;
        }

        var duels = duelManager.GetDuels(curUser);

        if (duels.Length == 0)
        {
            var embed = Defaults.WarningEmbed("You have no duels!");
            await RespondAsync(embed: embed, ephemeral: true);
            return;
        }

        var embedBuilder = Defaults.DefaultEmbedBuilder
            .WithTitle("Your Duels");

        foreach (var duel in duels)
        {
            var desc = "";
            if (!duel.Accepted)
            {
                desc += "(Pending)";
            }
            else
            {
                var map = mapsManager.GetMap(duel.MapId);
                desc += $"[{map.Artist} - {map.Title} [{map.DifficultyName}]](https://quavergame.com/mapset/map/{map.Id}); <t:{((DateTimeOffset)duel.EndAt!).ToUnixTimeSeconds()}:R>";
            }

            embedBuilder.Description += $"**<@{duel.Challenger.DiscordId}> vs <@{duel.Challengee.DiscordId}>**\n{desc}\n\n ";
        }

        await RespondAsync(embed: embedBuilder.Build());
    }
}
