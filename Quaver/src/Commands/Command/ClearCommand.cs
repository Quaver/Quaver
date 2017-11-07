using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quaver.Commands
{
    internal class ClearCommand : ICommand
    {
        public string Name { get; set; } = "CLEAR";

        public int Args { get; set; } = 0;

        public string Description { get; set; } = "Clears the console of any logs.";

        public string Usage { get; set; } = "> clear";

        public void Execute()
        {
            Console.Clear();
        }
    }
}
