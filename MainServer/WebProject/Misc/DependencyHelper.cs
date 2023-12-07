using System.Runtime.InteropServices;

namespace WebProject.Misc;

public static class DependencyHelper
{
    [DllImport("opus", EntryPoint = "opus_get_version_string", CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr OpusVersionString();
    [DllImport("libsodium", EntryPoint = "sodium_version_string", CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr SodiumVersionString();
        
    public static void TestDependencies()
    {
        string opusVersion = Marshal.PtrToStringAnsi(OpusVersionString());
        Console.WriteLine($"Loaded opus with version string: {opusVersion}");
        string sodiumVersion = Marshal.PtrToStringAnsi(SodiumVersionString());
        Console.WriteLine($"Loaded sodium with version string: {sodiumVersion}");
    }
}