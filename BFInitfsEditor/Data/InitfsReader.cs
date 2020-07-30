using System;
using System.IO;
using BFInitfsEditor.Extension;
using BFInitfsEditor.Model;
using BFInitfsEditor.Service;

namespace BFInitfsEditor.Data
{
    public class InitfsReader : IInitfsReader
    {
        public static IInitfsReader GetInstance() => new InitfsReader();

        // private ctor 
        private InitfsReader() { }

        #region IInitfsReader

        public InitfsEntity Read(Stream source)
        {
            using (var reader = new BinaryReader(source))
            {
                reader.Seek(4, SeekOrigin.Begin); // skip DICE header
                reader.SkipZeroBytes();
                reader.Seek(1, SeekOrigin.Current); // skip 'x' begin indicator
                reader.Seek(InitfsConstants.HASH_SIZE, SeekOrigin.Current);
                reader.Seek(1, SeekOrigin.Current); // skip 'x' end indicator
                reader.SkipZeroBytes();

                var encryptionKey = _ReadKey(reader); // read XOR key

                reader.SkipZeroBytes();

                var encryptedData = _ReadData(reader);

                // TODO: Decrypt data here

                var contentSize = reader.ReadLEB128Unsigned(); // should be 19980929

                throw new NotImplementedException();
            }
        }

        #endregion

        #region Private helpers

        private InitfsEntity _BuildEntity() => new InitfsEntity();

        private bool _ValidateHeader(int header) =>
            InitfsConstants.DICE_HEADER_TYPE1 == header || InitfsConstants.DICE_HEADER_TYPE2 == header;

        private byte[] _ReadKey(BinaryReader reader) => reader.ReadBytes(InitfsConstants.MAGIC_SIZE);

        private byte[] _ReadData(BinaryReader reader) => reader.ReadAllBytes();

        #endregion
    }
}