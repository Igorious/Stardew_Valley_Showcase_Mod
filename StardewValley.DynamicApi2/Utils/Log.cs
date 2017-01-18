using System.Reflection;
using StardewModdingAPI;

namespace Igorious.StardewValley.DynamicApi2.Utils
{
    public sealed class Log
    {
        private static IMonitor Monitor { get; }

        static Log()
        {
            Monitor = (IMonitor)typeof(Program)
                .GetMethod("GetSecondaryMonitor", BindingFlags.Static | BindingFlags.NonPublic)
                .Invoke(null, new object[] {"DynamicAPI2"});
        }

        public static void Debug(string message) => Monitor.Log(message);

        public static void Trace(string message) => Monitor.Log(message, LogLevel.Trace);

        public static void Error(string message) => Monitor.Log(message, LogLevel.Error);

        public static void Warning(string message) => Monitor.Log(message, LogLevel.Warn);
    }
}