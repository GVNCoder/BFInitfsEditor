using System;
using System.IO;
using BFInitfsEditor.Model;

namespace BFInitfsEditor.Service
{
    public interface IInitfsReader
    {
        void Read(Stream source, out InitfsEntity entity);
    }
}