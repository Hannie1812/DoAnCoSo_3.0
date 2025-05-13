using System;

namespace WebTimNguoiThatLac.Helpers
{
    public static class FaceRecognitionHelper
    {
        public static float CalculateEuclideanDistance(float[] v1, float[] v2)
        {
            if (v1 == null || v2 == null || v1.Length != v2.Length)
                return float.MaxValue;

            float sum = 0;
            for (int i = 0; i < v1.Length; i++)
            {
                float diff = v1[i] - v2[i];
                sum += diff * diff;
            }
            return (float)Math.Sqrt(sum);
        }

        public static byte[] ToByteArray(this float[] floatArray)
        {
            byte[] byteArray = new byte[floatArray.Length * 4];
            Buffer.BlockCopy(floatArray, 0, byteArray, 0, byteArray.Length);
            return byteArray;
        }


        public static float[] ToFloatArray(this byte[] byteArray)
        {
            if (byteArray == null || byteArray.Length % 4 != 0)
                return null;

            float[] floatArray = new float[byteArray.Length / 4];
            Buffer.BlockCopy(byteArray, 0, floatArray, 0, byteArray.Length);
            return floatArray;
        }
        
    }
}