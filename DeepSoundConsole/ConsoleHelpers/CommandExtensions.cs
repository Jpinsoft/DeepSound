using DeepSoundConsole.ConsoleHandlers;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeepSoundConsole.ConsoleHelpers
{
    public static class CommandExtensions
    {
        public static readonly Argument<string> InputFileArgument = new Argument<string>("input-file-or-folder") { Description = "Source carrier audio file or folder into which the application will try to hide secret files." };
        public static readonly Argument<string> ActionArgument = new Argument<string>("action") { DefaultValueFactory = (_) => "-analyze", Description = "Action to perform: '-analyze or -a' (default), '-encode or -e', '-decode or -d'." };

        public static readonly Option<string> EncodeFormatOpt = new Option<string>("-f", "-F") { DefaultValueFactory = (_) => "wav", Description = "Encoder output audio format: Supported formats: wav (default), flac, ape." };
        public static readonly Option<string> PassOpt = new Option<string>("-p", "-P") { Description = "Password for AES-256 encryption for use in ENCODE/DECODE modes." };
        public static readonly Option<string[]> SecretFilesOpt = new Option<string[]>("-s", "-S") { AllowMultipleArgumentsPerToken = true, Description = "Secret files: List of input files to hide inside the carrier audio file (space-separated)." };
        public static readonly Option<string> OutDirOpt = new Option<string>("-o", "-O") { Description = "Output directory. (Optional - default is the current working directory.)" };
        public static readonly Option<bool> RecursiveOpt = new Option<bool>("-r", "-R") { Description = "Recursively search for files in directories (Analyze mode only)" };

        public static void SetDescription(this RootCommand rootCommand)
        {
            rootCommand.Description = @"
EXAMPLE (ANALYZE): DeepSoundConsole.exe ""D:\audio.wav"" -analyze
EXAMPLE (ANALYZE): DeepSoundConsole.exe ""D:\INPUT_FOLDER"" -analyze -r  (NOTE - Do not use '\' char at the end of a folder name)
EXAMPLE (DECODE):  DeepSoundConsole.exe ""D:\audio.wav"" -decode -o ""D:\Out"" -p ""*mypass*""
EXAMPLE (ENCODE):  DeepSoundConsole.exe ""D:\audio.wav"" -encode -s ""D:\Secret1.bmp"" ""D:\Secret2.png"" -o ""D:\Out"" -p ""*mypass*"" -f ""ape""";
        }

        public static void SetOptions(this RootCommand rootCommand)
        {
            // Remove default version option
            rootCommand.Options.Remove(rootCommand.Options.FirstOrDefault(o => o is VersionOption));

            rootCommand.Add(InputFileArgument);
            rootCommand.Add(ActionArgument);
            rootCommand.Add(EncodeFormatOpt);
            rootCommand.Add(PassOpt);
            rootCommand.Add(SecretFilesOpt);
            rootCommand.Add(OutDirOpt);
            rootCommand.Add(RecursiveOpt);
        }

        public static void SetDeepSoundConsoleAction(this RootCommand rootCommand)
        {
            rootCommand.SetAction(parseResult =>
            {
                string? action = parseResult.GetValue<string>(CommandExtensions.ActionArgument)?.ToUpper();

                switch (action)
                {
                    // TODO: Use Dependency Injection for ICommandHandler if more handlers are added in the future
                    case "-A":
                    case "-ANALYZE":
                        new AnalyzeHandler().Handle(parseResult);
                        break;

                    case "-E":
                    case "-ENCODE":
                        new EncodeHandler().Handle(parseResult);
                        break;

                    case "-D":
                    case "-DECODE":
                        new DecodeHandler().Handle(parseResult);
                        break;

                    default:
                        throw new ArgumentException($"Invalid action: '{action}'. {CommandExtensions.ActionArgument.Description}");
                }
            });
        }
    }
}