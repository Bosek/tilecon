using System;
using System.Linq;
using System.Text;

namespace TiledataConverter
{
    class StaticTiledata
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
        public byte Weight { get; set; }
        public byte Quality { get; set; }
        public ushort Unknown { get; set; }
        public byte Unknown1 { get; set; }
        public byte Quantity { get; set; }
        public ushort AnimationID { get; set; }
        public byte Unknown2 { get; set; }
        public byte Hue { get; set; }
        public ushort Unknown3 { get; set; }
        public byte Height { get; set; }
        public string TileName { get; set; }

        public static StaticTiledata Load(int ID, byte[] data)
        {
            var staticTile = new StaticTiledata
            {
                ID = ID,
                Flags = BitConverter.ToInt32(data, 0),
                Weight = data.Skip(4).Take(1).ToArray()[0],
                Quality = data.Skip(5).Take(1).ToArray()[0],
                Unknown = BitConverter.ToUInt16(data, 6),
                Unknown1 = data.Skip(8).Take(1).ToArray()[0],
                Quantity = data.Skip(9).Take(1).ToArray()[0],
                AnimationID = BitConverter.ToUInt16(data, 10),
                Unknown2 = data.Skip(12).Take(1).ToArray()[0],
                Hue = data.Skip(13).Take(1).ToArray()[0],
                Unknown3 = BitConverter.ToUInt16(data, 14),
                Height = data.Skip(16).Take(1).ToArray()[0],
                TileName = Encoding.ASCII.GetString(data, 17, 20).Replace('\u0000', ' ').Trim()
            };
            return staticTile;
        }

        public static byte[] GetBytes(StaticTiledata obj)
        {
            var data = new byte[37];

            BitConverter.GetBytes(obj.Flags).CopyTo(data, 0);
            BitConverter.GetBytes(obj.Weight).CopyTo(data, 4);
            BitConverter.GetBytes(obj.Quality).CopyTo(data, 5);
            BitConverter.GetBytes(obj.Unknown).CopyTo(data, 6);
            BitConverter.GetBytes(obj.Unknown1).CopyTo(data, 8);
            BitConverter.GetBytes(obj.Quantity).CopyTo(data, 9);
            BitConverter.GetBytes(obj.AnimationID).CopyTo(data, 10);
            BitConverter.GetBytes(obj.Unknown2).CopyTo(data, 12);
            BitConverter.GetBytes(obj.Hue).CopyTo(data, 13);
            BitConverter.GetBytes(obj.Unknown3).CopyTo(data, 14);
            BitConverter.GetBytes(obj.Height).CopyTo(data, 16);
            var trimmedTileName = obj.TileName.Trim();
            if (trimmedTileName.Length > 20)
                trimmedTileName.Substring(0, 20);
            Encoding.ASCII.GetBytes(trimmedTileName).CopyTo(data, 17);

            return data;
        }
    }
}
