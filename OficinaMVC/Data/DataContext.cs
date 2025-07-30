using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OficinaMVC.Data.Entities;

namespace OficinaMVC.Data
{
    /// <summary>
    /// Represents the Entity Framework database context for the application, including all DbSet properties for entities.
    /// </summary>
    public class DataContext : IdentityDbContext<User>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DataContext"/> class.
        /// </summary>
        /// <param name="options">The options to be used by a <see cref="DbContext"/>.</param>
        public DataContext(DbContextOptions<DataContext> options)
            : base(options)
        {
        }

        /// <summary>
        /// Gets or sets the vehicles in the system.
        /// </summary>
        public DbSet<Vehicle> Vehicles { get; set; }

        /// <summary>
        /// Gets or sets the appointments in the system.
        /// </summary>
        public DbSet<Appointment> Appointments { get; set; }

        /// <summary>
        /// Gets or sets the repairs in the system.
        /// </summary>
        public DbSet<Repair> Repairs { get; set; }

        /// <summary>
        /// Gets or sets the specialties in the system.
        /// </summary>
        public DbSet<Specialty> Specialties { get; set; }

        /// <summary>
        /// Gets or sets the user-specialty relationships in the system.
        /// </summary>
        public DbSet<UserSpecialty> UserSpecialties { get; set; }

        /// <summary>
        /// Gets or sets the schedules in the system.
        /// </summary>
        public DbSet<Schedule> Schedules { get; set; }

        /// <summary>
        /// Gets or sets the repair types in the system.
        /// </summary>
        public DbSet<RepairType> RepairTypes { get; set; }

        /// <summary>
        /// Gets or sets the car brands in the system.
        /// </summary>
        public DbSet<Brand> Brands { get; set; }

        /// <summary>
        /// Gets or sets the car models in the system.
        /// </summary>
        public DbSet<CarModel> CarModels { get; set; }

        /// <summary>
        /// Gets or sets the parts in the system.
        /// </summary>
        public DbSet<Part> Parts { get; set; }

        /// <summary>
        /// Gets or sets the repair-part relationships in the system.
        /// </summary>
        public DbSet<RepairPart> RepairParts { get; set; }

        /// <summary>
        /// Gets or sets the invoices in the system.
        /// </summary>
        public DbSet<Invoice> Invoices { get; set; }

        /// <summary>
        /// Gets or sets the invoice items in the system.
        /// </summary>
        public DbSet<InvoiceItem> InvoiceItems { get; set; }


        /// <summary>
        /// Configures the entity relationships, indexes, and behaviors for the database model using the Entity Framework Fluent API.
        /// This method customizes table mappings, sets up keys, relationships, delete behaviors, and precision for properties.
        /// </summary>
        /// <param name="modelBuilder">The builder used to construct the model for the context.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>()
                .HasIndex(u => u.NIF)
                .IsUnique();

            modelBuilder.Entity<Vehicle>()
                .HasIndex(v => v.LicensePlate)
                .IsUnique();

            modelBuilder.Entity<Repair>()
                .Property(r => r.TotalCost)
                .HasPrecision(10, 2);

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
                .HasOne(us => us.Specialty)
                .WithMany(s => s.UserSpecialties)
                .HasForeignKey(us => us.SpecialtyId);


            modelBuilder.Entity<User>()
                .HasMany(u => u.UserSpecialties)
                .WithOne(us => us.User)
                .HasForeignKey(us => us.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<User>()
                .HasMany(u => u.Schedules)
                .WithOne(s => s.User)
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RepairPart>().HasKey(rp => new { rp.RepairId, rp.PartId });


            modelBuilder.Entity<RepairPart>()
                .HasOne(rp => rp.Repair)
                .WithMany(r => r.RepairParts)
                .HasForeignKey(rp => rp.RepairId);

            modelBuilder.Entity<RepairPart>()
                .HasOne(rp => rp.Part)
                .WithMany(p => p.RepairParts)
                .HasForeignKey(rp => rp.PartId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Repair)
                .WithOne(r => r.Appointment)
                .HasForeignKey<Repair>(r => r.AppointmentId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Invoice>()
                .HasOne(i => i.Repair)
                .WithMany()
                .HasForeignKey(i => i.RepairId);

            modelBuilder.Entity<InvoiceItem>()
                .HasOne(ii => ii.Invoice)
                .WithMany(i => i.InvoiceItems)
                .HasForeignKey(ii => ii.InvoiceId)
                .OnDelete(DeleteBehavior.Cascade);


            var cascadeFKs = modelBuilder.Model
                .GetEntityTypes()
                .SelectMany(t => t.GetForeignKeys())
                .Where(fk => !fk.IsOwnership && fk.DeleteBehavior == DeleteBehavior.Cascade);

            foreach (var fk in cascadeFKs)
            {
                fk.DeleteBehavior = DeleteBehavior.Restrict;
            }
        }
    }
}