namespace osu.Shared.Serialization
{
    public interface ISerializable
    {
        void ReadFromStream(SerializationReader r);
        void WriteToStream(SerializationWriter w);
    }
}
