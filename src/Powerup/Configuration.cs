using System.Data.SqlClient;

namespace Powerup
{
    public class Configuration
    {
        public string OutputFolder { get; internal set; }
        public string ConnectionString { get; set; }
        public bool IsSilent { get; set; }
        public SqlConnectionStringBuilder ConnectionStringBuilder { get; private set; }

        public Configuration()
        {
            ConnectionStringBuilder = new SqlConnectionStringBuilder();
        }

        public bool IsValid
        {
            get
            {
                // can have the full connection string or the database and server pieces:
                return !string.IsNullOrEmpty(ConnectionStringBuilder.ConnectionString) || 
                          (!string.IsNullOrEmpty(ConnectionStringBuilder.DataSource) &&
                          !string.IsNullOrEmpty(ConnectionStringBuilder.InitialCatalog)) &&
                          !string.IsNullOrEmpty(OutputFolder);
            }
        }


    }
}