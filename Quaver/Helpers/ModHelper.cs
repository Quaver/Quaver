using System.Text;
using Quaver.API.Enums;
using Quaver.Main;
using Quaver.Modifiers;

namespace Quaver.Helpers
{
    internal class ModHelper
    {
        internal static string GetActivatedModsString(bool addPlus = false)
        {
            var sb = new StringBuilder();
            
            // Add mods to the string if mods exist
            
            for (var i = 0; i < GameBase.CurrentGameModifiers.Count; i++)
            {
                if (GameBase.CurrentGameModifiers[i].ModIdentifier == ModIdentifier.Autoplay)
                    continue;
                
                if ((i == 0 || !sb.ToString().Contains(" + ")) && addPlus)
                    sb.Append(" +");

                if (i > 0)
                    sb.Append(",");
                    
                if (GameBase.CurrentGameModifiers[i].Type == ModType.Speed)
                {
                    sb.Append($" {GameBase.AudioEngine.PlaybackRate}x");
                    continue;
                }

                sb.Append(" " + GameBase.CurrentGameModifiers[i].Name);
            }

            return sb.ToString();
        }
    }
}