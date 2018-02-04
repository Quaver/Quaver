using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ionic.Zip;
using Microsoft.Xna.Framework;
using NAudio.Wave;
using Quaver.Config;
using Quaver.Logging;
using Quaver.Peppy;
using Quaver.Skins;

namespace Quaver.Commands
{
    internal class ConvertOsuSkin : ICommand
    {
        public string Name { get; set; } = "OSK";

        public int Args { get; set; } = 2;

        public string Description { get; set; } = "Converts an osu! skin (.osk) to Quaver.";

        public string Usage { get; set; } = "osk <file path>";

        public void Execute(string[] args)
        {
            var argsList = new List<string>(args);
            argsList.RemoveAt(0);
            var path = string.Join(" ", argsList);

            Osu.ConvertOsk(path);
        }
    }
}
