using Jospin.DeepSound.Common.TempWav;
using Jospin.DeepSound.Steganography.Exceptions;
using Jospin.DeepSound.Steganography.Fragile;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jospin.Utils.Extensions;

namespace DeepSoundConsole.ConsoleHelpers
{
    internal static class DSConsoleUtils
    {
        public static void CheckDeepsoundOutputFormat(string carrierFile)
        {
            if (!TempWavFactory.Instance.IsOutputFileSupported(carrierFile))
                throw new NotSupportedException($"The input file '{carrierFile}' is not supported for analysis and decoding. Supported formats: {TempWavFactory.Instance.SupportedOutputFormats.FormatInline()}");
        }

        public static void LoadCarrierFile(Coder coder, string carrierFile)
        {
            Console.WriteLine($"Loading input audio file {carrierFile}");

            if (!TempWavFactory.Instance.IsInputFileSupported(carrierFile))
            {
                throw new NotSupportedException($"The input audio file '{carrierFile}' is not supported. Supported formats: {TempWavFactory.Instance.SupportedInputFormats.FormatInline()}");
            }

            string tempFileName = TempWavFactory.Instance[carrierFile].ToTempWav(carrierFile);

            if (string.Compare(carrierFile, tempFileName, true) != 0)
                Console.WriteLine($"Input audio file '{carrierFile}' was converted to '{tempFileName}'.");

            CarrierFileInfo carrierFileInfo;

            try
            {
                carrierFileInfo = coder.AnalyzeWav(tempFileName);
            }
            catch (KeyEnterCanceledException ex)
            {
                throw new KeyEnterCanceledException("The input file is a DeepSound audio file. It contains encrypted files; a password is required for decoding (Use -p <password> option).");
            }

            coder.BaseFile = new BaseFileInfoItem(tempFileName, coder.EncodeQualityMode, coder.BaseFileInfo.WavHeadLenth);
            coder.OriginalBaseFile = carrierFile;
        }
    }
}
