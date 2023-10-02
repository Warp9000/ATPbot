using System.Threading.Tasks;
using Discord.Interactions;

namespace ATPbot.Commands.Core;

public partial class Core : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("ping", "pong")]
    public async Task Ping()
    {
        await RespondAsync($"pong ({Context.Client.Latency}ms)");
    }
}
