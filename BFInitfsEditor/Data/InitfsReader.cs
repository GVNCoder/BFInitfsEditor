using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using BFInitfsEditor.Extension;
using BFInitfsEditor.Model;
using BFInitfsEditor.Service;

namespace BFInitfsEditor.Data
{
    public class InitfsReader : IInitfsReader
    {
        public static IInitfsReader GetInstance() => new InitfsReader();

        private readonly IXOR _xor;
        private readonly ILeb128 _leb128;

        // private ctor 
        private InitfsReader()
        {
            _xor = XOR.GetInstance();
            _leb128 = Leb128.GetInstance();
        }

        #region IInitfsReader

        public Entity Read(Stream source)
        {
            var entity = _BuildEntity();
            var data = entity.Data;

            using (var reader = new BinaryReader(source))
            {
                // read DICE header
                entity.Header = reader.ReadBytes(4);
                reader.Seek(4, SeekOrigin.Current); // skip 4 zero bytes

                // read initfs_ hash 'x' + hash + 'x'
                entity.Hash = reader.ReadBytes(InitfsConstants.HASH_SIZE + 2);
                reader.Seek(30, SeekOrigin.Current); // skip 30 zero bytes

                // read XOR key
                entity.EncryptionKey = _ReadKey(reader);
                reader.Seek(3, SeekOrigin.Current); // skip 3 zero bytes

                // decrypt data
                var encryptedData = _ReadData(reader);
                var decryptedData = _xor.Decrypt(encryptedData, entity.EncryptionKey);

                // parse payload
                var position = 0;
                var dataEntries = new List<FileEntry>();
                
                // read unknown data
                data.DataSize = _leb128.ReadLEB128Unsigned(decryptedData, ref position);

                while (position < decryptedData.Length)
                {
                    var entry = _ReadEntry(decryptedData, ref position);
                    if (entry == null) break;

                    dataEntries.Add(entry);
                }

                data.Entries = dataEntries.ToArray();
            }

            return entity;
        }

        #endregion

        #region Private helpers

        private Entity _BuildEntity() => new Entity();

        private bool _ValidateHeader(int header) =>
            InitfsConstants.DICE_HEADER_TYPE1 == header || InitfsConstants.DICE_HEADER_TYPE2 == header;

        private static byte[] _ReadKey(BinaryReader reader) => reader.ReadBytes(InitfsConstants.MAGIC_SIZE);

        private static byte[] _ReadData(BinaryReader reader) => reader.ReadAllBytes();

        private static string _ReadString(IReadOnlyList<byte> data, ref int position)
        {
            var str = new StringBuilder();
            while (data[position] != 0x00)
                str.Append((char)data[position++]);
            ++position; // skip end of string
            return str.ToString();
        }

        private FileEntry _ReadEntry(byte[] data, ref int position)
        {
            if (data[position] == 0) return null;

            var entry = new FileEntry();
            var localPosition = 0;

            // read unknown data
            entry.UnknownSize = _leb128.ReadLEB128Unsigned(data, ref position);
            // read unknown header part
            entry.UnknownHeaderPart = new[] { data[position++], data[position++] };
            // read entry type name
            entry.Type = _ReadString(data, ref position);
            // read struct size
            entry.StructSize = _leb128.ReadLEB128Unsigned(data, ref position);
            
            // fix local position
            localPosition = position;

            // read file entry
            while (data[position] != 0)
            {
                // read next data type and name
                var dataType = data[position++];
                var dataName = _ReadString(data, ref position);

                switch (dataType)
                {
                    case InitfsConstants.TYPE_STRING:
                    
                        // read string data size
                        entry.FilePathSize = _leb128.ReadLEB128Unsigned(data, ref position);
                        // read file path
                        entry.FilePath = _ReadString(data, ref position);

                        break;
                    case InitfsConstants.TYPE_PAYLOAD:

                        var readDataSize = position - localPosition;
                        // read data block for initfs_ write
                        entry.UnknownData = new byte[readDataSize];
                        Array.Copy(data, (long) localPosition, entry.UnknownData, 0, readDataSize);

                        // read file size
                        entry.FileSize = _leb128.ReadLEB128Unsigned(data, ref position);
                        // read file content
                        entry.FileData = new byte[entry.FileSize];
                        Array.Copy(data, (long) position, entry.FileData, 0, (int) entry.FileSize);

                        // move position
                        position += (int) entry.FileSize;

                        break;
                    default: return null;
                }
            }

            // move position => skip 2 zeros
            position += 2;

            return entry;
        }

        #endregion
    }
}