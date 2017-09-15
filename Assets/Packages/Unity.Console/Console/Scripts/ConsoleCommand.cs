// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Wenzil.Console
{
    public delegate string ConsoleCommandCallback(params string[] args);

    public struct ConsoleCommand
    {
        public string name { get; private set; }
        public string description { get; private set; }
        public string usage { get; private set; }
        public ConsoleCommandCallback callback { get; private set; }

        public ConsoleCommand(string name, string description, string usage, ConsoleCommandCallback callback) : this()
        {
            this.name = name;
            this.description = (string.IsNullOrEmpty(description.Trim()) ? "No description provided" : description);
            this.usage = (string.IsNullOrEmpty(usage.Trim()) ? "No usage information provided" : usage);
            this.callback = callback;
        }
    }
}