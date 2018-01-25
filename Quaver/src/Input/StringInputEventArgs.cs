using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quaver.Input
{
    class StringInputEventArgs : EventArgs
    {
        private String Input { get; set; }

        internal StringInputEventArgs(String input)
        {
            Input = input;
        }

        internal String GetInput()
        {
            return Input;
        }
    }
}
