using System.Collections.Generic;
using System.IO;
using BelowTheStone;
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
        public static Harmony Harmony { get; private set; } = new Harmony(PluginGUID);

        private List<Doc> docs;

        private void Awake() {
            Log = Logger;

            Doc.BaseDir = Path.GetDirectoryName(Info.Location);

            docs = new List<Doc>() {
                new ItemDoc(),
                new CreatureDoc(),
            };
        }
    }
}
