using System;
using System.IO;
using System.Linq;
using Sentry;
using Sentry.Protocol;

namespace SentryCrashListener
{
    class CrashReporter
    {
        private readonly float m_reportCrashIfYoungerThen;
        private readonly GameInfo m_gameInfo;
        private readonly string m_dsn;
        
       
        /// <summary>
        /// </summary>
        /// <param name="gameInfo">info to pass to sentry when doing a crash report</param>
        /// <param name="reportCrashIfYoungerThen">A report will only be considered when it is younger then this threshold in seconds</param>
        public CrashReporter( GameInfo gameInfo, string dsn, float reportCrashIfYoungerThen = 60 )
        {
            m_dsn = dsn;
            m_gameInfo = gameInfo;
            m_reportCrashIfYoungerThen = reportCrashIfYoungerThen;
        }

        /// <summary>
        /// Tries to fetch the latest crash report and reports it if it is younger then <see cref="m_reportCrashIfYoungerThen"/>
        /// </summary>
        /// <param name="crashDirectoriesPath">This should be the path that contains the crash reports for the game</param>
        public void TryReportLatestCrash( string crashDirectoriesPath )
        {
            if (!Directory.Exists( crashDirectoriesPath ))
            {
                Console.Write( $"File doesn't exit: {crashDirectoriesPath}" );
                // No crashes reported
                return;
            }
            
            
            var crashDirectoryInfo = new DirectoryInfo( crashDirectoriesPath );
            var childDirectories = crashDirectoryInfo.EnumerateDirectories( );
            childDirectories = childDirectories.OrderByDescending( i => i.LastWriteTime );
            
            var crashDirectory = childDirectories.FirstOrDefault();

            if (crashDirectory == null)
            {
                Console.WriteLine( "Couldn't find crash directory" );
                return;
            }

            var timeSinceAccess = DateTime.Now - crashDirectory.LastWriteTime;
            if (timeSinceAccess.TotalSeconds > m_reportCrashIfYoungerThen)
            {
                Console.WriteLine($"Crash directory is to old: {crashDirectory.LastWriteTime}, Now: {DateTime.Now}");
                return;
            }
            
            var crashPath = Path.Combine( crashDirectoriesPath, crashDirectory.Name );
            
            var dmp = Path.Combine( crashPath, "crash.dmp" );
            var log = Path.Combine( crashPath, "Player.log" );
            var error = Path.Combine( crashPath, "error.log" );

            using (SentrySdk.Init( m_dsn ))
            {                
                var scope = CreateScopeFromGameInfo( m_gameInfo );
                
                scope.AddAttachment( dmp );
                scope.AddAttachment( error );
                scope.AddAttachment( log );

                var sentryEvent = new SentryEvent
                {
                    Message = ExtractErrorMessageFromLog(error)
                };
         
                Console.WriteLine( $"Sending crash originating from {crashPath}" );

                SentrySdk.CaptureEvent( sentryEvent, scope );
            }
        }
            
        /// <summary>
        /// Tries to fetch the crash thread info from the Error.log and returns it if found/successful
        /// </summary>
        static string ExtractErrorMessageFromLog( string errorLogPath )
        {
            if (!File.Exists( errorLogPath ))
                return "Error Log Not found";

            var allLines = File.ReadAllLines( errorLogPath ).ToList();

            var stackTraceLineBegin = allLines.FindIndex( i => i.Contains( "Stack Trace of Crashed Thread" ) );

            if (stackTraceLineBegin == -1)
                return "Crashed Thread not found";

            string message = "";


            int line = stackTraceLineBegin + 1;

            while (line < allLines.Count && !String.IsNullOrWhiteSpace( allLines[line] ))
            {
                message += allLines[line++] + "\n";
            }

            return message;
        }
        
        static Scope CreateScopeFromGameInfo( GameInfo gameInfo )
        {
            var scope = new Scope( new SentryOptions
            {
                AttachStacktrace = false,
                ReportAssemblies = false,
            } );

            var contexts = scope.Contexts;
            contexts.Clear( );

            contexts["runtime"] = new Runtime
            {
                Name = gameInfo.Runtime,
            };
            
            var gpu = gameInfo.Gpu;

            contexts["gpu"] = new Gpu
            {
                Id = gpu.Id,    
                ApiType = gpu.GraphicsDeviceType,
                MemorySize = gpu.MemorySize,
                MultiThreadedRendering = gpu.MultiThreaded,
                Name = gpu.Name,
                VendorId = gpu.VendorId,
                VendorName = gpu.VendorName,
                Version = gpu.Version,
            };

            contexts["app"] = new App
            {
                Name = gameInfo.AppName,
                Version = gameInfo.GameVersion,
            };

            contexts["os"] = new Sentry.Protocol.OperatingSystem( )
            {
                RawDescription = gameInfo.OperatingSystem
            };
            
            return scope;
        }
    }
}