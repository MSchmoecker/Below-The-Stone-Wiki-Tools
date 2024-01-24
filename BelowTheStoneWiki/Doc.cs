using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using BelowTheStone.Crafting;
using UnityEngine;

namespace BelowTheStoneWiki {
    public class Doc {
        internal static string BaseDir { get; set; }

        public string FilePath { get; protected set; }

        private StreamWriter writer;

        public Doc(string filePath, string fileFormat = "wiki") {
            FilePath = Path.Combine(BaseDir, "data", $"{filePath}.{fileFormat}");

            // create directory if it doesn't exist
            Directory.CreateDirectory(Path.GetDirectoryName(FilePath));

            writer = File.CreateText(FilePath);
        }

        public void AddText(string text) {
            writer.WriteLine(text);
            writer.Flush();
        }

        public void AddTable(string caption, string[] headers, object[][] rows) {
            AddText("{| class=\"wikitable sortable\"");

            if (!string.IsNullOrEmpty(caption)) {
                AddText($"|+ {caption}");
            }

            AddText("|-");
            AddText("! " + string.Join(" !! ", headers));

            foreach (object[] row in rows) {
                AddText("|-");
                AddText("| " + string.Join(" || ", row.Select(ConvertToString)).Replace("\n ", "\n"));
            }

            AddText("|}");
        }

        protected static string ConvertToString(object row) {
            if (row == null) {
                return "-";
            } else if (row is string @string) {
                return @string;
            } else if (row is int @int) {
                return @int.ToString();
            } else if (row is float @float) {
                return @float.ToString(CultureInfo.InvariantCulture);
            } else if (row is Enum @enum) {
                return @enum.ToString();
            }

            Plugin.Log.LogWarning($"Unknown type {row.GetType()} in table, using ToString()");
            return row.ToString();
        }

        public void AddTable<T>(string caption, IEnumerable<T> items, string[] headers, Func<T, object[]> rowSelector) {
            List<object[]> rows = new List<object[]>();

            foreach (T item in items) {
                rows.Add(rowSelector(item));
            }

            AddTable(caption, headers, rows.ToArray());
        }

        protected static string LootToString(WeightedLoot loot) {
            if (loot.NoLoot) {
                return "-";
            }

            List<string> lootStrings = new List<string>();

            foreach (WeightedLoot.LootGroup group in loot.groups) {
                if (group.HasValidItems) {
                    string items = string.Empty;

                    for (int i = 0; i < group.items.Length; i++) {
                        float percent = Mathf.Round(group.chance / group.items.Length * 100f * 100f) / 100f;
                        items += $"{ConvertToString(percent)}% {group.items[i].DisplayName}";

                        if (i < group.items.Length - 1) {
                            items += ", ";
                        }
                    }

                    if (group.amount == 1) {
                        lootStrings.Add($"* {items}");
                    } else {
                        lootStrings.Add($"* {group.amount}x {items}");
                    }
                }
            }

            if (loot.allowEmptyLoot) {
                return "\n" + string.Join("\n", lootStrings) + "\n";
            } else {
                return "\n" + string.Join("\n", lootStrings) + "\n* Always drops something\n";
            }
        }

        protected string IngredientsToString(IReadOnlyList<Ingredient> ingredients) {
            return "\n" + string.Join("\n", ingredients.Select(i => $"* {i.ItemCount}x {i.ItemType.DisplayName}")) + "\n";
        }
    }
}
