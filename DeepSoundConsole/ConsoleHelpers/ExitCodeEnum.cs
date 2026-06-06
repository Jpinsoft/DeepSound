using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeepSoundConsole.ConsoleHelpers
{
    public enum ExitCodeEnum
    {
        Success = 0,
        InvalidLogin = 1,
        InvalidFilename = 2,
        UnknownError = 10,
        BadCommand = 22
    }
}
