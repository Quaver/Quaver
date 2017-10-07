using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Quaver.Tests
{
    private class English
    {
        public string score { get; set; }
        public string hpDrain { get; set; }
        public string play { get; set; }
    }

    internal static class JsonTest
    {
        internal static void DserializeJsonTest()
        {
            var assembly = Assembly.GetExecutingAssembly();
            using (Stream stream = assembly.GetManifestResourceStream("Quaver.Resources.Quaver_Localization.en.english.json"))
            using (StreamReader sr = new StreamReader(stream))
            {
                using (JsonReader reader = new JsonTextReader(sr))
                {
                    var serializer = new JsonSerializer();
                    English meme = serializer.Deserialize<English>(reader);
                    Console.WriteLine(meme.play);
                }
            }
        }
    }
}
