using System.Collections.Generic;
using Igorious.StardewValley.DynamicApi2;
using Igorious.StardewValley.DynamicApi2.Constants;
using Igorious.StardewValley.DynamicApi2.Data;
using Igorious.StardewValley.ShowcaseMod.Core;

namespace Igorious.StardewValley.ShowcaseMod.ModConfig
{
    public class ShowcaseModConfig : DynamicConfiguration
    {
        public override void CreateDefaultConfiguration()
        {
            Showcases = new List<ShowcaseConfig>
            {
                new ShowcaseConfig
                {
                    SpriteIndex = 0,
                    ID = 1228,
                    Name = "Showcase",
                    Bounds = new Bounds
                    {
                        Top = -2,
                        Bottom = 5,
                        Left = 1,
                        Right = 1,
                    },
                    Size = new Size(2, 2),
                    Price = 1000,
                    Columns = 3,
                    Rows = 3,
                    Kind = "table",
                    Filter = $"{ItemFilter.ShippableCategory}",
                },
                new ShowcaseConfig
                {
                    SpriteIndex = 2,
                    ID = 1230,
                    Name = "Wood Stand",
                    Size = new Size(1, 2),
                    Bounds = new Bounds
                    {
                        Top = 2,
                        Bottom = 14,
                    },
                    Price = 1000,
                    Columns = 1,
                    Rows = 1,
                    Kind = "painting",
                    Scale = 0.85f,
                    Filter = $"{nameof(CategoryID.Fish)}",
                },
                new ShowcaseConfig
                {
                    SpriteIndex = 3,
                    SpritesCount = 2,
                    ID = 1826,
                    Name = "Chinese Showcase",
                    Size = new Size(3, 3),
                    BoundingBox = new Size(3, 1),
                    Bounds = new Bounds
                    {
                        Top = 13,
                        Bottom = 23,
                        Right = 5,
                        Left = 5,
                    },
                    Price = 1000,
                    Columns = 3,
                    Rows = 1,
                    Kind = "other",
                    Scale = 0.80f,
                    Alpha = 0.60f,
                    Filter = $"{ItemFilter.ShippableCategory} !{nameof(CategoryID.Cooking)}",
                },
                new ShowcaseConfig
                {
                    SpriteIndex = 9,
                    ID = 1231,
                    Name = "Old Shield",
                    Size = new Size(1, 2),
                    Bounds = new Bounds { Top = 2 },
                    Price = 1000,
                    Columns = 1,
                    Rows = 1,
                    Kind = "painting",
                    Scale = 1,
                    InverseLayouts = true,
                    Filter = $"{nameof(CategoryID.Weapon)}",
                },
            };
        }

        public List<ShowcaseConfig> Showcases { get; set; } = new List<ShowcaseConfig>();
    }
}