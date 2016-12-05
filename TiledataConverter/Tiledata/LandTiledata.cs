using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace TiledataConverter.Tiledata
{
    class LandTiledata
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
                return Enum.GetValues(typeof(Flags)).Cast<Flags>()
                    .Where(flag => Flags.HasFlag(flag))
                    .Select(flag => flag.ToString())
                    .ToArray();
            }
            set
            {
                value.ToList().ForEach(flag => Flags |= (Flags)Enum.Parse(typeof(Flags), flag, true));
            }
        }

        public ushort TextureID { get; set; }

        public string TileName { get; set; }

        public static byte[] GetBytes(LandTiledata obj)
        {
            var data = new byte[26];

            BitConverter.GetBytes((uint)obj.Flags).CopyTo(data, 0);
            BitConverter.GetBytes(obj.TextureID).CopyTo(data, 4);
            var trimmedTileName = obj.TileName.Trim();
            if (trimmedTileName.Length > 20)
                trimmedTileName.Substring(0, 20);
            Encoding.ASCII.GetBytes(trimmedTileName).CopyTo(data, 6);

            return data;
        }
        public static LandTiledata Load(int ID, byte[] data)
        {
            return new LandTiledata
            {
                ID = ID,
                Flags = (Flags)BitConverter.ToInt32(data, 0),
                TextureID = BitConverter.ToUInt16(data, 4),
                TileName = Encoding.ASCII.GetString(data, 6, 20).Replace('\u0000', ' ').Trim()
            };
        }
    }
}
