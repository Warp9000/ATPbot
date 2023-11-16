using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ATPbot.Duels;
using ATPbot.Filtering;
using ATPbot.Maps;
using ATPbot.Users;
using Discord.Interactions;
using Newtonsoft.Json;

namespace ATPbot.Commands.Dev;

[Group("dev", ".")]
[RequireOwner]
public class Dev : InteractionModuleBase<SocketInteractionContext>
{
    private readonly DuelManager duelManager;
    private readonly UserManager userManager;
    private readonly QuaverWebApi.Wrapper quaverWebApi;
    private readonly MapsManager mapsManager;

    public Dev(DuelManager duelManager, UserManager userManager, QuaverWebApi.Wrapper quaverWebApi, MapsManager mapsManager)
    {
        this.duelManager = duelManager;
        this.userManager = userManager;
        this.quaverWebApi = quaverWebApi;
        this.mapsManager = mapsManager;
    }

    [SlashCommand("removeduel", ".")]
    public async Task RemoveDuel(string guid)
    {
        var duel = duelManager.GetDuel(new Guid(guid));

        if (duel == null)
        {
            await RespondAsync("Duel not found");
            return;
        }

        duelManager.RemoveDuel(duel);
        await RespondAsync("Duel removed");
    }

    [SlashCommand("endduel", ".")]
    public async Task EndDuel(string guid)
    {
        var duel = duelManager.GetDuel(new Guid(guid));

        if (duel == null)
        {
            await RespondAsync("Duel not found");
            return;
        }

        duelManager.EndDuel(duel);
        await RespondAsync("Duel ended");
    }

    [SlashCommand("listduels", ".")]
    public async Task ListDuels()
    {
        var duels = duelManager.Duels;

        if (duels.Count == 0)
        {
            await RespondAsync("No duels found");
            return;
        }

        var json = JsonConvert.SerializeObject(duels, Formatting.Indented);

        Stream stream = new MemoryStream();
        StreamWriter writer = new StreamWriter(stream);
        writer.Write(json);
        writer.Flush();
        stream.Position = 0;

        await RespondWithFileAsync(stream, "duels.json");
    }
}