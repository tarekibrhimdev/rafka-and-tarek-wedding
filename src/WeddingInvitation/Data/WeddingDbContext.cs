using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace WeddingInvitation.Data;

public class WeddingDbContext(DbContextOptions<WeddingDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Guest> Guests => Set<Guest>();
    public DbSet<GuestFamilyMember> GuestFamilyMembers => Set<GuestFamilyMember>();
    public DbSet<Invitation> Invitations => Set<Invitation>();
    public DbSet<InvitationAuditEntry> InvitationAuditEntries => Set<InvitationAuditEntry>();
    public DbSet<ReceptionTable> ReceptionTables => Set<ReceptionTable>();
    public DbSet<GuestTableAssignment> GuestTableAssignments => Set<GuestTableAssignment>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            var clr = entityType.ClrType;
            if (clr is null || !typeof(EntityBase).IsAssignableFrom(clr) || clr.IsAbstract)
                continue;

            var method = typeof(WeddingDbContext).GetMethod(
                nameof(SetSoftDeleteFilter),
                System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
            var generic = method!.MakeGenericMethod(clr);
            generic.Invoke(null, new object[] { builder });
        }

        builder.Entity<Guest>(e =>
        {
            e.Property(g => g.DisplayName).HasMaxLength(200).IsRequired();
            e.Property(g => g.Email).HasMaxLength(256);
            e.Property(g => g.Phone).HasMaxLength(64);
            e.Property(g => g.Notes).HasMaxLength(2000);
        });

        builder.Entity<GuestFamilyMember>(e =>
        {
            e.Property(m => m.FullName).HasMaxLength(200).IsRequired();
            e.HasOne(m => m.Guest)
                .WithMany(g => g.FamilyMembers)
                .HasForeignKey(m => m.GuestId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Invitation>(e =>
        {
            e.Property(i => i.Token).HasMaxLength(128).IsRequired();
            e.HasIndex(i => i.Token).IsUnique();
            e.HasIndex(i => i.GuestId)
                .IsUnique()
                .HasFilter("IsRemoved = 0");
            e.HasOne(i => i.Guest)
                .WithOne(g => g.Invitation)
                .HasForeignKey<Invitation>(i => i.GuestId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<InvitationAuditEntry>(e =>
        {
            e.Property(a => a.Details).HasMaxLength(4000);
            e.HasOne(a => a.Invitation)
                .WithMany(i => i.AuditEntries)
                .HasForeignKey(a => a.InvitationId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<ReceptionTable>(e =>
        {
            e.Property(t => t.Name).HasMaxLength(120).IsRequired();
        });

        builder.Entity<GuestTableAssignment>(e =>
        {
            e.HasOne(a => a.Guest)
                .WithOne(g => g.TableAssignment)
                .HasForeignKey<GuestTableAssignment>(a => a.GuestId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(a => a.ReceptionTable)
                .WithMany(t => t.Assignments)
                .HasForeignKey(a => a.ReceptionTableId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(a => a.GuestId)
                .IsUnique()
                .HasFilter("IsRemoved = 0");
        });

        builder.Entity<ApplicationUser>(e =>
        {
            e.Property(u => u.CreatedAtUtc).IsRequired();
        });
    }

    private static void SetSoftDeleteFilter<TEntity>(ModelBuilder builder)
        where TEntity : EntityBase
    {
        builder.Entity<TEntity>().HasQueryFilter(e => !e.IsRemoved);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var utc = DateTime.UtcNow;
        foreach (var entry in ChangeTracker.Entries<EntityBase>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    if (entry.Entity.Id == Guid.Empty)
                        entry.Entity.Id = Guid.NewGuid();
                    entry.Entity.CreatedAtUtc = utc;
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedAtUtc = utc;
                    break;
            }
        }

        foreach (var entry in ChangeTracker.Entries<ApplicationUser>())
        {
            if (entry.State == EntityState.Added && entry.Entity.CreatedAtUtc == default)
                entry.Entity.CreatedAtUtc = utc;
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}
