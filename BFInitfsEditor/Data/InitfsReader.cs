using System;
using System.IO;
using BFInitfsEditor.Model;
using BFInitfsEditor.Service;

namespace BFInitfsEditor.Data
{
    public class InitfsReader : IInitfsReader
    {
        #region IInitfsReader

        public InitfsEntity Read(Stream source)
        {
            using (var reader = new BinaryReader(source))
            {
                var header = reader.ReadInt32();

                throw new NotImplementedException();
            }
        }

        #endregion

        #region Private helpers

        private InitfsEntity _BuildEntity() => new InitfsEntity();

        #endregion
    }
}