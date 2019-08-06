﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;

namespace WowAucDumper
{
    class Arguments
    {
        [Option('i', "id", HelpText = "API client ID", Required = true)]
        public string ClientID { get; set; }

        [Option('s', "secret", HelpText = "API client Secret", Required = true)]
        public string ClientSecret { get; set; }

        [Option('r', "region", HelpText = "Region index", Required = true)]
        public string Region { get; set; }

        [Option('l', "realms", HelpText = "Realm name list", Required = true)]
        public IEnumerable<string> Realms { get; set; }
    }
}
