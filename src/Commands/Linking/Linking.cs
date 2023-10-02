using System.Linq;
using System.Threading.Tasks;
using ATPbot.Users;
using Discord.Interactions;

namespace ATPbot.Commands.Linking;

public partial class Linking : InteractionModuleBase<SocketInteractionContext>
{
    private readonly UserManager _userManager;
    private readonly QuaverWebApi.Wrapper _quaverWebApi;

    public Linking(QuaverWebApi.Wrapper quaverWebApi, UserManager userManager)
    {
        _quaverWebApi = quaverWebApi;
        _userManager = userManager;
    }
}
