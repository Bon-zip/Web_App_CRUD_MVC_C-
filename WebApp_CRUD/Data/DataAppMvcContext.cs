using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace WebApp_CRUD.Data;

public partial class DataAppMvcContext : DbContext
{
    public DataAppMvcContext()
    {
    }

    public DataAppMvcContext(DbContextOptions<DataAppMvcContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Blog> Blogs { get; set; }

    public virtual DbSet<Brand> Brands { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<User> Users { get; set; }


    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Data Source=.;Initial Catalog=BTLC;Persist Security Info=True;User ID=sa;Password=1234$;Encrypt=True;Trust Server Certificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Blog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Blogs__3214EC076D5BBEA1");

            entity.Property(e => e.Description).HasColumnType("ntext");
            entity.Property(e => e.Images).HasMaxLength(255);
            entity.Property(e => e.Name).HasMaxLength(255);
        });

        modelBuilder.Entity<Brand>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Brands__3214EC0724EC2152");

            entity.Property(e => e.Description).HasColumnType("ntext");
            entity.Property(e => e.Name).HasMaxLength(200);
            entity.Property(e => e.Slug).HasMaxLength(200);
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Categori__3214EC07D5F6D2AA");

            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Name).HasMaxLength(200);
            entity.Property(e => e.Slug).HasMaxLength(500);
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Products__3214EC0721DF25EB");

            entity.Property(e => e.Description).HasColumnType("ntext");
            entity.Property(e => e.Name).HasMaxLength(200);
            entity.Property(e => e.Price).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.Slug).HasMaxLength(500);
            entity.Property(e => e.Images).HasMaxLength(200);

            entity.HasOne(d => d.Brand).WithMany(p => p.Products)
                .HasForeignKey(d => d.BrandId)
                .HasConstraintName("FK__Products__BrandI__3A81B327");

            entity.HasOne(d => d.Category).WithMany(p => p.Products)
                .HasForeignKey(d => d.CategoryId)
                .HasConstraintName("FK__Products__Catego__3B75D760");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Users__3214EC07830761D2");

            entity.Property(e => e.Address).HasMaxLength(200);
            entity.Property(e => e.Avatar).HasMaxLength(200);
            entity.Property(e => e.FirstName).HasMaxLength(200);
            entity.Property(e => e.LastName).HasMaxLength(200);
            entity.Property(e => e.Phone).HasMaxLength(200);
            entity.Property(e => e.UserName).HasMaxLength(200);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
