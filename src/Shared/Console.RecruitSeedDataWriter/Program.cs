using CommandLine;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using static System.Console;

namespace Console.RecruitSeedDataWriter
{
    class Program
    {
        public static async Task<int> Main(string[] args)
        {
            await CommandLine.Parser.Default.ParseArguments<Options>(args)
                                        .MapResult(opts => RunOptionsAndReturnExitCode(opts), ArgumentsNotParsed);
            
            if (Debugger.IsAttached) WaitPrompt();

            return 0;
        }

        private static Task ArgumentsNotParsed(IEnumerable<Error> errs)
        {
            Environment.Exit((int)SystemErrorCode.ERROR_INVALID_PARAMETER);
            return null;
        }

        private static async Task RunOptionsAndReturnExitCode(Options opts)
        {   
            string jsonContent = await GetDataContent(opts.InputFile);

            if (!string.IsNullOrWhiteSpace(jsonContent) && BsonDocument.TryParse(jsonContent, out var bsonDoc))
            {
                var writerOptions = new WriterOptions(opts.ConnectionString, opts.Collection);
                var writer = new MongoWriter(writerOptions);

                try
                {
                    var writeResult = await writer.Write(bsonDoc, opts.Overwrite).ConfigureAwait(false);
                    WriteSuccessLine($"Loading {opts.InputFile} into {opts.ConnectionString.DatabaseName} DB collection {opts.Collection} {writeResult.ToString().ToLower()}.");
                }
                catch (Exception ex)
                {
                    WriteErrorLine(ex.Message);
                    Environment.Exit((int)CustomErrorCode.FAILED_WRITE_DATA);
                }
            }
            else
            {
                WriteErrorLine($"File '{opts.InputFile}' is empty or contains invalid formatted json.");
                Environment.Exit((int)SystemErrorCode.ERROR_BAD_FORMAT);
            }
        }

        private static async Task<string> GetDataContent(string filePath)
        {
            CheckFileExists(filePath);

            try
            {
                return await File.ReadAllTextAsync(filePath);
            }
            catch (Exception)
            {
                WriteErrorLine($"Error reading file {filePath}.");
                Environment.Exit((int)SystemErrorCode.ERROR_BAD_FORMAT);
            }

            return null;
        }

        private static void CheckFileExists(string filePath)
        {
            if (!File.Exists(filePath))
            {
                WriteErrorLine($"Could not find file {filePath}.");
                Environment.Exit((int)SystemErrorCode.ERROR_FILE_NOT_FOUND);
            }
        }

        private static void WriteSuccessLine(string message)
        {
            ForegroundColor = ConsoleColor.Green;
            WriteLine(message);
            ForegroundColor = ConsoleColor.White;

            if (Debugger.IsAttached) WaitPrompt();
        }

        private static void WriteErrorLine(string message)
        {
            ForegroundColor = ConsoleColor.Red;
            WriteLine(message);
            ForegroundColor = ConsoleColor.White;
        }

        private static void WaitPrompt()
        {
            WriteLine("Press any key to continue");
            ReadKey(true);
        }
    }

    class Options
    {
        [Option('f', "file", Required = true, HelpText = "Input file to be processed.")]
        public string InputFile { get; set; }

        [Option('c', "collection", Default = "referenceData", HelpText = "Collection to which input data would be stored in.")]
        public string Collection { get; set; }

        [Option('o', "overwrite", Default = false, HelpText = "Replace any matching data that already exists in target.")]
        public bool Overwrite { get; set; }

        [Value(0, MetaName = "cosmos connection string", Required = true, HelpText = "A Cosmos MongoDb connection string that includes the target database name.")]
        public MongoUrl ConnectionString { get; set; }
    }
}
