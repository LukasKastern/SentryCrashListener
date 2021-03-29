using System;
using System.Diagnostics;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

#if UNITY_EDITOR
public static class CopySentryCrashListenerToBuild
{
    [UnityEditor.Callbacks.PostProcessBuild]
    public static void OnPostProcessBuild( UnityEditor.BuildTarget target, string pathToBuiltProject )
    {
        CopyFilesRecursively( new DirectoryInfo( Path.Combine( Application.dataPath, "../", "CrashListener" ) ), new DirectoryInfo( Path.Combine(pathToBuiltProject, "../CrashListener") ) );
    }

    private static void CopyFilesRecursively(DirectoryInfo source, DirectoryInfo target) 
    {
        foreach (DirectoryInfo dir in source.GetDirectories())
            CopyFilesRecursively(dir, target.CreateSubdirectory(dir.Name));
        foreach (FileInfo file in source.GetFiles())
            file.CopyTo(Path.Combine(target.FullName, file.Name), true);
    }
}
#endif

public class StartCrashListener : MonoBehaviour
{
    [Serializable]
    [JsonObject(MemberSerialization.Fields)]
    class GameInfo
    {
        
        public GameInfo( )
        {
            AppName = Application.productName;

            GameVersion = Application.version;
            
#if ENABLE_IL2CPP
            Runtime = "IL2CPP";
#else
            Runtime = "Mono";
#endif

            OperatingSystem = SystemInfo.operatingSystem;

            Gpu = new GPU
            {
                Vendor = SystemInfo.graphicsDeviceVendor,
                Name = SystemInfo.graphicsDeviceName,
                VendorId = SystemInfo.graphicsDeviceVendorID.ToString( ),
                VendorName = SystemInfo.graphicsDeviceVendor,
                Version = SystemInfo.graphicsDeviceVersion,
                MultiThreaded = SystemInfo.graphicsMultiThreaded,
                GraphicsDeviceType = SystemInfo.graphicsDeviceType.ToString( ),
                MemorySize = SystemInfo.graphicsMemorySize,
                Id = SystemInfo.graphicsDeviceID,
            };
        }
        
        // app
        public string AppName;
        public string GameVersion;

        // runtime
        public string Runtime;
        
        // gpu

        public string OperatingSystem;

        public GPU Gpu;

        [Serializable]
        [JsonObject(MemberSerialization.Fields)]
        internal class GPU
        {
            public string Vendor;
            public string Name;
            public string VendorId;
            public string VendorName;

            public string Version;
            
            public bool MultiThreaded;
            public string GraphicsDeviceType;
            public int MemorySize;
            public int Id;
        }
    }
    
#if UNITY_STANDALONE_WIN // Only enable in build && only windows for now
    [RuntimeInitializeOnLoadMethod]
#endif
    
    public static void Init( )
    {
        var fullPath = GetFullPath( );
        
        if (!File.Exists( fullPath ))
        {
            UnityEngine.Debug.LogWarning( $"Failed to start process, couldn't find file at {GetFullPath( )}" );
            return;
        }

        var args = GetArgs( );

        var psi = new ProcessStartInfo( fullPath, args );
        psi.CreateNoWindow = true;
        psi.WindowStyle = ProcessWindowStyle.Hidden;
        Process.Start( psi );
    }

    private static string GetArgs( )
    {
        var args = "";
        
        args += "https://5a81b3b3bef549f19ccf506b8aaac872@o560177.ingest.sentry.io/5695538" + "   ";
        args += Process.GetCurrentProcess().Id.ToString() + "   ";
        args += "\"" + Path.Combine( Path.GetTempPath( ), Application.companyName, Application.productName, "Crashes" ) + "\"" + "   ";
        var gameInfoAsString = JsonConvert.SerializeObject( new GameInfo( ) );

        // we need to format the game info json so cmd doesn't strip our quotation marks
        for (int i = gameInfoAsString.Length - 1; i >= 0; --i)
        {
            if (gameInfoAsString[i] == '"')
            {
                gameInfoAsString = gameInfoAsString.Insert( i, "\"" );
            }
        }

        args += "\"" + gameInfoAsString + "\"";
        return args;
    }

    private static string GetFullPath()
    {
        return Path.Combine( Directory.GetCurrentDirectory( ), CrashReporterPath );
    }

    private const string CrashReporterPath = "CrashListener/SentryCrashListener.exe";
}
