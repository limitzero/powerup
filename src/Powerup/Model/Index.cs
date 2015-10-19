using System.Linq;
using System.Text;

namespace Powerup.Model
{
    public class Index : IScriptableDatabaseObject
    {
        private readonly string _tableName;
        private readonly string _script;
        private readonly Table _table;

        public Database Database { get; private set; }
        public string Owner { get; private set; }
        public string Name { get; private set; }
        public string QualifiedName { get; private set; }

        public Index(Database database, string name, string tableName, string script)
        {
            _tableName = tableName;
            _script = script;
            Database = database;
            Name = name;
            this.Owner = string.Empty;
            this.QualifiedName = string.Format("[{0}]", this.Name);
            this._table = database.Tables.FirstOrDefault(t => string.Format("[{0}]", t.Name) == _tableName);
        }

        public string Script()
        {
            StringBuilder builder = new StringBuilder();

            builder.AppendFormat(
               "IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'{0}') AND parent_object_id = OBJECT_ID(N'{1}'))",
               this.QualifiedName,
               this._table.QualifiedName).AppendLine();

            builder.AppendFormat("ALTER TABLE {0} DROP CONSTRAINT {1}",
                this._table.QualifiedName,
                this.QualifiedName).AppendLine();

            builder.AppendLine("GO")
                .AppendLine();

            builder.AppendLine(_script);
            builder.AppendLine("GO");

            return builder.ToString();
        }
    }
}