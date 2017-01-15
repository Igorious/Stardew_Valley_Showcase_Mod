using System;
using System.Collections.Generic;
using StardewModdingAPI;

namespace Igorious.StardewValley.ShowcaseMod
{
    public static class ConsoleCommand
    {
        public static void Register(string name, string description, Action<IReadOnlyList<string>> action)
        {
            Register(name, description, null, action);
        }

        public static void Register(string name, string description, string[] args, Action<IReadOnlyList<string>> action)
        {
            Command.RegisterCommand(name, description, args).CommandFired += (s, e) => action(e.Command.CalledArgs);
        }
    }
}