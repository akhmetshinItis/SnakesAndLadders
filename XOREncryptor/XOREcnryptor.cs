using System.Text;

namespace XOREncryptor
{
    public static class XOREncryptor
    {
        private static readonly string Key = "hard_key";
        private static byte[] KeyBytes = Encoding.UTF8.GetBytes(Key);

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

            for (int i = 0; i < data.Length; i++)
            {
                result[i] = data[i];
            }
            return result;
        }
    }
}