using System;
using System.Threading.Tasks;
using ATPbot.Filtering;
using Discord.Interactions;

namespace ATPbot.Commands.Maps;

public partial class Maps : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("rngmap", "gets a random map with a filter")]
    public async Task RngMap(string filter = "")
    {
        var mapIds = FilterManager.GetMapIdsFromFilter(filter, mapsManager);
        if (mapIds.Length == 0)
        {
            await RespondAsync("No maps found with that filter");
            return;
        }

        var map = mapsManager.GetMap(mapIds[Random.Shared.Next(0, mapIds.Length)]);
        await RespondAsync($"https://quavergame.com/mapset/map/{map.Id}");
    }
}
