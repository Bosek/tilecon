using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TiledataConverter.Tiledata
{
    class TiledataManager
    {
        int landBlockCount = 512;
        int landBlockSize = 836;
        int landTileSize = 26;

        int staticBlockSize = 1188;
        int staticTileSize = 37;

        byte[] data;

        public TiledataManager(string filename = null)
        {
            if (filename != null)
                LoadMul(filename);
        }

        public void LoadMul(string filename)
        {
            data = File.ReadAllBytes(filename);
        }

        public List<TileGroup> LoadLandTiledata(Action<int, int> progressCallback = null)
        {
            if (data == null)
                throw new NullReferenceException("Data buffer is empty.");

            var groupTileList = new List<TileGroup>();

            try
            {
                var landTiledata = data.GetSubArray(0, landBlockCount * landBlockSize);

                var progressTracking = 0;
                var progressFull = (landTiledata.Length / landBlockSize);
                var progressFraction = progressFull / 100;
                for (int block = 0; block < landBlockCount; block++)
                {
                    var landTileGroup = TileGroup.Load(block, landTiledata.GetSubArray(block * landBlockSize, 4));
                    var landTileBlock = landTiledata.GetSubArray(block * landBlockSize + 4, landBlockSize - 4);

                    var landTileList = new List<LandTiledata>();

                    var tilesInBlock = landBlockSize / landTileSize;
                    for (int tile = 0; tile < tilesInBlock; tile++)
                    {
                        var landTile = landTileBlock.GetSubArray(tile * landTileSize, landTileSize);
                        landTileList.Add(LandTiledata.Load(block * tilesInBlock + tile, landTile));
                    }

                    landTileGroup.LandTiles = GetDict(landTileList);
                    groupTileList.Add(landTileGroup);

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

        public List<TileGroup> LoadStaticTiledata(Action<int, int> progressCallback = null)
        {
            if (data == null)
                throw new NullReferenceException("Data buffer is empty.");

            var groupTileList = new List<TileGroup>();

            try
            {
                var staticTiledata = data.GetSubArray(landBlockCount * landBlockSize, data.Length - (landBlockCount * landBlockSize));

                var progressTracking = 0;
                var progressFull = (staticTiledata.Length / staticBlockSize);
                var progressFraction = progressFull / 100;
                for (int block = 0; block < staticTiledata.Length / staticBlockSize; block++)
                {
                    var staticTileGroup = TileGroup.Load(block, staticTiledata.GetSubArray(block * staticBlockSize, 4));
                    var staticTileBlock = staticTiledata.GetSubArray(block * staticBlockSize + 4, staticBlockSize - 4);

                    var staticTileList = new List<StaticTiledata>();

                    var tilesInBlock = staticBlockSize / staticTileSize;
                    for (int tile = 0; tile < tilesInBlock; tile++)
                    {
                        var staticTile = staticTileBlock.GetSubArray(tile * staticTileSize, staticTileSize);
                        staticTileList.Add(StaticTiledata.Load(block * tilesInBlock + tile, staticTile));
                    }
                    staticTileGroup.StaticTiles = GetDict(staticTileList);
                    groupTileList.Add(staticTileGroup);

                    progressTracking++;
                    if (progressTracking % progressFraction == 0)
                        progressCallback?.DynamicInvoke(progressTracking, staticTiledata.Length / staticBlockSize);
                }
                progressCallback?.DynamicInvoke(progressFull, progressFull);
            }
            catch (Exception exception)
            {
                throw new Exception("MUL file corrupted", exception);
            }

            return groupTileList;
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

        public static void SaveTileData(string filename, Dictionary<string, TileGroup> landTileGroupsDict, Dictionary<string, TileGroup> staticTileGroupsDict)
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
                data.AddRange(TileGroup.GetBytes(landTileGroup, true));
            }
            
            foreach (var staticTileGroup in staticTileGroups.OrderBy(group => group.ID))
            {
                data.AddRange(TileGroup.GetBytes(staticTileGroup, true));
            }

            File.WriteAllBytes(filename, data.ToArray());
        }
    }
}
