using System;
using System.Configuration;
using Powerup.Commandline.Options;
using Powerup.Model;
using Powerup.Output;

namespace Powerup
{
    class Program
    {
        private static void Main(string[] args)
        {
            using (var runner = new PowerupRunner())
            {
                runner.Run(args);
            }
        }

        static void MainEx(string[] args)
        {
            if (ConfigurationManager.ConnectionStrings.Count == 0) return;

            var conn = new Configuration();
            var optionSet = ParseCommandLineOptions(conn);
            
            try
            {
                var app = new Application(conn);

                optionSet.Parse(args);
                if(!conn.IsValid)
                {
                    throw new OptionException("missing parameters","");
                }

                // access the database via the connection:
                var database = new Database() {Connection = conn.ConnectionStringBuilder.ToString()};
                database.Define();

                var directory_builder = new RHDirectoryBuilder(conn, database); 
                directory_builder.Build();
            }
            catch (OptionException optionException)
            {
                Console.WriteLine(optionException.Message);
                ShowTheHelp(optionSet);
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
                    o => configuration.ConnectionStringBuilder.Password = o);
              
            
            return optionSet;
        }

        private static void ShowHelp(string option)
        {
            if (option != null) throw new OptionException("Help", "");
        }
    }
}
