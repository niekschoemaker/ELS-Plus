using System;

namespace ELSShared
{
    public static class EventNames
    {
        public const string NewFullSyncData = "ELS:NFSD";
        public const string NewLightSyncData = "ELS:NLSD";
        public const string NewSirenSyncData = "ELS:NSSD";
        public const string FullSyncBroadcast = "ELS:FS:Bc";
        public const string LightSyncBroadcast = "ELS:LS:Bc";
        public const string SirenSyncBroadcast = "ELS:SS:Bc";
        public const string RemoveStale = "ELS:FS:RS";
        public const string VehicleExited = "ELS:VE";
        public const string VcfSyncServer = "ELS:VcfSync:Server";
        public const string VcfSyncClient = "ELS:VcfSync:Client";
        public const string FullSyncNewSpawn = "ELS:FS:NS";
        public const string NewSpawnWithData = "ELS:FS:NSWD";
        public const string FullSycnRequestAll = "ELS:FS:Request:All";
        public const string FullSyncRequestOne = "ELS:FS:Request:One";
    }
}
