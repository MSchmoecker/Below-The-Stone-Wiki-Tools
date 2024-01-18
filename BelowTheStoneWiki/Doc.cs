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
    }
}
