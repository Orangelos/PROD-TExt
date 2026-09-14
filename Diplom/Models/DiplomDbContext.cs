
using Microsoft.EntityFrameworkCore;
namespace Diplom.Models
{
    public class DiplomDbContext:DbContext
    {
        public DiplomDbContext(DbContextOptions<DiplomDbContext> options)
             : base(options)
        { }
        public DbSet<TextFile> TextFiles { get; set; }
    }
}
