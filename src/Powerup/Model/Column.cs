using System.Collections;
using System.Text;

namespace Powerup.Model
{
    public class Column : IDatabaseObject, IScriptable
    {
        public Database Database { get; private set; }
        public string Owner { get; private set; }
        public string Name { get; protected set; }
        public string QualifiedName { get; private set; }
        public string DataType { get; set; }
        public int MaxLength { get; set; }
        public int Precision { get; set; }
        public int Scale { get; set; }
        public bool Nullable { get; set; }

        public Column(string name, string dataType, int maxlength, int precision, int scale, bool nullable)
        {
            Name = name;
            DataType = dataType;
            MaxLength = maxlength;
            Precision = precision;
            Scale = scale;
            Nullable = nullable;
            this.QualifiedName = string.Format("[{0}]", this.Name);
        }

        public string Script()
        {
            var builder = new StringBuilder();
            var dataType = this.DataType.Trim().ToLower();
            var nullableStatement = this.Nullable ? " NULL" : " NOT NULL";

            builder.AppendFormat("{0} ", this.QualifiedName);

            switch (dataType)
            {
                case "varchar":
                    builder.AppendFormat("[{0}]({1})", dataType, this.MaxLength);
                    break;
                case "nvarchar":
                    builder.AppendFormat("[{0}]({1})", dataType, this.MaxLength);
                    break;
                case "varbinary":
                    builder.AppendFormat("[{0}]({1})", dataType, this.MaxLength);
                    break;
                case "text":
                    builder.AppendFormat("[{0}]({1})", dataType, this.MaxLength);
                    break;
                case "numeric":
                    builder.AppendFormat("{0}({1}, {2})", dataType, this.Scale,
                        this.Precision);
                    break;
                case "decimal":
                    builder.AppendFormat("{0}({1}, {2})", dataType, this.Scale,
                          this.Precision);
                    break;
                case "datetime":
                    builder.AppendFormat("{0}", dataType);
                    break;
                case "smalldatetime":
                    builder.AppendFormat("{0}", dataType);
                    break;

                default:
                    builder.AppendFormat("[{0}]", dataType);
                    break;
            }

            builder.Append(nullableStatement);

            return builder.ToString();
        }
    }
}