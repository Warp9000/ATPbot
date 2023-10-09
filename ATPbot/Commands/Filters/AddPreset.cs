using System;
using System.Linq;
using System.Threading.Tasks;
using ATPbot.Filtering;
using ATPbot.Maps;
using Discord.Interactions;

namespace ATPbot.Commands.Filters;

public partial class Filters : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("addpreset", "Adds a preset filter")]
    [RequireOwner]
    public async Task AddPreset(string filter)
    {
        FilterManager.AddPreset(filter);
        await RespondAsync($"Added preset `{filter}`");
    }
}
