using ITMO_FinalWork.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ITMO_FinalWork.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<Passport> Passports { get; set; }
        public DbSet<MigrationCard> MigrationCards { get; set; }
        public DbSet<StayReason> StayReasons { get; set; }
        public DbSet<RegistrationReg> RegistrationReg { get; set; }
        public DbSet<MigrationReg> MigrationReg { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Настройка уникального индекса для паспорта
            modelBuilder.Entity<Passport>()
                .HasIndex(p => new { p.Series, p.Number })
                .IsUnique();

            // Настройка значений по умолчанию для даты регистрации
            modelBuilder.Entity<MigrationReg>()
                .Property(m => m.RegistrationDate)
                .HasDefaultValueSql("GETDATE()");

            modelBuilder.Entity<RegistrationReg>()
                .Property(r => r.RegistrationDate)
                .HasDefaultValueSql("GETDATE()");

            // Настройка проверочных ограничений
            modelBuilder.Entity<StayReason>()
                .Property(s => s.ReasonType)
                .HasConversion<string>();

            modelBuilder.Entity<StayReason>()
                .Property(s => s.DocumentType)
                .HasConversion<string>();

            modelBuilder.Entity<MigrationReg>()
                .Property(m => m.Gender)
                .HasConversion<string>();
        }
    }
}
