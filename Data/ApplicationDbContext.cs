using Microsoft.EntityFrameworkCore;
using YopoBackend.Modules.InvitationCRUD.Models;
using YopoBackend.Modules.UserTypeCRUD.Models;
using YopoBackend.Modules.UserCRUD.Models;
using YopoBackend.Modules.BuildingCRUD.Models;
using YopoBackend.Modules.FloorCRUD.Models;
using YopoBackend.Modules.UnitCRUD.Models;
using YopoBackend.Modules.AmenityCRUD.Models;
using YopoBackend.Modules.TenantCRUD.Models;
using YopoBackend.Modules.IntercomCRUD.Models;
using YopoBackend.Modules.Messaging.Models;
using YopoBackend.Modules.ThreadSocial.Models;
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

        // Module: UserCRUD (Module ID: 3)
        /// <summary>
        /// Gets or sets the Users DbSet for managing user entities.
        /// </summary>
        public DbSet<User> Users { get; set; }

        /// <summary>
        /// Gets or sets the UserTokens DbSet for managing user authentication tokens.
        /// </summary>
        public DbSet<UserToken> UserTokens { get; set; }

        /// <summary>
        /// Gets or sets the UserBuildingPermissions DbSet for per-user building access control.
        /// </summary>
        public DbSet<YopoBackend.Modules.UserCRUD.Models.UserBuildingPermission> UserBuildingPermissions { get; set; }

        /// <summary>
        /// Gets or sets the Customers DbSet for managing customer entities (property managers).
        /// </summary>
        public DbSet<Customer> Customers { get; set; }

        // Module: BuildingCRUD (Module ID: 4)
        /// <summary>
        /// Gets or sets the Buildings DbSet for managing building entities.
        /// </summary>
        public DbSet<Building> Buildings { get; set; }

        // Module: FloorCRUD
        /// <summary>
        /// Gets or sets the Floors DbSet for managing floor entities.
        /// </summary>
        public DbSet<Floor> Floors { get; set; }

        // Module: UnitCRUD
        /// <summary>
        /// Gets or sets the Units DbSet for managing unit entities.
        /// </summary>
        public DbSet<Unit> Units { get; set; }

        // Module: AmenityCRUD
        /// <summary>
        /// Gets or sets the Amenities DbSet for managing amenity entities.
        /// </summary>
        public DbSet<Amenity> Amenities { get; set; }

        // Module: TenantCRUD
        /// <summary>
        /// Gets or sets the Tenants DbSet for managing tenant entities.
        /// </summary>
        public DbSet<Tenant> Tenants { get; set; }

        // Module: IntercomCRUD
        /// <summary>
        /// Gets or sets the Intercoms DbSet for managing intercom entities.
        /// </summary>
        public DbSet<Intercom> Intercoms { get; set; }

        /// <summary>
        /// Gets or sets the InvitationBuildings DbSet for mapping invitations to buildings.
        /// </summary>
        public DbSet<YopoBackend.Modules.InvitationCRUD.Models.InvitationBuilding> InvitationBuildings { get; set; }

        // Module: Intercom Access Control
        public DbSet<YopoBackend.Modules.IntercomAccess.Models.IntercomMasterPin> IntercomMasterPins { get; set; }
        public DbSet<YopoBackend.Modules.IntercomAccess.Models.IntercomUserPin> IntercomUserPins { get; set; }
        public DbSet<YopoBackend.Modules.IntercomAccess.Models.IntercomAccessLog> IntercomAccessLogs { get; set; }
        public DbSet<YopoBackend.Modules.IntercomAccess.Models.IntercomAccessCode> IntercomAccessCodes { get; set; }

        // Module: Messaging
        public DbSet<Message> Messages { get; set; }

        // Module: ThreadSocial
        public DbSet<ThreadPost> ThreadPosts { get; set; }
        public DbSet<ThreadComment> ThreadComments { get; set; }

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
                // Removed unique index on Name to allow multiple user types with the same name
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
            
            // Configure User entity (Module ID: 3)
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("datetime");
                entity.Property(e => e.Email).HasMaxLength(255);
                entity.Property(e => e.PasswordHash).HasMaxLength(255);
                entity.Property(e => e.Name).HasMaxLength(200);
                entity.Property(e => e.Address).HasMaxLength(500);
                entity.Property(e => e.PhoneNumber).HasMaxLength(20);
                entity.Property(e => e.ProfilePhoto).HasMaxLength(1000);
                
                // Configure foreign key relationship with UserType
                entity.HasOne(e => e.UserType)
                    .WithMany()
                    .HasForeignKey(e => e.UserTypeId)
                    .OnDelete(DeleteBehavior.Restrict); // Prevent deleting UserType if it has users
            });
            
            // Configure UserToken entity (Module ID: 3)
            modelBuilder.Entity<UserToken>(entity =>
            {
                entity.HasKey(e => e.Id);
                // Removed TokenValue index due to MySQL key length limitations (2000 chars * 4 bytes > 3072 bytes max)
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => new { e.UserId, e.TokenType });
                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.ExpiresAt)
                    .HasColumnType("datetime");
                entity.Property(e => e.LastUsedAt)
                    .HasColumnType("datetime");
                entity.Property(e => e.RevokedAt)
                    .HasColumnType("datetime");
                entity.Property(e => e.TokenValue).HasMaxLength(2000);
                entity.Property(e => e.TokenType).HasMaxLength(50);
                entity.Property(e => e.DeviceInfo).HasMaxLength(500);
                entity.Property(e => e.IpAddress).HasMaxLength(50);
                
                // Configure foreign key relationship with User
                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade); // Delete tokens when user is deleted
            });
            
            // Configure Customer entity (Module ID: 3)
            modelBuilder.Entity<Customer>(entity =>
            {
                entity.HasKey(e => e.CustomerId);
                entity.Property(e => e.CustomerId).ValueGeneratedNever(); // CustomerId is manually set to User.Id
                entity.HasIndex(e => e.CustomerName);
                entity.HasIndex(e => e.CompanyName);
                entity.HasIndex(e => e.CompanyLicense).IsUnique(); // Company license should be unique if provided
                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("datetime");
                entity.Property(e => e.CustomerName).HasMaxLength(200);
                entity.Property(e => e.CompanyName).HasMaxLength(300);
                entity.Property(e => e.CompanyAddress).HasMaxLength(500);
                entity.Property(e => e.CompanyLicense).HasMaxLength(100);
                
                // Configure foreign key relationship with User (one-to-one relationship)
                entity.HasOne(e => e.User)
                    .WithOne()
                    .HasForeignKey<Customer>(e => e.CustomerId)
                    .OnDelete(DeleteBehavior.Cascade); // Delete customer when user is deleted
            });
            
            // Configure Building entity (Module ID: 4)
            modelBuilder.Entity<Building>(entity =>
            {
                entity.HasKey(e => e.BuildingId);
                entity.HasIndex(e => e.CustomerId);
                entity.HasIndex(e => e.Name);
                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("datetime");
                entity.Property(e => e.Name).HasMaxLength(200);
                entity.Property(e => e.Address).HasMaxLength(500);
                
                // Configure foreign key relationship with Customer
                entity.HasOne(e => e.Customer)
                    .WithMany()
                    .HasForeignKey(e => e.CustomerId)
                    .OnDelete(DeleteBehavior.Cascade); // Delete buildings when customer is deleted
                
                // Configure foreign key relationship with User (CreatedBy)
                entity.HasOne(e => e.CreatedByUser)
                    .WithMany()
                    .HasForeignKey(e => e.CreatedBy)
                    .OnDelete(DeleteBehavior.Restrict); // Don't delete user if they created buildings
            });

            // Configure Floor entity (FloorCRUD)
            modelBuilder.Entity<Floor>(entity =>
            {
                entity.HasKey(e => e.FloorId);
                entity.HasIndex(e => e.BuildingId);
                entity.HasIndex(e => new { e.BuildingId, e.Number });
                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("datetime");
                entity.Property(e => e.Name).HasMaxLength(200);
                entity.Property(e => e.Type).HasMaxLength(100);
                entity.Property(e => e.Status).HasMaxLength(50);
                entity.Property(e => e.AreaSqFt).HasColumnType("decimal(18,2)");

                // Configure foreign key with Building
                entity.HasOne(e => e.Building)
                    .WithMany()
                    .HasForeignKey(e => e.BuildingId)
                    .OnDelete(DeleteBehavior.Cascade); // Delete floors when building is deleted
            });
            
            // Configure UserBuildingPermission entity (Module ID: 3)
            modelBuilder.Entity<YopoBackend.Modules.UserCRUD.Models.UserBuildingPermission>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.UserId, e.BuildingId }).IsUnique();
                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                // No direct Building navigation; enforce via queries (BuildingCRUD module)
            });

            // Configure InvitationBuilding entity (Module ID: 2)
            modelBuilder.Entity<YopoBackend.Modules.InvitationCRUD.Models.InvitationBuilding>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.InvitationId);
                entity.HasIndex(e => e.BuildingId);
                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");
            });

            // Configure Unit entity (UnitCRUD)
            modelBuilder.Entity<Unit>(entity =>
            {
                entity.HasKey(e => e.UnitId);
                entity.HasIndex(e => e.FloorId);
                entity.HasIndex(e => e.BuildingId);
                entity.HasIndex(e => new { e.FloorId, e.UnitNumber }).IsUnique();
                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("datetime");
                entity.Property(e => e.UnitNumber).HasMaxLength(50);
                entity.Property(e => e.Type).HasMaxLength(100);
                entity.Property(e => e.Category).HasMaxLength(50);
                entity.Property(e => e.Status).HasMaxLength(50);
                entity.Property(e => e.AreaSqFt).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Amenities).HasColumnType("json");
                

                entity.HasOne(e => e.Floor)
                    .WithMany()
                    .HasForeignKey(e => e.FloorId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Building)
                    .WithMany()
                    .HasForeignKey(e => e.BuildingId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Tenant)
                    .WithMany()
                    .HasForeignKey(e => e.TenantId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Owner)
                    .WithMany()
                    .HasForeignKey(e => e.OwnerId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure Amenity entity (AmenityCRUD)
            modelBuilder.Entity<Amenity>(entity =>
            {
                entity.HasKey(e => e.AmenityId);
                entity.HasIndex(e => e.BuildingId);
                entity.HasIndex(e => e.FloorId);
                entity.HasIndex(e => e.Name);
                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("datetime");
                entity.Property(e => e.Name).HasMaxLength(200);
                entity.Property(e => e.Type).HasMaxLength(50);
                entity.Property(e => e.Description).HasMaxLength(1000);
                entity.Property(e => e.OpenHours).HasMaxLength(100);
                entity.Property(e => e.AccessControl).HasMaxLength(100);

                // Configure foreign key with Building
                entity.HasOne(e => e.Building)
                    .WithMany()
                    .HasForeignKey(e => e.BuildingId)
                    .OnDelete(DeleteBehavior.Cascade); // Delete amenities when building is deleted

                // Configure optional foreign key with Floor
                entity.HasOne(e => e.Floor)
                    .WithMany()
                    .HasForeignKey(e => e.FloorId)
                    .OnDelete(DeleteBehavior.Restrict); // Don't delete floor if it has amenities
            });

            // Configure Tenant entity (TenantCRUD)
            modelBuilder.Entity<Tenant>(entity =>
            {
                entity.HasKey(e => e.TenantId);
                entity.HasIndex(e => e.BuildingId);
                entity.HasIndex(e => e.FloorId);
                entity.HasIndex(e => e.UnitId);
                entity.HasIndex(e => e.TenantName);
                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("datetime");
                entity.Property(e => e.TenantName).HasMaxLength(200);
                entity.Property(e => e.Type).HasMaxLength(100);
                entity.Property(e => e.MemberType).HasMaxLength(100);
                entity.Property(e => e.DocumentFile).HasMaxLength(1000);

                entity.HasOne(e => e.Building)
                    .WithMany()
                    .HasForeignKey(e => e.BuildingId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Floor)
                    .WithMany()
                    .HasForeignKey(e => e.FloorId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.Unit)
                    .WithMany()
                    .HasForeignKey(e => e.UnitId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.CreatedByUser)
                    .WithMany()
                    .HasForeignKey(e => e.CreatedBy)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure Intercom entity (IntercomCRUD)
            modelBuilder.Entity<Intercom>(entity =>
            {
                entity.HasKey(e => e.IntercomId);
                entity.HasIndex(e => e.CustomerId);
                entity.HasIndex(e => e.BuildingId);
                entity.HasIndex(e => e.AmenityId);
                entity.HasIndex(e => e.IntercomName);
                entity.HasIndex(e => e.CreatedBy);
                entity.HasIndex(e => e.UpdatedBy);
                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("datetime");
                entity.Property(e => e.IntercomName).HasMaxLength(200);
                entity.Property(e => e.IntercomModel).HasMaxLength(100);
                entity.Property(e => e.IntercomType).HasMaxLength(50);
                entity.Property(e => e.IntercomSize).HasMaxLength(50);
                entity.Property(e => e.IntercomColor).HasMaxLength(50);
                entity.Property(e => e.OperatingSystem).HasMaxLength(50);
                entity.Property(e => e.InstalledLocation).HasMaxLength(200);
                entity.Property(e => e.Price).HasColumnType("decimal(18,2)");

                entity.HasOne(e => e.Customer)
                    .WithMany()
                    .HasForeignKey(e => e.CustomerId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Building)
                    .WithMany()
                    .HasForeignKey(e => e.BuildingId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Amenity)
                    .WithMany()
                    .HasForeignKey(e => e.AmenityId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.CreatedByUser)
                    .WithMany()
                    .HasForeignKey(e => e.CreatedBy)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.UpdatedByUser)
                    .WithMany()
                    .HasForeignKey(e => e.UpdatedBy)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure Intercom Access Control entities
            modelBuilder.Entity<YopoBackend.Modules.IntercomAccess.Models.IntercomMasterPin>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.IntercomId, e.IsActive });
                entity.HasOne(e => e.Intercom)
                      .WithMany()
                      .HasForeignKey(e => e.IntercomId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.CreatedByUser)
                      .WithMany()
                      .HasForeignKey(e => e.CreatedBy)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.UpdatedByUser)
                      .WithMany()
                      .HasForeignKey(e => e.UpdatedBy);
                entity.Property(e => e.PinHash).HasMaxLength(255);
                entity.Property(e => e.CreatedAt).HasColumnType("datetime").HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
            });

            modelBuilder.Entity<YopoBackend.Modules.IntercomAccess.Models.IntercomUserPin>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.IntercomId, e.UserId }).IsUnique();
                entity.HasOne(e => e.Intercom)
                      .WithMany()
                      .HasForeignKey(e => e.IntercomId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.User)
                      .WithMany()
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.CreatedByUser)
                      .WithMany()
                      .HasForeignKey(e => e.CreatedBy)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.UpdatedByUser)
                      .WithMany()
                      .HasForeignKey(e => e.UpdatedBy);
                entity.Property(e => e.PinHash).HasMaxLength(255);
                entity.Property(e => e.CreatedAt).HasColumnType("datetime").HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
            });

            modelBuilder.Entity<YopoBackend.Modules.IntercomAccess.Models.IntercomAccessLog>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.IntercomId);
                entity.HasIndex(e => e.OccurredAt);
                entity.Property(e => e.OccurredAt).HasColumnType("datetime");
                entity.Property(e => e.CredentialType).HasMaxLength(20);
                entity.Property(e => e.Reason).HasMaxLength(200);
                entity.HasOne(e => e.Intercom)
                      .WithMany()
                      .HasForeignKey(e => e.IntercomId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure IntercomAccessCode (building-level or intercom-level codes)
            modelBuilder.Entity<YopoBackend.Modules.IntercomAccess.Models.IntercomAccessCode>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.BuildingId);
                entity.HasIndex(e => e.IntercomId);
                entity.HasIndex(e => e.TenantId);
                entity.HasIndex(e => new { e.BuildingId, e.IntercomId, e.IsActive });
                entity.Property(e => e.CodeType).HasMaxLength(10);
                entity.Property(e => e.CodeHash).HasMaxLength(255);
                entity.Property(e => e.CreatedAt).HasColumnType("datetime").HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.ExpiresAt).HasColumnType("datetime");

                entity.HasOne(e => e.Building)
                      .WithMany()
                      .HasForeignKey(e => e.BuildingId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Intercom)
                      .WithMany()
                      .HasForeignKey(e => e.IntercomId)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.Tenant)
                      .WithMany()
                      .HasForeignKey(e => e.TenantId)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.CreatedByUser)
                      .WithMany()
                      .HasForeignKey(e => e.CreatedBy)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure Message entity (Messaging Module)
            modelBuilder.Entity<Message>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.SenderId);
                entity.HasIndex(e => e.ReceiverId);
                entity.HasIndex(e => new { e.ReceiverId, e.ReceiverType });
                entity.Property(e => e.SenderType).HasMaxLength(50);
                entity.Property(e => e.ReceiverType).HasMaxLength(50);
                entity.Property(e => e.Content).HasColumnType("text");
                entity.Property(e => e.CreatedAt).HasColumnType("datetime").HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
            });

            // Configure ThreadPost entity (ThreadSocial Module)
            modelBuilder.Entity<ThreadPost>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.AuthorId);
                entity.HasIndex(e => e.BuildingId);
                entity.Property(e => e.AuthorType).HasMaxLength(50);
                entity.Property(e => e.Content).HasColumnType("text");
                entity.Property(e => e.Image).HasColumnType("LONGBLOB");
                entity.Property(e => e.ImageMimeType).HasMaxLength(100);
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.CreatedAt).HasColumnType("datetime").HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
            });

            // Configure ThreadComment entity (ThreadSocial Module)
            modelBuilder.Entity<ThreadComment>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.PostId);
                entity.HasIndex(e => e.AuthorId);
                entity.HasIndex(e => e.ParentCommentId);
                entity.Property(e => e.AuthorType).HasMaxLength(50);
                entity.Property(e => e.Content).HasColumnType("text");
                entity.Property(e => e.CreatedAt).HasColumnType("datetime").HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

                entity.HasOne<ThreadPost>()
                    .WithMany()
                    .HasForeignKey(e => e.PostId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne<ThreadComment>()
                    .WithMany()
                    .HasForeignKey(e => e.ParentCommentId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

        }
    }
}
