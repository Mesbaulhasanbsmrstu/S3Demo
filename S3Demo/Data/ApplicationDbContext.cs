using Microsoft.EntityFrameworkCore;
using S3Demo.Models;

namespace S3Demo.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<S3FileDetails> S3FileDetails { get; set; }
    }
}
