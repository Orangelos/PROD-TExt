using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Diplom.Models; 

namespace Diplom.Models
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<DiplomDbContext>
    {
        public DiplomDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<DiplomDbContext>();
            // Строка подключения из вашего appsettings.json
            optionsBuilder.UseSqlServer("Server=localhost\\SQLEXPRESS;Database=DiplomDB;Integrated Security=True;MultipleActiveResultSets=true;TrustServerCertificate=True;");
            return new DiplomDbContext(optionsBuilder.Options);
        }
    }
}