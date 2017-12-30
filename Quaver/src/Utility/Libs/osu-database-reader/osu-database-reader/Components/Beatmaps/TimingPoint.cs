using osu.Shared.Serialization;

namespace osu_database_reader.Components.Beatmaps
{
    public class TimingPoint : ISerializable
    {
        public double Time, MsPerQuarter;
        public bool TimingChange;

        //only in .osu files
        public int? TimingSignature; // x/4 (eg. 4/4, 3/4)
        public int? SampleSet;
        public int? CustomSampleSet;
        public int? SampleVolume;
        public bool? Kiai;

        public static TimingPoint FromString(string line)
        {
            TimingPoint t = new TimingPoint();

            string[] splitted = line.Split(',');

            t.Time = double.Parse(splitted[0], Constants.NumberFormat);
            t.MsPerQuarter = double.Parse(splitted[1], Constants.NumberFormat);

            int temp;

            //lots of checks that are probably not needed. idc you're not my real dad.
            if (splitted.Length > 2) t.TimingSignature = (temp = int.Parse(splitted[2])) == 0 ? 4 : temp;
            if (splitted.Length > 3) t.SampleSet = int.Parse(splitted[3]);
            if (splitted.Length > 4) t.CustomSampleSet = int.Parse(splitted[4]);
            if (splitted.Length > 5) t.SampleVolume = int.Parse(splitted[5]);
            if (splitted.Length > 6) t.TimingChange = int.Parse(splitted[6]) == 1;
            if (splitted.Length > 7)
            {
                temp = int.Parse(splitted[7]);
                t.Kiai = (temp & 1) != 0;
            }

            return t;
        }

        public static TimingPoint ReadFromReader(SerializationReader r)
        {
            var t = new TimingPoint();
            t.ReadFromStream(r);
            return t;
        }

        public void ReadFromStream(SerializationReader r)
        {
            MsPerQuarter = r.ReadDouble();
            Time = r.ReadDouble();
            TimingChange = r.ReadBoolean();
        }

        public void WriteToStream(SerializationWriter w)
        {
            w.Write(MsPerQuarter);
            w.Write(Time);
            w.Write(TimingChange);
        }
    }
}
