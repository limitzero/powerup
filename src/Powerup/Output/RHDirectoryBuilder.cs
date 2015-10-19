using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Powerup.Model;

namespace Powerup.Output
{
    public class RHDirectoryBuilder
    {
        private readonly Configuration _configuration;
        private readonly Database _database;
        private readonly RHDirectoryConfiguration _directory_configuration = new RHDirectoryConfiguration();

        public RHDirectoryBuilder(Configuration configuration, Database database)
        {
            _configuration = configuration;
            _database = database;
        }

        public void Build()
        {
            foreach ( var scriptable_type in _directory_configuration.ObjectToDirectoryMapping.Keys )
            {
                Script(scriptable_type);
            }
        }

        private void Script(Type scriptable_object_type)
        {
            var listing_of_type = typeof(List<>).MakeGenericType(scriptable_object_type);

            var scriptable_property = _database.GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .FirstOrDefault(p => p.PropertyType == listing_of_type);

            if ( scriptable_property == null )
                return;

            var object_script_directory = _directory_configuration
                .ObjectToDirectoryMapping[scriptable_object_type];

            var directory = Path.Combine(_configuration.OutputFolder, object_script_directory);

            if ( Directory.Exists(directory) == false )
            {
                Console.WriteLine("Creating directory '{0}'...", directory);
                Directory.CreateDirectory(directory);
            }

            var enumerable = scriptable_property.GetValue(_database, null) as IEnumerable;
            var iterator = enumerable.GetEnumerator();

            while ( iterator.MoveNext() )
            {
                var scriptable_object = iterator.Current as IScriptableDatabaseObject;

                if ( scriptable_object == null )
                    continue;

                var writer = new RHScriptableObjectWriter(scriptable_object, directory);
                writer.WriteContent();
            }
        }
    }
}