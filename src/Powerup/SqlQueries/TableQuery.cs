using System.IO;
using System.Text;
using Powerup.SqlObjects;
using Powerup.Templates;

namespace Powerup.SqlQueries
{
    public class TableQuery : QueryBase
    {
        public TableQuery()
        {
            SetTextContentForTable();
        }

        public override string NameSql
        {
            get { return string.Format(nameSql, "= 'U'"); }
        }

        public override string Folder
        {
            get { return "up"; }
        }

        public override SqlType SqlType
        {
            get { return SqlType.Table; }
        }

        public override ITemplate TemplateToUse(SqlObject sqlObject)
        {
            return new TableDropCreateTemplate(sqlObject);
        }

        private void SetTextContentForTable()
        {
            var content = string.Empty;

            using ( var stream = this.GetType().Assembly
                .GetManifestResourceStream("Powerup.Templates.Sql.TableDropCreateQuery.txt") )
            using ( var ms = new MemoryStream() )
            {
                stream.CopyTo(ms);
                content =Encoding.ASCII.GetString(ms.ToArray());

                content = content.Replace("???", string.Empty); // kill BOM
                textQuery = content.Trim();
            }
        }

    }
}