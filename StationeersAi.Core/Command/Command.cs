using System.Collections.Generic;

namespace StationeersAi.Command;

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

/// <summary>One domain operation. It answers with a result and never throws.</summary>
public interface Command<REQ, RES> where RES : BaseResult
{
    RES Execute(REQ request);
}
