using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace cdc.AssetWorkflow.Editor
{
    public static class Crypto
    {
        public static void FromFileToMD5(ref string output, string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException(filePath);
            using (FileStream stream = File.OpenRead(filePath))
            {
                using (MD5 md5 = MD5.Create())
                {
                    byte[] bytes = md5.ComputeHash(stream);
                    output = BitConverter.ToString(bytes).Replace("-", "").ToLowerInvariant();
                }
            }
        }

        public static void FromBytesToMD5(ref string output, byte[] bytes)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] hashBytes = md5.ComputeHash(bytes);
                output = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
            }
        }

        public static void FromStringToMD5(ref string output)
        {
            if (string.IsNullOrEmpty(output))
                throw new ArgumentNullException();
            byte[] bytes = Encoding.UTF8.GetBytes(output);
            FromBytesToMD5(ref output, bytes);
        }
    }
}