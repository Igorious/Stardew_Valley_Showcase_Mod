using System.Collections.Generic;
using System.IO;
using System.Linq;
using Igorious.StardewValley.DynamicApi2.Constants;
using Igorious.StardewValley.DynamicApi2.Data;
using Igorious.StardewValley.DynamicApi2.Extensions;
using Igorious.StardewValley.DynamicApi2.Services;
using Igorious.StardewValley.ShowcaseMod.Core;
using Igorious.StardewValley.ShowcaseMod.ModConfig;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Objects;

namespace Igorious.StardewValley.ShowcaseMod
{
    public class ShowcaseMod : Mod
    {
        private static ShowcaseModConfig Config { get; } = new ShowcaseModConfig();

        public override void Entry(IModHelper helper)
        {
            Config.Load(Helper.DirectoryPath);

            Config.Showcases.ForEach(s => ClassMapper.Instance.MapFurniture<Showcase>(s.ID));
            
            var textureModule = TextureService.Instance.RegisterModule<ShowcaseMod>(Path.Combine(Helper.DirectoryPath, "Resources"));
            Config.Showcases.ForEach(s => textureModule.OverrideFurniture(new TextureRect(s.SpriteIndex, s.TextureSize.Width, s.TextureSize.Height), s.ID));

            Config.Showcases.ForEach(s =>
                DataService.Instance.RegisterFurniture(new FurnitureInfo
                {
                    ID = s.ID,
                    Name = s.Name,
                    Kind = s.Kind.ToLower(),
                    Size = s.Size,
                    BoundingBox = s.BoundingBox,
                    Price = s.Price,
                    Rotations = s.Rotations,
                }));

            Config.Showcases.ForEach(s => ShopService.Instance.AddFurniture(Locations.CarpentersShop, new ShopItemInfo(s.ID)));

            ConsoleCommand.Register("list_furniture", "List furniture", OnListFurnitureCommand);
            ConsoleCommand.Register("player_addfurniture", "Add furniture", new[] { "ID" }, OnPlayerAddFurnitureCommand);
            ConsoleCommand.Register("player_whatinhands", "Get info about item in hands", OnPlayerWhatInHands);
        }

        public static ShowcaseConfig GetShowcaseConfig(int id)
        {
            return Config.Showcases.First(c => c.ID == id);
        }

        private void OnListFurnitureCommand(IReadOnlyList<string> args)
        {
            var furnitureData = DataService.Instance.GetFurniture().Select(kv => (ID: kv.Key, Name: kv.Value.Split('/').First()));
            var orderedFurniture = args.Any()? furnitureData.OrderBy(f => f.Name) : furnitureData.OrderBy(f => f.ID);
            foreach (var furnitureInfo in orderedFurniture)
            {
                Monitor.Log($"{furnitureInfo.ID,4}: {furnitureInfo.Name}", LogLevel.Info);
            }
        }

        private void OnPlayerAddFurnitureCommand(IReadOnlyList<string> args)
        {
            if (!args.Any() || !int.TryParse(args.First(), out var id))
            {
                Monitor.Log("Invalid command arguments!", LogLevel.Error);
                return;
            }

            if (!DataService.Instance.GetFurniture().ContainsKey(id))
            {
                Monitor.Log($"Furniture with ID={id} is not registered!", LogLevel.Error);
                return;
            }

            Game1.player.addItemByMenuIfNecessary(new Furniture(id, Vector2.Zero));
        }

        private void OnPlayerWhatInHands(IReadOnlyList<string> args)
        {
            var item = Game1.player.ActiveObject;
            Monitor.Log(item != null ? $"{item.Name} [{item.GetType().Name} ID={item.ParentSheetIndex}]" : "Nothing", LogLevel.Info);
        }
    }
}