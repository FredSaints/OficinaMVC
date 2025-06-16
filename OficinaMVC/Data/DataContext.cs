using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OficinaMVC.Data.Entities;

namespace OficinaMVC.Data
{
    public class DataContext : IdentityDbContext<User>
    {
        public DataContext(DbContextOptions<DataContext> options)
            : base(options)
        {
        }

        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<Repair> Repairs { get; set; }
        public DbSet<Specialty> Specialties { get; set; }
        public DbSet<UserSpecialty> UserSpecialties { get; set; }
        public DbSet<Schedule> Schedules { get; set; }
        public DbSet<RepairType> RepairTypes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {


            modelBuilder.Entity<User>()
                .HasIndex(u => u.NIF)
                .IsUnique();

            modelBuilder.Entity<Vehicle>()
                .HasIndex(v => v.LicensePlate)
                .IsUnique();

            modelBuilder.Entity<Repair>()
                .HasMany(r => r.Mechanics)
                .WithMany()
                .UsingEntity<Dictionary<string, object>>(
                    "RepairUser",
                    j => j
                        .HasOne<User>()
                        .WithMany()
                        .HasForeignKey("MechanicsId")
                        .OnDelete(DeleteBehavior.Restrict),
                    j => j
                        .HasOne<Repair>()
                        .WithMany()
                        .HasForeignKey("RepairId")
                        .OnDelete(DeleteBehavior.Restrict)
                );

            modelBuilder.Entity<Repair>()
                .Property(r => r.TotalCost)
                .HasPrecision(10, 2);

            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Client)
                .WithMany(u => u.Appointments)
                .HasForeignKey(a => a.ClientId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Mechanic)
                .WithMany()
                .HasForeignKey(a => a.MechanicId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<UserSpecialty>()
                .HasKey(us => new { us.UserId, us.SpecialtyId });

            modelBuilder.Entity<UserSpecialty>()
                .HasOne(us => us.User)
                .WithMany(u => u.UserSpecialties)
                .HasForeignKey(us => us.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserSpecialty>()
                .HasOne(us => us.Specialty)
                .WithMany(s => s.UserSpecialties)
                .HasForeignKey(us => us.SpecialtyId)
                .OnDelete(DeleteBehavior.Cascade);

            var cascadeFKs = modelBuilder.Model
                .GetEntityTypes()
                .SelectMany(t => t.GetForeignKeys())
                .Where(fk => !fk.IsOwnership && fk.DeleteBehavior == DeleteBehavior.Cascade);

            foreach (var fk in cascadeFKs)
            {
                fk.DeleteBehavior = DeleteBehavior.Restrict;
            }


            base.OnModelCreating(modelBuilder);
        }
    }
}