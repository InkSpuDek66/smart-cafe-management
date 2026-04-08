using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Scaffolding.Internal;

namespace Project_CSI402_T2_Y3.Models.Db;

public partial class Csi402dbContext : DbContext
{
    public Csi402dbContext()
    {
    }

    public Csi402dbContext(DbContextOptions<Csi402dbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Ingredient> Ingredients { get; set; }

    public virtual DbSet<Inventorylog> Inventorylogs { get; set; }

    public virtual DbSet<Inventoryreasontype> Inventoryreasontypes { get; set; }

    public virtual DbSet<Member> Members { get; set; }

    public virtual DbSet<Menuitem> Menuitems { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<Orderitem> Orderitems { get; set; }

    public virtual DbSet<Orderitemoption> Orderitemoptions { get; set; }

    public virtual DbSet<Orderitemstatus> Orderitemstatuses { get; set; }

    public virtual DbSet<Orderstatus> Orderstatuses { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<Paymentmethod> Paymentmethods { get; set; }

    public virtual DbSet<Paymentstatus> Paymentstatuses { get; set; }

    public virtual DbSet<Pointtransaction> Pointtransactions { get; set; }

    public virtual DbSet<Pointtransactiontype> Pointtransactiontypes { get; set; }

    public virtual DbSet<Promotion> Promotions { get; set; }

    public virtual DbSet<Recipe> Recipes { get; set; }

    public virtual DbSet<Reward> Rewards { get; set; }

    public virtual DbSet<Staff> Staff { get; set; }

    public virtual DbSet<Staffrole> Staffroles { get; set; }

    public virtual DbSet<Staffshift> Staffshifts { get; set; }

    public virtual DbSet<Table> Tables { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseMySql("server=localhost;port=3306;database=csi402db;user=root;password=root", Microsoft.EntityFrameworkCore.ServerVersion.Parse("9.6.0-mysql"));

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_0900_ai_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<Ingredient>(entity =>
        {
            entity.HasKey(e => e.IngredientId).HasName("PRIMARY");

            entity.ToTable("ingredients");

            entity.Property(e => e.IngredientId).ValueGeneratedNever();
            entity.Property(e => e.CostPerUnit).HasPrecision(10, 4);
            entity.Property(e => e.IngredientName)
                .HasMaxLength(150)
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.Unit)
                .HasMaxLength(20)
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
        });

        modelBuilder.Entity<Inventorylog>(entity =>
        {
            entity.HasKey(e => e.LogId).HasName("PRIMARY");

            entity.ToTable("inventorylogs");

            entity.Property(e => e.LogId).ValueGeneratedNever();
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
        });

        modelBuilder.Entity<Inventoryreasontype>(entity =>
        {
            entity.HasKey(e => e.ReasonTypeId).HasName("PRIMARY");

            entity.ToTable("inventoryreasontype");

            entity.Property(e => e.ReasonTypeId).ValueGeneratedNever();
            entity.Property(e => e.ReasonName)
                .HasMaxLength(50)
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
        });

        modelBuilder.Entity<Member>(entity =>
        {
            entity.HasKey(e => e.MemberId).HasName("PRIMARY");

            entity.ToTable("members");

            entity.Property(e => e.MemberId).ValueGeneratedNever();
            entity.Property(e => e.FirstName)
                .HasMaxLength(50)
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.LastName)
                .HasMaxLength(50)
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
        });

        modelBuilder.Entity<Menuitem>(entity =>
        {
            entity.HasKey(e => e.MenuItemId).HasName("PRIMARY");

            entity.ToTable("menuitems");

            entity.Property(e => e.MenuItemId).ValueGeneratedNever();
            entity.Property(e => e.Category)
                .HasMaxLength(50)
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.ImageUrl)
                .HasMaxLength(255)
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.IsAvailable).HasColumnType("bit(1)");
            entity.Property(e => e.IsSeasonal).HasColumnType("bit(1)");
            entity.Property(e => e.MenuDescription)
                .HasMaxLength(500)
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.MenuName)
                .HasMaxLength(150)
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.Price).HasPrecision(10, 2);
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.OrderId).HasName("PRIMARY");

            entity.ToTable("orders");

            entity.Property(e => e.OrderId).ValueGeneratedNever();
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.DiscountAmount).HasPrecision(10, 2);
            entity.Property(e => e.GuestName)
                .HasMaxLength(100)
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.NetAmount).HasPrecision(10, 2);
            entity.Property(e => e.QueueNumber)
                .HasMaxLength(10)
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.ReservedUntil).HasColumnType("datetime");
            entity.Property(e => e.TotalAmount).HasPrecision(10, 2);
        });

        modelBuilder.Entity<Orderitem>(entity =>
        {
            entity.HasKey(e => e.OrderItemId).HasName("PRIMARY");

            entity.ToTable("orderitems");

            entity.Property(e => e.OrderItemId).ValueGeneratedNever();
            entity.Property(e => e.UnitPrice).HasPrecision(10, 2);
        });

        modelBuilder.Entity<Orderitemoption>(entity =>
        {
            entity.HasKey(e => e.OrderItemOptionId).HasName("PRIMARY");

            entity.ToTable("orderitemoptions");

            entity.Property(e => e.OrderItemOptionId).ValueGeneratedNever();
            entity.Property(e => e.OptionName)
                .HasMaxLength(100)
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.OptionValue)
                .HasMaxLength(100)
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.PriceAdjustment).HasPrecision(10, 2);
        });

        modelBuilder.Entity<Orderitemstatus>(entity =>
        {
            entity.HasKey(e => e.OrderItemStatusId).HasName("PRIMARY");

            entity.ToTable("orderitemstatus");

            entity.Property(e => e.OrderItemStatusId).ValueGeneratedNever();
            entity.Property(e => e.StatusName)
                .HasMaxLength(50)
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
        });

        modelBuilder.Entity<Orderstatus>(entity =>
        {
            entity.HasKey(e => e.OrderStatusId).HasName("PRIMARY");

            entity.ToTable("orderstatus");

            entity.Property(e => e.OrderStatusId).ValueGeneratedNever();
            entity.Property(e => e.StatusName)
                .HasMaxLength(50)
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.PaymentId).HasName("PRIMARY");

            entity.ToTable("payments");

            entity.Property(e => e.PaymentId).ValueGeneratedNever();
            entity.Property(e => e.Amount).HasPrecision(10, 2);
            entity.Property(e => e.SlipUrl)
                .HasMaxLength(255)
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.RejectReason)
                .HasMaxLength(255)
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.VerifiedAt).HasColumnType("datetime");
        });

        modelBuilder.Entity<Paymentmethod>(entity =>
        {
            entity.HasKey(e => e.PaymentMethodId).HasName("PRIMARY");

            entity.ToTable("paymentmethod");

            entity.Property(e => e.PaymentMethodId).ValueGeneratedNever();
            entity.Property(e => e.MethodName)
                .HasMaxLength(50)
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
        });

        modelBuilder.Entity<Paymentstatus>(entity =>
        {
            entity.HasKey(e => e.PaymentStatusId).HasName("PRIMARY");

            entity.ToTable("paymentstatus");

            entity.Property(e => e.PaymentStatusId).ValueGeneratedNever();
            entity.Property(e => e.StatusName)
                .HasMaxLength(50)
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
        });

        modelBuilder.Entity<Pointtransaction>(entity =>
        {
            entity.HasKey(e => e.TransId).HasName("PRIMARY");

            entity.ToTable("pointtransactions");

            entity.Property(e => e.TransId).ValueGeneratedNever();
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
        });

        modelBuilder.Entity<Pointtransactiontype>(entity =>
        {
            entity.HasKey(e => e.PointTransactionTypeId).HasName("PRIMARY");

            entity.ToTable("pointtransactiontype");

            entity.Property(e => e.PointTransactionTypeId).ValueGeneratedNever();
            entity.Property(e => e.TypeName)
                .HasMaxLength(50)
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
        });

        modelBuilder.Entity<Promotion>(entity =>
        {
            entity.HasKey(e => e.PromotionId).HasName("PRIMARY");

            entity.ToTable("promotions");

            entity.Property(e => e.PromotionId).ValueGeneratedNever();
            entity.Property(e => e.ConditionType)
                .HasMaxLength(50)
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.ConditionValue)
                .HasMaxLength(255)
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.IsActive).HasColumnType("bit(1)");
            entity.Property(e => e.PromotionName)
                .HasMaxLength(150)
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.RewardType)
                .HasMaxLength(50)
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.RewardValue)
                .HasMaxLength(255)
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
        });

        modelBuilder.Entity<Recipe>(entity =>
        {
            entity.HasKey(e => e.RecipeId).HasName("PRIMARY");

            entity.ToTable("recipes");

            entity.Property(e => e.RecipeId).ValueGeneratedNever();
            entity.Property(e => e.Unit)
                .HasMaxLength(20)
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
        });

        modelBuilder.Entity<Reward>(entity =>
        {
            entity.HasKey(e => e.RewardId).HasName("PRIMARY");

            entity.ToTable("rewards");

            entity.Property(e => e.RewardId).ValueGeneratedNever();
            entity.Property(e => e.ImageUrl)
                .HasMaxLength(255)
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.IsActive).HasColumnType("bit(1)");
            entity.Property(e => e.RewardName)
                .HasMaxLength(150)
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
        });

        modelBuilder.Entity<Staff>(entity =>
        {
            entity.HasKey(e => e.StaffId).HasName("PRIMARY");

            entity.ToTable("staff");

            entity.Property(e => e.StaffId).ValueGeneratedNever();
            entity.Property(e => e.FirstName)
                .HasMaxLength(50)
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.IsActive).HasColumnType("bit(1)");
            entity.Property(e => e.LastName)
                .HasMaxLength(50)
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(255)
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.Username)
                .HasMaxLength(50)
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
        });

        modelBuilder.Entity<Staffrole>(entity =>
        {
            entity.HasKey(e => e.StaffRoleId).HasName("PRIMARY");

            entity.ToTable("staffrole");

            entity.Property(e => e.StaffRoleId).ValueGeneratedNever();
            entity.Property(e => e.RoleName)
                .HasMaxLength(50)
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
        });

        modelBuilder.Entity<Staffshift>(entity =>
        {
            entity.HasKey(e => e.ShiftId).HasName("PRIMARY");

            entity.ToTable("staffshifts");

            entity.Property(e => e.ShiftId).ValueGeneratedNever();
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.ShiftEnd).HasColumnType("datetime");
            entity.Property(e => e.ShiftStart).HasColumnType("datetime");
        });

        modelBuilder.Entity<Table>(entity =>
        {
            entity.HasKey(e => e.TableId).HasName("PRIMARY");

            entity.ToTable("tables");

            entity.Property(e => e.TableId).ValueGeneratedNever();
            entity.Property(e => e.IsActive).HasColumnType("bit(1)");
            entity.Property(e => e.QrCodeUrl)
                .HasMaxLength(255)
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.TableNumber)
                .HasMaxLength(10)
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
