using System;
using System.Reflection;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using Log = Igorious.StardewValley.DynamicApi2.Utils.Log;

namespace Igorious.StardewValley.DynamicApi2.Compatibility
{
    public sealed class EntoaroxFrameworkСompatibilityLayout
    {
        private const string EntoaroxFrameworkID = "Entoarox.EntoaroxFramework";

        private static readonly Lazy<EntoaroxFrameworkСompatibilityLayout> Lazy = new Lazy<EntoaroxFrameworkСompatibilityLayout>(() => new EntoaroxFrameworkСompatibilityLayout());
        public static EntoaroxFrameworkСompatibilityLayout Instance => Lazy.Value;

        private IModRegistry ModRegistry { get; }

        public event Action ContentIsReadyToOverride;

        private EntoaroxFrameworkСompatibilityLayout()
        {
            ModRegistry = (IModRegistry)typeof(Program).GetField("ModRegistry", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).GetValue(null);
            GameEvents.LoadContent += GameEventsOnLoadContent;
        }

        private void GameEventsOnLoadContent(object sender, EventArgs eventArgs)
        {
            GameEvents.LoadContent -= GameEventsOnLoadContent;
            if (ModRegistry.IsLoaded(EntoaroxFrameworkID))
            {
                Log.Trace("Found Entoarox Framework. Used compatibility mode.");
                GameEvents.UpdateTick += GameEventsOnUpdateTick;
            }
            else
            {
                ContentIsReadyToOverride?.Invoke();
            }
        }

        private void GameEventsOnUpdateTick(object sender, EventArgs eventArgs)
        {
            if (Game1.content.GetType() == typeof(LocalizedContentManager)) return;
            GameEvents.UpdateTick -= GameEventsOnUpdateTick;
            ContentIsReadyToOverride?.Invoke();
        }
    }
}