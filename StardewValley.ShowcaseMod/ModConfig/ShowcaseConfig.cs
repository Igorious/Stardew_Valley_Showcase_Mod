using Igorious.StardewValley.DynamicApi2.Data;

namespace Igorious.StardewValley.ShowcaseMod.ModConfig
{
    public class ShowcaseConfig
    {
        public string Name { get; set; }
        public int ID { get; set; }
        public Size Size { get; set; }
        public int Price { get; set; }
        public Bounds Bounds { get; set; }
        public int Rows { get; set; }
        public int Columns { get; set; }
    }
}