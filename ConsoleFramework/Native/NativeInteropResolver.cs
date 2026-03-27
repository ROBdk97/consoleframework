using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;

namespace ConsoleFramework.Native
{
    internal static class NativeInteropResolver
    {
        private static readonly Lock syncRoot = new();
        private static bool initialized;

        internal static void EnsureInitialized(Assembly assembly)
        {
            lock (syncRoot)
            {
                if (initialized)
                {
                    return;
                }

                NativeLibrary.SetDllImportResolver(assembly, ResolveLibrary);
                initialized = true;
            }
        }

        private static nint ResolveLibrary(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
        {
            if (string.Equals(libraryName, "ncursesw", StringComparison.Ordinal))
            {
                return TryLoadCandidates(
                    assembly,
                    searchPath,
                    ["libncursesw.so.6", "libncursesw.so.5", "libncursesw.so"],
                    "Unable to load ncurses library. Tried libncursesw.so.6, libncursesw.so.5 and libncursesw.so.");
            }

            if (string.Equals(libraryName, "termkey", StringComparison.Ordinal))
            {
                return TryLoadCandidates(
                    assembly,
                    searchPath,
                    ["libtermkey.so.1", "libtermkey.so.0", "libtermkey.so"],
                    "Unable to load libtermkey. Tried libtermkey.so.1, libtermkey.so.0 and libtermkey.so.");
            }

            return nint.Zero;
        }

        private static nint TryLoadCandidates(
            Assembly assembly,
            DllImportSearchPath? searchPath,
            string[] candidates,
            string errorMessage)
        {
            foreach (string candidate in candidates)
            {
                if (NativeLibrary.TryLoad(candidate, assembly, searchPath, out nint handle))
                {
                    return handle;
                }
            }

            throw new DllNotFoundException(errorMessage);
        }
    }
}
