using System;
using System.Diagnostics;
using System.Management;

namespace SentryCrashListener
{
    /// <summary>
    /// Class responsible for tracking the UnityCrashHandler.exe
    /// </summary>
    public class UnityCrashReporterListener
    {
        private const string UnityCrashReporterProcessName = "UnityCrashHandler64.exe";
        private const string UnityCrashReporterProcessName32Bit = "UnityCrashHandler32.exe";

        public void ListenForExit( string gameAppId )
        {
            var unityCrashReporterAppId = FindUnityCrashReporterAppId( gameAppId );
            WaitForProcessExit( unityCrashReporterAppId );
        }
        
        private static void WaitForProcessExit( int processToTrack )
        {
            var process = Process.GetProcessById( processToTrack );
            
            Console.WriteLine( $"Tracking app: {process.ProcessName}" );
            process.WaitForExit( );
        }

        private static int FindUnityCrashReporterAppId( string gameAppId )
        {
            var searcher = new ManagementObjectSearcher( "Select * From Win32_Process Where ParentProcessID=" + gameAppId );
            var moc = searcher.Get( );

            int appId = -1;

            foreach (var app in moc)
            {
                var appName = ( string ) app["Name"];
                
                if (appName == UnityCrashReporterProcessName || appName == UnityCrashReporterProcessName32Bit)
                    appId = ( int ) ( uint ) app["ProcessId"];
            }

            return appId;
        }
    }
}