﻿using System.IO;

namespace BFInitfsEditor.Extension
{
    public static class BinaryReaderExtension
    {
        public static void Seek(this BinaryReader reader, int offset, SeekOrigin origin) =>
            reader.BaseStream.Seek(offset, origin);

        /// <summary>
        /// https://stackoverflow.com/questions/8613187/an-elegant-way-to-consume-all-bytes-of-a-binaryreader/8613300#8613300
        /// </summary>
        public static byte[] ReadAllBytes(this BinaryReader reader)
        {
            const int bufferSize = 4096;
            using (var ms = new MemoryStream())
            {
                var buffer = new byte[bufferSize];
                int count;
                while ((count = reader.Read(buffer, 0, buffer.Length)) != 0)
                    ms.Write(buffer, 0, count);
                return ms.ToArray();
            }

        }
    }
}