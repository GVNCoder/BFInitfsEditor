using System;
using System.IO;
using BFInitfsEditor.Model;

namespace BFInitfsEditor.Service
{
    public interface IInitfsReader
    {
        InitfsEntity Read(Stream source);
    }
}