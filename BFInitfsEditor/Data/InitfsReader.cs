using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using BFInitfsEditor.Extension;
using BFInitfsEditor.Model;
using BFInitfsEditor.Service;

// set short alias
using CONST = BFInitfsEditor.Data.InitfsConstants;

namespace BFInitfsEditor.Data
{
    public class InitfsReader : IInitfsReader
    {
        public static IInitfsReader GetInstance() => new InitfsReader();

        private readonly IXOR _xor;
        private readonly ILeb128 _leb128;
        private static readonly byte[] DICE_HEADER1 = { 0x00, 0xD1, 0xCE, 0x00 };
        private static readonly byte[] DICE_HEADER2 = { 0x00, 0xD1, 0xCE, 0x01 };
        private const int DICE_HEADER_SIZE = 4;

        // private ctor 
        private InitfsReader()
        {
            _xor = XOR.GetInstance();
            _leb128 = Leb128.GetInstance();
        }

        #region IInitfsReader

        public Entity Read(Stream source)
        {
            var entity = new Entity();
            var data = entity.Data;

            using (var reader = new BinaryReader(source))
            {
                // read DICE header
                entity.Header = reader.ReadBytes(DICE_HEADER_SIZE);
                reader.Seek(CONST.POST_HEADER_SPACE, SeekOrigin.Current); // skip zero bytes

                if (! _ValidateHeader(entity.Header)) throw new InvalidOperationException("File header was not recognized.");

                // read hash 'x' + hash + 'x'
                entity.Hash = reader.ReadBytes(CONST.FULL_HASH_SIZE);
                reader.Seek(CONST.POST_HASH_SPACE, SeekOrigin.Current); // skip zero bytes

                // read XOR key
                entity.EncryptionKey = reader.ReadBytes(CONST.MAGIC_SIZE);
                reader.Seek(CONST.POST_MAGIC_SPACE, SeekOrigin.Current); // skip zero bytes

                // decrypt data
                var encryptedData = reader.ReadAllBytes();
                var decryptedData = _xor.Decrypt(encryptedData, entity.EncryptionKey);

                // parse payload
                var position = 0;
                var dataEntries = new List<FileEntry>();
                
                // read unknown data
                data.DataSize = _leb128.ReadLEB128Unsigned(decryptedData, ref position);

                // read all file entries
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

        private static bool _ValidateHeader(IReadOnlyList<byte> header) =>
            header.SequenceEqual(DICE_HEADER1) || header.SequenceEqual(DICE_HEADER2);

        private static string _ReadString(IReadOnlyList<byte> data, ref int position)
        {
            var str = new StringBuilder();

            while (data[position] != 0)
                str.Append((char)data[position++]);

            ++position; // skip end of string
            return str.ToString();
        }

        private FileEntry _ReadEntry(byte[] data, ref int position)
        {
            // end of data reached
            if (data[position] == 0) return null;

            var entry = new FileEntry { ID = position };
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
                    case CONST.TYPE_STRING:
                    
                        // read string data size
                        entry.FilePathSize = _leb128.ReadLEB128Unsigned(data, ref position);
                        // read file path
                        entry.FilePath = _ReadString(data, ref position);

                        break;
                    case CONST.TYPE_PAYLOAD:

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

            // move position
            position += CONST.ENTRY_EOF_SIZE;

            return entry;
        }

        #endregion
    }
}