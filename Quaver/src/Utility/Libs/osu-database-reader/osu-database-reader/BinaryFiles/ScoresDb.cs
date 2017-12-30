using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using osu.Shared.Serialization;
using osu_database_reader.Components.Player;

namespace osu_database_reader.BinaryFiles
{
    public class ScoresDb
    {
        public int OsuVersion;
        public Dictionary<string, List<Replay>> Beatmaps = new Dictionary<string, List<Replay>>();
        public IEnumerable<Replay> Scores => Beatmaps.SelectMany(a => a.Value);

        public static ScoresDb Read(string path) {
            var db = new ScoresDb();
            using (var r = new SerializationReader(File.OpenRead(path)))
            {
                db.OsuVersion = r.ReadInt32();
                int amount = r.ReadInt32();

                for (int i = 0; i < amount; i++) {
                    string md5 = r.ReadString();

                    Tuple<string, List<Replay>> tuple = new Tuple<string, List<Replay>>(md5, new List<Replay>());

                    int amount2 = r.ReadInt32();
                    for (int j = 0; j < amount2; j++) {
                        tuple.Item2.Add(Replay.ReadFromReader(r, true));
                    }

                    db.Beatmaps.Add(tuple.Item1, tuple.Item2);
                }
            }

            return db;
        }
    }
}
