using System;
using System.IO;
using System.Text;
using Ionic.Zlib;

namespace Quaver.Helpers
{
    public static class GzipHelper
    {
        /// <summary>
        ///     Compresses a string
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string Compress(string text)
        {
            if (string.IsNullOrEmpty(text))
                return "";

            byte[] buffer = Encoding.UTF8.GetBytes(text);
            MemoryStream ms = new MemoryStream();
            using (GZipStream zip = new GZipStream(ms, CompressionMode.Compress, true))
            {
                zip.Write(buffer, 0, buffer.Length);
            }

            ms.Position = 0;
            MemoryStream outStream = new MemoryStream();

            byte[] compressed = new byte[ms.Length];
            ms.Read(compressed, 0, compressed.Length);

            byte[] gzBuffer = new byte[compressed.Length + 4];
            Buffer.BlockCopy(compressed, 0, gzBuffer, 4, compressed.Length);
            Buffer.BlockCopy(BitConverter.GetBytes(buffer.Length), 0, gzBuffer, 0, 4);
            return Convert.ToBase64String(gzBuffer);
        }

        /// <summary>
        ///     Decompresses a gzip string.
        /// </summary>
        /// <param name="compressedText"></param>
        /// <returns></returns>
        public static string Decompress(string compressedText)
        {
            if (string.IsNullOrEmpty(compressedText))
                return "";

            byte[] gzBuffer = Convert.FromBase64String(compressedText);
            using (MemoryStream ms = new MemoryStream())
            {
                int msgLength = BitConverter.ToInt32(gzBuffer, 0);
                ms.Write(gzBuffer, 4, gzBuffer.Length - 4);

                byte[] buffer = new byte[msgLength];

                ms.Position = 0;
                using (GZipStream zip = new GZipStream(ms, CompressionMode.Decompress))
                {
                    zip.Read(buffer, 0, buffer.Length);
                }

                return Encoding.UTF8.GetString(buffer);
            }
        }
    }
}
