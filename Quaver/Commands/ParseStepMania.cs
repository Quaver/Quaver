using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quaver.API.Maps;
using Quaver.API.Maps.Parsers;

namespace Quaver.Commands
{
    internal class ParseStepMania : ICommand
    {
        public string Name { get; set; } = "SM";

        public int Args { get; set; } = 2;

        public string Description { get; set; } = "Parses a StepMania (.sm) file";

        public string Usage { get; set; } = "sm <path to .sm file>";

        public void Execute(string[] args)
        {
            var argsList = new List<string>(args);
            argsList.RemoveAt(0);
            var path = string.Join(" ", argsList);

            try
            {
                var sm = StepManiaFile.Parse(path);

                var sb = new StringBuilder();
                sb.AppendLine("Artist: " + sm.Artist);
                sb.AppendLine("Title: " + sm.Title);
                sb.AppendLine("Subtitle: " + sm.Subtitle);
                sb.AppendLine("Creator: " + sm.Credit);
                sb.AppendLine("Music: " + sm.Music);
                sb.AppendLine("Background: " + sm.Background);
                sb.AppendLine("Offset: " + sm.Offset);
                sb.AppendLine("Sample Start: " + sm.SampleStart);
                sb.AppendLine("BPM Count " + sm.Bpms.Count);

                foreach (var chart in sm.Charts)
                {
                    sb.AppendLine("//////////////////////////////////////");
                    sb.AppendLine("Chart Type: " + chart.ChartType);
                    sb.AppendLine("Difficulty: " + chart.Difficulty);
                    sb.AppendLine("Description: " + chart.Description);
                    sb.AppendLine("Measure Count: " + chart.Measures.Count);
                }

                Console.WriteLine(sb.ToString());

                Console.WriteLine("----------------------");

                var qua = Qua.ConvertStepManiaChart(sm);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}
