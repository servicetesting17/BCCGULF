using BCCGULF.BatchJobs.Jobs;
using Microsoft.PowerPlatform.Dataverse.Client;
using System;
using System.Linq;

namespace BCCGULF.BatchJobs
{
    /// <summary>
    /// Usage:
    ///   BatchJobs projected-leaves --url https://bccgulf.crm15.dynamics.com [--commit] [--verbose]
    ///   BatchJobs projected-leaves --connection "&lt;Dataverse connection string&gt;" [--commit]
    ///
    /// The connection string can also come from the BCCGULF_DATAVERSE_CONNECTION environment variable.
    /// Without --commit the job only prints what it would do.
    /// </summary>
    public static class Program
    {
        private const string ConnectionEnvironmentVariable = "BCCGULF_DATAVERSE_CONNECTION";

        // Microsoft's sample app registration for interactive sign-in during development
        private const string InteractiveConnectionFormat =
            "AuthType=OAuth;Url={0};AppId=51f81489-12ee-4a9e-aaae-a2591f45987d;RedirectUri=app://58145B91-0C36-4500-8554-080854F2AC97;LoginPrompt=Auto";

        public static int Main(string[] args)
        {
            string job = args.FirstOrDefault(a => !a.StartsWith("--"));
            bool commit = args.Contains("--commit");
            bool verbose = args.Contains("--verbose");

            string connectionString = GetOption(args, "--connection")
                ?? (GetOption(args, "--url") != null ? string.Format(InteractiveConnectionFormat, GetOption(args, "--url")) : null)
                ?? Environment.GetEnvironmentVariable(ConnectionEnvironmentVariable);

            if (string.IsNullOrWhiteSpace(job) || string.IsNullOrWhiteSpace(connectionString))
            {
                PrintUsage();
                return 1;
            }

            try
            {
                using (var service = new ServiceClient(connectionString))
                {
                    if (!service.IsReady)
                    {
                        Console.Error.WriteLine("Could not connect to Dataverse: " + service.LastError);
                        return 1;
                    }

                    Console.WriteLine("Connected to {0} ({1})", service.ConnectedOrgFriendlyName, commit ? "COMMIT" : "DRY RUN");
                    Console.WriteLine();

                    var tracingService = new ConsoleTracingService { Verbose = verbose };

                    switch (job.ToLowerInvariant())
                    {
                        case "projected-leaves":
                            new ProjectedLeavesBackfillJob(service, tracingService, commit).Run();
                            break;

                        default:
                            Console.Error.WriteLine("Unknown job: " + job);
                            PrintUsage();
                            return 1;
                    }
                }

                return 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Batch job failed: " + ex);
                return 1;
            }
        }

        private static string GetOption(string[] args, string name)
        {
            int index = Array.IndexOf(args, name);
            return index >= 0 && index + 1 < args.Length ? args[index + 1] : null;
        }

        private static void PrintUsage()
        {
            Console.WriteLine("Usage: BatchJobs <job> (--url <org url> | --connection <connection string>) [--commit] [--verbose]");
            Console.WriteLine("       The connection string can also be set in " + ConnectionEnvironmentVariable + ".");
            Console.WriteLine();
            Console.WriteLine("Jobs:");
            Console.WriteLine("  projected-leaves   Create Projected Leaves (current leave year) for existing Bookable Resources");
            Console.WriteLine();
            Console.WriteLine("Without --commit the job runs as a dry run and creates nothing.");
        }
    }
}
