using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;

namespace VersaTracker
{
    class Arguments
    {
        [Option('i', "id", HelpText = "API client ID", Required = true)]
        public string ClientID { get; set; }

        [Option('s', "secret", HelpText = "API client Secret", Required = true)]
        public string ClientSecret { get; set; }

        [Option('r', "region", HelpText = "Region index", Required = true)]
        public string Region { get; set; }

        [Option('l', "realms", HelpText = "Realm name list (recomended to use slug style)", Required = true)]
        public IEnumerable<string> Realms { get; set; }

        [Option('d', "desync", HelpText = "Desync threads after jobs started.")]
        public bool Desync { get; set; }
    }
}
