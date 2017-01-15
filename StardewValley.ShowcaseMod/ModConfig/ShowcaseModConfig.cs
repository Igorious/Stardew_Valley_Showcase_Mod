using Igorious.StardewValley.DynamicApi2.Data;

namespace Igorious.StardewValley.ShowcaseMod.ModConfig
{
    public class ShowcaseModConfig
    {
        public ShowcaseModConfig()
        {
            Showcase = new ShowcaseConfig
            {
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
            };
        }

        public ShowcaseConfig Showcase { get; set; }
    }
}