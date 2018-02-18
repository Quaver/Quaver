using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quaver.StepMania
{
    internal static class Etterna
    {
        /// <summary>
        ///     Read and serializes an Etterna cache file
        /// </summary>
        internal static EtternaCache ReadCacheFile(string path)
        {
            var ec = new EtternaCache();

            var file = File.ReadAllLines(path);

            foreach (var line in file)
            {
                if (!line.Contains("#"))
                    continue;

                var key = line.Substring(0, line.IndexOf(':')).Trim().ToUpper();
                var value = line.Split(':').Last().Trim().Replace(";", "");

                switch (key)
                {
                    case "#VERSION":
                        ec.Version = value;
                        break;
                    case "#TITLE":
                        ec.Title = Encoding.ASCII.GetString(Encoding.ASCII.GetBytes(value));
                        break;
                    case "#SUBTITLE":
                        ec.Subtitle = Encoding.ASCII.GetString(Encoding.ASCII.GetBytes(value));
                        break;
                    case "#ARTIST":
                        ec.Artist = Encoding.ASCII.GetString(Encoding.ASCII.GetBytes(value));
                        break;
                    case "#CREDIT":
                        ec.Credit = Encoding.ASCII.GetString(Encoding.ASCII.GetBytes(value));
                        break;
                    case "#BANNER":
                        ec.Banner = Encoding.ASCII.GetString(Encoding.ASCII.GetBytes(value));
                        break;
                    case "#BACKGROUND":
                        ec.Background = Encoding.ASCII.GetString(Encoding.ASCII.GetBytes(value));
                        break;
                    case "#CDTITLE":
                        ec.CdTitle = Encoding.ASCII.GetString(Encoding.ASCII.GetBytes(value));
                        break;
                    case "#MUSIC":
                        ec.Music = Encoding.ASCII.GetString(Encoding.ASCII.GetBytes(value));
                        break;
                    case "#SAMPLESTART":
                        ec.SampleStart = float.Parse(value, CultureInfo.InvariantCulture);
                        break;
                    case "#SONGFILENAME":
                        ec.SongFileName = Encoding.ASCII.GetString(Encoding.ASCII.GetBytes(value));;
                        break;
                    case "#STEPFILENAME":
                        ec.StepFileName = Encoding.ASCII.GetString(Encoding.ASCII.GetBytes(value));
                        break;
                    default:
                        break;
                }
            }

            return ec;
        }
    }
}
