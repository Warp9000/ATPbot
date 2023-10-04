namespace ATPbot.Filtering;

[System.Serializable]
public class FilterParseException : System.Exception
{
    public FilterParseException() { }
    public FilterParseException(string message) : base(message) { }
    public FilterParseException(string message, System.Exception inner) : base(message, inner) { }
    protected FilterParseException(
        System.Runtime.Serialization.SerializationInfo info,
        System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}