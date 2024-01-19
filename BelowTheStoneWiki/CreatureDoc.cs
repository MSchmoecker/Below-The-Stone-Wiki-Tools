using System;
using System.Collections.Generic;
using System.Linq;
using BelowTheStone;
using BelowTheStone.NewDatabase;
using HarmonyLib;
using UnityEngine;

namespace BelowTheStoneWiki {
    public class CreatureDoc : Doc {
        private static CreatureDoc Instance { get; set; }

        public CreatureDoc() : base("creatures") {
            Instance = this;
            Plugin.Harmony.PatchAll(typeof(Patches));
        }

        private static class Patches {
            [HarmonyPatch(typeof(SODatabase), nameof(SODatabase.Init), new Type[0]), HarmonyPrefix]
            public static void SODatabaseInit(SODatabase __instance) {
                Instance.DocCreatures(__instance);
            }
        }

        private void DocCreatures(SODatabase database) {
            List<CreatureEntityType> uncategorized = new List<CreatureEntityType>();

            foreach (DatabaseElement databaseElement in database.MasterList) {
                if (databaseElement is CreatureEntityType creatureEntityType) {
                    if (creatureEntityType.NameID == "test_creature") {
                        continue;
                    }

                    uncategorized.Add(creatureEntityType);
                }
            }

            AddText("== Creatures ==");
            AddTable("",
                uncategorized.OrderBy(i => i.BaseHealth),
                new string[] { "Name", "Name ID", "Health", "Damage", "Knockback", "Speed", "Loot", "Description" },
                i => {
                    CreatureObject creature = i.MainEntityPrefab.GetComponent<CreatureObject>();
                    return new object[] {
                        i.DisplayName, i.NameID, i.BaseHealth, i.BaseDamage, i.BaseKnockback, creature.MoveSpeed, LootToString(i.Loot), i.JournalDescription
                    };
                }
            );

            Plugin.Log.LogInfo($"Finished documenting creatures");
        }
    }
}
