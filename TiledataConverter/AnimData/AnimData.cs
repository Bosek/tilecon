using System;
using System.Linq;

namespace TiledataConverter.AnimData
{
    class AnimData
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

        public sbyte[] FrameData { get; set; }
        public byte Unknown { get; set; }
        public byte FrameCount { get; set; }
        public byte FrameInterval { get; set; }
        public byte FrameStart { get; set; }

        public static AnimData Load(int ID, byte[] data)
        {
            return new AnimData
            {
                ID = ID,
                FrameData = data.Take(64).Select(b => (sbyte)b).ToArray(),
                Unknown = data[65],
                FrameCount = data[66],
                FrameInterval = data[67],
                FrameStart = data[68]
            };
        }
    }
}
