using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Igorious.StardewValley.DynamicApi2.Constants;
using Igorious.StardewValley.DynamicApi2.Data;
using Igorious.StardewValley.DynamicApi2.Extensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Objects;
using StardewValley.Tools;

namespace Igorious.StardewValley.ShowcaseMod.Core
{
    public partial class Showcase
    {
        private static Vector2 TileSize => new Vector2(Game1.tileSize, Game1.tileSize);

        public override void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float alpha, float layerDepth, bool drawStackNumber)
        {
            var iconScale = GetMenuIconScaleSize();
            var iconOffsetW = sourceRect.Width * scaleSize * iconScale;
            var iconOffsetH = sourceRect.Height * scaleSize * iconScale;
            var offset = (TileSize - new Vector2(iconOffsetW, iconOffsetH)) / 2;
            Draw(spriteBatch, alpha, location + offset, scaleSize * iconScale, layerDepth);
            UpdateLightSources();
        }

        public override void draw(SpriteBatch spriteBatch, int x, int y, float alpha = 1)
        {
            var viewPosition = GetViewPosition(x, y);
            var layerDepth = (boundingBox.Bottom - 8) / 10000f;
            Draw(spriteBatch, alpha, viewPosition, Game1.pixelZoom, layerDepth);
            UpdateLightSources();
        }

        private void Draw(SpriteBatch spriteBatch, float alpha, Vector2 viewPosition, float scaleSize, float layerDepth)
        {
            if (Config.InverseLayouts)
            {
                if (Config.IsTwoLayer) DrawFurniture(spriteBatch, viewPosition, alpha, scaleSize, false, ref layerDepth);
                DrawItems(spriteBatch, alpha * Config.Alpha, viewPosition, scaleSize, ref layerDepth);
                DrawFurniture(spriteBatch, viewPosition, alpha, scaleSize, Config.IsTwoLayer, ref layerDepth);
            }
            else
            {
                DrawFurniture(spriteBatch, viewPosition, alpha, scaleSize, Config.IsTwoLayer, ref layerDepth);
                DrawItems(spriteBatch, alpha * Config.Alpha, viewPosition, scaleSize, ref layerDepth);
                if (Config.IsTwoLayer) DrawFurniture(spriteBatch, viewPosition, alpha, scaleSize, false, ref layerDepth);
            }
        }

        private float GetMenuIconScaleSize() => (float) GetType().BaseType
            .GetMethod("getScaleSize", BindingFlags.Instance | BindingFlags.NonPublic)
            .Invoke(this, new object[0]);

        private Vector2 GetViewPosition(int x, int y)
        {
            var globalPosition = x == -1
                ? drawPosition
                : new Vector2(x * Game1.tileSize, y * Game1.tileSize - (sourceRect.Height * Game1.pixelZoom - boundingBox.Height));
            return Game1.GlobalToLocal(Game1.viewport, globalPosition);
        }

        private void DrawFurniture(SpriteBatch spriteBatch, Vector2 viewPosition, float alpha, float scaleSize, bool useOverlay, ref float layerDepth)
        {
            var baseSourceRect = useOverlay
                ? new Rectangle(sourceRect.X + sourceRect.Width, sourceRect.Y, sourceRect.Width, sourceRect.Height)
                : sourceRect;

            spriteBatch.Draw(
                furnitureTexture,
                viewPosition,
                baseSourceRect,
                Color.White * alpha,
                0,
                Vector2.Zero,
                scaleSize,
                flipped? SpriteEffects.FlipHorizontally : SpriteEffects.None,
                layerDepth);

            ChangeDepth(ref layerDepth);
        }

        private void ChangeDepth(ref float layerDepth)
        {
            layerDepth += 0.0000001f;
        }

        private void DrawItems(SpriteBatch spriteBatch, float alpha, Vector2 viewPosition, float scaleSize, ref float layerDepth)
        {
            if (Items.All(i => i == null)) return;

            var itemProvider = new ItemContainerProvider(Items, GetRowsCount(), GetColumnsCount());

            var bounds = IsHorizontal()? Config.AltSpriteBounds : Config.SpriteBounds;

            float workWidth = sourceRect.Width - bounds.Left - bounds.Right;
            float workHeigth = sourceRect.Height - bounds.Top - bounds.Bottom;

            var rowsCount = itemProvider.BottomRow - itemProvider.TopRow + 1;
            var columnsCount = itemProvider.RightColumn - itemProvider.LeftColumn + 1;

            var leftEmptyOffset = (workWidth - spriteSheetTileSize * columnsCount) / 2;
            if (workWidth >= spriteSheetTileSize && leftEmptyOffset < 0)
            {
                leftEmptyOffset = 0;
            }

            var topEmptyOffset = (workHeigth - spriteSheetTileSize * rowsCount) / 2;
            if (workHeigth >= spriteSheetTileSize && topEmptyOffset < 0)
            {
                topEmptyOffset = 0;
            }

            var horizontalItemOffset = columnsCount > 1 ? (workWidth - leftEmptyOffset * 2 - spriteSheetTileSize) / (columnsCount - 1) : 0;
            var verticalItemOffset = rowsCount > 1 ? (workHeigth - topEmptyOffset * 2 - spriteSheetTileSize) / (rowsCount - 1) : 0;

            var offset = new Vector2((flipped? bounds.Right : bounds.Left) + leftEmptyOffset, bounds.Top + topEmptyOffset) * scaleSize;

            for (var i = itemProvider.TopRow; i <= itemProvider.BottomRow; ++i)
            {
                for (var j = itemProvider.LeftColumn; j <= itemProvider.RightColumn; ++j)
                {
                    var item = itemProvider[i, j];
                    if (item == null) continue;
                    var tileDelta = new Vector2((j - itemProvider.LeftColumn) * horizontalItemOffset, (i - itemProvider.TopRow) * verticalItemOffset);
                    var itemViewPosition = viewPosition + offset + tileDelta * scaleSize;
                    UpdateItemGlow(item, spriteBatch, itemViewPosition, alpha, scaleSize, layerDepth);
                    DrawItem((dynamic)item, spriteBatch, itemViewPosition, alpha, scaleSize, layerDepth);
                    ChangeDepth(ref layerDepth);
                }
            }
        }

        private void DrawItem(Object item, SpriteBatch spriteBatch, Vector2 viewPosition, float alpha, float scaleSize, float layerDepth)
        {
            void DrawObjectSprite(int spriteIndex, Color color, float depth) => spriteBatch.Draw(
                Game1.objectSpriteSheet,
                viewPosition + TileSize / 2 * (scaleSize / Game1.pixelZoom),
                TextureInfo.Objects.GetSourceRect(spriteIndex),
                color * alpha,
                0,
                new Vector2(spriteSheetTileSize, spriteSheetTileSize) / 2f,
                scaleSize * Config.Scale,
                SpriteEffects.None,
                depth);

            DrawObjectSprite(item.ParentSheetIndex, Color.White, layerDepth);
            if (!(item is ColoredObject coloredObject)) return;
            DrawObjectSprite(item.ParentSheetIndex + 1, coloredObject.color, layerDepth + 0.0000001f);
        }

        private IDictionary<Item, LightSource> LightSources { get; } = new Dictionary<Item, LightSource>();

        private void UpdateItemGlow(Item item, SpriteBatch spriteBatch, Vector2 viewPosition, float alpha, float scaleSize, float layerDepth)
        {
            var color = GetItemGlowColor(item);

            if (color == null) return;

            var colorAlpha = color == Color.Black? 0.8f : 0.6f;

            spriteBatch.Draw(
                ShowcaseMod.GlowTexture,
                viewPosition + TileSize / 2 * (scaleSize / Game1.pixelZoom),
                ShowcaseMod.GlowTexture.Bounds,
                color.Value * colorAlpha * alpha,
                0,
                ShowcaseMod.GlowTexture.Bounds.Center.ToVector(),
                4f * (scaleSize / Game1.pixelZoom),
                SpriteEffects.None,
                layerDepth - 0.00000009f);

            if (color == Color.Black) return;

            if (!LightSources.TryGetValue(item, out var light))
            {
                light = new LightSource(LightSource.lantern, Vector2.Zero, 32f / Game1.options.lightingQuality);
                LightSources.Add(item, light);
            }
            light.position = viewPosition + new Vector2(Game1.viewport.X, Game1.viewport.Y) + TileSize / 2;
        }

        private Color? GetItemGlowColor(Item item)
        {
            if (item is Object o)
            {
                switch ((ObjectID)o.ParentSheetIndex)
                {
                    case ObjectID.PrismaticShard:
                        return Color.White;

                    case ObjectID.VoidEssence:
                        return Color.Black;

                    case ObjectID.SolarEssence:
                        return Color.Yellow;
                }

                switch (o.quality)
                {
                    case bestQuality:
                        return Color.Lerp(Color.Purple, Color.Magenta, 0.3f);
                    case highQuality:
                        return Color.Yellow;
                    default:
                        return null;
                }
            }

            if (item is MeleeWeapon weapon)
            {
                switch ((WeaponID)weapon.indexOfMenuItemView)
                {
                    case WeaponID.HolyBlade:
                        return Color.White;

                    case WeaponID.GalaxySword:
                    case WeaponID.GalaxyDagger:
                    case WeaponID.GalaxyHammer:
                        return Color.Lerp(Color.Purple, Color.Magenta, 0.3f);

                    case WeaponID.DarkSword:
                        return Color.Black;
                        
                    case WeaponID.LavaKatana:
                        return Color.Red;

                    case WeaponID.NeptunesGlaive:
                        return Color.Aqua;

                    case WeaponID.ForestSword:
                        return Color.Green;
                }
            }

            return null;
        }

        private void UpdateLightSources()
        {
            var isPlaced = (Game1.currentLocation as DecoratableLocation)?.furniture.Contains(this) ?? false;
            foreach (var kv in LightSources.ToList())
            {
                if (Items.Contains(kv.Key) && isPlaced)
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

        private void DrawItem(Tool tool, SpriteBatch spriteBatch, Vector2 viewPosition, float alpha, float scaleSize, float layerDepth)
        {
            float GetSpriteRotation()
            {
                if (tool is Slingshot) return 0;
                if (IsSwordLike()) return -5 * MathHelper.PiOver4;
                if (IsWeapon() || tool is Pickaxe || tool is Axe || tool is FishingRod || tool is Hoe) return -MathHelper.PiOver4;
                return 0;
            }

            bool IsSwordLike()
            {
                var weaponType = (tool as MeleeWeapon)?.type;
                return (tool is Sword) || (weaponType != null && weaponType != MeleeWeapon.club && tool.indexOfMenuItemView != (int)WeaponID.Scythe);
            }

            bool IsWeapon() => (tool is MeleeWeapon) || (tool is Slingshot);


            var textureInfo = IsWeapon()? TextureInfo.Weapons : TextureInfo.Tools;
            var rotation = GetSpriteRotation();
            var tileScaledSize = Game1.tileSize / 2f * (scaleSize / Game1.pixelZoom);

            spriteBatch.Draw(
                textureInfo.Texture,
                viewPosition + new Vector2(tileScaledSize, tileScaledSize / ((rotation != 0)? 1.414f : 1)),
                textureInfo.GetSourceRect(tool.indexOfMenuItemView),
                Color.White * alpha,
                rotation,
                new Vector2(spriteSheetTileSize, spriteSheetTileSize) / 2f,
                scaleSize * Config.Scale,
                SpriteEffects.None,
                layerDepth);
        }
    }
}