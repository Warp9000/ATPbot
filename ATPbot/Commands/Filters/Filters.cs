using System;
using System.Linq;
using System.Threading.Tasks;
using ATPbot.Filtering;
using ATPbot.Maps;
using Discord.Interactions;

namespace ATPbot.Commands.Filters;

[Group("filters", "Filter commands")]
public partial class Filters : InteractionModuleBase<SocketInteractionContext>
{}
