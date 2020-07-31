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
                data.DataSize = _leb128.ReadLEB128Unsigned(decryptedData, ref position);

                
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
            ++position;
            return str.ToString();
        }

        private bool _ReadBlocks(byte[] data, ref int position)
        {
            var maybeContentSize = _leb128.ReadLEB128Unsigned(data, ref position);
            position += 2; // skip two unknown bytes
            var typeName = _ReadString(data, ref position);
            var structSize = _leb128.ReadLEB128Unsigned(data, ref position);

            string ts, ts1, fname = string.Empty;
            while (data[position] != 0)
            {
                var sType = data[position++];
                ts = _ReadString(data, ref position);

                switch (sType)
                {
                    case InitfsConstants.TYPE_STRING: //string
                        _leb128.ReadLEB128Unsigned(data, ref position); //skip string size
                        ts1 = _ReadString(data, ref position);

                        if (ts == "name")
                        {
                            fname = "Z_" + ts1;
                            fname = fname.Replace('/', '-');
                        }
                        break;
                    case InitfsConstants.TYPE_PAYLOAD: //payload
                    {
                        var tsize = _leb128.ReadLEB128Unsigned(data, ref position);
                        using (var file = File.Create(fname))
                        {
                            file.Write(data, position, (int) tsize);
                        }
                        position += (int) tsize;
                        break;
                    }
                    default:
                        return false;
                }
            }
            position += 2; //end zeros
            return true;
        }

        #endregion
    }
}