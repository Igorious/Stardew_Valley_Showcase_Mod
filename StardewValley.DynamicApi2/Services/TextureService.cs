using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Igorious.StardewValley.DynamicApi2.Data;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Objects;

namespace Igorious.StardewValley.DynamicApi2.Services
{
    public sealed class TextureService
    {
        private class TextureOverrideInfo
        {
            public TextureOverrideInfo(string module, TextureRect rect)
            {
                Module = module;
                Rect = rect;
            }

            public string Module { get; }
            public TextureRect Rect { get; }
        }

        private static readonly Lazy<TextureService> Lazy = new Lazy<TextureService>(() => new TextureService());
        public static TextureService Instance => Lazy.Value;

        private readonly IDictionary<string, string> _modulePaths = new Dictionary<string, string>();
        private readonly IDictionary<int, TextureOverrideInfo> _furnitureOverrides = new Dictionary<int, TextureOverrideInfo>();

        private TextureService()
        {
            GameEvents.LoadContent += OnLoadContent;
        }

        public TextureModule RegisterModule<TModule>(string path) => RegisterModule(typeof(TModule).FullName, path);

        public TextureModule RegisterModule(string module, string path)
        {
            _modulePaths.Add(module, path);
            return new TextureModule(module);
        }

        public TextureService OverrideFurniture(string module, TextureRect source, int target)
        {
            _furnitureOverrides.Add(target, new TextureOverrideInfo(module, source));
            return this;
        }

        public Texture2D LoadTexture(string module, string name)
        {
            var texture = TextureLoader.Instance.Load(Path.Combine(_modulePaths[module], $"{name}.png"));
            SaveTexture(texture, _modulePaths[module], $"{name}.temp.png");
            return texture;
        }

        private void OnLoadContent(object sender, EventArgs eventArgs)
        {
            OverrideSprites();
            GameEvents.LoadContent -= OnLoadContent;
        }

        private void OverrideSprites()
        {
            LoadFurnitureTexture();
            OverrideTexture(ref Furniture.furnitureTexture, _furnitureOverrides, TextureInfo.Furnitures);

            // TODO: Other textures.
        }

        private void LoadFurnitureTexture()
        {
            new Furniture(0, Vector2.Zero);
        }

        private void OverrideTexture(ref Texture2D originalTexture, IDictionary<int, TextureOverrideInfo> spriteOverrides, TextureInfo info)
        {
            if (spriteOverrides.Count == 0) return;

            ExtendTexture(ref originalTexture, spriteOverrides, info);

            foreach (var group in spriteOverrides.GroupBy(s => s.Value.Module))
            {
                using (var imageStream = new FileStream(Path.Combine(_modulePaths[group.Key], $"{info.Name}.png"), FileMode.Open))
                {
                    var overrideTexture = Texture2D.FromStream(Game1.graphics.GraphicsDevice, imageStream);
                    foreach (var spriteOverride in spriteOverrides)
                    {
                        var textureRect = spriteOverride.Value.Rect;
                        if (textureRect.Height > 1)
                        {
                            var data = new Color[info.SpriteWidth * textureRect.Length * info.SpriteHeigth * textureRect.Height];
                            overrideTexture.GetData(0, info.GetSourceRect(overrideTexture, textureRect.Index, textureRect.Length, textureRect.Height), data, 0, data.Length);
                            originalTexture.SetData(0, info.GetSourceRect(originalTexture, spriteOverride.Key, textureRect.Length, textureRect.Height), data, 0, data.Length);
                        }
                        else
                        {
                            for (var i = 0; i < textureRect.Length; ++i)
                            {
                                var data = new Color[info.SpriteWidth * info.SpriteHeigth];
                                overrideTexture.GetData(0, info.GetSourceRect(overrideTexture, textureRect.Index + i), data, 0, data.Length);
                                originalTexture.SetData(0, info.GetSourceRect(originalTexture, spriteOverride.Key + i), data, 0, data.Length);
                            }
                        }
                    }
                }
            }

            SaveTexture(originalTexture, _modulePaths[spriteOverrides.First().Value.Module], info.Name);
        }

        [Conditional("DEBUG")]
        private void SaveTexture(Texture2D texture, string path, string name)
        {
            using (var imageStream = new FileStream(Path.Combine(path, $"{name}.temp.png"), FileMode.OpenOrCreate))
            {
                texture.SaveAsPng(imageStream, texture.Width, texture.Height);
            }
        }

        private void ExtendTexture(ref Texture2D originalTexture, IDictionary<int, TextureOverrideInfo> spriteOverrides, TextureInfo info)
        {
            var texture = originalTexture;
            var maxHeight = spriteOverrides.Select(so => info.GetSourceRect(texture, so.Key, so.Value.Rect.Length, so.Value.Rect.Height)).Max(r => r.Bottom);
            if (maxHeight > originalTexture.Height)
            {
                var allData = new Color[originalTexture.Width * originalTexture.Height];
                originalTexture.GetData(allData);

                var newData = new Color[originalTexture.Width * maxHeight];
                Array.Copy(allData, newData, allData.Length);

                originalTexture = new Texture2D(Game1.graphics.GraphicsDevice, originalTexture.Width, maxHeight);
                originalTexture.SetData(newData);
            }
        }
    }
}