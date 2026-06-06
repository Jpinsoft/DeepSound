using DeepSoundConsole.ConsoleHandlers;
using DeepSoundConsole.ConsoleHelpers;
using Jospin.DeepSound.Common.TempWav;
using Jospin.DeepSound.Steganography.Exceptions;
using Jospin.Utils.Exceptions;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace DeepSoundConsole
{
    internal class Program
    {
        static int Main(string[] args)
        {
            ConsoleWriter.WriteLineAppInfo();
            Environment.ExitCode = (int)ExitCodeEnum.Success;
            Environment.CurrentDirectory = AppDomain.CurrentDomain.BaseDirectory;

            try
            {
                RootCommand rootCommand = new RootCommand();

                rootCommand.SetDescription();
                rootCommand.SetOptions();
                rootCommand.SetDeepSoundConsoleAction();

                ParseResult parseResult = rootCommand.Parse(args);
                // Invoke the command action directly without using the default exception handler, so we can catch exceptions and log them properly
                parseResult.Invoke(new InvocationConfiguration { EnableDefaultExceptionHandler = false });
            }
            catch (KeyEnterCanceledException ex)
            {
                ConsoleWriter.WriteLineWithColor("Ending processing - the input file is a DeepSound audio file. A password is required.", ConsoleColor.DarkYellow, 2);
            }
            catch (Exception ex)
            {
#if DEBUG
                ConsoleWriter.WriteLineError($"{DateTime.Now} - ERROR: {ExceptionUtils.AddInnerMessages(ex)}{Environment.NewLine}{Environment.NewLine} StackTrace: {ex.StackTrace}");
#else
                ConsoleWriter.WriteLineError($"ERROR: {ExceptionUtils.AddInnerMessages(ex)}");
#endif
                Environment.ExitCode = (int)ExitCodeEnum.UnknownError;
            }
            finally
            {
                TempWavFactory.Instance.ClearTemFolder();
            }

            ConsoleWriter.WriteLineEnd();

            return Environment.ExitCode;
        }
    }
}
