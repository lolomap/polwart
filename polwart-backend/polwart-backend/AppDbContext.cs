using Microsoft.EntityFrameworkCore;
using polwart_backend.Entities;

namespace polwart_backend
{
    public partial class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("user_pkey");

                entity.ToTable("user");

                entity.Property(e => e.Id)
                    .UseIdentityAlwaysColumn()
                    .HasColumnName("id");
                entity.Property(e => e.Login)
                    .HasMaxLength(50)
                    .HasColumnName("login");
                entity.Property(e => e.Name)
                    .HasMaxLength(50)
                    .HasColumnName("name");
                entity.Property(e => e.Password).HasColumnName("password");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }

}
