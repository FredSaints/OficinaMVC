using System.Reflection;
using System.Runtime.Loader;

namespace OficinaMVC.Helpers
{
    /// <summary>
    /// Provides a custom assembly load context for loading unmanaged libraries in the application.
    /// </summary>
    internal class CustomAssemblyLoadContext : AssemblyLoadContext
    {
        /// <summary>
        /// Loads an unmanaged library from the specified absolute path.
        /// </summary>
        /// <param name="absolutePath">The absolute path to the unmanaged library.</param>
        /// <returns>A pointer to the loaded unmanaged library.</returns>
        public IntPtr LoadUnmanagedLibrary(string absolutePath)
        {
            return LoadUnmanagedDll(absolutePath);
        }
        /// <summary>
        /// Loads an unmanaged DLL by its name using the base implementation.
        /// </summary>
        /// <param name="unmanagedDllName">The name or path of the unmanaged DLL.</param>
        /// <returns>A pointer to the loaded unmanaged DLL.</returns>
        protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
        {
            return LoadUnmanagedDllFromPath(unmanagedDllName);
        }

        /// <summary>
        /// Loads a managed assembly by its name. Not implemented in this context.
        /// </summary>
        /// <param name="assemblyName">The name of the assembly to load.</param>
        /// <returns>Always throws <see cref="NotImplementedException"/>.</returns>
        /// <exception cref="NotImplementedException">Always thrown as this method is not implemented.</exception>
        protected override Assembly Load(AssemblyName assemblyName)
        {
            throw new NotImplementedException();
        }
    }
}
