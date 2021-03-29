using System;
using Newtonsoft.Json;

namespace SentryCrashListener
{
    static class Entry
    {
        private static void Main( string[] args )
        {
            if (args.Length != 4)
            {
                Console.WriteLine( "Expected 4 arguments in format {dsn} {appId} {crashDirectoriesRoot} {gameInfo}" );
                return;
            }

            var dsn = args[0];
            var gameAppId = args[1];
            var crashDirectoriesPath = args[2];
            var gameInfoArg = args[3];
            
            var gameInfo = JsonConvert.DeserializeObject<GameInfo>( gameInfoArg );
            var crashReporter = new CrashReporter( gameInfo, dsn, 60 );
            var unityCrashReporter = new UnityCrashReporterListener( );

            unityCrashReporter.ListenForExit( gameAppId );
            crashReporter.TryReportLatestCrash( crashDirectoriesPath );
        }
    }
}