using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.Configuration;
using System.Linq;

namespace Bia.Internal.BookLibrary.Data
{
    public class BookDbContextFactory : IDesignTimeDbContextFactory<BookDbContext>
    {
        private readonly string _connectionString;

        public BookDbContextFactory()
        {

        }

        public BookDbContextFactory(string connectionString)
        {
            _connectionString = connectionString;
        }

        public BookDbContext CreateDbContext(string[] args)
        {
            var dbContextBuilder = new DbContextOptionsBuilder<BookDbContext>();

            string migrationAssemblyName = typeof(BookDbContext).Namespace;

            string connection = _connectionString;
            // Dev
            // = "Server=localhost;Port=3306;Database=booklib;Uid=UserAdmin;Pwd=P@ssw0rd";
            // vdi
            // = "Server=10.198.1.41;Port=3306;Database=booklib;Uid=UserAdmin;Pwd=P@ssw0rd"; 
            //test
            // = "Server=10.214.143.21;Port=3306;Database=booklib;Uid=UserAdmin;Pwd=LxjLf5BTFWIMk*";
            // Prod
            // = "Server=10.210.143.21;Port=3306;Database=booklib;Uid=UserAdmin;Pwd=LxjLf5BTFWIMk*"

            //if (args.Any())
            //{
            //    connection = args.First();
            //}
            //else
            //{
            //    connection = ConfigurationManager.ConnectionStrings[S.ConnectionStringName].ConnectionString;
            //}

            if (string.IsNullOrWhiteSpace(connection)) connection = _connectionString;

            dbContextBuilder.UseMySql(connection, optionsBuilder => optionsBuilder.MigrationsAssembly(migrationAssemblyName));

            return new BookDbContext(dbContextBuilder.Options);
        }

        public BookDbContext CreateDbContext()
        {
            return new BookDbContext(new DbContextOptionsBuilder<BookDbContext>().UseMySql(_connectionString).Options);
        }
    }

    public class BookDbContextProvider
    {
        private readonly DbContextOptionsBuilder<BookDbContext> _optionsBuilder;
        private readonly BookDbContextFactory _factory;

        public BookDbContextProvider(string connectionString)
        {
            _optionsBuilder = new DbContextOptionsBuilder<BookDbContext>();
            _optionsBuilder.UseMySql(connectionString);
            _factory = new BookDbContextFactory();
        }

        public DbContextOptions<BookDbContext> Options => _optionsBuilder.Options;

        public BookDbContext CreateDbContext()
        {
            return new BookDbContext(_optionsBuilder.Options);
        }

        public BookDbContextFactory GetRpsDbContextFactory()
        {
            return _factory;
        }

    }
}
