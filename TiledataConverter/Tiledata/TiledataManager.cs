using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TiledataConverter.Tiledata
{
    class Tiledata
    {
        int landBlockCount = 512;
        int landBlockSize = 836;
        int landTileSize = 26;

        int staticBlockSize = 1188;
        int staticTileSize = 37;

        byte[] data;

        public Tiledata(string filename = null)
        {
            if (filename != null)
                LoadMul(filename);
        }

        public void LoadMul(string filename)
        {
            data = File.ReadAllBytes(filename);
        }

        public static Dictionary<string, TileGroup> GetDict(List<TileGroup> landGroups)
        {
            var dict = new Dictionary<string, TileGroup>();
            landGroups.ForEach(group => dict.Add(group.HexID, group));
            return dict;
        }
        public static Dictionary<string, LandTiledata> GetDict(List<LandTiledata> landTiles)
        {
            var dict = new Dictionary<string, LandTiledata>();
            landTiles.ForEach(tile => dict.Add(tile.HexID, tile));
            return dict;
        }
        public static Dictionary<string, StaticTiledata> GetDict(List<StaticTiledata> staticTiles)
        {
            var dict = new Dictionary<string, StaticTiledata>();
            staticTiles.ForEach(tile => dict.Add(tile.HexID, tile));
            return dict;
        }

        public static List<TileGroup> GetList(Dictionary<string, TileGroup> dict)
        {
            var list = new List<TileGroup>();
            dict.ToList().ForEach(kvPair =>
            {
                var tileGroup = kvPair.Value;
                tileGroup.HexID = kvPair.Key;
                list.Add(tileGroup);
            });
            return list;
        }
        public static List<LandTiledata> GetList(Dictionary<string, LandTiledata> dict)
        {
            var list = new List<LandTiledata>();
            dict.ToList().ForEach(kvPair =>
            {
                var landTile = kvPair.Value;
                landTile.HexID = kvPair.Key;
                list.Add(landTile);
            });
            return list;
        }
        public static List<StaticTiledata> GetList(Dictionary<string, StaticTiledata> dict)
        {
            var list = new List<StaticTiledata>();
            dict.ToList().ForEach(kvPair =>
            {
                var staticTile = kvPair.Value;
                staticTile.HexID = kvPair.Key;
                list.Add(staticTile);
            });
            return list;
        }

        public static void Save(string filename, Dictionary<string, TileGroup> landTileGroupsDict, Dictionary<string, TileGroup> staticTileGroupsDict)
        {
            var landTileGroups = GetList(landTileGroupsDict);
            var staticTileGroups = GetList(staticTileGroupsDict);
            var landTiles = GetList(landTileGroups
                .SelectMany(landTileGroup => landTileGroup.LandTiles)
                .ToDictionary(kvPair => kvPair.Key, kvPair => kvPair.Value));
            var staticTiles = GetList(staticTileGroups
                .SelectMany(staticTileGroup => staticTileGroup.StaticTiles)
                .ToDictionary(kvPair => kvPair.Key, kvPair => kvPair.Value));

            if (landTiles.Count() != 512 * 32)
                throw new ArgumentException("There must be exactly 512 * 32 land tiles.", nameof(landTiles));
            if (staticTiles.Count() % 32 != 0)
                throw new ArgumentException("Number of static tiles must be divisible by 32.", nameof(staticTiles));

            var data = new List<byte>();

            foreach (var landTileGroup in landTileGroups.OrderBy(group => group.ID))
            {
                if (landTileGroup.LandTiles.Count != 32)
                    throw new ArgumentException($"There are not 32 items{landTileGroup.LandTiles.Count} in land tile group {landTileGroup.HexID}");
                data.AddRange(TileGroup.GetBytes(landTileGroup, true));
            }
            
            foreach (var staticTileGroup in staticTileGroups.OrderBy(group => group.ID))
            {
                if (staticTileGroup.StaticTiles.Count != 32)
                    throw new ArgumentException($"There are not 32 items{staticTileGroup.StaticTiles.Count} in static tile group {staticTileGroup.HexID}");
                data.AddRange(TileGroup.GetBytes(staticTileGroup, true));
            }

            File.WriteAllBytes(filename, data.ToArray());
        }

        public List<TileGroup> Load<T>(Action<int, int> progressCallback = null)
        {
            if (data == null)
                throw new NullReferenceException("Data buffer is empty.");

            var tileType = typeof(T);

            var blockSize = 0;
            var startPosition = 0;
            var tiledataSize = 0;
            var tileSize = 0;

            if (tileType == typeof(LandTiledata))
            {
                blockSize = landBlockSize;
                tileSize = landTileSize;
                tiledataSize = landBlockCount * landBlockSize;
            }
            else if (tileType == typeof(StaticTiledata))
            {
                blockSize = staticBlockSize;
                startPosition = landBlockCount * landBlockSize;
                tileSize = staticTileSize;
                tiledataSize = data.Length - (landBlockCount * landBlockSize);
            }
            else
            {
                throw new ArgumentException($"Only {nameof(LandTiledata)} or {nameof(StaticTiledata)} is acceptable.");
            }

            var groupTileList = new List<TileGroup>();

            try
            {
                var tiledata = data.GetSubArray(startPosition, tiledataSize);

                var progressTracking = 0;
                var progressFull = (tiledata.Length / blockSize);
                var progressFraction = progressFull / 100;
                for (int block = 0; block < tiledata.Length / blockSize; block++)
                {
                    var tileGroup = TileGroup.Load(block, tiledata.GetSubArray(block * blockSize, 4));
                    var tileBlock = tiledata.GetSubArray(block * blockSize + 4, blockSize - 4);

                    var tileList = new List<T>();
                    var landTileList = new List<LandTiledata>();
                    var staticTileList = new List<StaticTiledata>();

                    for (int tileIndex = 0; tileIndex < 32; tileIndex++)
                    {
                        var tile = tileBlock.GetSubArray(tileIndex * tileSize, tileSize);
                        var loadedTile = typeof(T).GetMethod("Load").Invoke(null, new object[] { block * 32 + tileIndex, tile });
                        if (tileType == typeof(LandTiledata))
                            landTileList.Add((LandTiledata)loadedTile);
                        else if (tileType == typeof(StaticTiledata))
                            staticTileList.Add((StaticTiledata)loadedTile);
                    }

                    if (tileType == typeof(LandTiledata))
                        tileGroup.LandTiles = GetDict(landTileList);
                    else if (tileType == typeof(StaticTiledata))
                        tileGroup.StaticTiles = GetDict(staticTileList);
                    groupTileList.Add(tileGroup);

                    progressTracking++;
                    if (progressTracking % progressFraction == 0)
                        progressCallback?.DynamicInvoke(progressTracking, progressFull);
                }
                progressCallback?.DynamicInvoke(progressFull, progressFull);
            }
            catch (Exception exception)
            {
                throw new Exception("MUL file corrupted", exception);
            }

            return groupTileList;
        }
    }
}
