namespace OdinsAi.Tool
{
    /// <summary>Result of executing a tool.</summary>
    public abstract record ToolResult
    {
        public abstract bool IsSuccess { get; }

        /// <summary>Text handed back to the model.</summary>
        public abstract string Content { get; }
    }

    /// <summary>Successful tool result carrying a typed payload.</summary>
    public sealed record ToolSuccess<T> : ToolResult
    {
        public ToolSuccess(T value, string content)
        {
            Value = value;
            Content = content;
        }

        public T Value { get; init; }

        public override string Content { get; }

        public override bool IsSuccess => true;
    }

    /// <summary>Failed tool result. The error is fed back to the model.</summary>
    public sealed record ToolFailure(string Error) : ToolResult
    {
        public override bool IsSuccess => false;

        public override string Content => Error;
    }
}