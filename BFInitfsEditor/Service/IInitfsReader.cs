using System.IO;
using BFInitfsEditor.Model;

namespace BFInitfsEditor.Service
{
    public interface IInitfsReader
    {
        Entity ReadEncrypted(Stream source);
        Entity ReadDecrypted(Stream source);
    }
}