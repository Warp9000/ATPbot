using ATPbot.Duels;
using ATPbot.Users;
using Discord.Interactions;

namespace ATPbot.Commands.Duels;

[Group("duels", "Duel commands")]
public partial class Duels : InteractionModuleBase<SocketInteractionContext>
{
    private const string DUEL_ACCEPT = "duel_accept";
    private const string DUEL_DECLINE = "duel_decline";
    private readonly DuelManager duelManager;
    private readonly UserManager userManager;
    private readonly QuaverWebApi.Wrapper quaverWebApi;

    public Duels(DuelManager duelManager, UserManager userManager, QuaverWebApi.Wrapper quaverWebApi)
    {
        this.duelManager = duelManager;
        this.userManager = userManager;
        this.quaverWebApi = quaverWebApi;
    }
}
