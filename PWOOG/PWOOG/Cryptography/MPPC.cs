using System;
using System.Collections.Generic;

namespace PWOOG
{
    class MPPC
    {
        UInt32 m_Code1;
        UInt32 m_Code2;
        UInt32 m_Stage;
        UInt32 m_Shift;
        byte m_PackedOffset;
        public readonly List<byte> m_Packed = new List<byte>();
        public readonly List<byte> m_Unpacked = new List<byte>();

        public MPPC()
        {
            m_PackedOffset = 0;
            m_Stage = 0;
        }

        bool HasBits(UInt32 Count)
        {
            return (m_Packed.Count * 8 - m_PackedOffset) >= Count;
        }

        public List<byte> AddByte(byte InB)
        {
            m_Packed.Add(InB);
            var UnpackedChunk = new List<byte>();

            for (; ; )
            {
                if (m_Stage == 0)
                {
                    if (HasBits(4))
                    {
                        if (GetPackedBits(1) == 0)
                        {
                            // 0-xxxxxxx
                            m_Code1 = 1;
                            m_Stage = 1;
                            continue;
                        }
                        else
                        {
                            if (GetPackedBits(1) == 0)
                            {
                                // 10-xxxxxxx
                                m_Code1 = 2;
                                m_Stage = 1;
                                continue;
                            }
                            else
                            {
                                if (GetPackedBits(1) == 0)
                                {
                                    // 110-xxxxxxxxxxxxx-*
                                    m_Code1 = 3;
                                    m_Stage = 1;
                                    continue;
                                }
                                else
                                {
                                    if (GetPackedBits(1) == 0)
                                    {
                                        // 1110-xxxxxxxx-*
                                        m_Code1 = 4;
                                        m_Stage = 1;
                                        continue;
                                    }
                                    else
                                    {
                                        // 1111-xxxxxx-*
                                        m_Code1 = 5;
                                        m_Stage = 1;
                                        continue;
                                    }
                                }
                            }
                        }
                    }
                    else
                        break;
                }
                else if (m_Stage == 1)
                {
                    if (m_Code1 == 1)
                    {
                        if (HasBits(7))
                        {
                            byte OutB = (byte)(GetPackedBits(7));
                            UnpackedChunk.Add(OutB);
                            m_Unpacked.Add(OutB);
                            m_Stage = 0;
                            continue;
                        }
                        else
                            break;
                    }
                    else if (m_Code1 == 2)
                    {
                        if (HasBits(7))
                        {
                            byte OutB = (byte)((GetPackedBits(7)) | 0x80);
                            UnpackedChunk.Add(OutB);
                            m_Unpacked.Add(OutB);
                            m_Stage = 0;
                            continue;
                        }
                        else
                            break;
                    }
                    else if (m_Code1 == 3)
                    {
                        if (HasBits(13))
                        {
                            m_Shift = GetPackedBits(13) + 0x140;
                            m_Stage = 2;
                            continue;
                        }
                        else
                            break;
                    }
                    else if (m_Code1 == 4)
                    {
                        if (HasBits(8))
                        {
                            m_Shift = GetPackedBits(8) + 0x40;
                            m_Stage = 2;
                            continue;
                        }
                        else
                            break;
                    }
                    else if (m_Code1 == 5)
                    {
                        if (HasBits(6))
                        {
                            m_Shift = GetPackedBits(6);
                            m_Stage = 2;
                            continue;
                        }
                        else
                            break;
                    }
                }
                else if (m_Stage == 2)
                {
                    if (m_Shift == 0)
                    {
                        if (m_PackedOffset != 0)
                        {
                            m_PackedOffset = 0;
                            m_Packed.RemoveAt(0);
                        }
                        m_Stage = 0;
                        continue;
                    }
                    m_Code2 = 0;
                    m_Stage = 3;
                    continue;
                }
                else if (m_Stage == 3)
                {
                    if (HasBits(1))
                    {
                        if (GetPackedBits(1) == 0)
                        {
                            m_Stage = 4;
                            continue;
                        }
                        else
                        {
                            m_Code2++;
                            continue;
                        }
                    }
                    else
                        break;
                }
                else if (m_Stage == 4)
                {
                    UInt32 CopySize = 0;
                    if (m_Code2 == 0)
                        CopySize = 3;
                    else
                    {
                        UInt32 Sz = m_Code2 + 1;
                        if (HasBits(Sz))
                            CopySize = GetPackedBits(Sz) + (UInt32)(1 << ((Int32)Sz));
                        else
                            break;
                    }

                    Copy(m_Shift, CopySize, ref UnpackedChunk);
                    m_Stage = 0;
                    continue;
                }
            }
            return UnpackedChunk;
        }

        void Copy(UInt32 Shift, UInt32 Size, ref List<byte> UnpackedChunk)
        {
            for (UInt32 i = 0; i < Size; i++)
            {
                var PIndex = m_Unpacked.Count - Shift;
                if (PIndex < 0)
                    throw new Exception("Unpack error: PIndex < 0");
                else
                {
                    byte B = m_Unpacked[(Int32)PIndex];
                    m_Unpacked.Add(B);
                    UnpackedChunk.Add(B);
                }
            }
        }

        UInt32 GetPackedBits(UInt32 BitCount)
        {
            if (BitCount > 16)
                return 0;

            if (!HasBits(BitCount))
                throw new Exception("Unpack bit stream overflow");

            UInt32 AlBitCount = BitCount + m_PackedOffset;
            UInt32 AlByteCount = (AlBitCount + 7) / 8;

            UInt32 V = 0;
            for (UInt32 i = 0; i < AlByteCount; i++)
                V |= (UInt32)(m_Packed[(Int32)i]) << (Int32)(24 - i * 8);
            V <<= m_PackedOffset;
            V >>= (Int32)(32 - BitCount);

            m_PackedOffset += (byte)BitCount;
            Int32 FreeBytes = m_PackedOffset / 8;
            if (FreeBytes != 0)
                m_Packed.RemoveRange(0, FreeBytes);
            m_PackedOffset %= 8;

            return V;
        }
    }

}