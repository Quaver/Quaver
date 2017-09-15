// Copyright (c)  Swan. All rights reserved.  
// See the Copyright notice in the root of the project.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Wenzil.Console.Commands
{
    /// <summary>
    /// Clear Command. Clears the console of all text
    /// </summary>
    public static class ClearCommand
    {
        public static readonly string name = "CLEAR";
        public static readonly string description = "Clears the console of all text";
        public static readonly string usage = "CLEAR";

        public static string Execute(params string[] args)
        {
            return "\n\n\n\n\n\n\n\n\n\n\n";
        }
    }
}
