using System.Collections.Generic;
using System.IO;
using osu.Shared.Serialization;
using osu_database_reader.Components.Beatmaps;

namespace osu_database_reader.BinaryFiles
{
    public class CollectionDb
    {
        public int OsuVersion;
        public List<Collection> Collections = new List<Collection>();

        public static CollectionDb Read(string path) {
            var db = new CollectionDb();
            using (var r = new SerializationReader(File.OpenRead(path))) {
                db.OsuVersion = r.ReadInt32();
                int amount = r.ReadInt32();

                for (int i = 0; i < amount; i++) {
                    var c = new Collection();
                    c.BeatmapHashes = new List<string>();
                    c.Name = r.ReadString();
                    int amount2 = r.ReadInt32();
                    for (int j = 0; j < amount2; j++) c.BeatmapHashes.Add(r.ReadString());

                    db.Collections.Add(c);
                }
            }

            return db;
        }
    }
}
