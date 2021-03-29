using Newtonsoft.Json;

namespace SentryCrashListener
{
    /// <summary>
    /// This class holds game and machine specific information that unity passes to us so we can pass them to sentry when doing the crash report
    /// </summary>
    [JsonObject(MemberSerialization.Fields)]
    class GameInfo
    {
        public string AppName;
        public string GameVersion;

        public string Runtime;
        
        public string OperatingSystem;

        public GPU Gpu;
        
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
}