using DeepSoundConsole.ConsoleHelpers;
using Jospin.DeepSound.Common.Helpers;
using Jospin.DeepSound.Common.TempWav;
using Jospin.DeepSound.Steganography.Exceptions;
using Jospin.DeepSound.Steganography.Fragile;
using Jospin.Utils;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeepSoundConsole.ConsoleHandlers
{
    internal class DecodeHandler : ICommandHandler
    {
        private Coder coder;
        const int BufferSize = 1048576;
        string tempFileName;
        private string pass = "";

        public DecodeHandler()
        {
            coder = new Coder();
            coder.OnKeyRequired += Coder_OnKeyRequired;
            coder.BuffSize = BufferSize;
        }

        private void Coder_OnKeyRequired(object? sender, KeyRequiredEventArgs e)
        {
            if (string.IsNullOrEmpty(pass))
            {
                e.Cancel = true; // Cause throw KeyEnterCanceledException
                return;
            }

            e.Key = pass;
        }

        public void Handle(ParseResult parseResult)
        {
            string? carrierFile = parseResult.GetValue<string>(CommandExtensions.InputFileArgument);
            this.pass = parseResult.GetValue<string>(CommandExtensions.PassOpt);
            coder.DecoderFolder = parseResult.GetValue<string>(CommandExtensions.OutDirOpt) ?? DSHelpers.GetDeepSoundFolder();

            if (!Directory.Exists(coder.DecoderFolder))
                Directory.CreateDirectory(coder.DecoderFolder);

            ConsoleWriter.WriteLineStart();
            Console.WriteLine($"Starting decoding of secret files from carrier audio file '{carrierFile}'.");

            DSConsoleUtils.CheckDeepsoundOutputFormat(carrierFile);

            coder.Encrypt = false;

            DSConsoleUtils.LoadCarrierFile(coder, carrierFile);

            if (coder.SecretFilesInfoItems.Count == 0)
            {
                ConsoleWriter.WriteLineWithColor("Ending processing - The input file does not contain any secret files.", ConsoleColor.DarkYellow, 2);
                return;
            }

            Console.WriteLine("Decoding files...");
            coder.DecodeFilesFromWav();

            ConsoleWriter.WriteLineEnd();
            Console.WriteLine();
            coder.SecretFilesInfoItems.ForEach(_ => Console.WriteLine($"- {Path.Combine(coder.DecoderFolder, _.FileName)} ({_.FileSize} bytes)"));
            Console.WriteLine();
            Console.WriteLine("Decoding complete. Secret files were created in the output directory: '{0}'", Path.GetFullPath(coder.DecoderFolder));
        }
    }
}
