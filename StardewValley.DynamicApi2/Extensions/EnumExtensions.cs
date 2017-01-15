using System;

namespace Igorious.StardewValley.DynamicApi2.Extensions
{
    public static class EnumExtensions
    {
        public static TEnum ToEnum<TEnum>(this string s)
        {
            return (TEnum)Enum.Parse(typeof(TEnum), s, true);
        }

        public static string ToLower(this Enum e)
        {
            return e.ToString().ToLower();
        }
    }
}
