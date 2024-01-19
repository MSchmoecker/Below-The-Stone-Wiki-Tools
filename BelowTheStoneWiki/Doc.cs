using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

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

        private string ConvertToString(object row) {
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
    }
}
