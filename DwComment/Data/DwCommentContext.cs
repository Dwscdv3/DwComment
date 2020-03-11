using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace DwComment.Models
{
    public class DwCommentContext : DbContext
    {
        public DwCommentContext(DbContextOptions<DwCommentContext> options)
            : base(options)
        {
            Database.Migrate();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Comment>()
                .HasIndex(c => c.ThreadId);
        }

        public DbSet<DwComment.Models.Comment> Comment { get; set; }
    }
}
