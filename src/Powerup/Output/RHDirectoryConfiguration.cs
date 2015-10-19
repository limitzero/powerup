using System;
using System.Collections.Generic;
using Powerup.Model;

namespace Powerup.Output
{
    public class RHDirectoryConfiguration
    {
        public IDictionary<Type, string> ObjectToDirectoryMapping { get; private set; }

        public RHDirectoryConfiguration()
        {
            this.ObjectToDirectoryMapping = new Dictionary<Type, string>();
            this.Configure();
        }

        private void Configure()
        {
            this.ObjectToDirectoryMapping.Add(typeof(Table), "tables");
            this.ObjectToDirectoryMapping.Add(typeof(Index), "indexes");
            this.ObjectToDirectoryMapping.Add(typeof(Function), "functions");
            this.ObjectToDirectoryMapping.Add(typeof(StoredProcedure), "sprocs");
            this.ObjectToDirectoryMapping.Add(typeof(ForeignKey), "indexes");  // RoundhousE does not have a directory for FK's....
            this.ObjectToDirectoryMapping.Add(typeof(View), "views");
            this.ObjectToDirectoryMapping.Add(typeof(Permission), "permissions");
        }
    }
}