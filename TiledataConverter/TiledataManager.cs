using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TiledataConverter
{
    class TiledataManager
    {
        int landBlockSize = 836;
        int landTileSize = 26;

        int staticBlockSize = 1188;
        int staticTileSize = 37;

        byte[] data;

        public TiledataManager(string filename)
        {
            data = File.ReadAllBytes(filename);
        }

        public List<LandTiledata> LoadLandTiledata(Action<int, int> progressCallback = null)
        {
            var blockSize = landBlockSize;
            var tileSize = landTileSize;

            var landTiledata = data.Take(512 * landBlockSize).ToArray();
            var landTileList = new List<LandTiledata>();

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
            return landTileList;
        }

        public List<StaticTiledata> LoadStaticTiledata(Action<int, int> progressCallback = null)
        {
            var blockSize = staticBlockSize;
            var tileSize = staticTileSize;

            var staticTiledata = data.Skip(512 * landBlockSize).ToArray();
            var staticTileList = new List<StaticTiledata>();

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
            return staticTileList;
        }

        public static void SaveTileData(string filename, LandTiledata[] landTiles, StaticTiledata[] staticTiles)
        {
            landTiles = landTiles.OrderBy((tile) => tile.ID).ToArray();
            staticTiles = staticTiles.OrderBy((tile) => tile.ID).ToArray();

            var data = new List<byte>();

            var landBlock = 0;
            foreach (LandTiledata landTile in landTiles)
            {
                if (landBlock == 0)
                    data.AddRange(new byte[4]);

                data.AddRange(LandTiledata.GetBytes(landTile));

                landBlock++;
                if (landBlock == 32)
                    landBlock = 0;
            }

            var staticBlock = 0;
            foreach (StaticTiledata staticTile in staticTiles)
            {
                if (staticBlock == 0)
                    data.AddRange(new byte[4]);

                data.AddRange(StaticTiledata.GetBytes(staticTile));

                staticBlock++;
                if (staticBlock == 32)
                    staticBlock = 0;
            }

            File.WriteAllBytes(filename, data.ToArray());
        }
    }
}
