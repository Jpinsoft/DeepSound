using DeepSoundConsole.ConsoleHelpers;
using Jospin.DeepSound.Common.Helpers;
using Jospin.DeepSound.Common.Modules;
using Jospin.DeepSound.Common.TempWav;
using Jospin.DeepSound.Steganography.Exceptions;
using Jospin.DeepSound.Steganography.Fragile;
using Jospin.Utils;
using Jospin.Utils.Extensions;
using Jospin.WPFUtils.Dialogs;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace DeepSoundConsole.ConsoleHandlers
{
    internal class EncodeHandler : ICommandHandler
    {
        private Coder coder;
        const int BufferSize = 1048576;
        string tempFileName;

        public EncodeHandler()
        {
            coder = new Coder();
            coder.OnKeyRequired += Coder_OnKeyRequired;
            coder.BuffSize = BufferSize;
        }

        private void Coder_OnKeyRequired(object? sender, KeyRequiredEventArgs e)
        {
            e.Cancel = true;
        }

        public void Handle(ParseResult parseResult)
        {
            string? carrierFile = parseResult.GetValue<string>(CommandExtensions.InputFileArgument);
            string? outputFolder = parseResult.GetValue<string>(CommandExtensions.OutDirOpt);
            string? pass = parseResult.GetValue<string>(CommandExtensions.PassOpt);
            string? outpuFormat = parseResult.GetValue<string>(CommandExtensions.EncodeFormatOpt);
            string[] secretFiles = parseResult.GetRequiredValue<string[]>(CommandExtensions.SecretFilesOpt);

            ConsoleWriter.WriteLineStart();
            Console.Write($"Starting to hide files into carrier '{carrierFile}'. Secret input files:\n - ");
            Console.WriteLine(string.Join($"{Environment.NewLine} - ", secretFiles));
            Console.WriteLine();

            coder.Encrypt = false;
            outputFolder = string.IsNullOrEmpty(outputFolder) ? DSHelpers.GetDeepSoundFolder() : outputFolder;

            if (!TempWavFactory.Instance.SupportedOutputFormats.Any(_ => string.Compare(_.Item1, outpuFormat, true) == 0))
            {
                throw new NotImplementedException($"Output format {outpuFormat} is not supported. Supported formats: {Environment.NewLine}{TuppleExt.FormatInline(TempWavFactory.Instance.SupportedOutputFormats, Environment.NewLine, (tuppleItem) => tuppleItem.Item2)}");
            }

            if (!string.IsNullOrEmpty(pass))
            {
                coder.Encrypt = true;
                coder.KeyUnicode = pass;
            }

            DSConsoleUtils.LoadCarrierFile(coder, carrierFile);

            if (coder.SecretFilesInfoItems.Count != 0)
            {
                ConsoleWriter.WriteLineWithColor($"Ending processing - the file already contains secret files.", ConsoleColor.DarkYellow, 2);
                return;
            }

            AddSecretFiles(secretFiles);

            coder.EncoderOutputFilePath = Path.Combine(outputFolder, Path.ChangeExtension(Path.GetFileName(carrierFile), "wav"));
            coder.EncoderOutputFilePath = FileUtils.GetUniqueFileNameWithoutExtension(coder.EncoderOutputFilePath);

            Console.WriteLine($"Starting EncodeFilesToWav {coder.EncoderOutputFilePath}");
            coder.EncodeFilesToWav();

            string outputFile = Path.ChangeExtension(coder.EncoderOutputFilePath, outpuFormat);
            TempWavFactory.Instance[outputFile].FromTempWav(coder.EncoderOutputFilePath);

            if (string.Compare(outputFile, coder.EncoderOutputFilePath, true) != 0)
                Console.WriteLine($"Temp file '{coder.EncoderOutputFilePath}' was converted to '{outputFile}'");

            Console.WriteLine($"Output file '{outputFile}' has been successfully created..");
        }


        private void AddSecretFiles(string[] files)
        {
            foreach (string fileName in files)
            {

                if (!coder.BaseFile.AddInnerFileSize((int)new FileInfo(fileName).Length))
                {
                    Console.WriteLine("Selected files exceed limit {0}B.", (coder.BaseFile.MaxInnerFilesSize - SecretFile.SecretHeadSize - SecretFile.SecretEndHeadSizeMax));
                    // TODo throw EX??
                    return;
                }

                coder.SecretFilesInfoItems.Add(new SecretFileInfoItem(fileName, true));
            }
        }

    }
}
