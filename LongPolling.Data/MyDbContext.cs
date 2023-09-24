using LongPolling.Data.Entity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LongPolling.Data
{
    public class MyDbContext : DbContext
    {
        public DbSet<Author> Authors { get; set; }
        public MyDbContext()
        {
        }

        public MyDbContext(DbContextOptions options) : base(options)
        {
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.UseInMemoryDatabase("test");
        }
    }
}
