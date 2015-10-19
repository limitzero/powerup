using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace Powerup.Model
{
    public class Table : IScriptableDatabaseObject
    {
        public Database Database { get; protected set; }
        public string Owner { get; protected set; }
        public string Name { get; protected set; }
        public Idenitity Idenitity { get; protected set; }
        public List<Column> Columns = new List<Column>();

        public Table(Database database, string name) :
            this(database, string.Empty, name)
        {
        }

        public Table(Database database, string owner, string name)
        {
            Database = database;
            Owner = owner;
            Name = name;
            BuildDefinition();
        }

        public string QualifiedName
        {
            get { return string.Format("[{0}].[{1}]", this.Owner, this.Name); }
        }

        public void AddColumn(string name, string type, int maxlength, int precision, int scale, bool nullable)
        {
            var column = new Column(name, type, maxlength, precision, scale, nullable);
            if ( this.Columns.Any(c => c.Name == name) == false )
                this.Columns.Add(column);
        }

        public string Script()
        {
            StringBuilder builder = new StringBuilder();

            builder.AppendFormat(
                "IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'{0}') AND type = N'U')",
                this.QualifiedName)
                .AppendLine();
            builder.AppendFormat("DROP TABLE {0}", this.QualifiedName).AppendLine();
            builder.AppendLine("GO").AppendLine();

            var query = @"declare @sql table(s varchar(1000), id int identity)
                    -- create statement
                    insert into  @sql(s) values ('CREATE TABLE  [{0}].[{1}] (')
 
                    -- column list
                    insert into @sql(s)
                    select 
                        '  ['+column_name+'] ' + 
                        upper(data_type) + coalesce('('+cast(character_maximum_length as varchar)+')','') + ' ' +
                        case when exists ( 
                            select id from syscolumns
                            where object_name(id)='{1}'
                            and name=column_name
                            and columnproperty(id,name,'IsIdentity') = 1 
                        ) then
                            'IDENTITY(' + 
                            cast(ident_seed('{1}') as varchar) + ',' + 
                            cast(ident_incr('{1}') as varchar) + ')'
                        else ''
                        end + ' ' +
                        ( case when IS_NULLABLE = 'No' then 'NOT ' else '' end ) + 'NULL ' + 
                        coalesce('DEFAULT '+COLUMN_DEFAULT,'') + ','
 
                     from information_schema.columns where table_name = '{1}'
                     order by ordinal_position
 
                    -- primary key
                    declare @pkname varchar(100)
                    select @pkname = constraint_name from information_schema.table_constraints
                    where table_name = '{1}' and constraint_type='PRIMARY KEY'
 
                    if ( @pkname is not null ) begin
                        insert into @sql(s) values('  PRIMARY KEY (')
                        insert into @sql(s)
                            select '   ['+COLUMN_NAME+'] ASC,' from information_schema.key_column_usage
                            where constraint_name = @pkname
                            order by ordinal_position
                        -- remove trailing comma
                        update @sql set s=left(s,len(s)-1) where id=@@identity
                        insert into @sql(s) values ('  )')
                    end
                    else begin
                        -- remove trailing comma
                        update @sql set s=left(s,len(s)-1) where id=@@identity
                    end
 
                    -- closing bracket
                    insert into @sql(s) values( ')' )
 
                    -- result!
                    select s from @sql order by id";

            using ( var connection = this.Database.GetConnection() )
            using ( var command = new SqlCommand(string.Format(query, this.Owner, this.Name), connection) )
            {
                connection.Open();
                using ( var reader = command.ExecuteReader() )
                {
                    while ( reader.Read() )
                    {
                        builder.AppendLine(reader[0].ToString());
                    }
                }
            }

            builder.AppendLine("GO");

            return builder.ToString();
        }

        private void BuildDefinition()
        {
            string query = @"SELECT
                c.name AS ColumnName
                ,t.name AS TypeName
                ,t.is_user_defined
                ,t.is_assembly_type
                ,c.is_identity
                ,c.is_rowguidcol
                ,c.is_xml_document
                ,c.max_length
                ,c.PRECISION
                ,c.scale
                ,c.is_nullable
                FROM sys.columns AS c
                JOIN sys.types AS t ON c.user_type_id=t.user_type_id
                where object_id('{0}') = c.object_id
                ORDER BY c.OBJECT_ID;";

            using ( var connnection = this.Database.GetConnection() )
            using ( var command = new SqlCommand(string.Format(query, this.QualifiedName), connnection) )
            {
                connnection.Open();
                using ( var reader = command.ExecuteReader() )
                {
                    if ( reader.HasRows == false )
                        return;
                    while ( reader.Read() )
                    {
                        var name = reader[0].ToString();
                        var type = reader[1].ToString();
                        var isIdentity = Convert.ToBoolean(reader[4].ToString());
                        var maxLength = Convert.ToInt32(reader[7].ToString());
                        var precision = Convert.ToInt32(reader[8].ToString());
                        var scale = Convert.ToInt32(reader[9].ToString());
                        var isNullable = Convert.ToBoolean(reader[10].ToString());

                        if ( isIdentity == true & this.Idenitity == null )
                            this.Idenitity = new Idenitity(this, name, type, maxLength, precision, scale, false);
                        else
                        {
                            this.AddColumn(name, type, maxLength, precision, scale, isNullable);
                        }
                    }
                }
            }
        }
    }
}