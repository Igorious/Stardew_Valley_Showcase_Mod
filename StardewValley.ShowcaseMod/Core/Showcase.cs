using System.Collections.Generic;
using System.Linq;
using Igorious.StardewValley.DynamicApi2.Contracts;
using Igorious.StardewValley.DynamicApi2.Data;
using Igorious.StardewValley.ShowcaseMod.Constants;
using Igorious.StardewValley.ShowcaseMod.Data;
using Igorious.StardewValley.ShowcaseMod.ModConfig;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Objects;
using Object = StardewValley.Object;

namespace Igorious.StardewValley.ShowcaseMod.Core
{
    public partial class Showcase : Furniture, IInitializable
    {
        private ShowcaseConfig Config { get; }
        private GlowConfig GlowConfig { get; }
        private ItemFilter Filter { get; }
        private ItemGridProvider ItemProvider { get; set; }
        private Texture2D LocalFurnitureTexture { get; }
        private Texture2D GlowTexture { get; }
        private IReadOnlyCollection<RotationEffect> RotationEffects { get; }

        public Showcase(int id) : base(id, Vector2.Zero)
        {
            Config = ShowcaseMod.GetShowcaseConfig(ParentSheetIndex);
            GlowConfig = ShowcaseMod.Config.Glows;
            Filter = new ItemFilter(Config.Filter);
            LocalFurnitureTexture = ShowcaseMod.TextureModule.GetTexture(TextureNames.Furniture);
            GlowTexture = ShowcaseMod.TextureModule.GetTexture(TextureNames.Glow);
            RotationEffects = ShowcaseMod.Config.RotationEffects;
        }

        public override bool clicked(Farmer who)
        {
            var chest = heldObject;
            heldObject = null;
            var result = base.clicked(who);
            heldObject = chest;
            return result;
        }

        public override bool checkForAction(Farmer who, bool justCheckingForActivity = false)
        {
            if (justCheckingForActivity) return true;

            ItemProvider.Recalculate();
            var rows = ItemProvider.Rows;
            var cellsCount = (ItemProvider.Capacity + rows - 1) / rows * rows;
            var allowColoring = (Config.Tint != null || Config.SecondTint != null) && !Config.AutoTint;
            Game1.activeClickableMenu = new ShowcaseContainer(this, ItemProvider.GetInternalList(), cellsCount, rows, Filter.IsPass, allowColoring);
            return true;
        }

        public override bool performObjectDropInAction(Object dropIn, bool probe, Farmer who)
        {
            if (!Filter.IsPass(dropIn)) return false;
            ItemProvider.Recalculate();
            if (!ItemProvider.HasItem(null)) return false;
            if (probe) return true;

            Game1.playSound("woodyStep");
            who.reduceActiveItemByOne();
            ItemProvider.AddItem(dropIn.getOne());
            return true;
        }

        private Texture2D GetTexture(SpriteInfo spriteInfo)
        {
            return (spriteInfo.Kind == TextureKind.Local)? LocalFurnitureTexture : furnitureTexture;
        }

        private Rectangle GetDefaultSourceRect(SpriteInfo spriteInfo, int width, int heigth)
        {
            return TextureInfo.Furnitures.GetSourceRect(GetTexture(spriteInfo), spriteInfo.Index, width / spriteSheetTileSize, heigth / spriteSheetTileSize);
        }

        public void Initialize()
        {
            name = Config.Name;
            description = Config.Description ?? description;
            defaultSourceRect = sourceRect = GetDefaultSourceRect(Config.Sprite, defaultSourceRect.Width, defaultSourceRect.Height);
            for (var i = 0; i < currentRotation; ++i) rotate();

            var heldChest = heldObject as Chest;
            if (heldChest == null)
            {
                heldChest = new Chest(true);
                heldChest.items.AddRange(Enumerable.Repeat<Item>(null, Config.Rows * Config.Columns));
                heldChest.items[0] = heldObject;
                heldObject = heldChest;
            }
            ItemProvider = new ItemGridProvider(heldChest.items, Config.Rows, Config.Columns, currentRotation);
        }
    }
}