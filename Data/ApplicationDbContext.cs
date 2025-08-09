using Microsoft.EntityFrameworkCore;
using YopoBackend.Modules.InvitationCRUD.Models;
using YopoBackend.Modules.UserTypeCRUD.Models;
using YopoBackend.Modules.BuildingCRUD.Models;
using YopoBackend.Models;

namespace YopoBackend.Data
{
    /// <summary>
    /// The main application database context that provides access to the database entities.
    /// </summary>
    public class ApplicationDbContext : DbContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationDbContext"/> class.
        /// </summary>
        /// <param name="options">The options for this context.</param>
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        /// <summary>
        /// Gets or sets the Modules DbSet for managing module entities.
        /// </summary>
        public DbSet<Module> Modules { get; set; }

        // Module: UserTypeCRUD (Module ID: 1)
        /// <summary>
        /// Gets or sets the UserTypes DbSet for managing user type entities.
        /// </summary>
        public DbSet<UserType> UserTypes { get; set; }

        /// <summary>
        /// Gets or sets the UserTypeModulePermissions DbSet for managing user type module permissions.
        /// </summary>
        public DbSet<UserTypeModulePermission> UserTypeModulePermissions { get; set; }

        // Module: InvitationCRUD (Module ID: 2)
        /// <summary>
        /// Gets or sets the Invitations DbSet for managing invitation entities.
        /// </summary>
        public DbSet<Invitation> Invitations { get; set; }

        // Module: BuildingCRUD (Module ID: 4)
        /// <summary>
        /// Gets or sets the Buildings DbSet for managing building entities.
        /// </summary>
        public DbSet<Building> Buildings { get; set; }

        /// <summary>
        /// Configures the model and entity relationships using the model builder.
        /// </summary>
        /// <param name="modelBuilder">The builder being used to construct the model for this context.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Configure Module entity
            modelBuilder.Entity<Module>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedNever(); // We manually set module IDs
                entity.HasIndex(e => e.Name).IsUnique();
                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("datetime");
                entity.Property(e => e.Name).HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.Version).HasMaxLength(20);
            });
            
            // Configure UserType entity (Module ID: 1)
            modelBuilder.Entity<UserType>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd(); // Allow both auto-generated and manual IDs
                entity.HasIndex(e => e.Name).IsUnique();
                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("datetime");
                entity.Property(e => e.Name).HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
            });
            
            // Configure UserTypeModulePermission entity (Module ID: 1)
            modelBuilder.Entity<UserTypeModulePermission>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");
                
                // Configure relationships
                entity.HasOne(e => e.UserType)
                    .WithMany(ut => ut.ModulePermissions)
                    .HasForeignKey(e => e.UserTypeId)
                    .OnDelete(DeleteBehavior.Cascade);
                
                entity.HasOne(e => e.Module)
                    .WithMany()
                    .HasForeignKey(e => e.ModuleId)
                    .OnDelete(DeleteBehavior.Restrict);
                
                // Create composite index for UserTypeId and ModuleId to prevent duplicates
                entity.HasIndex(e => new { e.UserTypeId, e.ModuleId }).IsUnique();
            });
            
            // Configure Invitation entity (Module ID: 2)
            modelBuilder.Entity<Invitation>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.EmailAddress);
                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.ExpiryTime)
                    .HasColumnType("datetime");
                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("datetime");
                entity.Property(e => e.EmailAddress).HasMaxLength(255);
                
                // Configure foreign key relationship with UserType
                entity.HasOne(e => e.UserType)
                    .WithMany()
                    .HasForeignKey(e => e.UserTypeId)
                    .OnDelete(DeleteBehavior.Restrict); // Prevent deleting UserType if it has invitations
            });
            
            // Configure Building entity (Module ID: 4)
            modelBuilder.Entity<Building>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Name).IsUnique();
                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("datetime");
                entity.Property(e => e.Name).HasMaxLength(200);
                entity.Property(e => e.Address).HasMaxLength(500);
                entity.Property(e => e.Photo).HasMaxLength(1000);
            });
        }
    }
}
