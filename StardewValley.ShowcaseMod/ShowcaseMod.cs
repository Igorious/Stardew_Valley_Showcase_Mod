using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Igorious.StardewValley.DynamicApi2.Services;
using Igorious.StardewValley.ShowcaseMod.ModConfig;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Objects;

namespace Igorious.StardewValley.ShowcaseMod
{
    public class ShowcaseMod : Mod
    {
        public override void Entry(IModHelper helper)
        {
            Config = helper.ReadConfig<ShowcaseModConfig>();
            TextureOverrideService.Instance.Run<ShowcaseMod>(Path.Combine(Helper.DirectoryPath, "Resources"));
            TextureOverrideService.Instance.Furniture<ShowcaseMod>(new TextureRect(0, Config.Showcase.Size.Width, Config.Showcase.Size.Height), Config.Showcase.ID);
            RegisterClassService.Instance.Furniture<Showcase>(Config.Showcase.ID);
            CarpenterShopInterceptor.Instance.Run();
            CarpenterShopInterceptor.Instance.AddFurniture(Config.Showcase.ID);
            MapperService.Instance.Run();
            GameEvents.LoadContent += OnLoadContent;

            Command.RegisterCommand("list_furniture", "List furniture").CommandFired += OnListFurnitureCommand;
            Command.RegisterCommand("player_addfurniture", "Add furniture", new []{ "ID" }).CommandFired += OnPlayerAddFurnitureCommand;
        }

        private void OnListFurnitureCommand(object sender, EventArgsCommand e)
        {
            var furnitureData = Game1.content.Load<Dictionary<int, string>>(@"Data\Furniture")
                .Select(kv => (ID: kv.Key, Name: kv.Value.Split('/').First()));

            var args = e.Command.CalledArgs;
            var orderedFurniture = args.Any()? furnitureData.OrderBy(f => f.Name) : furnitureData.OrderBy(f => f.ID);
            foreach (var furnitureInfo in orderedFurniture)
            {
                Monitor.Log($"{furnitureInfo.ID,4}: {furnitureInfo.Name}");
            }
        }

        private void OnPlayerAddFurnitureCommand(object sender, EventArgsCommand e)
        {
            var args = e.Command.CalledArgs;
            if (!args.Any() || !int.TryParse(args.First(), out var id)) return;
            Game1.player.addItemByMenuIfNecessary(new Furniture(id, Vector2.Zero));
        }

        public static ShowcaseModConfig Config { get; private set; }

        private void OnLoadContent(object sender, EventArgs eventArgs)
        {
            var furnitureData = Game1.content.Load<Dictionary<int, string>>(@"Data\Furniture");
            furnitureData.Add(Config.Showcase.ID, new FurnitureInfo
            {
                Name = Config.Showcase.Name,
                Type = "table",
                Size = Config.Showcase.Size,
                BoundingBox = Config.Showcase.Size,
                Price = Config.Showcase.Price,
                Rotations = 1,
            }.ToString());
        }
    }
}