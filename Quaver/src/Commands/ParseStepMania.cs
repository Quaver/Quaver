using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quaver.StepMania;

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
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}
