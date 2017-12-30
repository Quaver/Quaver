using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace osu.Shared.Serialization
{
    public class SerializationWriter : BinaryWriter
    {
        public SerializationWriter(Stream output) : base(output) { }
        public SerializationWriter(Stream output, Encoding encoding) : base(output, encoding) { }
        public SerializationWriter(Stream output, Encoding encoding, bool leaveOpen) : base(output, encoding, leaveOpen) { }

        public override void Write(string str)
        {
            WriteObject(str);
            if (str == null)
                Write((byte) TypeBytes.Null);
            else {
                Write((byte) TypeBytes.Null);
                base.Write(str);
            }
        }
        
        public override void Write(byte[] bytes)
        {
            if (bytes == null)
                Write(-1);
            else {
                Write(bytes.Length);
                if (bytes.Length > 0)
                    base.Write(bytes);
            }
        }

        public void WriteRaw(byte[] bytes)
        {
            base.Write(bytes);
        }

        public override void Write(char[] chars)
        {
            if (chars == null)
                Write(-1);
            else {
                Write(chars.Length);
                if (chars.Length > 0)
                    base.Write(chars);
            }
        }

        public void WriteRaw(char[] chars)
        {
            base.Write(chars);
        }

        public void Write(DateTime time)
        {
            Write(time.ToUniversalTime().Ticks);
        }

        public void Write(ISerializable s)
        {
            s.WriteToStream(this);
        }

        public void WriteSerializableList<T>(IList<T> list) where T : ISerializable
        {
            if (list == null)
                Write(-1);
            else {
                Write(list.Count);
                foreach (T t in list)
                    t.WriteToStream(this);
            }
        }

        public void Write<T>(List<T> list)
        {
            if (list == null)
                Write(-1);
            else {
                Write(list.Count);
                foreach (T t in list)
                    WriteObject(t);
            }
        }

        public void Write<TKey, TValue>(IDictionary<TKey, TValue> dic)
        {
            if (dic == null)
                Write(-1);
            else {
                Write(dic.Count);
                foreach (KeyValuePair<TKey, TValue> kvp in dic)
                {
                    WriteObject(kvp.Key);
                    WriteObject(kvp.Value);
                }
            }
        }

        private void WriteObject(object o)
        {
            if (o == null)
                Write((byte) TypeBytes.Null);
            else
                switch (o)
                {
                    case bool v:
                        Write((byte) TypeBytes.Bool);
                        base.Write(v);
                        break;
                    case byte v:
                        Write((byte) TypeBytes.Byte);
                        base.Write(v);
                        break;
                    case ushort v:
                        Write((byte) TypeBytes.UShort);
                        base.Write(v);
                        break;
                    case uint v:
                        Write((byte) TypeBytes.UInt);
                        base.Write(v);
                        break;
                    case ulong v:
                        Write((byte) TypeBytes.ULong);
                        base.Write(v);
                        break;
                    case sbyte v:
                        Write((byte) TypeBytes.SByte);
                        base.Write(v);
                        break;
                    case short v:
                        Write((byte) TypeBytes.Short);
                        base.Write(v);
                        break;
                    case int v:
                        Write((byte) TypeBytes.Int);
                        base.Write(v);
                        break;
                    case long v:
                        Write((byte) TypeBytes.Long);
                        base.Write(v);
                        break;
                    case char v:
                        Write((byte) TypeBytes.Char);
                        base.Write(v);
                        break;
                    case string v:
                        Write((byte) TypeBytes.String);
                        base.Write(v);
                        break;
                    case float v:
                        Write((byte) TypeBytes.Float);
                        base.Write(v);
                        break;
                    case double v:
                        Write((byte) TypeBytes.Double);
                        base.Write(v);
                        break;
                    case decimal v:
                        Write((byte) TypeBytes.Decimal);
                        base.Write(v);
                        break;
                    case DateTime v:
                        Write((byte) TypeBytes.DateTime);
                        Write(v);
                        break;
                    case byte[] v:
                        Write((byte) TypeBytes.ByteArray);
                        base.Write(v);
                        break;
                    case char[] v:
                        Write((byte) TypeBytes.CharArray);
                        base.Write(v);
                        break;
                    default:
                        throw new NotImplementedException();
                }
        }
    }
}
