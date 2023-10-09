using System;
using System.Diagnostics;
using System.Reflection;
using Discord;

namespace ATPbot;

public static class Defaults
{
    public static EmbedBuilder DefaultEmbedBuilder => new EmbedBuilder()
        .WithColor(Color.Gold)
        .WithFooter("ATPbot by Warp")
        .WithTimestamp(DateTime.UtcNow);

    public static Embed SuccessEmbed(string title, string message) => DefaultEmbedBuilder
        .WithTitle(title)
        .WithDescription(message)
        .WithColor(Color.Green)
        .Build();

    public static Embed WarningEmbed(string message) => DefaultEmbedBuilder
        .WithTitle("!")
        .WithDescription(message)
        .WithColor(Color.Orange)
        .Build();

    public static Embed ErrorEmbed(StackFrame frame, string? errMessage = null) => DefaultEmbedBuilder
        .WithTitle("Error")
        .WithDescription((errMessage ?? "Something went wrong") + $" ({frame})")
        .WithColor(Color.Red)
        .Build();
}