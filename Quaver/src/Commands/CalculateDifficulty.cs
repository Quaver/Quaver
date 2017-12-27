using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quaver.Maps;

namespace Quaver.Commands
{
    internal class CalculateDifficulty : ICommand
    {
        public string Name { get; set; } = "CALCDIFF";

        public int Args { get; set; } = 2;

        public string Description { get; set; } = "Calculates difficulty for a .qua file";

        public string Usage { get; set; } = "calcdiff <path to file>";

        public void Execute(string[] args)
        {
            var argsList = new List<string>(args);
            argsList.RemoveAt(0);
            var path = string.Join(" ", argsList);

            var qua = Qua.Parse(path);
            Console.WriteLine();
            Console.WriteLine($"Artist: {qua.Artist}");
            Console.WriteLine($"Title: {qua.Title}");
            Console.WriteLine($"Difficulty Name: {qua.DifficultyName}");
            qua.CalculateDifficulty();
        }
    }
}
