namespace Sage.Entity;

/// <summary>
/// An IC10 program: its source text and the hash of that text. The hash
/// names the program, so a burn can be checked without shipping the text.
/// </summary>
public sealed record Program(string SourceCode, string Hash);
