using System;
using System.Collections.Generic;
using System.Linq;

namespace TiledataConverter.Tiledata
{
    struct TileGroup
    {
        [Newtonsoft.Json.JsonIgnore]
        public int ID { get; set; }

        [Newtonsoft.Json.JsonIgnore]
        public string HexID
        {
            get { return ID.ToString("X4"); }
            set
            {
                ID = int.Parse(value, System.Globalization.NumberStyles.HexNumber);
            }
        }

        public int Unknown { get; set; }
        
        public Dictionary<string, LandTiledata> LandTiles { get; set; }
        public Dictionary<string, StaticTiledata> StaticTiles { get; set; }

        public static byte[] GetBytes(TileGroup obj, bool withTiles = false)
        {
            var data = new byte[4];

            BitConverter.GetBytes(obj.Unknown).CopyTo(data, 0);

            if (withTiles && (obj.LandTiles != null || obj.StaticTiles != null))
            {
                if (obj.LandTiles != null)
                {
                    foreach (var landTile in obj.LandTiles.OrderBy(kvPair => kvPair.Key))
                    {
                        var landTileData = LandTiledata.GetBytes(landTile.Value);
                        Array.Resize(ref data, data.Length + landTileData.Length);
                        landTileData.CopyTo(data, data.Length - landTileData.Length);
                    }
                }
                else if (obj.StaticTiles != null)
                {
                    foreach (var staticTile in obj.StaticTiles.OrderBy(kvPair => kvPair.Key))
                    {
                        var staticTileData = StaticTiledata.GetBytes(staticTile.Value);
                        Array.Resize(ref data, data.Length + staticTileData.Length);
                        staticTileData.CopyTo(data, data.Length - staticTileData.Length);
                    }
                }
            }
            return data;
        }
        public static TileGroup Load(int ID, byte[] data)
        {
            return new TileGroup
            {
                ID = ID,
                Unknown = BitConverter.ToInt32(data, 0)
            };
        }
    } 
}
