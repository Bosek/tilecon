using System;
using System.Linq;
using System.Text;

namespace TiledataConverter.Tiledata
{
    struct StaticTiledata
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

        [Newtonsoft.Json.JsonIgnore]
        public Flags Flags { get; set; }

        [Newtonsoft.Json.JsonProperty(nameof(Flags))]
        public string[] FlagsArray
        {
            get
            {
                var flags = Flags;
                return Enum.GetValues(typeof(Flags)).Cast<Flags>()
                    .Where(flag => flags.HasFlag(flag))
                    .Select(flag => flag.ToString())
                    .ToArray();
            }
            set
            {
                var flags = Flags;
                value.ToList().ForEach(flag => flags |= (Flags)Enum.Parse(typeof(Flags), flag, true));
            }
        }
        public byte Weight { get; set; }
        public byte Quality { get; set; }
        public ushort MiscData { get; set; }
        public byte Unknown1 { get; set; }
        public byte Quantity { get; set; }
        public ushort AnimationID { get; set; }
        public byte Unknown2 { get; set; }
        public byte Hue { get; set; }
        public byte StackOff { get; set; }
        public byte Value { get; set; }
        public byte Height { get; set; }
        public string TileName { get; set; }

        public static byte[] GetBytes(StaticTiledata obj)
        {
            var data = new byte[37];

            BitConverter.GetBytes((uint)obj.Flags).CopyTo(data, 0);
            BitConverter.GetBytes(obj.Weight).CopyTo(data, 4);
            BitConverter.GetBytes(obj.Quality).CopyTo(data, 5);
            BitConverter.GetBytes(obj.MiscData).CopyTo(data, 6);
            BitConverter.GetBytes(obj.Unknown1).CopyTo(data, 8);
            BitConverter.GetBytes(obj.Quantity).CopyTo(data, 9);
            BitConverter.GetBytes(obj.AnimationID).CopyTo(data, 10);
            BitConverter.GetBytes(obj.Unknown2).CopyTo(data, 12);
            BitConverter.GetBytes(obj.Hue).CopyTo(data, 13);
            BitConverter.GetBytes(obj.StackOff).CopyTo(data, 14);
            BitConverter.GetBytes(obj.Value).CopyTo(data, 15);
            BitConverter.GetBytes(obj.Height).CopyTo(data, 16);
            var trimmedTileName = obj.TileName.Trim();
            if (trimmedTileName.Length > 20)
                trimmedTileName.Substring(0, 20);
            Encoding.ASCII.GetBytes(trimmedTileName).CopyTo(data, 17);

            return data;
        }

        public static StaticTiledata Load(int ID, byte[] data)
        {
            return new StaticTiledata
            {
                ID = ID,
                Flags = (Flags)BitConverter.ToInt32(data, 0),
                Weight = data.Skip(4).Take(1).ToArray()[0],
                Quality = data.Skip(5).Take(1).ToArray()[0],
                MiscData = BitConverter.ToUInt16(data, 6),
                Unknown1 = data.Skip(8).Take(1).ToArray()[0],
                Quantity = data.Skip(9).Take(1).ToArray()[0],
                AnimationID = BitConverter.ToUInt16(data, 10),
                Unknown2 = data.Skip(12).Take(1).ToArray()[0],
                Hue = data.Skip(13).Take(1).ToArray()[0],
                StackOff = data.Skip(14).Take(1).ToArray()[0],
                Value = data.Skip(15).Take(1).ToArray()[0],
                Height = data.Skip(16).Take(1).ToArray()[0],
                TileName = Encoding.ASCII.GetString(data, 17, 20).Replace('\u0000', ' ').Trim()
            };
        }
    }
}
