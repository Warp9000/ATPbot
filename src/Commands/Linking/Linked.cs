using System.Linq;
using System.Threading.Tasks;
using ATPbot.Users;
using Discord;
using Discord.Interactions;

namespace ATPbot.Commands.Linking;

public partial class Linking : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("linked", "Check what Quaver account is linked to your Discord account")]
    public async Task Linked(IGuildUser user)
    {
        var linkedUser = _userManager.GetUserWithDiscordId(user.Id);

        if (linkedUser == null)
        {
            await RespondAsync("You don't have a linked Quaver account.");
            return;
        }

        var quaverUser = (await _quaverWebApi.Endpoints.GetUsersAsync(linkedUser.QuaverId)).First();

        await RespondAsync($"Your linked Quaver account is `{quaverUser.Username}` ({quaverUser.Id}).");
    }
}