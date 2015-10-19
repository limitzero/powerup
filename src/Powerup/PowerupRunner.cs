using System;
using Powerup.Commandline.Options;

namespace Powerup
{
    public class PowerupRunner : IDisposable
    {
        public void Run(string[] args = null)
        {
            var title = string.Format("PowerUP - SQL database schema extractor and scripting tool for RoundhousE (v.{0})",
                this.GetType().Assembly.GetName().Version);

            Console.Title = title;

            var environment = String.Format("PowerUP console runner ({0}-bit .NET {1})", IntPtr.Size * 8, Environment.Version);
            Console.WriteLine(environment);

            var configuration = new Configuration();
            var optionSet = ParseCommandLineOptions(configuration);

            try
            {
                optionSet.Parse(args);
                ValidateConfiguration(configuration);

                var application = new Application(configuration);
                application.DefineTheDatabase();
                application.BuildDirectoriesAndScriptOutEntities();
            }
            catch (OptionException oex)
            {
                Console.WriteLine(oex.Message);
                ShowTheHelp(optionSet);
                Environment.Exit(-1);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Environment.Exit(-1);
            }

            if (configuration.IsSilent == false)
            {
                Console.WriteLine("Press any key to exit....");
                Console.ReadKey();
            }

            Environment.Exit(0);
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        private static void ValidateConfiguration(Configuration configuration)
        {
            if ( !configuration.IsValid )
            {
                throw new OptionException("missing parameters", "");
            }
        }

        private static void ShowTheHelp(OptionSet optionSet)
        {
            optionSet.WriteOptionDescriptions(Console.Out);
        }

        private static OptionSet ParseCommandLineOptions(Configuration configuration)
        {
            var optionSet = new OptionSet()
                .Add("?|h|help", ShowHelp)
                .Add("c|connection=",
                    "The connection string to use for access to the database.",
                    o => configuration.ConnectionStringBuilder.ConnectionString = o)
                .Add("d=|database=",
                    "Specify the name of a database to use.",
                    o => configuration.ConnectionStringBuilder.InitialCatalog = o)
                .Add("s=|server=",
                    "Specify the name of a server to use.",
                    o => configuration.ConnectionStringBuilder.DataSource = o)
                .Add("o=|output=",
                    "The location to write the generated files.",
                    o => configuration.OutputFolder = o)
                .Add("t=|trusted=",
                    "Whether connection uses integrated security or not.",
                    o => configuration.ConnectionStringBuilder.IntegratedSecurity = bool.Parse(o))
                .Add("u=|username=",
                    "Database username.",
                    o => configuration.ConnectionStringBuilder.UserID = o)
                .Add("p=|password=",
                    "Database password",
                    o => configuration.ConnectionStringBuilder.Password = o)
                .Add("q=|quiet",
                    "Suppresses prompts from console when running.",
                    o => configuration.IsSilent = bool.Parse(o));

            return optionSet;
        }

        private static void ShowHelp(string option)
        {
            if ( option != null )
                throw new OptionException("Help", "");
        }
    }
}