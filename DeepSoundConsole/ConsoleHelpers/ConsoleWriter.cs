using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DeepSoundConsole.ConsoleHelpers
{
    public static class ConsoleWriter
    {
        public static void WriteLineWithColor(string text, ConsoleColor color, int inbound = 0)
        {
            ConsoleColor originalColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(new string(' ', inbound) + text);
            Console.ForegroundColor = originalColor;
        }

        public static void WriteLineStart()
        {
            Console.WriteLine();
            WriteLineWithColor($"----------------- START [{DateTime.Now.ToString("T", System.Globalization.CultureInfo.InvariantCulture)}] -------------------", ConsoleColor.Green);
        }

        public static void WriteLineError(string errMesg)
        {
            Console.WriteLine();
            WriteLineWithColor($"----------------- ERROR [{DateTime.Now.ToString("T", System.Globalization.CultureInfo.InvariantCulture)}] -------------------", ConsoleColor.Red);
            WriteLineWithColor(errMesg, ConsoleColor.Red);
        }

        public static void WriteLineEnd()
        {
            Console.WriteLine();
            WriteLineWithColor($"----------------- END [{DateTime.Now.ToString("T", System.Globalization.CultureInfo.InvariantCulture)}] --------------------", ConsoleColor.Blue);
        }

        public static void WriteLineAppInfo()
        {
            Console.WriteLine("=================================================================================");
            Console.WriteLine($"DeepSoundConsole - Command-line DeepSound encoder/decoder version {Assembly.GetExecutingAssembly()?.GetName()?.Version}");
            Console.WriteLine("DeepSoundConsole.exe is Licenced under GPLv3 licence,");
            Console.WriteLine("Author Jozef Bátora, https://github.com/Jpinsoft/DeepSound");
            Console.WriteLine("=================================================================================");
        }
    }
}
