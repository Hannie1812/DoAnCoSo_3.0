﻿namespace WebTimNguoiThatLac.Extensions
{
    public static class FloatArrayExtensions
    {
        public static byte[] ToByteArray(this float[] floats)
        {
            if (floats == null) return null;
            byte[] bytes = new byte[floats.Length * sizeof(float)];
            Buffer.BlockCopy(floats, 0, bytes, 0, bytes.Length);
            return bytes;
        }

        public static float[] ToFloatArray(this byte[] bytes)
        {
            if (bytes == null) return null;
            float[] floats = new float[bytes.Length / sizeof(float)];
            Buffer.BlockCopy(bytes, 0, floats, 0, bytes.Length);
            return floats;
        }
    }
}
