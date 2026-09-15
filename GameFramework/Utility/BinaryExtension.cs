//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using System;
using System.IO;

/// <summary>
/// 对 BinaryReader 和 BinaryWriter 的扩展方法。
/// </summary>
public static class BinaryExtension
{
    /// <summary>
    /// 从二进制流读取编码后的 32 位有符号整数。
    /// </summary>
    public static int Read7BitEncodedInt32(this BinaryReader binaryReader)
    {
        int value = 0;
        int shift = 0;
        byte b;
        do
        {
            if (shift >= 35)
            {
                throw new InvalidOperationException("7 bit encoded int is invalid.");
            }

            b = binaryReader.ReadByte();
            value |= (b & 0x7f) << shift;
            shift += 7;
        } while ((b & 0x80) != 0);

        return value;
    }

    /// <summary>
    /// 向二进制流写入编码后的 32 位有符号整数。
    /// </summary>
    public static void Write7BitEncodedInt32(this BinaryWriter binaryWriter, int value)
    {
        uint num = (uint)value;
        while (num >= 0x80)
        {
            binaryWriter.Write((byte)(num | 0x80));
            num >>= 7;
        }

        binaryWriter.Write((byte)num);
    }

    /// <summary>
    /// 从二进制流读取编码后的 64 位有符号整数。
    /// </summary>
    public static long Read7BitEncodedInt64(this BinaryReader binaryReader)
    {
        long value = 0L;
        int shift = 0;
        byte b;
        do
        {
            if (shift >= 70)
            {
                throw new InvalidOperationException("7 bit encoded int is invalid.");
            }

            b = binaryReader.ReadByte();
            value |= (b & 0x7fL) << shift;
            shift += 7;
        } while ((b & 0x80) != 0);

        return value;
    }

    /// <summary>
    /// 向二进制流写入编码后的 64 位有符号整数。
    /// </summary>
    public static void Write7BitEncodedInt64(this BinaryWriter binaryWriter, long value)
    {
        ulong num = (ulong)value;
        while (num >= 0x80)
        {
            binaryWriter.Write((byte)(num | 0x80));
            num >>= 7;
        }

        binaryWriter.Write((byte)num);
    }
}
