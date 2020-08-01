namespace BFInitfsEditor.Data
{
    public static class InitfsConstants
    {
        public const int HASH_SIZE = 256;
        public const int FULL_HASH_SIZE = 258; // size with two 'x'
        public const int TYPE_STRING = 7;
        public const int TYPE_PAYLOAD = 19;
        public const int MAGIC_SIZE = 257;
        public const int POST_HEADER_SPACE = 4;
        public const int POST_HASH_SPACE = 30;
        public const int POST_MAGIC_SPACE = 3;
        public const int ENTRY_EOF_SIZE = 2;
    }
}