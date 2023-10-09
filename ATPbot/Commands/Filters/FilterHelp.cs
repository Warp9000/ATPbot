using System;
using System.Linq;
using System.Threading.Tasks;
using ATPbot.Filtering;
using ATPbot.Maps;
using Discord.Interactions;

namespace ATPbot.Commands.Filters;

public partial class Filters : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("help", "Gets help with the map filter")]
    public async Task FilterHelp()
    {
        var type = typeof(MapFilter);
        var props = type.GetProperties().Where(x => x.GetCustomAttributes(typeof(FilterParamAttribute), false).Length > 0).ToList();
        string msg = "```\n";
        foreach (var prop in props)
        {
            var attr = (FilterParamAttribute)prop.GetCustomAttributes(typeof(FilterParamAttribute), false)[0];
            var propType = prop.PropertyType;
            if (prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                propType = prop.PropertyType.GetGenericArguments()[0];
            msg += $"{attr.FullName} ({string.Join(", ", attr.Aliases)}): {propType.Name} ({attr.Description})\n";
        }
        msg += "```";
        await RespondAsync(msg);
    }
}
