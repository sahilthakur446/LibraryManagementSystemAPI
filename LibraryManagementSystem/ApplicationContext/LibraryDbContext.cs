using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Models;

public partial class LibraryDbContext : DbContext
{
    public LibraryDbContext()
    {
    }

    public LibraryDbContext(DbContextOptions<LibraryDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Author> Authors { get; set; }

    public virtual DbSet<Book> Books { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<User> Users { get; set; }
    public virtual DbSet<BorrowedBook> BorrowedBooks { get; set; }

    public virtual DbSet<BorrowedBookStatus> BorrowedBookStatuses { get; set; }

    public virtual DbSet<Fine> Fines { get; set; }

    public virtual DbSet<FineStatus> FineStatuses { get; set; }

    public virtual DbSet<Reservation> Reservations { get; set; }

    public virtual DbSet<ReservationStatus> ReservationStatuses { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Author>(entity =>
        {
            entity.HasKey(e => e.AuthorId).HasName("PK__Authors__70DAFC349CC131A1");

            entity.HasIndex(e => e.AuthorName, "UQ__Authors__4A1A120BBA247B75").IsUnique();

            entity.Property(e => e.AuthorName)
                .HasMaxLength(255)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Book>(entity =>
        {
            entity.HasKey(e => e.BookId).HasName("PK__Books__3DE0C207DEC1EE11");

            entity.HasIndex(e => e.Isbn, "UQ__Books__447D36EA67EC9966").IsUnique();

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Isbn)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("ISBN");
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();
            entity.Property(e => e.Title)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.CategoryId).HasName("PK__Categori__19093A0B995E988A");

            entity.HasIndex(e => e.CategoryName, "UQ__Categori__8517B2E06283AACD").IsUnique();

            entity.Property(e => e.CategoryName)
                .HasMaxLength(255)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PK__Roles__8AFACE1A5096FB41");

            entity.HasIndex(e => e.RoleName, "UQ__Roles__8A2B61602DA75CE0").IsUnique();

            entity.Property(e => e.RoleName)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__1788CC4CAC010669");

            entity.HasIndex(e => e.Email, "UQ__Users__A9D105348C001546").IsUnique();

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.FirstName)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.LastName)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.PasswordHash).HasMaxLength(255);
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .HasForeignKey(d => d.RoleId)
                .HasConstraintName("FK__Users__RoleId__5812160E");
        });

        modelBuilder.Entity<BorrowedBook>(entity =>
        {
            entity.HasKey(e => e.BorrowId).HasName("PK__Borrowed__4295F83F042C3ACD");

            entity.Property(e => e.BorrowDate)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.DueDate).HasColumnType("datetime");
            entity.Property(e => e.FineAmount).HasDefaultValue(0);
            entity.Property(e => e.ReturnDate).HasColumnType("datetime");

            entity.HasOne(d => d.Status).WithMany(p => p.BorrowedBooks)
                .HasForeignKey(d => d.StatusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__BorrowedB__Statu__208CD6FA");
        });

        modelBuilder.Entity<BorrowedBookStatus>(entity =>
        {
            entity.HasKey(e => e.StatusId).HasName("PK__Borrowed__C8EE2063F71CD66B");

            entity.ToTable("BorrowedBookStatus");

            entity.HasIndex(e => e.StatusName, "UQ__Borrowed__05E7698A37D3FB9C").IsUnique();

            entity.Property(e => e.StatusName)
                .HasMaxLength(100)
                .IsUnicode(false);
        });
        modelBuilder.Entity<Fine>(entity =>
        {
            entity.HasKey(e => e.FineId).HasName("PK__Fines__9D4A9B2C7510F047");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.FineStatus).WithMany(p => p.Fines)
                .HasForeignKey(d => d.FineStatusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Fines__FineStatu__4E53A1AA");
        });

        modelBuilder.Entity<FineStatus>(entity =>
        {
            entity.HasKey(e => e.FineStatusId).HasName("PK__FineStat__ED02CC59ECB0B4AC");

            entity.ToTable("FineStatus");

            entity.HasIndex(e => e.StatusName, "UQ__FineStat__05E7698AE4C24E91").IsUnique();

            entity.Property(e => e.StatusName)
                .HasMaxLength(20)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Reservation>(entity =>
        {
            entity.HasKey(e => e.ReservationId).HasName("PK__Reservat__B7EE5F240E523474");

            entity.Property(e => e.ReservedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.ReservationStatus).WithMany(p => p.Reservations)
                .HasForeignKey(d => d.ReservationStatusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Reservati__Reser__56E8E7AB");
        });

        modelBuilder.Entity<ReservationStatus>(entity =>
        {
            entity.HasKey(e => e.ReservationStatusId).HasName("PK__Reservat__DFC0EEAAB76D67DA");

            entity.ToTable("ReservationStatus");

            entity.HasIndex(e => e.StatusName, "UQ__Reservat__05E7698A2A46CA6A").IsUnique();

            entity.Property(e => e.StatusName)
                .HasMaxLength(20)
                .IsUnicode(false);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
