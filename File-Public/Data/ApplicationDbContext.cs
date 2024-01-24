using File_Public.DbModels;
using Microsoft.EntityFrameworkCore;

namespace File_Public.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
            
        }
        public DbSet<Document> Documents { get; set; }
    }
}
