# 0.2.0

## Change list
- Include 3 built-in commands
    * ``HELP`` - Display the list of available commands or details about a specific command.
    * ``LOAD`` - Load the specified scene by name. Before you can load a scene you have to add it to the list of levels used in the game. Use File->Build Settings... in Unity and add the levels you need to the level list there.
    * ``QUIT`` - Quit the application.
- Allow the overwriting of an existing command by simply registering a new command with the same name
- Add optional parameters ``description`` and ``usage`` to the simpler overload of ``ConsoleCommandsDatabase.RegisterCommand()``
- Improve the explanatory text in the sample scenes
- Update the README with a more meaty custom command example


# 0.1.0

### Migrating from a previous version
0.1.0 modifies the Console prefab. If you want to upgrade an existing project, make sure you use the updated prefab.

## Change list
- Fix incompatibility with Unity 5 and drop Unity 4.6 support
- Fix console preventing user to select any other input field
- Fix scrolling not being enabled by default and weird scrolling behavior
- Fix the input field sometimes losing focus randomly
- Fix the console output text overflowing its rectangle when viewed from behind in World Space render mode
- Fix changing Time.timeScale messing up input field reactivation when submitting a command or reopening the console.
- Fix a strange bug when Unity rebuilds the project while in play mode
- Make input history capacity a constant rather than a inspector-editable value (helps fix the previous bug)
- Add some helpful text to each sample scene
- Overhaul cursor locking in the world space sample scene