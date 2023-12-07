using System.Runtime.InteropServices;
using DiscordBotProject;

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
        ConsoleWriter.WriteSuccessLn($"Loaded opus with version string: {opusVersion}");
        string sodiumVersion = Marshal.PtrToStringAnsi(SodiumVersionString());
        ConsoleWriter.WriteSuccessLn($"Loaded sodium with version string: {sodiumVersion}");
    }
}