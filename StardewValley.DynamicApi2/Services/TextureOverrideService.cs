using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Objects;

namespace Igorious.StardewValley.DynamicApi2.Services
{
    public sealed class TextureOverrideService
    {
        public static TextureOverrideService Instance = new TextureOverrideService();

        private readonly IDictionary<Type, string> _modulePaths = new Dictionary<Type, string>();
        private readonly IDictionary<int, (string path, TextureRect rect)> _furnitureSpriteOverrides = new Dictionary<int, (string path, TextureRect rect)>();

        private bool IsRun { get; set; }
        
        public void Run<TModule>(string path)
        {
            _modulePaths.Add(typeof(TModule), path);
            if (IsRun) return;
            IsRun = true;
            GameEvents.LoadContent += OnLoadContent;
        }

        public void Furniture<TModule>(TextureRect source, int target)
        {
            _furnitureSpriteOverrides.Add(target, (_modulePaths[typeof(TModule)], source));
        }

        private void OnLoadContent(object sender, EventArgs eventArgs)
        {
            OverrideSprites();
            GameEvents.LoadContent -= OnLoadContent;
        }

        private void OverrideSprites()
        {
            new Furniture(0, Vector2.Zero); // Load furniture texture.
            OverrideTexture(ref global::StardewValley.Objects.Furniture.furnitureTexture, _furnitureSpriteOverrides, TextureInfo.Furniture);
        }

        private void OverrideTexture(ref Texture2D originalTexture, IDictionary<int, (string path, TextureRect rect)> spriteOverrides, TextureInfo info)
        {
            if (spriteOverrides.Count == 0) return;

            ExtendTexture(ref originalTexture, spriteOverrides, info);

            foreach (var group in spriteOverrides.GroupBy(s => s.Value.path))
            {
                using (var imageStream = new FileStream(Path.Combine(group.Key, $"{info.Name}.png"), FileMode.Open))
                {
                    var overrideTexture = Texture2D.FromStream(Game1.graphics.GraphicsDevice, imageStream);
                    foreach (var spriteOverride in spriteOverrides)
                    {
                        var textureRect = spriteOverride.Value.rect;
                        try
                        {
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
                        catch (Exception e)
                        {
                            throw;
                        }
                    }
                }
            }
        }

        private void ExtendTexture(ref Texture2D originalTexture, IDictionary<int, (string path, TextureRect rect)> spriteOverrides, TextureInfo info)
        {
            var texture = originalTexture;
            var maxHeight = spriteOverrides.Select(so => info.GetSourceRect(texture, so.Key, so.Value.rect.Length, so.Value.rect.Height)).Max(r => r.Bottom);
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