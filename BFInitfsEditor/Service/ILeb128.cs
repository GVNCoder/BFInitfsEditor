﻿namespace BFInitfsEditor.Service
{
    public interface ILeb128
    {
        byte[] BuildLEB128Signed(long value);
        byte[] BuildLEB128Unsigned(ulong value);

        long ReadLEB128Signed(byte[] buffer, ref int beginPosition);
        ulong ReadLEB128Unsigned(byte[] buffer, ref int beginPosition);
    }
}