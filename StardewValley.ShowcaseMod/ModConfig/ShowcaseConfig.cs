using System.ComponentModel;
using Igorious.StardewValley.DynamicApi2.Data;
using Newtonsoft.Json;

namespace Igorious.StardewValley.ShowcaseMod.ModConfig
{
    public class ShowcaseConfig
    {
        [JsonProperty(Required = Required.Always), DefaultValue(-1)]
        public int ID { get; set; }

        [JsonProperty(Required = Required.Always), DefaultValue("{Incognita}")]
        public string Name { get; set; }

        [JsonProperty(Required = Required.Always), DefaultValue(-1)]
        public int SpriteIndex { get; set; }

        [DefaultValue(1)]
        public int SpritesCount { get; set; } = 1;

        public Size Size { get; set; } = new Size(1, 1);

        public Size BoundingBox { get; set; } = Size.Default;

        public int Price { get; set; }

        public Bounds Bounds { get; set; } = new Bounds();

        [DefaultValue(1)]
        public int Rows { get; set; } = 1;

        [DefaultValue(1)]
        public int Columns { get; set; } = 1;

        [JsonProperty(Required = Required.Always)]
        public string Kind { get; set; }

        [DefaultValue(1)]
        public float Scale { get; set; } = 1;

        [DefaultValue(1)]
        public float Alpha { get; set; } = 1;

        [DefaultValue(false)]
        public bool InverseLayouts { get; set; }

        public string Filter { get; set; }
    }
}