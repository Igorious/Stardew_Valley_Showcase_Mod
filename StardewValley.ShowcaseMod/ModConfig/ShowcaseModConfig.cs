using System.Collections.Generic;
using Igorious.StardewValley.DynamicApi2;
using Igorious.StardewValley.DynamicApi2.Data;

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
                },
                new ShowcaseConfig
                {
                    SpriteIndex = 2,
                    ID = 1230,
                    Name = "Wood Stand",
                    Size = new Size(1, 2),
                    Bounds = new Bounds
                    {
                        Top = 1,
                        Bottom = 15,
                    },
                    Price = 1000,
                    Columns = 1,
                    Rows = 1,
                    Kind = "painting",
                    Scale = 0.85f,
                },
            };
        }

        public List<ShowcaseConfig> Showcases { get; set; } = new List<ShowcaseConfig>();
    }
}