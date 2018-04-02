using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quaver.Commands
{
    public interface ICommand
    {
        /// <summary>
        ///     The name of the command
        /// </summary>
        string Name { get; set; }

        /// <summary>
        ///     How many arguments the command takes
        /// </summary>
        int Args { get; set; }

        /// <summary>
        ///     Description of the command
        /// </summary>
        string Description { get; set; }

        /// <summary>
        ///     Command usage
        /// </summary>
        string Usage { get; set; }

        /// <summary>
        ///     Executes the command.
        /// </summary>
        void Execute(string[] args);
    }
}
