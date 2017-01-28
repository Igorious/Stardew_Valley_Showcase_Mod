using System.Reflection;
using StardewModdingAPI;

namespace Igorious.StardewValley.DynamicApi2.Utils
{
    internal class Smapi
    {
        public static IMonitor GetMonitor(string source)
        {
            return (IMonitor)typeof(Program)
                .GetMethod("GetSecondaryMonitor", BindingFlags.Static | BindingFlags.NonPublic)
                .Invoke(null, new object[] { source });
        }

        public static IModRegistry GetModRegistry()
        {
            return (IModRegistry)typeof(Program)
                .GetField("ModRegistry", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                .GetValue(null);
        }
    }
}