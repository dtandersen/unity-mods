using Sage.Entity;

namespace Sage.Tool;

/// <summary>One live structure in full: ports, settings, and slots.</summary>
public sealed record DescribeStructureResult(StructureDetails Details);
