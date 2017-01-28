using System.Text;
using StardewValley;

namespace Igorious.StardewValley.DynamicApi2.Extensions
{
    public static class ItemExtensions
    {
        public static string GetInfo(this Item item)
        {
            return item is Object o
                ? o.GetInfo() 
                : $"{item.Name} [{item.GetType().Name}]";
        }

        public static string GetInfo(this Object o)
        {
            if (o == null) return "{Empty}";
            var buffer = new StringBuilder(o.Name);
            if (o.Stack != 1) buffer.Append(" x").Append(o.Stack);
            if (o.GetType() != typeof(Object)) buffer.Append(" [").Append(o.GetType().Name).Append("]");
            return buffer.ToString();
        }
    }
}
