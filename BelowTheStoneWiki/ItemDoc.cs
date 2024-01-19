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
            List<ItemType> consumables = new List<ItemType>();

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
                            consumables.Add(itemType);
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
                ore.OrderBy(i => i.GoldCoinValue).ThenBy(i => i.DisplayName),
                new[] { "Name", "Item ID", "Coin Value", "Stack Limit", "Description" },
                i => new object[] { i.DisplayName, i.NameID, i.GoldCoinValue, i.StackLimit, i.Description }
            );

            AddText("\n\n=== Ingots ===\n");
            AddTable("",
                ingots.OrderBy(i => i.GoldCoinValue).ThenBy(i => i.DisplayName),
                new[] { "Name", "Item ID", "Coin Value", "Stack Limit", "Description" },
                i => new object[] { i.DisplayName, i.NameID, i.GoldCoinValue, i.StackLimit, i.Description }
            );

            AddText("\n\n=== Tools ===\n");
            AddTable("",
                tools.OrderBy(i => i.GoldCoinValue).ThenBy(i => i.DisplayName),
                new[] { "Name", "Item ID", "Coin Value", "Pickaxe Power", "Mining Tier", "Description" },
                i => {
                    PickaxeItemType pickaxe = i as PickaxeItemType;
                    return new object[] {
                        i.DisplayName, i.NameID, i.GoldCoinValue, pickaxe ? pickaxe.PickaxePower : (int?)null, pickaxe ? pickaxe.MiningTier : (int?)null,
                        i.Description
                    };
                }
            );

            AddText("\n\n=== Melee Weapons ===\n");
            AddTable("",
                meleeWeapons.OrderBy(i => i.GoldCoinValue).ThenBy(i => i.DisplayName),
                new[] { "Name", "Item ID", "Coin Value", "Damage", "Attack Speed", "Reach", "Sweep", "Knockback", "Description" },
                i => new object[] { i.DisplayName, i.NameID, i.GoldCoinValue, i.Damage, i.SwingRate, i.ReachDistance, i.SwingWidth, i.Knockback, i.Description }
            );

            AddText("\n\n=== Ranged Weapons ===\n");
            AddTable("",
                rangedWeapons.OrderBy(i => i.GoldCoinValue).ThenBy(i => i.DisplayName),
                new[] { "Name", "Item ID", "Coin Value", "Ammunition", "Ammunition Count", "Fire Rate", "Description" },
                i => {
                    RangedWeaponLogic itemLogic = i.itemPrefab.GetComponent<RangedWeaponLogic>();
                    return new object[] {
                        i.DisplayName, i.NameID, i.GoldCoinValue, itemLogic.ammunition.DisplayName, itemLogic.ammunitionCount, itemLogic.fireRate, i.Description
                    };
                }
            );

            AddText("\n\n=== Ammunition ===\n");
            AddTable("",
                ammo.OrderBy(i => i.GoldCoinValue).ThenBy(i => i.DisplayName),
                new[] { "Name", "Item ID", "Coin Value", "Stack Limit", "Description" },
                i => new object[] { i.DisplayName, i.NameID, i.GoldCoinValue, i.StackLimit, i.Description }
            );

            AddText("\n\n=== Thrown Weapons ===\n");
            AddTable("",
                thrownWeapons.OrderBy(i => i.GoldCoinValue).ThenBy(i => i.DisplayName),
                new[] { "Name", "Item ID", "Coin Value", "Damage", "Description" },
                i => {
                    ThrowableItemLogic itemLogic = i.itemPrefab.GetComponent<ThrowableItemLogic>();
                    return new object[] { i.DisplayName, i.NameID, i.GoldCoinValue, itemLogic.projectileDamage, i.Description };
                }
            );

            AddText("\n\n=== Armor ===\n");
            AddTable("",
                armour.OrderBy(i => i.GoldCoinValue).ThenBy(i => i.DisplayName),
                new[] { "Name", "Item ID", "Coin Value", "Damage Resistance", "Body Part", "Description" },
                i => new object[] { i.DisplayName, i.NameID, i.GoldCoinValue, i.DamageResistence, i.BodyPart, i.Description }
            );

            AddText("\n\n=== Food ===\n");
            AddTable("",
                food.OrderBy(i => i.GoldCoinValue).ThenBy(i => i.DisplayName),
                new[] { "Name", "Item ID", "Coin Value", "Stack Limit", "Heal", "Description" },
                i => {
                    BasicItemLogic itemLogic = i.itemPrefab.GetComponent<BasicItemLogic>();
                    return new object[] { i.DisplayName, i.NameID, i.GoldCoinValue, i.StackLimit, itemLogic.healAmount, i.Description };
                }
            );

            AddText("\n\n=== Consumables ===\n");
            AddTable("",
                consumables.OrderBy(i => i.GoldCoinValue).ThenBy(i => i.DisplayName),
                new[] { "Name", "Item ID", "Coin Value", "Stack Limit", "Effect", "Effect Duration in Seconds", "Description" },
                i => {
                    BasicItemLogic itemLogic = i.itemPrefab.GetComponent<BasicItemLogic>();
                    return new object[] {
                        i.DisplayName, i.NameID, i.GoldCoinValue, i.StackLimit, itemLogic.applyEffectOnConsume.DisplayName, itemLogic.effectDuration,
                        i.Description
                    };
                }
            );

            AddText("\n\n== Uncategorized ==\n");
            AddTable("",
                uncategorized.OrderBy(i => i.GoldCoinValue).ThenBy(i => i.DisplayName),
                new[] { "Name", "Item ID", "Coin Value", "Stack Limit", "Description" },
                i => new object[] { i.DisplayName, i.NameID, i.GoldCoinValue, i.StackLimit, i.Description }
            );

            Plugin.Log.LogInfo("Finished documenting items.");
        }
    }
}
