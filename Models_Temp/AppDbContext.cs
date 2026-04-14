using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace PMMS.Models_Temp;

public partial class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserRole> UserRoles { get; set; }

    public virtual DbSet<UserRoleMapping> UserRoleMappings { get; set; }

    public virtual DbSet<UserRoleMenuAccess> UserRoleMenuAccesses { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Data Source=103.120.179.247;Initial Catalog=padhyaso_CoreTemplate;Persist Security Info=True;User ID=padhyaso_admin;Password=satest@123;TrustServerCertificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("padhyaso_admin");

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users", "dbo");

            entity.Property(e => e.CreatedDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Email)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.NextChangePasswordDate).HasColumnName("Next_Change_Password_Date");
            entity.Property(e => e.NoOfWrongPasswordAttempts).HasColumnName("No_Of_Wrong_Password_Attempts");
        });

        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.ToTable("UserRoles", "dbo");

            entity.Property(e => e.CreatedDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<UserRoleMapping>(entity =>
        {
            entity.ToTable("UserRoleMapping", "dbo");

            entity.Property(e => e.CreatedDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
        });

        modelBuilder.Entity<UserRoleMenuAccess>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.RoleId, e.MenuId });

            entity.ToTable("UserRoleMenuAccess", "dbo");

            entity.Property(e => e.CreatedDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
