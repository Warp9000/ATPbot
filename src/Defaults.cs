using System;
using System.Reflection;
using Discord;

namespace ATPbot;

public static class Defaults
{
    public static EmbedBuilder DefaultEmbedBuilder => new EmbedBuilder()
        .WithColor(Color.Gold)
        .WithFooter("ATPbot by Warp")
        .WithTimestamp(DateTime.UtcNow);
}