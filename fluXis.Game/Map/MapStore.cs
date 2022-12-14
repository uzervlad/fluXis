using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using osu.Framework.Logging;
using osu.Framework.Platform;

namespace fluXis.Game.Map
{
    public class MapStore
    {
        private static readonly List<MapInfo> maps = new List<MapInfo>();
        private static Storage storage;

        public void LoadMaps(Storage stor)
        {
            storage = stor;

            if (storage.ExistsDirectory("maps"))
            {
                Logger.Log("Loading maps...");
                var dirs = storage.GetDirectories("maps");

                foreach (var dir in dirs)
                {
                    try
                    {
                        var charts = storage.GetFiles(dir, "*.fsc");

                        foreach (var chart in charts)
                        {
                            string dirName = dir.Split(Path.DirectorySeparatorChar).Last();
                            MapInfo map = JsonConvert.DeserializeObject<MapInfo>(File.ReadAllText(storage.GetFullPath(chart)));
                            map.MapsetID = dirName;
                            map.ID = Path.GetFileNameWithoutExtension(chart);
                            Logger.Log($"Loaded map {map.ID} from {map.MapsetID}");
                            maps.Add(map);
                        }
                    }
                    catch (Exception e)
                    {
                        Logger.Error(e, "Error loading map");
                    }
                }
            }
            else
            {
                Logger.Log("No maps found.");
            }
        }

        public List<MapInfo> GetMaps() => maps;
        public MapInfo GetMap(string mapsetID, string mapID) => maps.Find(x => x.MapsetID == mapsetID && x.ID == mapID);
        public string GetMapAudioPath(MapInfo map) => storage.GetFullPath($"maps{Path.DirectorySeparatorChar}{map.MapsetID}{Path.DirectorySeparatorChar}{map.GetAudioFile()}");
    }
}
