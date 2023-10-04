using ATPbot.Duels;
using ATPbot.Maps;
using ATPbot.Users;
using Discord.Interactions;

namespace ATPbot.Commands.Duels;

[Group("duels", "Duel commands")]
public partial class Duels : InteractionModuleBase<SocketInteractionContext>
{
    private const string DUEL_ACCEPT = "duel_accept";
    private const string DUEL_DECLINE = "duel_decline";
    private const string DUEL_FORFEIT = "duel_forfeit";
    private const string DUEL_REROLL = "duel_reroll";
    private readonly DuelManager duelManager;
    private readonly UserManager userManager;
    private readonly QuaverWebApi.Wrapper quaverWebApi;
    private readonly MapsManager mapsManager;

    public Duels(DuelManager duelManager, UserManager userManager, QuaverWebApi.Wrapper quaverWebApi, MapsManager mapsManager)
    {
        this.duelManager = duelManager;
        this.userManager = userManager;
        this.quaverWebApi = quaverWebApi;
        this.mapsManager = mapsManager;
    }
}
