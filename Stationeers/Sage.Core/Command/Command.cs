using System.Collections.Generic;

namespace Sage.Command;

/// <summary>How a command ended.</summary>
public enum Status
{
    SUCCESS,
    NOT_FOUND,
    BAD_REQUEST, // validation error
}

/// <summary>
/// The answer every command gives: how it ended, and the sentences naming
/// what went wrong.
/// </summary>
public abstract record BaseResult(Status Status, List<string> Errors);

/// <summary>The ask for a command that takes nothing.</summary>
public sealed record NoRequest();

/// <summary>One domain operation. It answers with a result and never throws.</summary>
public interface Command<REQ, RES> where RES : BaseResult
{
    RES Execute(REQ request);
}
