using Igorious.StardewValley.DynamicApi2.Data;
using Microsoft.Xna.Framework.Graphics;

namespace Igorious.StardewValley.DynamicApi2.Services
{
    public sealed class TextureModule
    {
        private string Module { get; }

        internal TextureModule(string module)
        {
            Module = module;
        }

        public TextureModule OverrideFurniture(TextureRect source, int target)
        {
            TextureService.Instance.OverrideFurniture(Module, source, target);
            return this;
        }

        public Texture2D LoadTexture(string name)
        {
            return TextureService.Instance.LoadTexture(Module, name);
        }
    }
}