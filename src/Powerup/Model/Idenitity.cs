using System.CodeDom;
using System.Text;

namespace Powerup.Model
{
    public class Idenitity : Column
    {
        public Table Table { get; protected set; }
        private string template = string.Empty;

        public Idenitity(Table table, string name, string dataType, int maxlength, int precision, int scale, bool nullable) : 
            base(name, dataType, maxlength, precision, scale, nullable)
        {
            Table = table;
            BuildDefinition();
        }

        private void BuildDefinition()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("select id from syscolumns");
            builder.AppendLine("where object_name = '{0}' -- table name");
            builder.AppendLine("and columnproperty(id,name,'IsIdentity') = 1");
        }

        public string Script()
        {
            return string.Empty;
        }
    }
}