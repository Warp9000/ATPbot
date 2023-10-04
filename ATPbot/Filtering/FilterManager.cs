using System;
using System.Collections.Generic;
using System.Linq;
using ATPbot.Maps;
using Discord;
using QuaverWebApi;
using QuaverWebApi.v1.Structures.Enums;

namespace ATPbot.Filtering;

public static class FilterManager
{
    public static (bool success, string? errMessage) Verify(string filter)
    {
        try
        {
            Parse(filter);
            return (true, null);
        }
        catch (FilterParseException e)
        {
            return (false, e.Message);
        }
    }

    public static int[] GetMapsFromFilter(string filter, MapsManager mapsManager)
    {
        var parsed = Parse(filter);
        mapsManager.Update();
        var maps = mapsManager.GetMaps(parsed);
        return maps.Select(x => x.Id).ToArray();
    }

    // "-q query -md 123"
    private static MapFilter Parse(string filter)
    {
        var mapsetFilter = new MapFilter();

        if (string.IsNullOrEmpty(filter))
            return mapsetFilter;

        List<string> split = filter.Split(' ').ToList();

        while (split.Count > 0)
        {
            var param = split[0];
            split.RemoveAt(0);
            if (!param.StartsWith('-'))
                throw new FilterParseException($"Invalid filter parameter: {param}");
            var value = split.TakeWhile(x => !x.StartsWith('-')).Aggregate((x, y) => $"{x} {y}");
            split.RemoveRange(0, split.TakeWhile(x => !x.StartsWith('-')).Count());

            var prop = mapsetFilter.GetType().GetProperties().FirstOrDefault(x => x.GetCustomAttributes(typeof(FilterParamAttribute), false).Length > 0
                && (((FilterParamAttribute)x.GetCustomAttributes(typeof(FilterParamAttribute), false)[0]).Aliases.Contains(param)
                    || ((FilterParamAttribute)x.GetCustomAttributes(typeof(FilterParamAttribute), false)[0]).FullName == param));
            if (prop == null)
                throw new FilterParseException($"Invalid filter parameter: {param}");

            var propType = prop.PropertyType;
            if (propType.IsGenericType && propType.GetGenericTypeDefinition() == typeof(Nullable<>))
                propType = propType.GetGenericArguments()[0];

            if (propType == typeof(string))
                prop.SetValue(mapsetFilter, value);
            else if (propType == typeof(int))
                prop.SetValue(mapsetFilter, int.Parse(value));
            else if (propType == typeof(double))
                prop.SetValue(mapsetFilter, double.Parse(value));
            else if (propType == typeof(float))
                prop.SetValue(mapsetFilter, float.Parse(value));
            else if (propType == typeof(GameMode))
                prop.SetValue(mapsetFilter, Enum.Parse(typeof(GameMode), value));
            else
                throw new FilterParseException($"Invalid filter parameter: {param}");
        }
        return mapsetFilter;
    }
}