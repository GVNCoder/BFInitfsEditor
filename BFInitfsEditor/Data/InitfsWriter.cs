using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

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

        // private ctor 
        private InitfsWriter()
        {
            _xor = XOR.GetInstance();
            _leb128 = Leb128.GetInstance();
        }

        #region IInitfsWriter

        public void Write(Stream target, Entity entity)
        {
            var entityData = entity.Data;
            byte[] data = null;

            // prepare files for writing
            using (var dataBlock = new MemoryStream())
            {
                // write full data size
                var dataSize = _leb128.BuildLEB128Unsigned(entityData.DataSize);
                dataBlock.Write(dataSize, 0, dataSize.Length);

                // handle all file entires
                foreach (var entry in entityData.Entries)
                {
                    byte[] entryBytes = null;

                    using (var memorySteam = new MemoryStream())
                    {
                        // write unknown data
                        var unknownSize = _leb128.BuildLEB128Unsigned(entry.UnknownSize);
                        memorySteam.Write(unknownSize, 0, unknownSize.Length);
                        memorySteam.Write(entry.UnknownHeaderPart, 0, entry.UnknownHeaderPart.Length);

                        // write type name string
                        var typeString = Encoding.ASCII.GetBytes(entry.Type).Concat(new byte[] { 0x00 }).ToArray();
                        memorySteam.Write(typeString, 0, typeString.Length);

                        // write entry struct size + entry header
                        var structSize = _leb128.BuildLEB128Unsigned(entry.StructSize);
                        memorySteam.Write(structSize, 0, structSize.Length);
                        memorySteam.Write(entry.UnknownData, 0, entry.UnknownData.Length);
                        
                        // write file size + file data + eof
                        var fileSize = _leb128.BuildLEB128Unsigned(entry.FileSize);
                        memorySteam.Write(fileSize, 0, fileSize.Length);
                        memorySteam.Write(entry.FileData, 0, entry.FileData.Length);
                        memorySteam.Write(_eof, 0, _eof.Length);

                        // extract payload bytes
                        entryBytes = memorySteam.ToArray();
                    }

                    // write entry bytes
                    dataBlock.Write(entryBytes, 0, entryBytes.Length);
                }

                // extract payload bytes
                data = dataBlock.ToArray().Concat(new byte[] { 0x00 }).ToArray();
            }

            // write constructed initfs_ to file
            using (var writer = new BinaryWriter(target))
            {
                // TODO: Write here
                writer.Write(data);
            }
        }

        #endregion

        #region Private helpers

        

        #endregion
    }
}