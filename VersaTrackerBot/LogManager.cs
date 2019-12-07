using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;
using NLog.Config;
using NLog.Targets;
using NLog.Targets.Wrappers;

namespace VersaTrackerBot
{
    class LogManager
    {
        public static Logger SetupLogger()
        {
            LoggingConfiguration config = new LoggingConfiguration();

            // ### FILE ###
            FileTarget logfile = new FileTarget()
            {
                FileName = string.Format(Path.Combine("logs", "{0}_{1}.txt"), typeof(Program).Namespace, DateTime.Now.ToString("yyyyMMdd.HHmmss")),
                Name = "logfile",
                KeepFileOpen = true,
                ConcurrentWrites = false,
                Encoding = Encoding.UTF8
            };
            AsyncTargetWrapper asyncfile = new AsyncTargetWrapper(logfile, 512, AsyncTargetWrapperOverflowAction.Grow);

            // ### CONSOLE ###
            ColoredConsoleTarget logconsole = new ColoredConsoleTarget() { Name = "logconsole", DetectConsoleAvailable = true };

            config.LoggingRules.Add(new LoggingRule("*", LogLevel.Trace, logconsole));
            config.LoggingRules.Add(new LoggingRule("*", LogLevel.Trace, asyncfile));

            NLog.LogManager.Configuration = config;
            return NLog.LogManager.GetCurrentClassLogger();
        }
    }
}