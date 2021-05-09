#define ENABLE_LOGGING

using System;
using System.IO;
using Newtonsoft.Json;

namespace SentryCrashListener
{
    static class Entry
    {
        private static void Main( string[] args )
        {
            try
            {
                if (args.Length != 4)
                {
                    Logger.Write("[Entry] Expected 4 arguments in format {dsn} {appId} {crashDirectoriesRoot} {gameInfo}");
                    return;
                }

                Logger.Write("[Entry] Args:");

                foreach (var arg in args)
                    Logger.Write( arg );
            
            
                var dsn = args[0];
                var gameAppId = args[1];
                var crashDirectoriesPath = args[2];
                var gameInfoArg = args[3];
            
                Logger.Write( "[Entry] Deserializing Game Info now" );
                var gameInfo = JsonConvert.DeserializeObject<GameInfo>( gameInfoArg );
            
                Logger.Write( "[Entry] Starting crash reporter now" );
                var crashReporter = new CrashReporter( gameInfo, dsn, 60 );
            
                Logger.Write( "[Entry] Starting crash listener now" );
                var unityCrashReporter = new UnityCrashReporterListener( );

                Logger.Write( "[Entry] Waiting for exit now" );
                unityCrashReporter.ListenForExit( gameAppId );

                Logger.Write( "[Entry] Trying to report crash now" );
                crashReporter.TryReportLatestCrash( crashDirectoriesPath );
            }
            catch (Exception e)
            {
                Logger.Write( $"SentryCrashListener failed, Exception got thrown: {e.ToString( )}" );
            }
        }
    }

    static class Logger
    {
        public static void Write( string arg )
        {
#if ENABLE_LOGGING
            using (var file = File.AppendText( "Sentry.log" ))
                file.WriteLine( arg );
#endif
        }
    }
}