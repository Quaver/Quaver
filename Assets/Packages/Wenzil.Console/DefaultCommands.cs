
using UnityEngine;
using Wenzil.Console.Commands;

namespace Wenzil.Console
{
    public class DefaultCommands : MonoBehaviour
    {
        private void Start()
        {
            //ConsoleCommandsDatabase.RegisterCommand(QuitCommand.name, QuitCommand.description, QuitCommand.usage, QuitCommand.Execute);
            ConsoleCommandsDatabase.RegisterCommand(HelpCommand.name, HelpCommand.description, HelpCommand.usage, HelpCommand.Execute);
            //ConsoleCommandsDatabase.RegisterCommand(LoadCommand.name, LoadCommand.description, LoadCommand.usage, LoadCommand.Execute);  
            ConsoleCommandsDatabase.RegisterCommand(PingCommand.name, PingCommand.description, PingCommand.usage, PingCommand.Execute);
            ConsoleCommandsDatabase.RegisterCommand(ClearCommand.name, ClearCommand.description, ClearCommand.usage, ClearCommand.Execute);
            ConsoleCommandsDatabase.RegisterCommand(ParseBeatmapCommand.name, ParseBeatmapCommand.description, ParseBeatmapCommand.usage, ParseBeatmapCommand.Execute);
            ConsoleCommandsDatabase.RegisterCommand(ConvertToQuaCommand.name, ConvertToQuaCommand.description, ConvertToQuaCommand.usage, ConvertToQuaCommand.Execute);
            ConsoleCommandsDatabase.RegisterCommand(ParseQuaCommand.name, ParseQuaCommand.description, ParseQuaCommand.usage, ParseQuaCommand.Execute);
            ConsoleCommandsDatabase.RegisterCommand(ParseConfigCommand.name, ParseConfigCommand.description, ParseConfigCommand.usage, ParseConfigCommand.Execute);
            ConsoleCommandsDatabase.RegisterCommand(ConfigGenCommand.name, ConfigGenCommand.description, ConfigGenCommand.usage, ConfigGenCommand.Execute);
            ConsoleCommandsDatabase.RegisterCommand(NotificationCommand.name, NotificationCommand.description, NotificationCommand.usage, NotificationCommand.Execute);
            ConsoleCommandsDatabase.RegisterCommand(CalculateDifficultyCommand.name, CalculateDifficultyCommand.description, CalculateDifficultyCommand.usage, CalculateDifficultyCommand.Execute);
            ConsoleCommandsDatabase.RegisterCommand(ParseSkinCommand.name, ParseSkinCommand.description, ParseSkinCommand.usage, ParseSkinCommand.Execute);
        }
    }
}
