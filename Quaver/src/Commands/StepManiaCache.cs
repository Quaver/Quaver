using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quaver.Commands;
using Quaver.StepMania;

namespace Quaver.Commands
{
    internal class StepManiaCache : ICommand
    {
        public string Name { get; set; } = "SMCACHE";

        public int Args { get; set; } = 2;

        public string Description { get; set; } = "Reads an Etterna (StepMania) Cache file";

        public string Usage { get; set; } = "smcache <path>";

        public void Execute(string[] args)
        {
            var argsList = new List<string>(args);
            argsList.RemoveAt(0);
            var path = string.Join(" ", argsList);

            Etterna.ReadCacheFile(path);
        }
    }
}
