using System.Threading.Tasks;
using Discord.Interactions;

namespace ATPbot.Commands;

public partial class Common : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("ping", "pong")]
    public async Task Ping()
    {
        await RespondAsync($"pong ({Context.Client.Latency}ms)");
    }
}
