using System;
using System.Text;

namespace TiledataConverter
{
    class LandTiledata
    {
        [Newtonsoft.Json.JsonIgnore]
        public int ID { get; set; }

        [Newtonsoft.Json.JsonProperty(PropertyName = nameof(ID))]
        public string HexID
        {
            get { return "0x" + ID.ToString("X4"); }
            set
            {
                if (value.StartsWith("0x", StringComparison.CurrentCultureIgnoreCase))
                    ID = Int32.Parse(value.Substring(2), System.Globalization.NumberStyles.HexNumber);
                else
                    ID = Int32.Parse(value);
            }
        }

        [Newtonsoft.Json.JsonIgnore]
        public int Flags { get; set; }
        [Newtonsoft.Json.JsonProperty(PropertyName = nameof(Flags))]
        public string HexFlags
        {
            get { return "0x" + Flags.ToString("X8"); }
            set
            {
                if (value.StartsWith("0x", StringComparison.CurrentCultureIgnoreCase))
                    Flags = Int32.Parse(value.Substring(2), System.Globalization.NumberStyles.HexNumber);
                else
                    Flags = Int32.Parse(value);
            }
        }
        public ushort TextureID { get; set; }

        public string TileName { get; set; }

        public static LandTiledata Load(int ID, byte[] data)
        {
            var landTile = new LandTiledata
            {
                ID = ID,
                Flags = BitConverter.ToInt32(data, 0),
                TextureID = BitConverter.ToUInt16(data, 4),
                TileName = Encoding.ASCII.GetString(data, 6, 20).Replace('\u0000', ' ').Trim()
            };
            return landTile;
        }

        public static byte[] GetBytes(LandTiledata obj)
        {
            var data = new byte[26];

            BitConverter.GetBytes(obj.Flags).CopyTo(data, 0);
            BitConverter.GetBytes(obj.TextureID).CopyTo(data, 4);
            var trimmedTileName = obj.TileName.Trim();
            if (trimmedTileName.Length > 20)
                trimmedTileName.Substring(0, 20);
            Encoding.ASCII.GetBytes(trimmedTileName).CopyTo(data, 6);

            return data;
        }
    }
}
