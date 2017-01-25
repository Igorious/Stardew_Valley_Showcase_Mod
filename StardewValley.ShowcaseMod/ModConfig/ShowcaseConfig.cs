using System.ComponentModel;
using Igorious.StardewValley.DynamicApi2.Constants;
using Igorious.StardewValley.DynamicApi2.Data;
using Igorious.StardewValley.ShowcaseMod.Constants;
using Igorious.StardewValley.ShowcaseMod.Data;
using Newtonsoft.Json;

namespace Igorious.StardewValley.ShowcaseMod.ModConfig
{
    public class ShowcaseConfig
    {
        [JsonProperty(Required = Required.Always)]
        public int ID { get; set; }

        [JsonProperty(Required = Required.Always)]
        public string Name { get; set; }

        public string Description { get; set; }

        [JsonProperty(Required = Required.Always)]
        public SpriteInfo Sprite { get; set; }

        public SpriteInfo SecondSprite { get; set; }

        public SpriteInfo Tint { get; set; }

        public SpriteInfo SecondTint { get; set; }

        public bool AutoTint { get; set; }

        public Size Size { get; set; } = Size.Default;
        public bool ShouldSerializeSize() => Size != Size.Default;

        public Size BoundingBox { get; set; } = Size.Default;
        public bool ShouldSerializeBoundingBox() => BoundingBox != Size.Default;

        public int Price { get; set; }

        public Bounds SpriteBounds { get; set; } = Bounds.Empty;
        public bool ShouldSerializeSpriteBounds() => SpriteBounds != Bounds.Empty;

        public Bounds AltSpriteBounds { get; set; } = Bounds.Empty;
        public bool ShouldSerializeAltSpriteBounds() => AltSpriteBounds != Bounds.Empty;

        [DefaultValue(1)]
        public int Rows { get; set; } = 1;

        [DefaultValue(1)]
        public int Columns { get; set; } = 1;

        [JsonProperty(Required = Required.Always)]
        public FurnitureKind Kind { get; set; }

        [DefaultValue(1)]
        public float Scale { get; set; } = 1;

        public string Filter { get; set; }

        [DefaultValue(1)]
        public int Rotations { get; set; } = 1;

        [DefaultValue(ShowcaseLayoutKind.Fixed)]
        public ShowcaseLayoutKind Layout { get; set; } = ShowcaseLayoutKind.Fixed;
    }
}