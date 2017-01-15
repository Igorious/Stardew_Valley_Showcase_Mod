using Igorious.StardewValley.DynamicApi2.Data;

namespace Igorious.StardewValley.ShowcaseMod.ModConfig
{
    public class ShowcaseConfig
    {
        public string Name { get; set; }
        public int ID { get; set; }
        public int SpriteIndex { get; set; }
        public Size Size { get; set; } = new Size(1, 1);
        public int Price { get; set; }
        public Bounds Bounds { get; set; } = new Bounds();
        public int Rows { get; set; } = 1;
        public int Columns { get; set; } = 1;
        public string Kind { get; set; }
        public float Scale { get; set; } = 1;
    }
}