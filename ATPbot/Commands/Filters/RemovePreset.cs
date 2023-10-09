using System;
using System.Linq;
using System.Threading.Tasks;
using ATPbot.Filtering;
using ATPbot.Maps;
using Discord.Interactions;

namespace ATPbot.Commands.Filters;

public partial class Filters : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("removepreset", "Removes a preset filter")]
    [RequireOwner]
    public async Task RemovePreset([Autocomplete(typeof(FilterPresetAutocompleteHandler))] string filter)
    {
        FilterManager.RemovePreset(filter);
        await RespondAsync($"Removed preset `{filter}`");
    }
}
