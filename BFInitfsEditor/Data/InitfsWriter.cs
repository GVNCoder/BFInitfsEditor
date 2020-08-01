using System.IO;
using System.Linq;
using System.Text;

using BFInitfsEditor.Extension;
using BFInitfsEditor.Model;
using BFInitfsEditor.Service;

namespace BFInitfsEditor.Data
{
    public class InitfsWriter : IInitfsWriter
    {
        public static IInitfsWriter GetInstance() => new InitfsWriter();

        private readonly IXOR _xor;
        private readonly ILeb128 _leb128;
        private readonly byte[] _eof = { 0x00, 0x00 };
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
            var entityDataRef = entity.Data;
            byte[] data = null;

            // prepare output initfs_ file data
            using (var dataStream = new MemoryStream())
            {
                // write header + 4 zero bytes
                dataStream.Write(entity.Header, 0, entity.Header.Length);
                dataStream.Write(_zeros, 0, Entity.AfterHeaderZero);

                // write hash + 30 zero bytes
                dataStream.Write(entity.Hash, 0, entity.Hash.Length);
                dataStream.Write(_zeros, 0, Entity.AfterHashZero);

                // write encryption key + 3 zero bytes
                dataStream.Write(entity.EncryptionKey, 0, entity.EncryptionKey.Length);
                dataStream.Write(_zeros, 0, Entity.AfterEncryptionKeyZero);
                
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
                        var typeString = Encoding.ASCII.GetBytes(entry.Type).Concat(new byte[] { 0x00 }).ToArray();
                        entriesStream.Write(typeString, 0, typeString.Length);

                        // write entry struct size + entry header
                        var structSize = _leb128.BuildLEB128Unsigned(entry.StructSize);
                        entriesStream.Write(structSize, 0, structSize.Length);
                        entriesStream.Write(entry.UnknownData, 0, entry.UnknownData.Length);
                        
                        // write file size + file data + eof
                        var fileSize = _leb128.BuildLEB128Unsigned(entry.FileSize);
                        entriesStream.Write(fileSize, 0, fileSize.Length);
                        entriesStream.Write(entry.FileData, 0, entry.FileData.Length);
                        entriesStream.Write(_eof, 0, _eof.Length);
                    }

                    entriesStream.Write(new byte[] { 0x00 }, 0, 1);

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

        #endregion

        #region Private helpers

        

        #endregion
    }
}