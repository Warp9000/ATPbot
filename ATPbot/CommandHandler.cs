using System;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using ATPbot.Logging;
using ATPbot.Users;
using ATPbot.Duels;
using ATPbot.Maps;

namespace ATPbot;

public class CommandHandler
{
    public Logger Logger { get; private set; }
    public DiscordSocketClient Client { get; private set; }
    public InteractionService InteractionService { get; private set; }

    private readonly IServiceProvider services;

    private readonly QuaverWebApi.Wrapper quaverWebApi;
    private readonly MapsManager mapsManager;
    private readonly UserManager userManager;
    private readonly DuelManager duelManager;

    public CommandHandler(DiscordSocketClient client, Logger logger)
    {
        Logger = logger;
        Client = client;
        InteractionServiceConfig config = new InteractionServiceConfig
        {
            DefaultRunMode = RunMode.Async,
            LogLevel = logger.MinimumSeverity.ToDiscordSeverity()
        };
        InteractionService = new InteractionService(client);

        quaverWebApi = new QuaverWebApi.Wrapper();
        mapsManager = new MapsManager(quaverWebApi);
        userManager = new UserManager(logger);
        duelManager = new DuelManager(logger, client, quaverWebApi, mapsManager, userManager);

        services = new ServiceCollection()
            .AddSingleton(Logger)
            .AddSingleton(quaverWebApi)
            .AddSingleton(mapsManager)
            .AddSingleton(userManager)
            .AddSingleton(duelManager)
            .BuildServiceProvider();
    }

    internal async Task InstallCommandsAsync()
    {
        Client.JoinedGuild += async (guild) =>
        {
            await InteractionService.RegisterCommandsToGuildAsync(guild.Id, true);
            Logger.Log("Added commands to " + guild.Id, this, Severity.Debug);
        };
        Client.Ready += () =>
        {
            Task.Run(RegisterCommands);
            return Task.CompletedTask;
        };
        InteractionService.SlashCommandExecuted += SlashCommandExecuted;

        await InteractionService.AddModulesAsync(assembly: Assembly.GetExecutingAssembly(), services: services);

        Logger.Log(InteractionService.Modules.Count + " modules loade:", this, Severity.Debug);
        Logger.Log(InteractionService.SlashCommands.Count + " commands loaded", this, Severity.Debug);

        Client.InteractionCreated += HandleInteraction;
    }

    private async Task HandleInteraction(SocketInteraction interaction)
    {
        try
        {
            var context = new SocketInteractionContext(Client, interaction);
            var result = await InteractionService.ExecuteCommandAsync(context, services);
            // Logger.Debug($"Executed command result: {result.IsSuccess}: {result.Error}: {result.ErrorReason}", this);
        }
        catch (Exception ex)
        {
            Logger.Log(ex, this);
            if (interaction.Type is InteractionType.ApplicationCommand)
                await interaction.GetOriginalResponseAsync().ContinueWith(async (msg) => await msg.Result.DeleteAsync());
        }
    }

    private async Task RegisterCommands()
    {
        try
        {
            foreach (var guild in Client.Guilds)
            {
                await InteractionService.RegisterCommandsToGuildAsync(guild.Id, true);
                Logger.Log("Added commands to " + guild.Id, this, Severity.Debug);
            }
        }
        catch (Exception ex)
        {
            Logger.Log(ex, this);
        }
    }

    private async Task SlashCommandExecuted(SlashCommandInfo slashCommandInfo, Discord.IInteractionContext context, IResult result)
    {
        if (!result.IsSuccess)
        {
            switch (result.Error)
            {
                case InteractionCommandError.UnmetPrecondition:
                    await context.Interaction.RespondAsync($"Unmet Precondition: {result.ErrorReason}");
                    break;
                case InteractionCommandError.UnknownCommand:
                    await context.Interaction.RespondAsync("Unknown command");
                    break;
                case InteractionCommandError.BadArgs:
                    await context.Interaction.RespondAsync("Invalid number or arguments");
                    break;
                case InteractionCommandError.Exception:
                    Logger.Log(result.ErrorReason, this, Severity.Error);
                    await context.Interaction.RespondAsync("Exception: " + result.ErrorReason);
                    break;
                case InteractionCommandError.Unsuccessful:
                    await context.Interaction.RespondAsync("Command could not be executed");
                    break;
                default:
                    await context.Interaction.RespondAsync(result.Error.ToString() + ": " + result.ErrorReason);
                    break;
            }
        }
    }
}