using System;
using System.Linq;

namespace ATPbot.Maps;

[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
public sealed class FilterParamAttribute : Attribute
{
    public readonly string FullName;
    public readonly string Description;
    public readonly string[] Aliases;
    
    public FilterParamAttribute(string fullName, string description, params string[] aliases)
    {
        FullName = fullName;
        Description = description;
        Aliases = aliases;
    }
}