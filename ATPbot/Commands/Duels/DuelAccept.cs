using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using ATPbot.Duels;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;

namespace ATPbot.Commands.Duels;

public partial class Duels : InteractionModuleBase<SocketInteractionContext>
{
    [ComponentInteraction($"{DUEL_ACCEPT}:*", true)]
    public async Task DuelAccept(string guid)
    {
        var duel = duelManager.GetDuel(new Guid(guid));
        if (duel == null)
        {
            var embed = Defaults.WarningEmbed("That duel does not exist!");
            await RespondAsync(embed: embed, ephemeral: true);
            return;
        }

        var challenger = duel.Challenger;
        if (challenger == null)
        {
            var embed = Defaults.ErrorEmbed(new StackFrame());
            await RespondAsync(embed: embed);
            return;
        }
        var challengee = userManager.GetUserWithDiscordId(Context.Interaction.User.Id);
        if (challengee == null)
        {
            var embed = Defaults.WarningEmbed("You must link your Quaver account first!");
            await RespondAsync(embed: embed, ephemeral: true);
            return;
        }

        if (duel.Challengee != challengee)
        {
            var embed = Defaults.WarningEmbed("You are not the challengee of that duel!");
            await RespondAsync(embed: embed, ephemeral: true);
            return;
        }

        if (duel.Accepted)
        {
            var embed = Defaults.WarningEmbed("You have already accepted that duel!");
            await RespondAsync(embed: embed, ephemeral: true);
            return;
        }

        duelManager.AcceptDuel(duel.Id);


        var map = await quaverWebApi.Endpoints.GetMap(duel.MapId);

        MessageComponent comp = new ComponentBuilder()
            .WithButton("Forfeit", $"{DUEL_FORFEIT}:{duel.Id}", ButtonStyle.Danger)
            .WithButton("Reroll", $"{DUEL_REROLL}:{duel.Id}", ButtonStyle.Secondary, disabled: !duel.CanReroll())
            .Build();

        var embedBuilder = Defaults.DefaultEmbedBuilder
            .WithTitle("Duel Challenge Accepted")
            .WithDescription($"<@{challenger.DiscordId}>, your duel with <@{challengee.DiscordId}> has been accepted.")
            .AddField("Map", $"[{map.Artist} - {map.Title} [{map.DifficultyName}]](https://quavergame.com/mapset/map/{map.Id})")
            .AddField("Ends", $"<t:{((DateTimeOffset)duel.EndAt!).ToUnixTimeSeconds()}:R>");

        await DisableOldButtons(duel);

        await RespondAsync($"<@{challenger.DiscordId}>", embed: embedBuilder.Build(), components: comp);
        
        duel.ChannelId = Context.Channel.Id;
        duel.MessageId = (await Context.Interaction.GetOriginalResponseAsync()).Id;
    }
}
