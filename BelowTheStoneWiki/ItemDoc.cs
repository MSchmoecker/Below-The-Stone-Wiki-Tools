using System;
using BelowTheStone.NewDatabase;
using HarmonyLib;
using UnityEngine;

namespace BelowTheStoneWiki {
    public class ItemDoc : Doc {
        private static ItemDoc Instance { get; set; }

        public ItemDoc() : base("items") {
            Instance = this;
            Plugin.Harmony.PatchAll(typeof(Patches));
        }

        private static class Patches {
            [HarmonyPatch(typeof(SODatabase), nameof(SODatabase.Init), new Type[0]), HarmonyPrefix]
            public static void SODatabaseInit(SODatabase __instance) {
                Instance.DocItems(__instance);
            }
        }

        private void DocItems(SODatabase database) {
            foreach (DatabaseElement databaseElement in database.MasterList) {
                if (databaseElement is ItemType itemType) {
                    AddText(itemType.NameID);
                }
            }
        }
    }
}
