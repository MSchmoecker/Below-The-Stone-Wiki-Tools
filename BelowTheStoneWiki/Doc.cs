using System.IO;

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

        public void AddTable(string caption, string[] headers, string[][] rows) {
            AddText("{| class=\"wikitable\"");

            if (!string.IsNullOrEmpty(caption)) {
                AddText($"|+ {caption}");
            }

            AddText("|-");
            AddText("! " + string.Join(" !! ", headers));

            foreach (string[] row in rows) {
                AddText("|-");
                AddText("| " + string.Join(" || ", row).Replace("\n ", "\n"));
            }

            AddText("|}");
        }
    }
}
