using System.Linq;
using System.Threading.Tasks;
using ATPbot.Users;
using Discord.Interactions;

namespace ATPbot.Commands.Linking;

public partial class Linking : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("unlink", "Unlink your Quaver account")]
    public async Task Unlink()
    {
        var user = _userManager.GetUserWithDiscordId(Context.User.Id);
        if (user == null)
        {
            await RespondAsync("You have not linked your Quaver account!");
            return;
        }

        _userManager.RemoveUser(user);
        await RespondAsync("You have successfully unlinked your Quaver account!");
    }
}
