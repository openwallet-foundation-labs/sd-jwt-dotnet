#if NETSTANDARD
namespace System.Runtime.CompilerServices
{
    // Polyfill for C# 9+ init-only setters on netstandard2.1.
    internal static class IsExternalInit { }
}
#endif
