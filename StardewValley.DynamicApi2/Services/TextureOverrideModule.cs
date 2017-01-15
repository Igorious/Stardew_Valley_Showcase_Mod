using Igorious.StardewValley.DynamicApi2.Data;

namespace Igorious.StardewValley.DynamicApi2.Services
{
    public sealed class TextureOverrideModule
    {
        private string Module { get; }

        internal TextureOverrideModule(string module)
        {
            Module = module;
        }

        public TextureOverrideModule OverrideFurniture(TextureRect source, int target)
        {
            TextureService.Instance.OverrideFurniture(Module, source, target);
            return this;
        }
    }
}