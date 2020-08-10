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
    public class InitfsWriter : IInitfsWriter
    {
        public static IInitfsWriter GetInstance() => new InitfsWriter();

        private const int __EOF_SIZE = 2; // end of file
        private const int __EOP_SIZE = 1; // end of payload

        private readonly IXOR _xor;
        private readonly ILeb128 _leb128;
        private readonly byte[] _zeros = new byte[30];

        // private ctor 
        private InitfsWriter()
        {
            _xor = XOR.GetInstance();
            _leb128 = Leb128.GetInstance();
            _zeros.Fill<byte>(0x00);
        }

        #region IInitfsWriter

        public void Write(Stream target, Entity entity)
        {
            if (entity.IsEncrypted) _WriteEncrypted(target, entity);
            else _WriteDecrypted(target, entity);
        }

        #endregion

        #region Private helpers

        private static byte[] _GetStringBytes(string source)
            => Encoding.ASCII.GetBytes(source).Concat(new byte[] { 0x00 }).ToArray();

        private void _WriteEncrypted(Stream target, Entity entity)
        {
            var entityDataRef = entity.Data;
            byte[] data = null;

            // prepare output initfs_ file data
            using (var dataStream = new MemoryStream())
            {
                // write header + 4 zero bytes
                dataStream.Write(entity.Header, 0, entity.Header.Length);
                dataStream.Write(_zeros, 0, CONST.POST_HEADER_SPACE);

                // write hash + 30 zero bytes
                dataStream.Write(entity.Hash, 0, entity.Hash.Length);
                dataStream.Write(_zeros, 0, CONST.POST_HASH_SPACE);

                // write encryption key + 3 zero bytes
                dataStream.Write(entity.EncryptionKey, 0, entity.EncryptionKey.Length);
                dataStream.Write(_zeros, 0, CONST.POST_MAGIC_SPACE);

                byte[] entriesStreamData = null;

                // prepare file entries data
                using (var entriesStream = new MemoryStream())
                {
                    // write full data size
                    var dataSize = _leb128.BuildLEB128Unsigned(entityDataRef.DataSize);
                    entriesStream.Write(dataSize, 0, dataSize.Length);

                    foreach (var entry in entityDataRef.Entries)
                    {
                        // write unknown data
                        var unknownSize = _leb128.BuildLEB128Unsigned(entry.UnknownSize);
                        entriesStream.Write(unknownSize, 0, unknownSize.Length);
                        entriesStream.Write(entry.UnknownHeaderPart, 0, entry.UnknownHeaderPart.Length);

                        // write type name string
                        var typeString = _GetStringBytes(entry.Type);
                        entriesStream.Write(typeString, 0, typeString.Length);

                        // write entry struct size + entry header
                        var structSize = _leb128.BuildLEB128Unsigned(entry.StructSize);
                        entriesStream.Write(structSize, 0, structSize.Length);
                        entriesStream.Write(entry.UnknownData, 0, entry.UnknownData.Length);

                        // write file size + file data + eof
                        var fileSize = _leb128.BuildLEB128Unsigned(entry.FileSize);
                        entriesStream.Write(fileSize, 0, fileSize.Length);
                        entriesStream.Write(entry.FileData, 0, entry.FileData.Length);
                        entriesStream.Write(_zeros, 0, __EOF_SIZE);
                    }

                    entriesStream.Write(_zeros, 0, __EOP_SIZE);

                    // extract recorded data
                    entriesStreamData = entriesStream.ToArray();
                }

                // encrypt and write entries data
                var encryptedEntries = _xor.Encrypt(entriesStreamData, entity.EncryptionKey);
                dataStream.Write(encryptedEntries, 0, encryptedEntries.Length);

                // extract recorded data
                data = dataStream.ToArray();
            }

            // write constructed initfs_ to file
            using (var writer = new BinaryWriter(target))
            {
                writer.Write(data);
            }
        }

        private void _WriteDecrypted(Stream target, Entity entity)
        {
            var entityDataRef = entity.Data;
            byte[] data = null;

            // prepare output initfs_ file data
            using (var dataStream = new MemoryStream())
            {
                byte[] entriesStreamData = null;

                // prepare file entries data
                using (var entriesStream = new MemoryStream())
                {
                    // write full data size
                    var dataSize = _leb128.BuildLEB128Unsigned(entityDataRef.DataSize);
                    entriesStream.Write(dataSize, 0, dataSize.Length);

                    foreach (var entry in entityDataRef.Entries)
                    {
                        // write unknown data
                        var unknownSize = _leb128.BuildLEB128Unsigned(entry.UnknownSize);
                        entriesStream.Write(unknownSize, 0, unknownSize.Length);
                        entriesStream.Write(entry.UnknownHeaderPart, 0, entry.UnknownHeaderPart.Length);

                        // write type name string
                        var typeString = _GetStringBytes(entry.Type);
                        entriesStream.Write(typeString, 0, typeString.Length);

                        // write entry struct size + entry header
                        var structSize = _leb128.BuildLEB128Unsigned(entry.StructSize);
                        entriesStream.Write(structSize, 0, structSize.Length);
                        entriesStream.Write(entry.UnknownData, 0, entry.UnknownData.Length);

                        // write file size + file data + eof
                        var fileSize = _leb128.BuildLEB128Unsigned(entry.FileSize);
                        entriesStream.Write(fileSize, 0, fileSize.Length);
                        entriesStream.Write(entry.FileData, 0, entry.FileData.Length);
                        entriesStream.Write(_zeros, 0, __EOF_SIZE);
                    }

                    entriesStream.Write(_zeros, 0, __EOP_SIZE);

                    // extract recorded data
                    entriesStreamData = entriesStream.ToArray();
                }

                // encrypt and write entries data
                dataStream.Write(entriesStreamData, 0, entriesStreamData.Length);

                // extract recorded data
                data = dataStream.ToArray();
            }

            // write constructed initfs_ to file
            using (var writer = new BinaryWriter(target))
            {
                writer.Write(data);
            }
        }

        #endregion
    }
}