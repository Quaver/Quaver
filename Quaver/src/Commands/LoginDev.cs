using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quaver.Online.Wobble;

namespace Quaver.Commands
{
    internal class LoginDev : ICommand
    {
        public string Name { get; set; } = "LOGINDEV";

        public int Args { get; set; } = 2;

        public string Description { get; set; } = "Command that allows you to log into the Quaver server.";

        public string Usage { get; set; } = "logindev <username> <password>";

        public void Execute(string[] args)
        {
            if (args.Length != 3)
            {
                Console.WriteLine("Invalid Args Given. Must be 'logindev <username> <password>'");
                return;
            }

            // Connect
            SocketClient.Connect(args[1], args[2], true);
        }
    }
}
