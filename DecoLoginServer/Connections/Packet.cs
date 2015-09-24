using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DecoLoginServer
{
    public class Packet
    {
        public ushort Opcode = 0;
        public byte[] Data = new byte[0];

        public int ReaderPosition = 0;

        public Packet(ushort Opcode)
        {
            this.Opcode = Opcode;
        }

        public Packet(byte[] Buffer, bool FromServer, out int Length)
        {
            using (Stream stream = new MemoryStream(Buffer))
            using (BinaryReader Reader = new BinaryReader(stream))
            {
                byte SecBytesLen = Reader.ReadByte();
                ushort DataLen = Reader.ReadUInt16();
                Length = SecBytesLen + DataLen + 2;
                Opcode = (ushort)(Reader.ReadUInt16() >> 1);
                Data = SubArray(Buffer, 14, DataLen - 14);
            }
        }

        #region Reader
        public byte[] Read(int Count)
        {
            if (ReaderPosition + Count <= Data.Length)
            {
                byte[] Result = SubArray(Data, ReaderPosition, Count);
                ReaderPosition += Count;
                return Result;
            }
            throw new Exception("There's Not Enought Data To Read :" + Count.ToString() + " Byte(s)");
        }

        public byte ReadByte()
        {
            byte[] Array = Read(1);
            return Array[0];
        }
        public sbyte ReadSByte()
        {
            byte[] Array = Read(1);
            return unchecked((sbyte)Array[0]);
        }

        public short ReadShort()
        {
            byte[] Array = Read(2);
            return BitConverter.ToInt16(Array, 0);
        }
        public ushort ReadUShort()
        {
            byte[] Array = Read(2);
            return BitConverter.ToUInt16(Array, 0);
        }

        public int ReadInt()
        {
            byte[] Array = Read(4);
            return BitConverter.ToInt32(Array, 0);
        }
        public uint ReadUInt()
        {
            byte[] Array = Read(4);
            return BitConverter.ToUInt32(Array, 0);
        }

        public long ReadLong()
        {
            byte[] Array = Read(8);
            return BitConverter.ToInt64(Array, 0);
        }
        public ulong ReadULong()
        {
            byte[] Array = Read(8);
            return BitConverter.ToUInt64(Array, 0);
        }

        public float ReadFloat()
        {
            byte[] Array = Read(4);
            return BitConverter.ToSingle(Array, 0);
        }
        public double ReadDouble()
        {
            byte[] Array = Read(8);
            return BitConverter.ToDouble(Array, 0);
        }

        public string ReadString(int MaxLen)
        {
            byte[] Array = Read(MaxLen);
            Array = TrimEnd(Array, 0x00);
            return Encoding.ASCII.GetString(Array);
        }
        #endregion

        #region Writer
        public void Write(byte[] Value)
        {
            Array.Resize(ref Data, Data.Length + Value.Length);
            Buffer.BlockCopy(Value, 0, Data, Data.Length - Value.Length, Value.Length);
        }

        public void WriteBool(bool Value)
        {
            Write(BitConverter.GetBytes(Value));
        }

        public void WriteByte(byte Value)
        {
            Write(new byte[] { Value });
        }
        public void WriteSByte(sbyte Value)
        {
            Write(new byte[] { unchecked((byte)Value) });
        }

        public void WriteShort(short Value)
        {
            Write(BitConverter.GetBytes(Value));
        }
        public void WriteUShort(ushort Value)
        {
            Write(BitConverter.GetBytes(Value));
        }

        public void WriteInt(int Value)
        {
            Write(BitConverter.GetBytes(Value));
        }
        public void WriteUInt(uint Value)
        {
            Write(BitConverter.GetBytes(Value));
        }

        public void WriteLong(long Value)
        {
            Write(BitConverter.GetBytes(Value));
        }
        public void WriteULong(ulong Value)
        {
            Write(BitConverter.GetBytes(Value));
        }

        public void WriteFloat(float Value)
        {
            Write(BitConverter.GetBytes(Value));
        }
        public void WriteDouble(double Value)
        {
            Write(BitConverter.GetBytes(Value));
        }

        public void WriteString(string Value, int MaxLen)
        {
            byte[] Bytes = new byte[MaxLen];
            Encoding.ASCII.GetBytes(Value).CopyTo(Bytes, 0);
            Write(Bytes);
        }
        #endregion

        public override string ToString()
        {
            byte[] OpcodeArray = BitConverter.GetBytes(Opcode);
            Array.Reverse(OpcodeArray);
            return "Opcode : " + BitConverter.ToString(OpcodeArray).Replace("-", "")
                + ", Data : " + BitConverter.ToString(Data).Replace("-", " ");
        }

        public byte[] SubArray(byte[] Data, int Offset, int Len)
        {
            byte[] Result = new byte[Len];
            Buffer.BlockCopy(Data, Offset, Result, 0, Len);
            return Result;
        }

        public byte[] TrimEnd(byte[] Data, byte Char)
        {
            byte[] Result = new byte[Data.Length];
            Buffer.BlockCopy(Data, 0, Result, 0, Data.Length);
            for (int i = Data.Length - 1; i > 0; i--)
            {
                if (Result[i] == Char)
                    Array.Resize(ref Result, Result.Length - 1);
                else
                    return Result;
            }
            return Result;
        }
    }
}
