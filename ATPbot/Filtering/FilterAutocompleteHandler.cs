using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;

namespace ATPbot.Filtering;

public class FilterPresetAutocompleteHandler : AutocompleteHandler
{
    public override Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services)
    {
        var suggestions = new List<AutocompleteResult>();
        foreach (var preset in FilterManager.Presets)
        {
            suggestions.Add(new AutocompleteResult(preset, preset));
        }
        return Task.FromResult(AutocompletionResult.FromSuccess(suggestions));
    }
}