using System.Linq;
using System.Text;

namespace Powerup.Model
{
    public class ForeignKey : IScriptableDatabaseObject
    {
        private readonly string _referencingColumnName;
        private readonly string _referencedColumnName;
        public Database Database { get; private set; }

        public string Owner { get; private set; }

        public string Name { get; protected set; }

        /// <summary>
        /// /// Gets or sets the table is the target of the data relationship
        /// </summary>
        public Table ReferencingTable { get; set; }

        /// <summary>
        /// Gets or sets the table is the source for the data relationship
        /// </summary>
        public Table ReferencedTable { get; set; }

        /// <summary>
        /// /// Gets or sets the column in the table is the target of the data relationship (requires the data)
        /// </summary>
        public Column ReferencingColumn { get; set; }

        /// <summary>
        /// /// Gets or sets the column in the table is the target of the data relationship (supplies the data)
        /// </summary>
        public Column ReferencedColumn { get; set; }

        public string QualifiedName { get; private set; }

        public ForeignKey(Database database, string name,
            string referencingTable,
            string referencedTable,
            string referencingColumnName,
            string referencedColumnName)
        {
            this.Database = database;
            this.Name = name;

            _referencingColumnName = referencingColumnName;
            _referencedColumnName = referencedColumnName;

            QualifiedName = string.Format("[{0}]", this.Name);

            this.ReferencingTable = database.FindTable(referencingTable);
            this.ReferencedTable = database.FindTable(referencedTable);

            if ( this.ReferencingTable != null )
            {
                this.ReferencingColumn =
                    this.ReferencingTable.Columns
                    .FirstOrDefault(c => c.QualifiedName == _referencingColumnName);
            }

            if ( this.ReferencedTable != null )
                this.ReferencedColumn = this.ReferencedTable.Columns
                    .FirstOrDefault(c => c.QualifiedName == _referencedColumnName);
        }

        public string Script()
        {
            StringBuilder builder = new StringBuilder();

            if ( this.ReferencedTable != null & this.ReferencingTable != null
                && this.ReferencedColumn != null & this.ReferencingColumn != null )
            {
                builder.AppendFormat(
                "IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'{0}') AND parent_object_id = OBJECT_ID(N'{1}'))",
                this.QualifiedName,
                this.ReferencingTable.QualifiedName).AppendLine();

                builder.AppendFormat("ALTER TABLE {0} DROP CONSTRAINT {1}",
                    this.ReferencingTable.QualifiedName,
                    this.QualifiedName).AppendLine();

                builder.AppendLine("GO").AppendLine();

                builder.AppendFormat("ALTER TABLE {0} WITH CHECK ADD CONSTRAINT {1} FOREIGN KEY([{2}])",
                    this.ReferencingTable.QualifiedName,
                    this.QualifiedName,
                    this.ReferencingColumn.Name).AppendLine();

                builder.AppendFormat("REFERENCES {0} ([{1}])",
                    this.ReferencedTable.Name,
                    this.ReferencedColumn.Name)
                    .AppendLine();

                builder.AppendLine("GO");
            }

            return builder.ToString();
        }
    }
}