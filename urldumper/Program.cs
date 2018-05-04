using System;
using System.Collections.Generic;
using CommandLine;
using IniParser;

namespace urldumper
{
    class Options
    {
        [Option('i', "input", Required = false, HelpText = "Input file to be processed.")]
        public string InputFile { get; set; }

        [Option('o', "output", Required = false, HelpText = "Output file.")]
        public string OutputFile { get; set; }

        /*[Option('f', "format", Required = false, HelpText = "Format of the output file.")]
        public string Format { get; set; }*/

        // Omitting long name, defaults to name of property, ie "--verbose"
        [Option(Default = false, HelpText = "Prints all messages to standard output.")]
        public bool Verbose { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            CommandLine.Parser.Default.ParseArguments<Options>(args)
                .WithParsed<Options>(opts => RunOptionsAndReturnExitCode(opts))
                .WithNotParsed<Options>((errs) => HandleParseError(errs));
        }

        static void RunOptionsAndReturnExitCode(Options opts)
        {
            var input = opts.InputFile;
            var output = opts.OutputFile;
            if (String.IsNullOrEmpty(input))
            {
                string firefox = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Mozilla\Firefox";
                string profileini = $"{firefox}\\profiles.ini";
                var inidata = new FileIniDataParser().ReadFile(profileini);
                var profile = "";

                try
                {
                    profile = inidata["Profile0"]["Path"];
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine(e);
                    throw;
                }
                profile = $"{firefox}\\{profile}";
                input = $"{profile}\\sessionstore-backups\\recovery.jsonlz4";
            }
            if (String.IsNullOrEmpty(output))
            {
                output = $"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}\\links\\firefox{DateTime.Today.ToString("ddMMyyyy")}.html";
            }
            new UrlDumper().Dump(input, output);
        }

        static void HandleParseError(IEnumerable<Error> errs)
        {
            foreach(var err in errs)
            {
                Console.WriteLine(err.ToString());
            }
        }
    }
}
