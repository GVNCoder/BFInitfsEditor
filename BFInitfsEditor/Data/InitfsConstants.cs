namespace BFInitfsEditor.Data
{
    public static class InitfsConstants
    {
        public const int MAGIC_SIZE = 257;
        public const int MAGIC_XOR = 0x7B;
        public const int DICE_HEADER_TYPE1 = 0x00D1CE00;
        public const int DICE_HEADER_TYPE2 = 0x00D1CE01;
        public const int DICE_HEADER_SIZE = 4;
        public const int HASH_SIZE = 256;
    }
}