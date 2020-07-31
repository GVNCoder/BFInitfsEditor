using System.IO;
using BFInitfsEditor.Model;

namespace BFInitfsEditor.Service
{
    public interface IInitfsWriter
    {
        void Write(Stream target, Entity entity);
    }
}