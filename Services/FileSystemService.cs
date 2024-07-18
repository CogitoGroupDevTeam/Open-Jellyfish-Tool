
using System;
using System.IO;
using System.Threading.Tasks;

namespace JellyfishTool.Services {

    public class FileSystemService {
        
        public static async Task<string> ReadAsText(string input) {

            try {

                string path = input.Trim();

                const int MAX_PRINTABLE_LENGTH = 128;
                string printablePath = path.Length <= MAX_PRINTABLE_LENGTH ? path : path[..MAX_PRINTABLE_LENGTH];

                Console.WriteLine($"Reading text from: '{printablePath}'...");
                string content = await File.ReadAllTextAsync(path);
                return content;

            } catch (Exception) {
                Console.WriteLine("Input cannot be opened as a text file");
                return null;
            }
        }

        public static async Task<byte[]> ReadAsBytes(string input) {

            try {

                string path = input.Trim();

                const int MAX_PRINTABLE_LENGTH = 128;
                string printablePath = path.Length <= MAX_PRINTABLE_LENGTH ? path : path[..MAX_PRINTABLE_LENGTH];

                Console.WriteLine($"Reading bytes from: '{printablePath}'...");
                byte[] content = await File.ReadAllBytesAsync(path);
                return content;

            } catch (Exception) {
                Console.WriteLine("Input cannot be opened as a byte file");
                return null;
            }
        }

        public static async Task WriteAsText(string input, string path) {

            try {

                Console.WriteLine($"Writing text to: '{path}'...");
                await File.WriteAllTextAsync(path, input);

            } catch (Exception) {
                Console.WriteLine($"Input cannot be written as text to path: '{path}");
            }
        }
    }
}