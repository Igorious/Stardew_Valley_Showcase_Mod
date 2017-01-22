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
                    SpriteBounds = new Bounds
                    {
                        Top = -2,
                        Bottom = 5,
                        Left = 1,
                        Right = 1,
                    },
                    TextureSize = new Size(2, 2),
                    Size = new Size(2, 2),
                    BoundingBox = new Size(2, 2),
                    Price = 1000,
                    Columns = 3,
                    Rows = 3,
                    Kind = FurnitureKind.Table,
                    Filter = $"{ItemFilter.ShippableCategory}",
                },
                new ShowcaseConfig
                {
                    SpriteIndex = 2,
                    ID = 1230,
                    Name = "Wood Stand",
                    TextureSize = new Size(2, 1),
                    Size = new Size(2, 1),
                    BoundingBox = new Size(2, 1),
                    SpriteBounds = new Bounds
                    {
                        Top = 2,
                        Bottom = 14,
                    },
                    Price = 1000,
                    Kind = FurnitureKind.Painting,
                    Scale = 0.85f,
                    Filter = $"{nameof(CategoryID.Fish)}",
                },
                new ShowcaseConfig
                {
                    SpriteIndex = 3,
                    TextureSize = new Size(3, 6),
                    IsTwoLayer = true,
                    ID = 1826,
                    Name = "Chinese Showcase",
                    Size = new Size(3, 3),
                    BoundingBox = new Size(1, 3),
                    SpriteBounds = new Bounds
                    {
                        Top = 13,
                        Bottom = 23,
                        Right = 5,
                        Left = 5,
                    },
                    Price = 1000,
                    Columns = 3,
                    Kind = FurnitureKind.Other,
                    Scale = 0.80f,
                    Alpha = 0.60f,
                    Filter = $"{ItemFilter.ShippableCategory} !{nameof(CategoryID.Cooking)}",
                },
                new ShowcaseConfig
                {
                    SpriteIndex = 9,
                    ID = 1231,
                    Name = "Old Shield",
                    TextureSize = new Size(2, 1),
                    Size = new Size(2, 1),
                    BoundingBox = new Size(2, 1),
                    SpriteBounds = new Bounds { Top = 2 },
                    Price = 1000,
                    Kind = FurnitureKind.Painting,
                    InverseLayouts = true,
                    Filter = $"{nameof(CategoryID.Weapon)} {nameof(CategoryID.Tool)}",
                },
                new ShowcaseConfig
                {
                    SpriteIndex = 10,
                    ID = 1832,
                    Name = "Darkwood Dresser",
                    TextureSize = new Size(3, 5),
                    SpriteBounds = new Bounds
                    {
                        Top = -3,
                        Left = 2,
                        Right = 2,
                        Bottom = 21,
                    },
                    AltSpriteBounds = new Bounds
                    {
                        Top = -3,
                        Left = 2,
                        Right = 5,
                        Bottom = 24,
                    },
                    Price = 1000,
                    Columns = 2,
                    Kind = FurnitureKind.Dresser,
                    Scale = 0.85f,
                    Filter = $"{ItemFilter.ShippableCategory}",
                    Rotations = 4,
                },
            };
        }

        public List<ShowcaseConfig> Showcases { get; set; } = new List<ShowcaseConfig>();
    }
}