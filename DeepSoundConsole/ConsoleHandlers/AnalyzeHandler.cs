using DeepSoundConsole.ConsoleHelpers;
using Jospin.DeepSound.Common.TempWav;
using Jospin.DeepSound.Steganography.Exceptions;
using Jospin.DeepSound.Steganography.Fragile;
using Jospin.Utils.Extensions;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeepSoundConsole.ConsoleHandlers
{
    internal class AnalyzeHandler : ICommandHandler
    {
        private Coder coder;
        string tempFileName;

        public AnalyzeHandler()
        {
            coder = new Coder();
            coder.OnKeyRequired += Coder_OnKeyRequired;
        }

        private void Coder_OnKeyRequired(object? sender, KeyRequiredEventArgs e)
        {
            e.Cancel = true; // coder.AnalyzeWav vrati KeyEnterCanceledException
        }

        public void Handle(ParseResult parseResult)
        {
            string? carrierFilesFolder = parseResult.GetValue<string>(CommandExtensions.InputFileArgument);
            bool recursive = parseResult.GetValue<bool>(CommandExtensions.RecursiveOpt);

            ConsoleWriter.WriteLineStart();

            SearchOption searchOption = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            IEnumerable<FileInfo> audioFiles;

            if (Directory.Exists(carrierFilesFolder))
            {
                Console.WriteLine($"Analyzing directory: '{carrierFilesFolder}', recursive: {recursive}, target audio formats: ({TempWavFactory.Instance.SupportedOutputFormats.FormatInline()})");

                DirectoryInfo directoryInfo = new DirectoryInfo(carrierFilesFolder);

                audioFiles = TempWavFactory.Instance.SupportedOutputFormats
                   .SelectMany(tuppleItem => directoryInfo.GetFiles($"*.{tuppleItem.Item1}", searchOption));

                Console.WriteLine($"Found '{audioFiles.Count()}' files.");
            }
            else
            {
                if(!File.Exists(carrierFilesFolder))
                {
                    ConsoleWriter.WriteLineError($"File or directory does not exist: '{carrierFilesFolder}'");
                    return;
                }

                Console.WriteLine($"Analyzing file: '{carrierFilesFolder}'");
                DSConsoleUtils.CheckDeepsoundOutputFormat(carrierFilesFolder);

                audioFiles = new List<FileInfo> { new FileInfo(carrierFilesFolder) };
            }

            foreach (FileInfo audioFileInfo in audioFiles)
            {
                try
                {
                    Console.WriteLine();

                    coder.Encrypt = false;
                    DSConsoleUtils.LoadCarrierFile(coder, audioFileInfo.FullName);

                    if (coder.SecretFilesInfoItems.Count == 0)
                        ConsoleWriter.WriteLineWithColor("CLEAR  - The file does not contain any secret files.", ConsoleColor.Green, 2);
                    else
                    {
                        ConsoleWriter.WriteLineWithColor($"SECRET - The input file is a DeepSound audio file. Secret files:", ConsoleColor.DarkYellow, 2);
                        ConsoleWriter.WriteLineWithColor($"{Environment.NewLine}    - " + string.Join($"{Environment.NewLine}    - ", coder.SecretFilesInfoItems.Select(_ => _.FileName)), ConsoleColor.DarkYellow, 2);
                    }
                }
                catch (KeyEnterCanceledException)
                {
                    ConsoleWriter.WriteLineWithColor("SECRET - The input file is a DeepSound audio file. A password is required.", ConsoleColor.DarkYellow, 2);
                }
                catch (Exception ex)
                {
                    ConsoleHelpers.ConsoleWriter.WriteLineWithColor($"Error analyzing file '{audioFileInfo.FullName}': {ex.Message}", ConsoleColor.Red, 2);
                }
            }
        }

    }
}
