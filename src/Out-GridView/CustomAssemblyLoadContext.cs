using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.DotNet.PlatformAbstractions;
using System.Threading.Tasks;

namespace OutGridView
{
    using System.Reflection;
    using System.Runtime.InteropServices;

    public class CustomAssemblyLoadContext
    {
        [DllImport("kernel32")]
        private static extern IntPtr LoadLibrary(string path);

        [DllImport("libdl")]
        private static extern IntPtr dlopen(string path, int flags);

        private IntPtr LoadUnmanagedDll(String unmanagedDllName)
        {
            var assemblyBasePath = Path.GetDirectoryName(this.GetType().GetTypeInfo().Assembly.Location);

            try
            {
                var fileInfo = new FileInfo(unmanagedDllName);
                if (!File.Exists(Path.Combine(assemblyBasePath, fileInfo.Name)))
                {
                    File.Copy(fileInfo.FullName, Path.Combine(assemblyBasePath, fileInfo.Name));
                }
            }
            catch
            {

            }

            IntPtr libraryHandle = IntPtr.Zero;
            try
            {
                var libraryPath = Path.Combine(assemblyBasePath, unmanagedDllName);
                libraryHandle =
                    System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                        ? LoadLibrary(libraryPath)
                        : dlopen(libraryPath, 0);
                if (libraryHandle == IntPtr.Zero)
                {
                    // ... (more error handling)
                    throw new DllNotFoundException(unmanagedDllName);
                }
            }
            catch
            {

            }

            //BUG: Leaky handle
            return libraryHandle;
        }

        private string GetNativeFolder()
        {
            var processArch = IntPtr.Size == 8 ? "x64" : "x86";
            if (System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture == Architecture.Arm)
            {
                processArch = "arm";
            }

            if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return "win-" + processArch;
            }

            if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return "linux-" + processArch;
            }

            if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return "osx";
            }

            throw new Exception("Operating system not supported: " + Microsoft.DotNet.PlatformAbstractions.RuntimeEnvironment.GetRuntimeIdentifier());
        }

        public void LoadLibs()
        {
            var libFolder = System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "win" : "unix";
            var assemblyBasePath = Path.GetDirectoryName(this.GetType().GetTypeInfo().Assembly.Location);
            var nativePath = Path.Combine(assemblyBasePath, "runtimes", libFolder, "lib", "netcoreapp2.1");

            if (!Directory.Exists(nativePath))
            {
                return;
            }

            foreach (var nativeFile in Directory.GetFiles(nativePath))
            {
                try
                {
                    Assembly.LoadFrom(nativeFile);
                }
                catch (Exception)
                {
                }
            }
        }

        public void LoadNativeLibraries()
        {
            var assemblyBasePath = Path.GetDirectoryName(this.GetType().GetTypeInfo().Assembly.Location);
            var nativePath = Path.Combine(assemblyBasePath, "runtimes", GetNativeFolder(), "native");

            if (!Directory.Exists(nativePath))
            {
                return;
            }

            foreach (var nativeFile in Directory.GetFiles(nativePath))
            {
                LoadUnmanagedDll(nativeFile);
            }
        }

    }
}