using System;
using System.Threading.Tasks;
using Discord.Interactions;

namespace ATPbot.Commands.Duels;

public partial class Duels : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("allduels", "List your duels")]
    public async Task AllDuels()
    {
        var duels = duelManager.Duels;

        if (duels.Count == 0)
        {
            var embed = Defaults.WarningEmbed("There are no duels!");
            await RespondAsync(embed: embed, ephemeral: true);
            return;
        }

        var embedBuilder = Defaults.DefaultEmbedBuilder
            .WithTitle("All Duels");

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
