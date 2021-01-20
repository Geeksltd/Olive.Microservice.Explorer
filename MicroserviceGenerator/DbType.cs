using System.Linq;
using NuGet;

namespace MicroserviceExplorer.MicroserviceGenerator
{
    public class DBType
    {
        public string Name, Provider, Driver, ConnectionString, Dialect, Manager;

        public string OliveProvider => "Olive.Entities.Data." + Provider;

        public string OliveVersion => GetPackageLatestVersion();

        public override string ToString() => Name;

        string GetPackageLatestVersion()
        {
            var repo = PackageRepositoryFactory.Default.CreateRepository("https://packages.nuget.org/api/v2");
            return repo.FindPackagesById(OliveProvider).Where(p => p.IsReleaseVersion()).Max(p => p.Version).Version.ToString();
        }

        public static DBType SqlServer { get; } = CreateSqlServer();
        public static DBType MySql { get; } = CreateMySql();
        public static DBType PostgreSql { get; } = CreatePostgreSQL();
        public static DBType SqLite { get; } = CreateSQlite();

        static DBType CreateSqlServer()
        {
            return new DBType
            {
                Manager = "SqlServerManager",
                Name = "Sql Server",
                Provider = "SqlServer",
                Driver = "System.Data.SqlClient",
                Dialect = "MSSQL",
                ConnectionString = @"Database=##DbName##.Temp; Server=.\\SQLExpress; Integrated Security=SSPI; MultipleActiveResultSets=True;"
            };
        }

        static DBType CreateMySql()
        {
            return new DBType
            {
                Manager = "MySqlManager",
                Name = "MySql",
                Provider = "MySql",
                Driver = "MySqlConnector",
                Dialect = "MySQL",
                ConnectionString = "Server=127.0.0.1; Database=##DbName##; Uid=root; Pwd=...;"
            };
        }

        static DBType CreateSQlite()
        {
            return new DBType
            {
                Manager = "SqLiteManager",
                Name = "SQLite",
                Provider = "SQLite",
                Driver = "Microsoft.Data.Sqlite.Core",
                Dialect = "SQLite",
                ConnectionString = @"Data Source=.\\Database.db; Version=3;"
            };
        }

        static DBType CreatePostgreSQL()
        {
            return new DBType
            {
                Manager = "PostgreSQLManager",
                Name = "PostgreSQL",
                Provider = "PostgreSQL",
                Driver = "Npgsql",
                Dialect = "PostgreSQL",
                ConnectionString = "User ID=root;Password=myPassword;Host=localhost;Port=5432;Database=##DbName##;Pooling = true; Min Pool Size = 0; Max Pool Size = 100; Connection Lifetime = 0;"
            };
        }
    }
}