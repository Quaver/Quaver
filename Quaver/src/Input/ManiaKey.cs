using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quaver.Input
{
    internal class ManiaKey : EventArgs
    {
        private int Key { get; set; }

        internal ManiaKey(int key)
        {
            Key = key;
        }

        internal int GetKey()
        {
            return Key;
        }
    }
}
