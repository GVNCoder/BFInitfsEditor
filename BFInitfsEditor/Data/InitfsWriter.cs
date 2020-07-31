using System;
using System.IO;

using BFInitfsEditor.Model;
using BFInitfsEditor.Service;

namespace BFInitfsEditor.Data
{
    public class InitfsWriter : IInitfsWriter
    {
        public static IInitfsWriter GetInstance() => new InitfsWriter();

        private readonly IXOR _xor;
        private readonly ILeb128 _leb128;

        // private ctor 
        private InitfsWriter()
        {
            _xor = XOR.GetInstance();
            _leb128 = Leb128.GetInstance();
        }

        #region IInitfsWriter

        public void Write(Stream target, Entity entity)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Private helpers

        

        #endregion
    }
}