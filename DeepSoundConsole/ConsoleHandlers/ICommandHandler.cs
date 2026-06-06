using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeepSoundConsole.ConsoleHandlers
{
    internal interface ICommandHandler
    {
        void Handle(ParseResult parseResult);
    }
}
