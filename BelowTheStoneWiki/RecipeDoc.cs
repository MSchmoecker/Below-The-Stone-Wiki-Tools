using System;
using System.Collections.Generic;
using System.Linq;
using BelowTheStone.Crafting;
using BelowTheStone.NewDatabase;
using HarmonyLib;

namespace BelowTheStoneWiki {
    public class RecipeDoc : Doc {
        private static RecipeDoc Instance { get; set; }

        public RecipeDoc() : base("recipes") {
            Instance = this;
            Plugin.Harmony.PatchAll(typeof(Patches));
        }

        private static class Patches {
            [HarmonyPatch(typeof(SODatabase), nameof(SODatabase.Init), new Type[0]), HarmonyPrefix]
            public static void SODatabaseInit(SODatabase __instance) {
                Instance.DocRecipes(__instance);
            }
        }

        private void DocRecipes(SODatabase database) {
            List<CraftingRecipe> uncategorized = new List<CraftingRecipe>();
            List<CraftingRecipe> blacksmith = new List<CraftingRecipe>();
            List<CraftingRecipe> alchemy = new List<CraftingRecipe>();

            foreach (DatabaseElement databaseElement in database.MasterList) {
                if (databaseElement is CraftingRecipe recipe) {
                    if (recipe.IsHidden || !recipe.IsValid) {
                        continue;
                    }

                    if (recipe.Location == "blacksmith") {
                        blacksmith.Add(recipe);
                    } else if (recipe.Location == "alchemy") {
                        alchemy.Add(recipe);
                    } else {
                        uncategorized.Add(recipe);
                    }
                }
            }

            AddText("=== Blacksmith ===");
            AddTable("",
                blacksmith.OrderBy(i => i.RecipeOutput.ItemType.DisplayName),
                new string[] { "Output", "Ingredients", "NameID" },
                i => {
                    return new object[] {
                        i.RecipeOutput.ItemType.DisplayName, IngredientsToString(i.Ingredients), i.NameID
                    };
                }
            );

            AddText("=== Alchemy ===");
            AddTable("",
                alchemy.OrderBy(i => i.RecipeOutput.ItemType.DisplayName),
                new string[] { "Output", "Ingredients", "NameID" },
                i => {
                    return new object[] {
                        i.RecipeOutput.ItemType.DisplayName, IngredientsToString(i.Ingredients), i.NameID
                    };
                }
            );

            AddText("=== Uncategorized ===");
            AddTable("",
                uncategorized.OrderBy(i => i.RecipeOutput.ItemType.DisplayName),
                new string[] { "Output", "Ingredients", "NameID" },
                i => {
                    return new object[] {
                        i.RecipeOutput.ItemType.DisplayName, IngredientsToString(i.Ingredients), i.NameID
                    };
                }
            );

            Plugin.Log.LogInfo($"Finished documenting recipes");
        }
    }
}
