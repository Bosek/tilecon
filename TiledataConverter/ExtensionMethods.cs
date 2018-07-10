using System;

namespace TiledataConverter
{
    public static class ExtensionMethods
    {
        public static byte[] GetSubArray(this byte[] sourceArray, int index, int length)
        {
            var destinationArray = new byte[length];
            Array.Copy(sourceArray, index, destinationArray, 0, length);
            return destinationArray;
        }
    }
}
