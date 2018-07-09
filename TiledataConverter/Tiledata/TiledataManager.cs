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
                var landTiledata = data.Take(landBlockCount * landBlockSize).ToArray();

                var progressTracking = 0;
                for (int block = 0; block < landBlockCount; block++)
                {
                    var landTileGroup = TileGroup.Load(block, landTiledata.Skip(block * landBlockSize).Take(4).ToArray());
                    var landTileBlock = landTiledata.Skip(block * landBlockSize + 4).ToArray();
                    
                    var landTileList = new List<LandTiledata>();

                    var tilesInBlock = landBlockSize / landTileSize;
                    for (int tile = 0; tile < tilesInBlock; tile++)
                    {
                        var landTile = landTileBlock.Skip(tile * landTileSize).Take(landTileSize).ToArray();
                        landTileList.Add(LandTiledata.Load(block * tilesInBlock + tile, landTile));
                    }

                    landTileGroup.LandTiles = GetDict(landTileList);
                    groupTileList.Add(landTileGroup);

                    progressTracking++;
                    progressCallback?.DynamicInvoke(progressTracking, landTiledata.Length / landBlockSize);
                }
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
                var staticTiledata = data.Skip(landBlockCount * landBlockSize).ToArray();

                var progressTracking = 0;
                for (int block = 0; block < staticTiledata.Length / staticBlockSize; block++)
                {
                    var staticTileGroup = TileGroup.Load(block, staticTiledata.Skip(block * staticBlockSize).Take(4).ToArray());
                    var staticTileBlock = staticTiledata.Skip(block * staticBlockSize + 4).ToArray();
                    
                    var staticTileList = new List<StaticTiledata>();

                    var tilesInBlock = staticBlockSize / staticTileSize;
                    for (int tile = 0; tile < tilesInBlock; tile++)
                    {
                        staticTileList.Add(StaticTiledata.Load(block * tilesInBlock + tile, staticTileBlock.Skip(tile * staticTileSize).Take(staticTileSize).ToArray()));
                    }
                    staticTileGroup.StaticTiles = GetDict(staticTileList);
                    groupTileList.Add(staticTileGroup);

                    progressTracking++;
                    progressCallback?.DynamicInvoke(progressTracking, staticTiledata.Length / staticBlockSize);
                }
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
