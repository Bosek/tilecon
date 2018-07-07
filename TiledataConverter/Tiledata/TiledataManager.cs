using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TiledataConverter.Tiledata
{
    class TiledataManager
    {
        int landBlockSize = 836;
        int landTileSize = 26;

        int staticBlockSize = 1188;
        int staticTileSize = 37;

        byte[] data;

        public TiledataManager(string filename = null, int landBlockSize = 836, int staticBlockSize = 1188)
        {
            if (filename != null)
                LoadMul(filename);

            this.landBlockSize = landBlockSize;
            this.staticBlockSize = staticBlockSize;
        }

        public void LoadMul(string filename)
        {
            data = File.ReadAllBytes(filename);
        }

        public List<LandTiledata> LoadLandTiledata(Action<int, int> progressCallback = null)
        {
            if (data == null)
                throw new NullReferenceException("Data buffer is empty.");

            var blockSize = landBlockSize;
            var tileSize = landTileSize;
            var landTileList = new List<LandTiledata>();

            try
            {
                var landTiledata = data.Take(512 * landBlockSize).ToArray();

                var progressTracking = 0;
                for (int block = 0; block < landTiledata.Length / blockSize; block++)
                {
                    var landTileBlock = landTiledata.Skip(block * blockSize + 4).ToArray();
                    for (int tile = 0; tile < 32; tile++)
                    {
                        landTileList.Add(LandTiledata.Load(block * 32 + tile, landTileBlock.Skip(tile * tileSize).Take(tileSize).ToArray()));
                    }
                    progressTracking++;
                    progressCallback?.DynamicInvoke(progressTracking, landTiledata.Length / blockSize);
                }
            }
            catch (Exception exception)
            {
                throw new Exception("MUL file corrupted", exception);
            }
            
            return landTileList;
        }

        public List<StaticTiledata> LoadStaticTiledata(Action<int, int> progressCallback = null)
        {
            if (data == null)
                throw new NullReferenceException("Data buffer is empty.");

            var blockSize = staticBlockSize;
            var tileSize = staticTileSize;
            var staticTileList = new List<StaticTiledata>();

            try
            {
                var staticTiledata = data.Skip(512 * landBlockSize).ToArray();

                var progressTracking = 0;
                for (int block = 0; block < staticTiledata.Length / blockSize; block++)
                {
                    var staticTileBlock = staticTiledata.Skip(block * blockSize + 4).ToArray();
                    for (int tile = 0; tile < 32; tile++)
                    {
                        staticTileList.Add(StaticTiledata.Load(block * 32 + tile, staticTileBlock.Skip(tile * tileSize).Take(tileSize).ToArray()));
                    }
                    progressTracking++;
                    progressCallback?.DynamicInvoke(progressTracking, staticTiledata.Length / blockSize);
                }
            }
            catch (Exception exception)
            {
                throw new Exception("MUL file corrupted", exception);
            }

            return staticTileList;
        }

        public static Dictionary<string, LandTiledata> GetDict(List<LandTiledata> landTiles)
        {
            var dict = new Dictionary<string, LandTiledata>();
            landTiles.ForEach(tile => dict.Add(tile.HexID, tile));
            return dict;
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

        public static Dictionary<string, StaticTiledata> GetDict(List<StaticTiledata> landTiles)
        {
            var dict = new Dictionary<string, StaticTiledata>();
            landTiles.ForEach(tile => dict.Add(tile.HexID, tile));
            return dict;
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

        public static void SaveTileData(string filename, LandTiledata[] landTiles, StaticTiledata[] staticTiles)
        {
            if (landTiles.Length != 512 * 32)
                throw new ArgumentException("There must be exactly 512 * 32 land tiles.", nameof(landTiles));
            if (staticTiles.Length % 32 != 0)
                throw new ArgumentException("Number of static tiles must be divisible by 32.", nameof(staticTiles));

            var data = new List<byte>();

            var landBlock = 0;
            foreach (var landTile in landTiles.OrderBy(tile => tile.ID))
            {
                if (landBlock % 32 == 0)
                    data.AddRange(new byte[4]);

                data.AddRange(LandTiledata.GetBytes(landTile));

                landBlock++;
            }

            var staticBlock = 0;
            foreach (var staticTile in staticTiles.OrderBy(tile => tile.ID))
            {
                if (staticBlock % 32 == 0)
                    data.AddRange(new byte[4]);

                data.AddRange(StaticTiledata.GetBytes(staticTile));

                staticBlock++;
            }

            File.WriteAllBytes(filename, data.ToArray());
        }
    }
}
