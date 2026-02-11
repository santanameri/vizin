using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace vizin.Models;

public partial class PostgresContext : DbContext
{
    public PostgresContext()
    {
    }

    public PostgresContext(DbContextOptions<PostgresContext> options)
        : base(options)
    {
    }

    public virtual DbSet<TbAmenity> TbAmenities { get; set; }

    public virtual DbSet<TbBooking> TbBookings { get; set; }

    public virtual DbSet<TbFavorite> TbFavorites { get; set; }

    public virtual DbSet<TbPayment> TbPayments { get; set; }

    public virtual DbSet<TbProperty> TbProperties { get; set; }

    public virtual DbSet<TbReview> TbReviews { get; set; }

    public virtual DbSet<TbUser> TbUsers { get; set; }

    public virtual DbSet<TbUser1> TbUsers1 { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseNpgsql("Host=localhost:5432;Database=postgres;Username=postgres;Password=root");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasPostgresEnum("booking_status_enum", new[] { "CREATED", "CONFIRMED", "CANCELED", "FINISHED" })
            .HasPostgresEnum("payment_status_enum", new[] { "PENDING", "PAID", "FAILED", "REFUNDED" })
            .HasPostgresEnum("user_type_enum", new[] { "HOST", "GUEST", "ADMIN" });

        modelBuilder.Entity<TbAmenity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("tb_amenity_pkey");

            entity.ToTable("tb_amenity", "sistema_locacao");

            entity.HasIndex(e => e.Name, "tb_amenity_name_key").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.Category).HasColumnName("category");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
        });

        modelBuilder.Entity<TbBooking>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("tb_booking_pkey");

            entity.ToTable("tb_booking", "sistema_locacao");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.CancelationDate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("cancelation_date");
            entity.Property(e => e.CheckinDate).HasColumnName("checkin_date");
            entity.Property(e => e.CheckoutDate).HasColumnName("checkout_date");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.GuestCount).HasColumnName("guest_count");
            entity.Property(e => e.PropertyId).HasColumnName("property_id");
            entity.Property(e => e.Status)
                .HasDefaultValue(1)
                .HasColumnName("status");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Property).WithMany(p => p.TbBookings)
                .HasForeignKey(d => d.PropertyId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_booking_property");

            entity.HasOne(d => d.User).WithMany(p => p.TbBookings)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_booking_user");
        });

        modelBuilder.Entity<TbFavorite>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("tb_favorite_pkey");

            entity.ToTable("tb_favorite", "sistema_locacao");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.PropertyId).HasColumnName("property_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Property).WithMany(p => p.TbFavorites)
                .HasForeignKey(d => d.PropertyId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_favorite_property");

            entity.HasOne(d => d.User).WithMany(p => p.TbFavorites)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_favorite_user");
        });

        modelBuilder.Entity<TbPayment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("tb_payment_pkey");

            entity.ToTable("tb_payment", "sistema_locacao");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.Amount)
                .HasPrecision(10, 2)
                .HasColumnName("amount");
            entity.Property(e => e.BookingId).HasColumnName("booking_id");
            entity.Property(e => e.PaymentDate).HasColumnName("payment_date");
            entity.Property(e => e.PaymentMethod).HasColumnName("payment_method");
            entity.Property(e => e.StatusPayment).HasColumnName("status_payment");
            entity.Property(e => e.Type).HasColumnName("type");

            entity.HasOne(d => d.Booking).WithMany(p => p.TbPayments)
                .HasForeignKey(d => d.BookingId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_payment_booking");
        });

        modelBuilder.Entity<TbProperty>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("tb_property_pkey");

            entity.ToTable("tb_property", "sistema_locacao");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.AccomodationType).HasColumnName("accomodation_type");
            entity.Property(e => e.Availability)
                .HasDefaultValue(true)
                .HasColumnName("availability");
            entity.Property(e => e.Capacity).HasColumnName("capacity");
            entity.Property(e => e.DailyValue)
                .HasPrecision(10, 2)
                .HasColumnName("daily_value");
            entity.Property(e => e.Description)
                .HasMaxLength(256)
                .HasColumnName("description");
            entity.Property(e => e.FullAddress)
                .HasMaxLength(256)
                .HasColumnName("full_address");
            entity.Property(e => e.PropertyCategory).HasColumnName("property_category");
            entity.Property(e => e.Title)
                .HasMaxLength(256)
                .HasColumnName("title");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.TbProperties)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_property_user");

            entity.HasMany(d => d.Amenities).WithMany(p => p.Properties)
                .UsingEntity<Dictionary<string, object>>(
                    "TbPropertyAmenity",
                    r => r.HasOne<TbAmenity>().WithMany()
                        .HasForeignKey("AmenityId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("fk_property_amenity_amenity"),
                    l => l.HasOne<TbProperty>().WithMany()
                        .HasForeignKey("PropertyId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("fk_property_amenity_property"),
                    j =>
                    {
                        j.HasKey("PropertyId", "AmenityId").HasName("tb_property_amenity_pkey");
                        j.ToTable("tb_property_amenity", "sistema_locacao");
                        j.IndexerProperty<Guid>("PropertyId").HasColumnName("property_id");
                        j.IndexerProperty<Guid>("AmenityId").HasColumnName("amenity_id");
                    });
        });

        modelBuilder.Entity<TbReview>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("tb_review_pkey");

            entity.ToTable("tb_review", "sistema_locacao");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.BookingId).HasColumnName("booking_id");
            entity.Property(e => e.Comment)
                .HasMaxLength(256)
                .HasColumnName("comment");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.Note).HasColumnName("note");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Booking).WithMany(p => p.TbReviews)
                .HasForeignKey(d => d.BookingId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_review_booking");

            entity.HasOne(d => d.User).WithMany(p => p.TbReviews)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_review_user");
        });

        modelBuilder.Entity<TbUser>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("tb_user_pkey");

            entity.ToTable("tb_user", "sistema_locacao");

            entity.HasIndex(e => e.Email, "tb_user_email_key").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.Email)
                .HasMaxLength(256)
                .HasColumnName("email");
            entity.Property(e => e.Name)
                .HasMaxLength(256)
                .HasColumnName("name");
            entity.Property(e => e.Password)
                .HasMaxLength(256)
                .HasColumnName("password");
            entity.Property(e => e.Type).HasColumnName("type");
        });

        modelBuilder.Entity<TbUser1>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("tb_user_pkey");

            entity.ToTable("tb_user");

            entity.HasIndex(e => e.Email, "tb_user_email_key").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.Email)
                .HasMaxLength(256)
                .HasColumnName("email");
            entity.Property(e => e.Name)
                .HasMaxLength(256)
                .HasColumnName("name");
            entity.Property(e => e.Password)
                .HasMaxLength(256)
                .HasColumnName("password");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
