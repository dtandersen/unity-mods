using System;
using StationeersAi.Entity;
using StationeersAi.Repository;

namespace StationeersAi.Command;

/// <summary>Domain operation: read one computer's screen text and pointed housing.</summary>
public sealed class ReadComputerCommand
{
    private readonly IComputerRepository _computers;

    public ReadComputerCommand(IComputerRepository computers)
    {
        _computers = computers;
    }

    public ReadComputerResult Execute(long computer)
    {
        IComputer target;
        try
        {
            target = _computers.Find(computer);
        }
        catch (ThingNotFound notFound)
        {
            return new ReadComputerResult(null, null, null, notFound.Message);
        }
        catch (Exception)
        {
            return new ReadComputerResult(null, null, null, "Computer lookup failed.");
        }

        var code = target.Read();
        if (code == null || !code.Found)
        {
            return new ReadComputerResult(null, null, null, code?.Error ?? "Computer not found.");
        }

        var pointed = target.Selected();
        return new ReadComputerResult(code.Device, pointed?.Device, code.Code, pointed?.Error);
    }
}
