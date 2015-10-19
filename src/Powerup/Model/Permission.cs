namespace Powerup.Model
{
    public class Permission : IScriptableDatabaseObject, ISupportQueryingForObjectDefinition
    {
        private readonly string _script;
        public Database Database { get; protected set; }
        public string Owner { get; private set; }
        public string Name { get; private set; }
        public string QualifiedName { get; private set; }

        public Permission(Database database, string script)
        {
            _script = script;
            Name = "permissions";
            this.Owner = string.Empty;
            this.QualifiedName = string.Empty;
            Database = database;
        }

        public string Script()
        {
            return _script;
        }

        public string GetQueryForObjectDefinition()
        {
            string query = @"
                -- citied from : http://www.sqlserver-query.com/script-out-of-database-users-in-sql-server/
                -- updates : suppressed PRINT statements with inserts into table for reading out contents row by row (supports copy & paste better)
                DECLARE @script table (s varchar(max), id int identity)

                DECLARE @DatabaseUserName [sysname] 
                SET NOCOUNT ON
                DECLARE
                @errStatement [varchar](8000),
                @msgStatement [varchar](8000),
                @DatabaseUserID [smallint],
                @ServerUserName [sysname],
                @RoleName [varchar](8000),
                @MEmberName [varchar](800),
                @ObjectID [int],
                @ObjectName [varchar](261)
                PRINT '——— CREATE USERS ————————————--'
                DECLARE _users
                CURSOR LOCAL FORWARD_ONLY READ_ONLY
                FOR 

                select [master].[sys].[server_principals].[name] ,
                         [sys].[database_principals].[name]
                from [sys].[database_principals] INNER JOIN [master].[sys].[server_principals]
                on [sys].[database_principals].[name]=[master].[sys].[server_principals].[name]
                where [master].[sys].[server_principals].[type] in ('U', 'G', 'S')

                OPEN _users FETCH NEXT FROM _users INTO @ServerUserNAme, @DatabaseUserName
                WHILE @@FETCH_STATUS = 0
                BEGIN

                SET @msgStatement = 'CREATE USER ['        --- ex: CREATE USER [jdoe] FOR LOGIN [jdoe] (windows authentication)
                 + @DatabaseUserName + ']' + ' FOR LOGIN [' + @ServerUserName + ']'  + CHAR(13) +  'GO' + CHAR(13)
                -- PRINT @msgStatement --> suppress print for insert
                INSERT INTO @script (s) values (@msgStatement)
                FETCH NEXT FROM _users INTO @ServerUserNAme, @DatabaseUserNAme
                END

                PRINT '——— CREATE DB ROLES————————-————'
                DECLARE _roles
                CURSOR LOCAL FORWARD_ONLY READ_ONLY 
                FOR
                select [NAME] from [sys].[database_principals] where type='R' and is_fixed_role != 1 and name not like 'public'
                OPEN _roles FETCH NEXT FROM _roles INTO @RoleName
                WHILE @@FETCH_STATUS=0
                BEGIN
                SET @msgStatement ='if not exists(SELECT 1 from sys.database_principals where type=''R'' and name ='''
                +@RoleName+''' ) '+ CHAR(13) +
                'BEGIN '+ CHAR(13) +
                'CREATE ROLE  ['+ @RoleName + ']'+CHAR(13) +
                'END' + CHAR(13)  + 'GO'  + CHAR(13)
                -- PRINT @msgStatement --> suppress print for insert
                INSERT INTO @script (s) values (@msgStatement)
                FETCH NEXT FROM _roles INTO @RoleName
                END

                PRINT '——— ADD ROLE MEMBERS———————--————--'
                DECLARE _role_members
                CURSOR LOCAL FORWARD_ONLY READ_ONLY
                FOR 
                SELECT a.name , b.name 
                from sys.database_role_members d INNER JOIN sys.database_principals  a
                                                on  d.role_principal_id=a.principal_id 
                                                 INNER JOIN sys.database_principals  b
                                                on d.member_principal_id=b.principal_id
                                                where    b.name <> 'dbo'
                                                order by 1,2

                OPEN _role_members FETCH NEXT FROM _role_members INTO @RoleName, @membername
                WHILE @@FETCH_STATUS = 0
                BEGIN
                SET @msgStatement = 'EXEC [sp_addrolemember] ' + '@rolename = [' + @RoleName + '], ' + '@membername = [' + @membername + ']' + CHAR(13)  + 'GO' + CHAR(13)
                -- PRINT @msgStatement --> suppress print for insert
                INSERT INTO @script (s) values (@msgStatement)
                FETCH NEXT FROM _role_members INTO @RoleName, @membername
                END

                -- SCRIPT GRANTS for Database Privileges
                PRINT '——— SCRIPT GRANTS for Database Privileges—————'
                INSERT INTO @script
                SELECT stmt = a.state_desc + ' ' + a.permission_name + ' ' + '[' + b.name + ']' + CHAR(13) +  'GO' + CHAR(13) COLLATE LATIN1_General_CI_AS
                FROM sys.database_permissions a inner join sys.database_principals b
                ON a.grantee_principal_id = b.principal_id 
                --WHERE b.principal_id not in (0,1,2) and a.type in ('VW','VWDS') --modified 9/17/2012
                WHERE b.principal_id not in (0,1,2) and a.type not in ('CO') and a.class = 0

                -- SCRIPT GRANTS for Schema Privileges
                PRINT '——— SCRIPT GRANTS for Schema Privileges—————'
                INSERT INTO @script
                SELECT stmt = a.state_desc + ' ' + a.permission_name + ' ' + 'ON SCHEMA::[' + b.name + ']' + ' ' + c.name + CHAR(13) + 'GO' + CHAR(13) COLLATE LATIN1_General_CI_AS
                FROM sys.database_permissions  a INNER JOIN sys.schemas b
                ON  a.major_id = b.schema_id INNER JOIN sys.database_principals c ON a.grantee_principal_id = c.principal_id

                -- SCRIPT GRANTS for Objects Level Privilegs
                PRINT '——— SCRIPT GRANTS for Object Privileges—————'
                INSERT INTO @script
                SELECT stmt =
                state_desc + ' ' + permission_name + ' on ['+ sys.schemas.name + '].[' + sys.objects.name + '] to [' + sys.database_principals.name + ']' + CHAR(13) + 'GO' + CHAR(13) COLLATE LATIN1_General_CI_AS
                from sys.database_permissions
                join sys.objects on sys.database_permissions.major_id = 
                sys.objects.object_id
                join sys.schemas on sys.objects.schema_id = sys.schemas.schema_id
                join sys.database_principals on sys.database_permissions.grantee_principal_id = 
                sys.database_principals.principal_id
                where sys.database_principals.name not in ( 'public', 'guest')
                --order by 1, 2, 3, 5

                -- results!!!
                select s from @script 

                -- PRINT 'GO'
        ";

            return query;
        }
    }
}