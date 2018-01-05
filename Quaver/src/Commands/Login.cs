using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#if !PUBLIC
using Quaver.Online;
using Quaver.Framework;


namespace Quaver.Commands
{
    internal class Login : ICommand
    {
        public string Name { get; set; } = "LOGIN";

        public int Args { get; set; } = 2;

        public string Description { get; set; } = "Command that allows you to log into the Quaver server.";

        public string Usage { get; set; } = "login <username> <password>";

        public void Execute(string[] args)
        {
            if (args.Length != 3)
            {
                Console.WriteLine("Invalid Args Given. Must be 'login <username> <password>'");
                return;
            }

            // Connect
            RattleClient.Connect(args[1], args[2], Rattle.OnlineEvents);
        }
    }
}
#endif
