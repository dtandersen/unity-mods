// Records use init-only setters, which need IsExternalInit. It exists in .NET 5+
// but not in netstandard2.0, so we provide it for the core library.
#if !NET5_0_OR_GREATER
namespace System.Runtime.CompilerServices
{
    internal static class IsExternalInit
    {
    }
}
#endif