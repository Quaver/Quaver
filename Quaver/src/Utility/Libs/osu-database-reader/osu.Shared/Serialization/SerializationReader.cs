using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace osu.Shared.Serialization
{
    public class SerializationReader : BinaryReader
    {
        public SerializationReader(Stream input) : base(input) { }
        public SerializationReader(Stream input, Encoding encoding) : base(input, encoding) { }
        public SerializationReader(Stream input, Encoding encoding, bool leaveOpen) : base(input, encoding, leaveOpen) { }

        public byte[] ReadBytes()   // an overload to ReadBytes(int count)
        {
            int length = ReadInt32();
            return length > 0
                ? base.ReadBytes(length)
                : null;
        }

        public char[] ReadChars()   // an overload to ReadChars(int count)
        {
            int length = ReadInt32();
            return length > 0
                ? base.ReadChars(length)
                : null;
        }

        public override string ReadString()
        {
            switch (ReadByte())
            {
                case (byte)TypeBytes.Null:
                    return null;
                case (byte)TypeBytes.String:
                    return base.ReadString();
                default:
                    throw new Exception($"Type byte is not {TypeBytes.Null} or {TypeBytes.String} (position: {BaseStream.Position})");
            }
        }

        public DateTime ReadDateTime()
        {
            return new DateTime(ReadInt64(), DateTimeKind.Utc);
        }

        public List<T> ReadSerializableList<T>() where T : ISerializable, new()
        {
            var list = new List<T>();
            int length = ReadInt32();
            for (int i = 0; i < length; i++)
            {
                var a = new T();
                a.ReadFromStream(this);
                list.Add(a);
            }
            return list;
        }

        public List<T> ReadList<T>()
        {
            var l = new List<T>();
            int length = ReadInt32();
            for (int i = 0; i < length; i++)
                l.Add((T)ReadObject());
            return l;
        }

        public Dictionary<TKey, TValue> ReadDictionary<TKey, TValue>()
        {
            var dic = new Dictionary<TKey, TValue>();
            int length = ReadInt32();
            for (int i = 0; i < length; i++)
                dic[(TKey) ReadObject()] = (TValue) ReadObject();
            return dic;
        }

        public object ReadObject()
        {
            switch ((TypeBytes)ReadByte())
            {
                case TypeBytes.Null:        return null;
                case TypeBytes.Bool:        return ReadBoolean();
                case TypeBytes.Byte:        return ReadByte();
                case TypeBytes.UShort:      return ReadUInt16();
                case TypeBytes.UInt:        return ReadUInt32();
                case TypeBytes.ULong:       return ReadUInt64();
                case TypeBytes.SByte:       return ReadSByte();
                case TypeBytes.Short:       return ReadInt16();
                case TypeBytes.Int:         return ReadInt32();
                case TypeBytes.Long:        return ReadInt64();
                case TypeBytes.Char:        return ReadChar();
                case TypeBytes.String:      return base.ReadString();
                case TypeBytes.Float:       return ReadSingle();
                case TypeBytes.Double:      return ReadDouble();
                case TypeBytes.Decimal:     return ReadDecimal();
                case TypeBytes.DateTime:    return ReadDateTime();
                case TypeBytes.ByteArray:   return ReadBytes();
                case TypeBytes.CharArray:   return ReadChars();
                case TypeBytes.Unknown:
                case TypeBytes.Serializable: 
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
