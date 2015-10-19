using System;
using Powerup.Model;
using Powerup.Output;

namespace Powerup
{
    public class Application
    {
        private readonly Configuration _configuration;
        private Database _database;

        public Application(Configuration configuration)
        {
            this._configuration = configuration;
        }

        public void DefineTheDatabase()
        {
            Console.WriteLine("Inspecting and defining the objects for the database...");
            _database = new Database() {Connection = _configuration.ConnectionStringBuilder.ConnectionString};
            _database.Define();
        }

        public void BuildDirectoriesAndScriptOutEntities()
        {
            if (_database == null) return;

            Console.WriteLine("Building directories for RoundhousE and scripting objects...");
            var directory_builder = new RHDirectoryBuilder(_configuration, _database);
            directory_builder.Build();
        }
    }
}