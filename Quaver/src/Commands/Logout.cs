using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#if !PUBLIC
using Quaver.Framework;

namespace Quaver.Commands
{
    internal class Logout : ICommand
    {
        public string Name { get; set; } = "LOGOUT";

        public int Args { get; set; } = 1;

        public string Description { get; set; } = "Logs out of the server";

        public string Usage { get; set; } = "logout";

        public void Execute(string[] args)
        {
            RattleClient.Disconnect();
        }
    }
}
#endif