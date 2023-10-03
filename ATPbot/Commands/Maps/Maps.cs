using System.Threading.Tasks;
using ATPbot.Maps;
using Discord.Interactions;

namespace ATPbot.Commands.Maps;

public partial class Maps : InteractionModuleBase<SocketInteractionContext>
{
    private readonly MapsManager mapsManager;
    public Maps(MapsManager mapsManager)
    {
        this.mapsManager = mapsManager;
    }
}
