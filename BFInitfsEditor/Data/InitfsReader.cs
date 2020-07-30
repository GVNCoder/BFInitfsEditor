using System;
using System.IO;
using BFInitfsEditor.Model;
using BFInitfsEditor.Service;

namespace BFInitfsEditor.Data
{
    public class InitfsReader : IInitfsReader
    {
        #region IInitfsReader

        public void Read(Stream source, out InitfsEntity entity)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}