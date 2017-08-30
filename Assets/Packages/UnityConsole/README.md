# UnityConsole
Welcome to UnityConsole, an easy-to-use developer console for Unity 5!

![Screenshot](https://dl.dropboxusercontent.com/u/106740647/UnityConsole/Screenshot.jpg)

## Getting Started
1. Import the UnityConsole package into your project (or clone the UnityConsole repository into your Assets folder)
2. Add a UI canvas to your scene if you don't already have one (GameObject > UI > Canvas)
3. Drag-and-Drop the ``Console`` prefab onto the canvas in the Hierarchy
4. Run your scene and press ``TAB`` to toggle the console

## Logging
Anywhere in your code, simply use ``Console.Log()`` to output to the console

## Default Commands
The console comes with three commands by default.

* ``HELP`` - Display the list of available commands or details about a specific command.
* ``LOAD`` - Load the specified scene by name. Before you can load a scene you have to add it to the list of levels used in the game. Use File->Build Settings... in Unity and add the levels you need to the level list there.
* ``QUIT`` - Quit the application.

## Extending the Console
Use the ``ConsoleCommandsDatabase.RegisterCommand()`` method to register your own commands. Here's an example.

```csharp
public class MyCommands : MonoBehaviour
{
    void Start()
    {
        ConsoleCommandsDatabase.RegisterCommand("TAKE", MyCommands.Take,
            description: "Partake in a great adventure alone.",
            usage: "TAKE");
            
        ConsoleCommandsDatabase.RegisterCommand("RANDOM", MyCommands.Random,
            description: "Display a random number between a and b using an optional seed.",
            usage: "RANDOM a b [seed]");
    }

    private static string Take(params string[] args)
    {
        return "It is dangerous to go alone! Take this."
    }

    private static string Random(params string[] args)
    {
        if (args.Length < 2)
        {
            return "Missing range bounds.";
        }

        if (args.Length >= 3)
        {
            int seed = 0;
            if (int.TryParse(args[2], out seed))
            {
                UnityEngine.Random.seed = seed;
            }
            else
            {
                return "Invalid seed.";
            }
        }

        float a = 0;
        float b = 0;
        if (float.TryParse(args[0], out a) &&
            float.TryParse(args[1], out b) &&
            a <= b)
        {
            return Convert.ToString(UnityEngine.Random.Range(a, b));
        }
        else
        {
            return "Invalid range bounds.";
        }
    }
```

Attaching this script to the console will extend it with the two commands at the start of the game. Registered commands can be overwritten and persist between scenes but don't persist between multiple application executions.

## World space UI
You can use UnityConsole in world space. Simply set your canvas ``Render Mode`` to ``World Space`` and you're good to go. You may need to scale down the canvas.

## Styling the Console
You can easily change the appearance of the console by changing the image sources, font styles and state transitions of the various UI components. It is also possible to anchor the console differently as needed.

## Contributing

Feel free to create pull requests or report any issues you may find. I'll be taking your feedback!

## Contact me

@Syncopath1 on Twitter
