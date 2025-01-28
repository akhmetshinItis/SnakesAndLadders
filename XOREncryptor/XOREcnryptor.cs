using System.Text;

namespace XOREncryptor
{
    public static class XOREncryptor
    {
        private static readonly string Key = "hard_key";

        public static byte[] Encrypt(byte[] data)
        {
            return XOR(data);
        }

        public static byte[] Decrypt(byte[] data)
        {
            return XOR(data);
        }

        private static byte[] XOR(byte[] data)
        {
            var result = new byte[data.Length];
            var keyBytes = Encoding.UTF8.GetBytes(Key);

            for (int i = 0; i < data.Length; i++)
            {
                result[i] = (byte)(data[i] ^ keyBytes[i % keyBytes.Length]);
            }

            return result;
        }
    }
}