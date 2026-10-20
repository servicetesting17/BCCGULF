using Microsoft.Xrm.Sdk;
using System;

namespace BCCGULF.BatchJobs
{
    /// <summary>
    /// Writes plugin-style trace messages to the console so shared helpers can run outside a plugin.
    /// </summary>
    public class ConsoleTracingService : ITracingService
    {
        public bool Verbose { get; set; }

        public void Trace(string format, params object[] args)
        {
            if (!Verbose)
                return;

            Console.WriteLine("    [trace] " + (args == null || args.Length == 0 ? format : string.Format(format, args)));
        }
    }
}
