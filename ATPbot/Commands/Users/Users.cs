using System.Linq;
using System.Threading.Tasks;
using ATPbot.Users;
using Discord.Interactions;

namespace ATPbot.Commands.Users;

public partial class Users : InteractionModuleBase<SocketInteractionContext>
{
    private readonly UserManager userManager;
    private readonly QuaverWebApi.Wrapper _quaverWebApi;

    public Users(QuaverWebApi.Wrapper quaverWebApi, UserManager userManager)
    {
        _quaverWebApi = quaverWebApi;
        this.userManager = userManager;
    }
}
