using System;
using System.Collections.Generic;
using System.Linq;
using Igorious.StardewValley.DynamicApi2.Constants;
using Igorious.StardewValley.DynamicApi2.Data;
using Igorious.StardewValley.DynamicApi2.Extensions;
using Igorious.StardewValley.ShowcaseMod.Constants;
using Igorious.StardewValley.ShowcaseMod.Core.Layouts;
using Igorious.StardewValley.ShowcaseMod.Data;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Objects;
using StardewValley.Tools;
using Object = StardewValley.Object;

namespace Igorious.StardewValley.ShowcaseMod.Core
{
    public enum ShowcaseDrawMode
    {
        WithoutItems,
        Icon,
        All,
    }

    public partial class Showcase
    {
        private static Vector2 TileSize => new Vector2(Game1.tileSize, Game1.tileSize);
        private IDictionary<Item, LightSource> LightSources { get; } = new Dictionary<Item, LightSource>();
        public Color Color
        {
            get { return ((Chest)heldObject).playerChoiceColor; }
            set { ((Chest)heldObject).playerChoiceColor = value; }
        }

        public override void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float alpha, float layerDepth, bool drawStackNumber)
        {
            var iconScale = GetMenuIconScaleSize();
            var iconOffsetW = defaultSourceRect.Width * scaleSize * iconScale;
            var iconOffsetH = defaultSourceRect.Height * scaleSize * iconScale;
            var offset = (TileSize - new Vector2(iconOffsetW, iconOffsetH)) / 2;
            Draw(spriteBatch, alpha, location + offset, scaleSize * iconScale, layerDepth, ShowcaseDrawMode.Icon);
            UpdateLightSources();
        }

        public override void draw(SpriteBatch spriteBatch, int x, int y, float alpha = 1)
        {
            var viewPosition = GetViewPosition(x, y);
            var layerDepth = (boundingBox.Bottom - 8) / 10000f;
            Draw(spriteBatch, alpha, viewPosition, Game1.pixelZoom, layerDepth, ShowcaseDrawMode.All);
            UpdateLightSources();
        }

        private Color GetAutoTintColor()
        {
            if (ItemProvider.IsEmpty()) return Color.Black;
            var color = GetItemGlowColor(ItemProvider.GetNotEmpty());
            return color == Color.Black? new Color(1, 1, 1) : color ?? Color.Black;
        }

        public void Draw(SpriteBatch spriteBatch, float alpha, Vector2 viewPosition, float scaleSize, float layerDepth, ShowcaseDrawMode drawMode)
        {
            DrawFurniture(spriteBatch, viewPosition, alpha, scaleSize, Config.Sprite, Color.White, drawMode, ref layerDepth);

            var tintColor = Config.AutoTint? GetAutoTintColor() : Color;
            if (Config.Tint != null && tintColor != Color.Black)
            {
                DrawFurniture(spriteBatch, viewPosition, alpha, scaleSize, Config.Tint, tintColor, drawMode, ref layerDepth);
            }
            if (drawMode != ShowcaseDrawMode.WithoutItems)
            {
                DrawItems(spriteBatch, alpha, viewPosition, scaleSize, drawMode, ref layerDepth);
            }
            if (Config.SecondSprite != null)
            {
                DrawFurniture(spriteBatch, viewPosition, alpha, scaleSize, Config.SecondSprite, Color.White, drawMode, ref layerDepth);
            }
            if (Config.SecondTint != null && tintColor != Color.Black)
            {
                DrawFurniture(spriteBatch, viewPosition, alpha, scaleSize, Config.SecondTint, tintColor, drawMode, ref layerDepth);
            }
        }

        // Copied from Furniture.getScaleSize().
        // Replaced sourceRect with defaultSourceRect.
        private float GetMenuIconScaleSize()
        {
            var xTiles = defaultSourceRect.Width / 16;
            var yTiles = defaultSourceRect.Height / 16;
            if (xTiles >= 5) return 0.75f;
            if (yTiles >= 3) return 1f;
            if (xTiles <= 2) return 2f;
            return xTiles <= 4 ? 1f : 0.1f;
        }

        private Vector2 GetViewPosition(int x, int y)
        {
            var globalPosition = x == -1
                ? drawPosition
                : new Vector2(x * Game1.tileSize, y * Game1.tileSize - (sourceRect.Height * Game1.pixelZoom - boundingBox.Height));
            return Game1.GlobalToLocal(Game1.viewport, globalPosition);
        }

        private void DrawFurniture(SpriteBatch spriteBatch, Vector2 viewPosition, float alpha, float scaleSize, SpriteInfo sprite, Color color, ShowcaseDrawMode drawMode, ref float layerDepth)
        {
            var currentSourceRect = GetDefaultSourceRect(sprite, defaultSourceRect.Width, defaultSourceRect.Height);
            if (drawMode != ShowcaseDrawMode.Icon)
            {
                currentSourceRect.X += (sourceRect.X - defaultSourceRect.X);
                currentSourceRect.Y += (sourceRect.Y - defaultSourceRect.Y);
                currentSourceRect.Height = sourceRect.Height;
                currentSourceRect.Width = sourceRect.Width;
            }

            spriteBatch.Draw(
                GetTexture(sprite),
                viewPosition,
                currentSourceRect,
                color * alpha,
                0,
                Vector2.Zero,
                scaleSize,
                (flipped && drawMode != ShowcaseDrawMode.Icon) ? SpriteEffects.FlipHorizontally : SpriteEffects.None,
                layerDepth);
            ChangeDepth(ref layerDepth);
        }

        private void ChangeDepth(ref float layerDepth)
        {
            layerDepth += 0.0000001f;
        }

        private void DrawItems(SpriteBatch spriteBatch, float alpha, Vector2 viewPosition, float scaleSize, ShowcaseDrawMode drawMode, ref float layerDepth)
        {
            if (ItemProvider.IsEmpty()) return;
            ItemProvider.UpdateCurrentRotation(currentRotation);

            var itemProvider = (drawMode != ShowcaseDrawMode.Icon)? ItemProvider : ItemProvider.Clone(newRotation: 0);
            var actualSourceRect = (drawMode != ShowcaseDrawMode.Icon)? sourceRect : defaultSourceRect;
            var layout = GetLayout(scaleSize, actualSourceRect, itemProvider);

            for (var i = 0; i < itemProvider.Rows; ++i)
            {
                for (var j = 0; j < itemProvider.Columns; ++j)
                {
                    var item = itemProvider[i, j];
                    if (item == null) continue;

                    var relativeItemPosition = layout.GetItemViewRelativePosition(i, j);
                    if (relativeItemPosition == null) return;

                    var itemViewPosition = viewPosition + relativeItemPosition.Value;
                    if (drawMode != ShowcaseDrawMode.Icon) UpdateItemGlow(item, spriteBatch, itemViewPosition, alpha, scaleSize, layerDepth);
                    DrawItem((dynamic)item, spriteBatch, itemViewPosition, alpha, scaleSize, ref layerDepth);
                }
            }
        }

        private IShowcaseLayout GetLayout(float scaleSize, Rectangle actualSourceRect, ItemGridProvider itemProvider)
        {
            switch (Config.Layout.Type)
            {
                case ShowcaseLayoutKind.Fixed:
                    return new ShowcaseFixedLayout(scaleSize, actualSourceRect, itemProvider, Config.Layout);
                case ShowcaseLayoutKind.Auto:
                    return new ShowcaseAutoLayout(scaleSize, actualSourceRect, itemProvider, Config.Layout);
                case ShowcaseLayoutKind.Manual:
                    return new ShowcaseManualLayout(scaleSize, Config.Layout);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private float GetRotationScale(float rotation) => (rotation != 0)? 1.14f : 1;

        private void DrawItem(Object item, SpriteBatch spriteBatch, Vector2 viewPosition, float alpha, float scaleSize, ref float layerDepth)
        {
            var rotation = GetItemRotation(item) * MathHelper.PiOver4;
            var tileScaledSize = Game1.tileSize / 2f * (scaleSize * Config.Layout.Scale / Game1.pixelZoom);

            void DrawObjectSprite(int spriteIndex, Color color, float depth) => spriteBatch.Draw(
                Game1.objectSpriteSheet,
                viewPosition + new Vector2(tileScaledSize, tileScaledSize * GetRotationScale(rotation)),
                TextureInfo.Objects.GetSourceRect(spriteIndex),
                color * alpha,
                rotation,
                new Vector2(spriteSheetTileSize, spriteSheetTileSize) / 2f,
                scaleSize * Config.Layout.Scale / GetRotationScale(rotation),
                SpriteEffects.None,
                depth);

            DrawObjectSprite(item.ParentSheetIndex, Color.White, layerDepth);
            ChangeDepth(ref layerDepth);
            if (!(item is ColoredObject coloredObject)) return;
            DrawObjectSprite(item.ParentSheetIndex + 1, coloredObject.color, layerDepth);
            ChangeDepth(ref layerDepth);
        }

        private void UpdateItemGlow(Item item, SpriteBatch spriteBatch, Vector2 viewPosition, float alpha, float scaleSize, float layerDepth)
        {
            var color = GetItemGlowColor(item);

            if (color == null) return;

            var isDarkItem = (color == Color.Black);
            if (GlowConfig.ShowGlows)
            {
                var colorAlpha = isDarkItem ? 0.8f : 0.6f;
                spriteBatch.Draw(
                    GlowTexture,
                    viewPosition + TileSize / 2 * (scaleSize * Config.Layout.Scale / Game1.pixelZoom),
                    GlowTexture.Bounds,
                    color.Value * colorAlpha * alpha,
                    0,
                    GlowTexture.Bounds.Center.ToVector(),
                    4f * (scaleSize * Config.Layout.Scale / Game1.pixelZoom),
                    SpriteEffects.None,
                    layerDepth - 0.00000009f);
            }

            if (isDarkItem || !GlowConfig.ShowLights) return;

            if (!LightSources.TryGetValue(item, out var light))
            {
                light = new LightSource(LightSource.lantern, Vector2.Zero, 32f / Game1.options.lightingQuality);
                LightSources.Add(item, light);
            }
            light.position = viewPosition + new Vector2(Game1.viewport.X, Game1.viewport.Y) + TileSize / 2;
        }

        private int GetItemRotation(Item item)
        {
            if (item is Object o)
            {
                var rotation = RotationEffects.FirstOrDefault(r => r.Category == null && r.ID == o.ParentSheetIndex)
                    ?? RotationEffects.FirstOrDefault(r => r.Category == (CategoryID)o.category);
                return rotation?.N ?? 0;
            }

            if (item is MeleeWeapon || item is Slingshot)
            {
                var id = ((Tool)item).indexOfMenuItemView;
                var kind = (WeaponKind?)(item as MeleeWeapon)?.type;
                var sub = (kind != null && Enum.IsDefined(typeof(WeaponKind), kind))? kind.ToString() : null;
                var rotation = RotationEffects.FirstOrDefault(r => r.Category == CategoryID.Weapon && r.ID == id)
                    ?? RotationEffects.FirstOrDefault(r => r.Category == CategoryID.Weapon && r.SubCategory != null && r.SubCategory == sub)
                    ?? RotationEffects.FirstOrDefault(r => r.Category == CategoryID.Weapon);
                return rotation?.N ?? 0;
            }

            if (item is Tool tool)
            {
                var id = tool.indexOfMenuItemView - Math.Max(0, tool.UpgradeLevel);
                var rotation = RotationEffects.FirstOrDefault(r => r.Category == CategoryID.Tool && r.ID == id)
                    ?? RotationEffects.FirstOrDefault(r => r.Category == CategoryID.Tool);
                return rotation?.N ?? 0;
            }

            return RotationEffects.FirstOrDefault(r => r.Category == (CategoryID)item.category)?.N ?? 0;
        }

        private Color? GetItemGlowColor(Item item)
        {
            if (item is Object o)
            {
                var glow = GlowConfig.Glows.FirstOrDefault(g => g.Category == null && g.ID == o.ParentSheetIndex);
                if (glow != null) return glow.ColorValue;

                switch (o.quality)
                {
                    case bestQuality:
                        return GlowConfig.IridiumQualityGlow.ColorValue;
                    case highQuality:
                        return GlowConfig.GoldQualityGlow.ColorValue;
                    default:
                        return null;
                }
            }

            if (item is MeleeWeapon weapon)
            {
                var glow = GlowConfig.Glows.FirstOrDefault(g => g.Category == CategoryID.Weapon && g.ID == weapon.indexOfMenuItemView);
                return glow?.ColorValue;
            }

            if (item is FishingRod rod)
            {
                return rod.UpgradeLevel == 3
                    ? GlowConfig.IridiumQualityGlow.ColorValue
                    : null;
            }

            if (item is Tool tool)
            {
                switch (tool.UpgradeLevel)
                {
                    case Tool.iridium:
                        return GlowConfig.IridiumQualityGlow.ColorValue;
                    case Tool.gold:
                        return GlowConfig.GoldQualityGlow.ColorValue;
                    default:
                        return null;
                }
            }

            return null;
        }

        private void UpdateLightSources()
        {
            var isPlaced = (Game1.currentLocation as DecoratableLocation)?.furniture.Contains(this) ?? false;
            foreach (var kv in LightSources.ToList())
            {
                if (ItemProvider.HasItem(kv.Key) && isPlaced && GlowConfig.ShowLights)
                {
                    Game1.currentLightSources.Add(kv.Value);
                }
                else
                {
                    LightSources.Remove(kv.Key);
                    Game1.currentLightSources.Remove(kv.Value);
                }
            }
        }

        private void DrawItem(Tool tool, SpriteBatch spriteBatch, Vector2 viewPosition, float alpha, float scaleSize, ref float layerDepth)
        {
            bool IsWeapon() => (tool is MeleeWeapon) || (tool is Slingshot);

            var textureInfo = IsWeapon()? TextureInfo.Weapons : TextureInfo.Tools;
            var rotation = GetItemRotation(tool) * MathHelper.PiOver4;
            var tileScaledSize = Game1.tileSize / 2f * (scaleSize * Config.Layout.Scale / Game1.pixelZoom);

            spriteBatch.Draw(
                textureInfo.Texture,
                viewPosition + new Vector2(tileScaledSize, tileScaledSize * GetRotationScale(rotation)),
                textureInfo.GetSourceRect(tool.indexOfMenuItemView),
                Color.White * alpha,
                rotation,
                new Vector2(spriteSheetTileSize, spriteSheetTileSize) / 2f,
                scaleSize * Config.Layout.Scale / GetRotationScale(rotation),
                SpriteEffects.None,
                layerDepth);
            ChangeDepth(ref layerDepth);
        }
    }
}