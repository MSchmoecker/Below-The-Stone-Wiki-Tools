using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BelowTheStone;
using BelowTheStone.Crafting;
using BelowTheStone.NewDatabase;
using HarmonyLib;
using UnityEngine;

namespace BelowTheStoneWiki {
    public class ItemDoc : Doc {
        private static ItemDoc Instance { get; set; }
        private Dictionary<ItemType, List<CraftingRecipe>> recipes = new Dictionary<ItemType, List<CraftingRecipe>>();

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

        private string FindRecipe(ItemType itemType) {
            if (recipes.TryGetValue(itemType, out List<CraftingRecipe> craftingRecipes)) {
                return string.Join("\n", craftingRecipes.Where(r => !r.IsHidden).Select(r => IngredientsToString(r.Ingredients)));
            }

            return "";
        }

        private void DocItems(SODatabase database) {
            List<ItemType> uncategorized = new List<ItemType>();

            List<ItemType> ore = new List<ItemType>();
            List<ItemType> ingots = new List<ItemType>();
            List<ItemType> resources = new List<ItemType>();

            List<ItemType> tools = new List<ItemType>();

            List<MeleItemType> meleeWeapons = new List<MeleItemType>();
            List<ItemType> rangedWeapons = new List<ItemType>();
            List<ItemType> thrownWeapons = new List<ItemType>();
            List<ItemType> ammo = new List<ItemType>();
            List<ClothingItem> armour = new List<ClothingItem>();
            List<ClothingItem> cosmetics = new List<ClothingItem>();

            List<ItemType> food = new List<ItemType>();
            List<ItemType> consumables = new List<ItemType>();

            recipes.Clear();

            foreach (DatabaseElement databaseElement in database.MasterList) {
                if (databaseElement is ItemType itemType) {
                    if (itemType.NameID == "test_item" || itemType.NameID == "torch_melee_data") {
                        continue;
                    }

                    if (itemType.Location == "ore") {
                        ore.Add(itemType);
                    } else if (itemType.Location == "ingot") {
                        ingots.Add(itemType);
                    } else if (itemType.Location == "resource") {
                        resources.Add(itemType);
                    } else if (itemType.Location == "tool") {
                        tools.Add(itemType);
                    } else if (itemType.Location == "ammo") {
                        ammo.Add(itemType);
                    } else if (itemType.Location == "weapon") {
                        if (itemType is MeleItemType meleItemType) {
                            meleeWeapons.Add(meleItemType);
                        } else if (itemType.itemPrefab.TryGetComponent(out ThrowableItemLogic throwableItemLogic)) {
                            thrownWeapons.Add(itemType);
                        } else if (itemType.itemPrefab.TryGetComponent(out RangedWeaponLogic rangedItemLogic)) {
                            rangedWeapons.Add(itemType);
                        } else {
                            uncategorized.Add(itemType);
                        }
                    } else if (itemType is ClothingItem clothingItem) {
                        if (itemType.Location == "armor") {
                            armour.Add(clothingItem);
                        } else if (itemType.Location == "cosmetic") {
                            cosmetics.Add(clothingItem);
                        } else {
                            uncategorized.Add(itemType);
                        }
                    } else if (itemType.Location == "consumable" && itemType.itemPrefab.TryGetComponent(out ConsumableItemLogic consumableItemLogic)) {
                        if (consumableItemLogic.applyEffectOnConsume) {
                            consumables.Add(itemType);
                        } else {
                            food.Add(itemType);
                        }
                    } else {
                        uncategorized.Add(itemType);
                    }
                }

                if (databaseElement is CraftingRecipe recipe) {
                    if (recipe.IsHidden || !recipe.IsValid) {
                        continue;
                    }

                    if (!recipes.ContainsKey(recipe.RecipeOutput.ItemType)) {
                        recipes[recipe.RecipeOutput.ItemType] = new List<CraftingRecipe>();
                    }

                    recipes[recipe.RecipeOutput.ItemType].Add(recipe);
                }
            }

            AddText("\n\n=== Ore ===\n");
            AddTable("",
                ore.OrderBy(i => i.GoldCoinValue).ThenBy(i => i.DisplayName),
                new[] { "Name", "Item ID", "Coin Value", "Stack Limit", "Description" },
                i => new object[] {
                    i.DisplayName, i.NameID, i.GoldCoinValue, i.StackLimit, i.Description
                }
            );

            AddText("\n\n=== Ingots ===\n");
            AddTable("",
                ingots.OrderBy(i => i.GoldCoinValue).ThenBy(i => i.DisplayName),
                new[] { "Name", "Item ID", "Coin Value", "Stack Limit", "Recipe", "Description" },
                i => new object[] {
                    i.DisplayName, i.NameID, i.GoldCoinValue, i.StackLimit, FindRecipe(i), i.Description
                }
            );

            AddText("\n\n=== Resources ===\n");
            AddTable("",
                resources.OrderBy(i => i.GoldCoinValue).ThenBy(i => i.DisplayName),
                new[] { "Name", "Item ID", "Coin Value", "Stack Limit", "Description" },
                i => new object[] {
                    i.DisplayName, i.NameID, i.GoldCoinValue, i.StackLimit, i.Description
                }
            );

            AddText("\n\n=== Tools ===\n");
            AddTable("",
                tools.OrderBy(i => i.GoldCoinValue).ThenBy(i => i.DisplayName),
                new[] { "Name", "Item ID", "Coin Value", "Pickaxe Power", "Mining Tier", "Recipe", "Description" },
                i => {
                    PickaxeItemType pickaxe = i as PickaxeItemType;
                    return new object[] {
                        i.DisplayName, i.NameID, i.GoldCoinValue, pickaxe ? pickaxe.PickaxePower : (int?)null, pickaxe ? pickaxe.MiningTier : (int?)null,
                        FindRecipe(i), i.Description
                    };
                }
            );

            AddText("\n\n=== Melee Weapons ===\n");
            AddTable("",
                meleeWeapons.OrderBy(i => i.GoldCoinValue).ThenBy(i => i.DisplayName),
                new[] { "Name", "Item ID", "Coin Value", "Damage", "Attack Speed", "Reach", "Sweep", "Knockback", "Recipe", "Description" },
                i => new object[] {
                    i.DisplayName, i.NameID, i.GoldCoinValue, i.Damage, i.SwingRate, i.ReachDistance, i.SwingWidth, i.Knockback, FindRecipe(i), i.Description
                }
            );

            AddText("\n\n=== Ranged Weapons ===\n");
            AddTable("",
                rangedWeapons.OrderBy(i => i.GoldCoinValue).ThenBy(i => i.DisplayName),
                new[] { "Name", "Item ID", "Coin Value", "Ammunition", "Ammunition Count", "Fire Rate", "Recipe", "Description" },
                i => {
                    RangedWeaponLogic itemLogic = i.itemPrefab.GetComponent<RangedWeaponLogic>();
                    return new object[] {
                        i.DisplayName, i.NameID, i.GoldCoinValue, itemLogic.ammunition.DisplayName, itemLogic.ammunitionCount, itemLogic.fireRate,
                        FindRecipe(i), i.Description
                    };
                }
            );

            AddText("\n\n=== Ammunition ===\n");
            AddTable("",
                ammo.OrderBy(i => i.GoldCoinValue).ThenBy(i => i.DisplayName),
                new[] { "Name", "Item ID", "Coin Value", "Stack Limit", "Recipe", "Description" },
                i => new object[] {
                    i.DisplayName, i.NameID, i.GoldCoinValue, i.StackLimit, FindRecipe(i), i.Description
                }
            );

            AddText("\n\n=== Thrown Weapons ===\n");
            AddTable("",
                thrownWeapons.OrderBy(i => i.GoldCoinValue).ThenBy(i => i.DisplayName),
                new[] { "Name", "Item ID", "Coin Value", "Damage", "Recipe", "Description" },
                i => {
                    ThrowableItemLogic itemLogic = i.itemPrefab.GetComponent<ThrowableItemLogic>();
                    return new object[] {
                        i.DisplayName, i.NameID, i.GoldCoinValue, itemLogic.projectileDamage, FindRecipe(i), i.Description
                    };
                }
            );

            AddText("\n\n=== Armor ===\n");
            AddTable("",
                armour.OrderBy(i => i.GoldCoinValue).ThenBy(i => i.DisplayName),
                new[] { "Name", "Item ID", "Coin Value", "Damage Resistance", "Body Part", "Recipe", "Description" },
                i => new object[] {
                    i.DisplayName, i.NameID, i.GoldCoinValue, i.DamageResistence, i.BodyPart, FindRecipe(i), i.Description
                }
            );

            AddText("\n\n=== Cosmetics ===\n");
            AddTable("",
                cosmetics.OrderBy(i => i.GoldCoinValue).ThenBy(i => i.DisplayName),
                new[] { "Name", "Item ID", "Coin Value", "Body Part", "Description" },
                i => new object[] { i.DisplayName, i.NameID, i.GoldCoinValue, i.BodyPart, i.Description }
            );

            AddText("\n\n=== Food ===\n");
            AddTable("",
                food.OrderBy(i => i.GoldCoinValue).ThenBy(i => i.DisplayName),
                new[] { "Name", "Item ID", "Coin Value", "Stack Limit", "Heal", "Recipe", "Description" },
                i => {
                    ConsumableItemLogic itemLogic = i.itemPrefab.GetComponent<ConsumableItemLogic>();
                    return new object[] {
                        i.DisplayName, i.NameID, i.GoldCoinValue, i.StackLimit, itemLogic.healAmount, FindRecipe(i), i.Description
                    };
                }
            );

            AddText("\n\n=== Consumables ===\n");
            AddTable("",
                consumables.OrderBy(i => i.GoldCoinValue).ThenBy(i => i.DisplayName),
                new[] { "Name", "Item ID", "Coin Value", "Stack Limit", "Effect", "Recipe", "Description" },
                i => {
                    ConsumableItemLogic consumableItemLogic = i.itemPrefab.GetComponent<ConsumableItemLogic>();
                    StringBuilder effectDescription = new StringBuilder();
                    consumableItemLogic.applyEffectOnConsume.AppendStatsToString(effectDescription, consumableItemLogic.effectDuration);

                    return new object[] {
                        i.DisplayName, i.NameID, i.GoldCoinValue, i.StackLimit,
                        $"{consumableItemLogic.applyEffectOnConsume.DisplayName}: {effectDescription.ToString()}",
                        FindRecipe(i), i.Description
                    };
                }
            );

            AddText("\n\n== Uncategorized ==\n");
            AddTable("",
                uncategorized.OrderBy(i => i.GoldCoinValue).ThenBy(i => i.DisplayName),
                new[] { "Name", "Item ID", "Coin Value", "Stack Limit", "Recipe", "Description" },
                i => new object[] { i.DisplayName, i.NameID, i.GoldCoinValue, i.StackLimit, FindRecipe(i), i.Description }
            );

            Plugin.Log.LogInfo("Finished documenting items");
        }
    }
}
