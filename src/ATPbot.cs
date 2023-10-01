using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using ATPbot.Logging;

namespace ATPbot;

public class ATPbot
{
    public Logger Logger { get; private set; }
    public DiscordSocketClient Client { get; private set; }
    public CommandHandler CommandHandler { get; private set; }

    private Config Config { get; set; }

    public ATPbot()
    {
        Config = JsonConvert.DeserializeObject<Config>(File.ReadAllText("config.json"));

#if DEBUG
        var severity = Severity.Debug;
#else
        var severity = Severity.Info;
#endif

        Logger = new Logger(severity, Logger.GetUniqueFileName("logs/log"), Logger.GetUniqueFileName("logs/crash"));
        Logger.Log("Starting", this, Severity.Verbose);

        Client = new DiscordSocketClient(new DiscordSocketConfig
        {
            LogLevel = severity.ToDiscordSeverity(),
            MessageCacheSize = 1024,
            AlwaysDownloadUsers = true,
            GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.GuildMembers
        });
        Client.Log += Logger.DiscordLog;
        CommandHandler = new CommandHandler(Client, Logger);
    }

    public async void Run(string[] args)
    {
        AppDomain.CurrentDomain.ProcessExit += (sender, args) => Stop();

        await CommandHandler.InstallCommandsAsync();

        await Client.LoginAsync(TokenType.Bot, Config.Token);
        await Client.StartAsync();

        await Client.SetStatusAsync(UserStatus.Online);
        await Client.SetCustomStatusAsync("smashing keys");

        while (true)
        {
            string? input = Console.ReadLine();
            switch (input)
            {
                case "exit":
                    return;
            }
        }
    }

    public async void Stop()
    {
        Logger.Log("Stopping", this, Severity.Verbose);
        await Client.StopAsync();
        await Client.LogoutAsync();
        Client.Dispose();
    }
}