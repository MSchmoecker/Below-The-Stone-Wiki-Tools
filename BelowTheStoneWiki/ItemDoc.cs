using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using BelowTheStone;
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
            List<ItemType> uncategorized = new List<ItemType>();

            List<ItemType> ore = new List<ItemType>();
            List<ItemType> ingots = new List<ItemType>();

            List<ItemType> tools = new List<ItemType>();

            List<MeleItemType> meleeWeapons = new List<MeleItemType>();
            List<ItemType> rangedWeapons = new List<ItemType>();
            List<ItemType> thrownWeapons = new List<ItemType>();
            List<ItemType> ammo = new List<ItemType>();
            List<ClothingItem> armour = new List<ClothingItem>();

            List<ItemType> food = new List<ItemType>();
            List<ItemType> potions = new List<ItemType>();

            foreach (DatabaseElement databaseElement in database.MasterList) {
                if (databaseElement is ItemType itemType) {
                    if (itemType.NameID == "test_item" || itemType.NameID == "torch_melee_data") {
                        continue;
                    }

                    if (itemType.Location == "ore") {
                        ore.Add(itemType);
                    } else if (itemType.Location == "ingot") {
                        ingots.Add(itemType);
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
                    } else if (itemType.Location == "armor" && itemType is ClothingItem clothingItem) {
                        armour.Add(clothingItem);
                    } else if (itemType.Location == "consumable" && itemType.itemPrefab.TryGetComponent(out BasicItemLogic itemLogic)) {
                        if (itemLogic.applyEffectOnConsume) {
                            potions.Add(itemType);
                        } else {
                            food.Add(itemType);
                        }
                    } else {
                        uncategorized.Add(itemType);
                    }
                }
            }

            AddText("\n\n=== Ore ===\n");
            AddTable("",
                new[] { "Name", "Item ID", "Coin Value", "Stack Limit", "Description" },
                ore.OrderBy(i => i.GoldCoinValue).ThenBy(i => i.DisplayName)
                    .Select(i => new[] {
                        i.DisplayName, i.NameID, i.GoldCoinValue.ToString(), i.StackLimit.ToString(), i.Description
                    }).ToArray()
            );

            AddText("\n\n=== Ingots ===\n");
            AddTable("",
                new[] { "Name", "Item ID", "Coin Value", "Stack Limit", "Description" },
                ingots.OrderBy(i => i.GoldCoinValue).ThenBy(i => i.DisplayName)
                    .Select(i => new[] {
                        i.DisplayName, i.NameID, i.GoldCoinValue.ToString(), i.StackLimit.ToString(), i.Description
                    }).ToArray()
            );

            AddText("\n\n=== Tools ===\n");
            AddTable("",
                new[] { "Name", "Item ID", "Coin Value", "Description" },
                tools.OrderBy(i => i.GoldCoinValue).ThenBy(i => i.DisplayName)
                    .Select(i => new[] {
                        i.DisplayName, i.NameID, i.GoldCoinValue.ToString(), i.Description
                    }).ToArray()
            );

            AddText("\n\n=== Melee Weapons ===\n");
            AddTable("",
                new[] { "Name", "Item ID", "Coin Value", "Damage", "Attack Speed", "Reach", "Sweep", "Knockback", "Description" },
                meleeWeapons.OrderBy(i => i.GoldCoinValue).ThenBy(i => i.DisplayName)
                    .Select(i => new[] {
                        i.DisplayName, i.NameID, i.GoldCoinValue.ToString(), i.Damage.ToString(), i.SwingRate.ToString(CultureInfo.InvariantCulture),
                        i.ReachDistance.ToString(CultureInfo.InvariantCulture), i.SwingWidth.ToString(CultureInfo.InvariantCulture),
                        i.Knockback.ToString(CultureInfo.InvariantCulture), i.Description
                    }).ToArray()
            );

            AddText("\n\n=== Ranged Weapons ===\n");
            AddTable("",
                new[] { "Name", "Item ID", "Coin Value", "Ammunition", "Ammunition Count", "Fire Rate", "Description" },
                rangedWeapons.OrderBy(i => i.GoldCoinValue).ThenBy(i => i.DisplayName)
                    .Select(i => {
                        RangedWeaponLogic itemLogic = i.itemPrefab.GetComponent<RangedWeaponLogic>();
                        return new[] {
                            i.DisplayName, i.NameID, i.GoldCoinValue.ToString(), itemLogic.ammunition.DisplayName, itemLogic.ammunitionCount.ToString(),
                            itemLogic.fireRate.ToString(CultureInfo.InvariantCulture), i.Description
                        };
                    }).ToArray()
            );

            AddText("\n\n=== Ammunition ===\n");
            AddTable("",
                new[] { "Name", "Item ID", "Coin Value", "Stack Limit", "Description" },
                ammo.OrderBy(i => i.GoldCoinValue).ThenBy(i => i.DisplayName)
                    .Select(i => new[] {
                        i.DisplayName, i.NameID, i.GoldCoinValue.ToString(), i.StackLimit.ToString(), i.Description
                    }).ToArray()
            );

            AddText("\n\n=== Thrown Weapons ===\n");
            AddTable("",
                new[] { "Name", "Item ID", "Coin Value", "Damage", "Description" },
                thrownWeapons.OrderBy(i => i.GoldCoinValue).ThenBy(i => i.DisplayName)
                    .Select(i => {
                        ThrowableItemLogic itemLogic = i.itemPrefab.GetComponent<ThrowableItemLogic>();
                        return new[] {
                            i.DisplayName, i.NameID, i.GoldCoinValue.ToString(), itemLogic.projectileDamage.ToString(), i.Description
                        };
                    }).ToArray()
            );

            AddText("\n\n=== Armor ===\n");
            AddTable("",
                new[] { "Name", "Item ID", "Coin Value", "Damage Resistance", "Body Part", "Description" },
                armour.OrderBy(i => i.GoldCoinValue).ThenBy(i => i.DisplayName)
                    .Select(i => new[] {
                        i.DisplayName, i.NameID, i.GoldCoinValue.ToString(), i.DamageResistence.ToString(), i.BodyPart.ToString(), i.Description
                    }).ToArray()
            );

            AddText("\n\n=== Food ===\n");
            AddTable("",
                new[] { "Name", "Item ID", "Coin Value", "Stack Limit", "Heal", "Description" },
                food.OrderBy(i => i.GoldCoinValue).ThenBy(i => i.DisplayName)
                    .Select(i => {
                        BasicItemLogic itemLogic = i.itemPrefab.GetComponent<BasicItemLogic>();
                        return new[] {
                            i.DisplayName, i.NameID, i.GoldCoinValue.ToString(), i.StackLimit.ToString(),
                            itemLogic.healAmount.ToString(),
                            i.Description
                        };
                    }).ToArray()
            );

            AddText("\n\n=== Potions ===\n");
            AddTable("",
                new[] { "Name", "Item ID", "Coin Value", "Stack Limit", "Effect", "Effect Duration in Seconds", "Description" },
                potions.OrderBy(i => i.GoldCoinValue).ThenBy(i => i.DisplayName)
                    .Select(i => {
                        BasicItemLogic itemLogic = i.itemPrefab.GetComponent<BasicItemLogic>();
                        return new[] {
                            i.DisplayName, i.NameID, i.GoldCoinValue.ToString(), i.StackLimit.ToString(),
                            itemLogic.applyEffectOnConsume.DisplayName, itemLogic.effectDuration.ToString(CultureInfo.InvariantCulture),
                            i.Description
                        };
                    }).ToArray()
            );

            AddText("\n\n== Uncategorized ==\n");
            AddTable("",
                new[] { "Name", "Item ID", "Coin Value", "Stack Limit", "Description" },
                uncategorized.OrderBy(i => i.GoldCoinValue).ThenBy(i => i.DisplayName)
                    .Select(i => new[] {
                        i.DisplayName, i.NameID, i.GoldCoinValue.ToString(), i.StackLimit.ToString(), i.Description
                    }).ToArray()
            );

            Plugin.Log.LogInfo("Finished documenting items.");
        }
    }
}
