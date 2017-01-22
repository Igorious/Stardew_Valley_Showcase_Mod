using System.Linq;
using Igorious.StardewValley.DynamicApi2.Contracts;
using Igorious.StardewValley.DynamicApi2.Data;
using Igorious.StardewValley.ShowcaseMod.Constants;
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
        private Texture2D FurnitureTexture { get; }
        private Texture2D GlowTexture { get; }
        
        public Showcase(int id) : base(id, Vector2.Zero)
        {
            Config = ShowcaseMod.GetShowcaseConfig(ParentSheetIndex);
            GlowConfig = ShowcaseMod.Config.Glows;
            Filter = new ItemFilter(Config.Filter);
            FurnitureTexture = Config.Texture == TextureKind.Local
                ? ShowcaseMod.TextureModule.GetTexture(TextureNames.Furniture)
                : furnitureTexture;
            GlowTexture = ShowcaseMod.TextureModule.GetTexture(TextureNames.Glow);
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
            var internalItems = ItemProvider.GetInternalList();
            var rows = ItemProvider.Rows;
            var cellsCount = (internalItems.Count + rows - 1) / rows * rows;
            Game1.activeClickableMenu = new ShowcaseContainer(internalItems, cellsCount, rows, Filter.IsPass);
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

        public void Initialize()
        {
            defaultSourceRect = sourceRect = TextureInfo.Furnitures.GetSourceRect(
                FurnitureTexture, 
                Config.SpriteIndex,
                defaultSourceRect.Width / spriteSheetTileSize,
                defaultSourceRect.Height / spriteSheetTileSize);

            for (var i = 0; i < currentRotation; ++i)
            {
                rotate();
            }

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