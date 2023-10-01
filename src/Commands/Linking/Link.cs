using System.Linq;
using System.Threading.Tasks;
using ATPbot.Users;
using Discord.Interactions;

namespace ATPbot.Commands.Linking
{
    public partial class Core : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("link", "Link your Quaver account")]
        public async Task Link(string username)
        {
            var user = _userManager.GetUserWithDiscordId(Context.User.Id);
            if (user != null)
            {
                await RespondAsync("You have already linked your Quaver account!");
                return;
            }

            var quaverUsers = await _quaverWebApi.Endpoints.SearchUsersAsync(username);
            var quaverUser = quaverUsers.Where(u => u.Username.ToLower() == username.ToLower()).FirstOrDefault();
            if (string.IsNullOrEmpty(quaverUser.Username))
            {
                await RespondAsync("That user does not exist!");
                return;
            }

            _userManager.CreateUser(Context.User.Id, quaverUser.Id);
            await RespondAsync("You have successfully linked your Quaver account!");
        }
    }
}