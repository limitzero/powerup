using System.Data.SqlClient;
using System.Text;

namespace Powerup.Model
{
    public class Function : IScriptableDatabaseObject
    {
        private readonly StringBuilder _script = new StringBuilder();
        public Database Database { get; private set; }
        public string Owner { get; protected set; }
        public string Name { get; protected set; }
        public string QualifiedName { get; private set; }

        public Function(Database database, string owner, string name)
        {
            Database = database;
            Owner = owner;
            Name = name;
            QualifiedName = string.Format("[{0}].[{1}]", this.Owner, this.Name);
            BuildDefintion();
        }

        public string Script()
        {
            return _script.ToString();
        }

        private void BuildDefintion()
        {
            _script.AppendFormat(
                "IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'{0}') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))",
                this.QualifiedName).AppendLine();
            _script.AppendFormat("DROP FUNCTION {0}", this.QualifiedName).AppendLine();
            _script.AppendLine("GO");

            var query = @"SELECT c.text
                FROM SYS.syscomments c
                where c.id = object_id('{0}')
                order by c.colid";

            using (var connnection = this.Database.GetConnection())
            using (var command = new SqlCommand(string.Format(query, this.QualifiedName), connnection))
            {
                connnection.Open();
                using (var reader = command.ExecuteReader())
                {
                    if (reader.HasRows == false) return;
                    while (reader.Read())
                    {
                        _script.AppendLine(reader[0].ToString());
                    }
                }
            }

            if (_script.Length > 0)
                _script.AppendLine("GO");
        }
    }
}