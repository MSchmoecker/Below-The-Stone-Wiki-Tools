using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace BelowTheStoneWiki {
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    public class Plugin : BaseUnityPlugin {
        public const string PluginGUID = "com.maxsch.BelowTheStone.BelowTheStoneWiki";
        public const string PluginName = "BelowTheStoneWiki";
        public const string PluginVersion = "0.0.1";

        public static ManualLogSource Log { get; private set; }

        private void Awake() {
            Harmony harmony = new Harmony(PluginGUID);
            harmony.PatchAll();

            Log = Logger;
        }
    }
}
