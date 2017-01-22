using System.Collections.Generic;
using Igorious.StardewValley.ShowcaseMod.Data;

namespace Igorious.StardewValley.ShowcaseMod.ModConfig
{
    public sealed class GlowConfig
    {
        public List<GlowEffect> Glows { get; set; } = new List<GlowEffect>();
        public GlowEffect GoldQualityGlow { get; set; }
        public GlowEffect IridiumQualityGlow { get; set; }
    }
}