using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ATPbot.Filtering;
using Discord.Interactions;

namespace ATPbot.Commands.Maps;

public partial class Maps : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("allmaps", "gets all maps with a filter")]
    public async Task AllMaps(string filter = "")
    {
        var mapIds = FilterManager.GetMapsFromFilter(filter, mapsManager);
        if (mapIds.Length == 0)
        {
            await RespondAsync("No maps found with that filter");
            return;
        }

        string maps = string.Join("\n", mapIds.Select(x => $"https://quavergame.com/mapset/map/{x.Id}"));

        if (maps.Length > 2000)
        {
            MemoryStream stream = new();
            StreamWriter writer = new(stream);
            writer.Write(maps);
            writer.Flush();
            stream.Position = 0;
            await RespondWithFileAsync(stream, "maps.txt");
        }

        await RespondAsync(maps);
    }
}
