// Copyright (c)  Swan. All rights reserved.  
// See the Copyright notice in the root of the project.

using UnityEngine;
using System;
using Wenzil.Console;
using Wenzil.Console.Commands;

/// <summary>
/// Two custom commands being registered with the console. Registered commands persist between scenes but don't persist between multiple application executions.
/// </summary>
public class CustomCommands : MonoBehaviour
{
    private void Start()
    {
        ConsoleCommandsDatabase.RegisterCommand("SPAWN", "Spawn a new game object from the given name and primitve type in front of the main camera. See PrimitiveType.", "SPAWN name primitiveType", Spawn);
        ConsoleCommandsDatabase.RegisterCommand("DESTROY", "Destroy the specified game object by name.", "DESTROY gameobject", Destroy);
    }

    /// <summary>
    /// Spawn a new game object from the given name and primitve type in front of the main camera. See PrimitiveType.
    /// </summary>
    private static string Spawn(params string[] args)
    {
        string name;
        PrimitiveType primitiveType;
        GameObject spawned;

        if (args.Length < 2)
        {
            return HelpCommand.Execute("SPAWN");
        }
        else
        {
            name = args[0];
            try
            {
                primitiveType = (PrimitiveType)Enum.Parse(typeof(PrimitiveType), args[1], true);
            }
            catch
            {
                return "Invalid primitive type specified: " + args[1];
            }

            spawned = GameObject.CreatePrimitive(primitiveType);
            spawned.name = name;
            spawned.transform.position = Camera.main.transform.position + Camera.main.transform.forward * 5;
            return "Spawned a new " + primitiveType + " named " + name + ".";
        }
    }

    /// <summary>
    /// Destroy the specified game object by name.
    /// </summary>
    private static string Destroy(params string[] args)
    {
        if (args.Length == 0)
        {
            return HelpCommand.Execute("DESTROY");
        }
        else
        {
            string name = args[0];
            GameObject gameobject = GameObject.Find(name);

            if (gameobject != null)
            {
                GameObject.Destroy(gameobject);
                return "Destroyed game object " + name + ".";
            }
            else
            {
                return "No game object named " + name + ".";
            }
        }
    }
}