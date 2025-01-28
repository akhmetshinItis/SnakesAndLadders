namespace XProtocol
{
    public static class XProtocolEncryptor
    {
        public static byte[] Encrypt(byte[] data)
        {
            return XOREncryptor.XOREncryptor.Encrypt(data);
        }

        public static byte[] Decrypt(byte[] data)
        {
            return XOREncryptor.XOREncryptor.Decrypt(data);
        }
    }
}