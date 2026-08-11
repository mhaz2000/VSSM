using Microsoft.EntityFrameworkCore;

namespace VSSM.Infrastructure.Data.Models;

public partial class VSSMDbContext(
    DbContextOptions<VSSMDbContext> options) : DbContext(options)
{
    public virtual DbSet<AppLog> AppLogs { get; set; }
    public virtual DbSet<Menu> Menus { get; set; }
    public virtual DbSet<Role> Roles { get; set; }
    public virtual DbSet<TblConsumerPerson> TblConsumerPeople { get; set; }
    public virtual DbSet<TblOrderInfo> TblOrderInfo { get; set; }
    public virtual DbSet<TblUidInfo> TblUidInfo { get; set; }
    public virtual DbSet<TblDispatchUid> TblDispatchUids { get; set; }
    public virtual DbSet<TblUidError> TblUidErrors { get; set; }
    public virtual DbSet<TblBaseInfo> TblBaseInfo { get; set; }
    public virtual DbSet<RoleMenu> RoleMenus { get; set; }
    public virtual DbSet<TblAdjustment> TblAdjustments { get; set; }
    public virtual DbSet<TblArrival> TblArrivals { get; set; }
    public virtual DbSet<TblArrivalLine> TblArrivalLines { get; set; }
    public virtual DbSet<TblCulture> TblCultures { get; set; }
    public virtual DbSet<TblDispatch> TblDispatches { get; set; }
    public virtual DbSet<TblDispatchLine> TblDispatchLines { get; set; }
    public virtual DbSet<TblDispatchRepConfig> TblDispatchRepConfigs { get; set; }
    public virtual DbSet<TblFiscalPeriod> TblFiscalPeriods { get; set; }
    public virtual DbSet<TblItem> TblItems { get; set; }
    public virtual DbSet<TblItemDetail> TblItemDetails { get; set; }
    public virtual DbSet<TblItemDetailStore> TblItemDetailStores { get; set; }
    public virtual DbSet<TblItemDoseAssignment> TblItemDoseAssignments { get; set; }
    public virtual DbSet<TblItemsClass> TblItemsClasses { get; set; }
    public virtual DbSet<TblLevel> TblLevels { get; set; }
    public virtual DbSet<TblManItemClass> TblManItemClasses { get; set; }
    public virtual DbSet<TblMinMaxItem> TblMinMaxItems { get; set; }
    public virtual DbSet<TblRecipient> TblRecipients { get; set; }
    public virtual DbSet<TblRecipientConfiguration> TblRecipientConfigurations { get; set; }
    public virtual DbSet<TblRecipientMaxItem> TblRecipientMaxItems { get; set; }
    public virtual DbSet<TblRequest> TblRequests { get; set; }
    public virtual DbSet<TblRequestLine> TblRequestLines { get; set; }
    public virtual DbSet<TblSource> TblSources { get; set; }
    public virtual DbSet<TblStore> TblStores { get; set; }
    public virtual DbSet<TblDestroyUid> TblDestroyUids { get; set; }
    public virtual DbSet<TblUser> TblUsers { get; set; }
    public virtual DbSet<TblVoucherConfig> TblVoucherConfigs { get; set; }
    public virtual DbSet<TblReceiveDispatches> TblReceiveDispatches { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

        modelBuilder.Entity<TblRequestLine>(entity =>
        {
            entity.ToTable("tblRequestLines");

            entity.Property(e => e.Id)
                .HasColumnName("PK_REQUEST_LINE_ID");

            entity.Property(e => e.RequestId)
                .HasColumnName("FK_REQUEST_ID");

            entity.Property(e => e.ItemId)
                .HasColumnName("FK_ITEM_ID");

            entity.Property(e => e.ItemClassId)
                .HasColumnName("FK_ITEM_CLASS_ID");

            entity.Property(e => e.DosePerVialId)
                .HasColumnName("FK_DOSE_PER_VIAL_ID");

            entity.Property(e => e.ReqCount)
                .HasColumnName("REQ_COUNT");

            entity.Property(e => e.ArriveCount)
                .HasColumnName("ARRIVE_COUNT");

            entity.Property(e => e.UsedCount)
                .HasColumnName("USED_COUNT");

            entity.Property(e => e.CurrentCount)
                .HasColumnName("CURRENT_COUNT");

            entity.Property(e => e.ConfirmCount)
                .HasColumnName("CONFIRM_COUNT");

            entity.Property(e => e.Note)
                .HasColumnName("NOTE")
                .HasMaxLength(500);


            entity.HasOne(d => d.Request)
                .WithMany(p => p.TblRequestLines)
                .HasForeignKey(d => d.RequestId)
                .HasConstraintName("FK_REQUESTLINE_REQUEST");

            entity.HasOne(d => d.Item)
                .WithMany(p => p.TblRequestLines)
                .HasForeignKey(d => d.ItemId)
                .HasConstraintName("FK_REQUESTLINE_ITEM");

            entity.HasOne(d => d.ItemClass)
                .WithMany()
                .HasForeignKey(d => d.ItemClassId)
                .HasConstraintName("FK_REQUESTLINE_ITEM_CLASS");

            entity.HasOne(d => d.DosePerVial)
                .WithMany()
                .HasForeignKey(d => d.DosePerVialId)
                .HasConstraintName("FK_REQUESTLINE_DOSE_PER_VIAL");
        });


        modelBuilder.Entity<TblDestroyUid>(entity =>
        {
            entity.ToTable("tblDestroyUid");
            entity.HasKey(e => e.PkDestroyUid);

            entity.Property(e => e.PkDestroyUid)
                  .HasColumnName("PK_DESTROY_UID")
                  .ValueGeneratedOnAdd();

            entity.Property(e => e.FkUid)
                  .HasColumnName("FK_UID")
                  .IsRequired();

            entity.Property(e => e.DestroyDate)
                  .HasColumnName("DESTROY_DATE")
                  .HasMaxLength(10)
                  .IsRequired();

            entity.Property(e => e.DestroyNumber)
                  .HasColumnName("DESTROY_NUMBER")
                  .IsRequired();

            entity.Property(e => e.FkLocationDestroy)
                  .HasColumnName("FK_LOCATION_DESTROY")
                  .IsRequired();

            entity.Property(e => e.FkDestroyeBy)
                  .HasColumnName("FK_DESTROYE_BY")
                  .IsRequired();

            entity.Property(e => e.FkDestroyReasonId)
                  .HasColumnName("FK_DESTROY_REASON_ID")
                  .IsRequired();

            entity.Property(e => e.CreationTime)
                  .HasColumnName("CREATION_TIME")
                  .IsRequired();

            entity.Property(e => e.FkCreatedBy)
                  .HasColumnName("FK_CREATED_BY")
                  .IsRequired();

            // FK → tblUidInfo
            entity.HasOne(e => e.UidInfo)
                  .WithOne(u => u.DestroyUid)
                  .HasForeignKey<TblDestroyUid>(e => e.FkUid)
                  .OnDelete(DeleteBehavior.Restrict);

            // FK → tblRecipients
            entity.HasOne(e => e.Location)
                  .WithMany(r => r.DestroyUids)
                  .HasForeignKey(e => e.FkLocationDestroy)
                  .OnDelete(DeleteBehavior.Restrict);

            // FK → tblUsers (DestroyedBy)
            entity.HasOne(e => e.DestroyedByUser)
                  .WithMany()
                  .HasForeignKey(e => e.FkDestroyeBy)
                  .OnDelete(DeleteBehavior.Restrict);

            // FK → tblBaseInfo
            entity.HasOne(e => e.DestroyReason)
                  .WithMany(b => b.DestroyUids)
                  .HasForeignKey(e => e.FkDestroyReasonId)
                  .OnDelete(DeleteBehavior.Restrict);

            // FK → tblUsers (CreatedBy)
            entity.HasOne(e => e.CreatedByUser)
                  .WithMany()
                  .HasForeignKey(e => e.FkCreatedBy)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<AppLog>(entity =>
        {
            entity.Property(e => e.Host).IsUnicode(false);
            entity.Property(e => e.IpAddress).IsUnicode(false);
            entity.Property(e => e.Method).IsUnicode(false);
            entity.Property(e => e.Path).IsUnicode(false);
            entity.Property(e => e.Protocol).IsUnicode(false);
            entity.Property(e => e.Scheme).IsUnicode(false);
            entity.Property(e => e.TimeStamp).HasColumnType("datetime");
            entity.Property(e => e.Token).IsUnicode(false);
            entity.Property(e => e.UserName).IsUnicode(false);
        });

        modelBuilder.Entity<Menu>(entity =>
        {
            entity.Property(e => e.Action).HasMaxLength(100);
            entity.Property(e => e.Controller).HasMaxLength(100);
            entity.Property(e => e.Icon).HasMaxLength(100);
            entity.Property(e => e.Ttitle)
                .IsRequired()
                .HasMaxLength(100);

            entity.HasOne(d => d.Parent).WithMany(p => p.InverseParent)
                .HasForeignKey(d => d.ParentId)
                .HasConstraintName("FK_Menus_Menus");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(50);
        });

        modelBuilder.Entity<RoleMenu>(entity =>
        {
            entity.ToTable("RoleMenu");

            entity.HasOne(d => d.Menu).WithMany(p => p.RoleMenus)
                .HasForeignKey(d => d.MenuId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RoleMenu_Menus");

            entity.HasOne(d => d.Role).WithMany(p => p.RoleMenus)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RoleMenu_Roles");
        });

        modelBuilder.Entity<TblAdjustment>(entity =>
        {
            entity.HasKey(e => e.AdjustmentId);

            entity.ToTable("tblAdjustments");

            entity.Property(e => e.AdjustmentId).HasColumnName("AdjustmentID");
            entity.Property(e => e.AdjustmentreasonId).HasColumnName("AdjustmentreasonID");
            entity.Property(e => e.ArrivalLineId).HasColumnName("ArrivalLineID");
            entity.Property(e => e.Date).HasColumnType("datetime");
            entity.Property(e => e.DispatchLineId).HasColumnName("DispatchLineID");
            entity.Property(e => e.EnteredById).HasColumnName("EnteredByID");
            entity.Property(e => e.EntryDate).HasColumnType("datetime");
            entity.Property(e => e.FiscalPeriodId).HasColumnName("FiscalPeriodID");
            entity.Property(e => e.ItemDetailStoreId).HasColumnName("ItemDetailStoreID");

            entity.HasOne(d => d.Adjustmentreason).WithMany()
                .HasForeignKey(d => d.AdjustmentreasonId)
                .HasConstraintName("FK_tblAdjustments_tblBaseInfo_adjustmentreason");

            entity.HasOne(d => d.ArrivalLine).WithMany(p => p.TblAdjustments)
                .HasForeignKey(d => d.ArrivalLineId)
                .HasConstraintName("FK_tblAdjustments_tblArrivalLines");

            entity.HasOne(d => d.DispatchLine).WithMany(p => p.TblAdjustments)
                .HasForeignKey(d => d.DispatchLineId)
                .HasConstraintName("FK_tblAdjustments_tblDispatchLines");

            entity.HasOne(d => d.EnteredBy).WithMany(p => p.TblAdjustments)
                .HasForeignKey(d => d.EnteredById)
                .HasConstraintName("FK_tblAdjustments_tblUsers");

            entity.HasOne(d => d.FiscalPeriod).WithMany(p => p.TblAdjustments)
                .HasForeignKey(d => d.FiscalPeriodId)
                .HasConstraintName("FK_tblAdjustments_tblFiscalPeriods");

            entity.HasOne(d => d.ItemDetailStore).WithMany(p => p.TblAdjustments)
                .HasForeignKey(d => d.ItemDetailStoreId)
                .HasConstraintName("FK_tblAdjustments_tblAdjustments");
        });


        modelBuilder.Entity<TblArrival>(entity =>
        {
            entity.HasKey(e => e.ArrivalId);

            entity.ToTable("tblArrivals");

            entity.Property(e => e.ArrivalId).HasColumnName("ArrivalID");
            entity.Property(e => e.ConfirmationTime)
                .HasColumnType("datetime")
                .HasColumnName("ConfirmationTIme");
            entity.Property(e => e.Date).HasColumnType("datetime");
            entity.Property(e => e.DispatchId).HasColumnName("DispatchID");
            entity.Property(e => e.EnteredById).HasColumnName("EnteredByID");
            entity.Property(e => e.EntryDate).HasColumnType("datetime");

            entity.Property(e => e.FiscalPeriodId).HasColumnName("FiscalPeriodID");
            entity.Property(e => e.ReceiverId).HasColumnName("ReceiverID");
            entity.Property(e => e.SenderId).HasColumnName("SenderID");

            entity.HasIndex(e => e.ReportNumber)
                 .IsUnique();

            entity.Property(e => e.ReportNumber)
                  .HasMaxLength(32)
                  .IsUnicode();


            entity.HasOne(d => d.FiscalPeriod).WithMany(p => p.TblArrivals)
                .HasForeignKey(d => d.FiscalPeriodId)
                .HasConstraintName("FK_tblArrivals_tblFiscalPeriods");

            entity.HasOne(d => d.Receiver).WithMany(p => p.TblArrivalReceivers)
                .HasForeignKey(d => d.ReceiverId)
                .HasConstraintName("FK_tblArrivals_tblRecipients1");

            entity.HasOne(d => d.Sender).WithMany(p => p.TblArrivalSenders)
                .HasForeignKey(d => d.SenderId)
                .HasConstraintName("FK_tblArrivals_tblRecipients");

        });


        modelBuilder.Entity<TblArrivalLine>(entity =>
        {
            entity.HasKey(e => e.ArrivalLineId);

            entity.ToTable("tblArrivalLines");

            entity.Property(e => e.ArrivalLineId).HasColumnName("ArrivalLineID");
            entity.Property(e => e.ArrivalId).HasColumnName("ArrivalID");
            entity.Property(e => e.ItemId).HasColumnName("ItemID");
            entity.Property(e => e.ManufacturerId).HasColumnName("ManufacturerID");
            entity.Property(e => e.DosePerVialId).HasColumnName("DosePerVialID");
            entity.Property(e => e.StoreId).HasColumnName("StoreID");
            entity.Property(e => e.ItemDetailId).HasColumnName("ItemDetailID");

            entity.Property(e => e.UseId).HasColumnName("UseID");
            entity.Property(e => e.CommertialNameId).HasColumnName("CommertialNameID");
            entity.Property(e => e.VvmtypeId).HasColumnName("VVMTypeID");
            entity.Property(e => e.VvmstageId).HasColumnName("VVMStageID");



            entity.Property(e => e.BatchNum).HasMaxLength(50);
            entity.Property(e => e.Note).HasMaxLength(500);

            entity.Property(e => e.ExpiryDate).HasColumnType("datetime");
            entity.Property(e => e.EntryDate).HasColumnType("datetime");
            entity.Property(e => e.ProductionDate).HasColumnType("datetime");


            entity.Property(e => e.FkUseId).HasColumnName("FK_USE_ID");
            entity.Property(e => e.FkVvmTypeId).HasColumnName("FK_VVM_TYPE_ID");
            entity.Property(e => e.FkVvmStageId).HasColumnName("FK_VVM_STAGE_ID");
            entity.Property(e => e.FkCommertialNameId).HasColumnName("FK_COMMERTIAL_NAME_ID");
            entity.Property(e => e.FkManufacturerId).HasColumnName("FK_MANUFACTURER_ID");

            entity.HasOne(d => d.FkUse)
                .WithMany()
                .HasForeignKey(d => d.FkUseId)
                .HasConstraintName("FK_ARRIVAL_LINE_USE");

            entity.HasOne(d => d.FkVvmType)
                .WithMany()
                .HasForeignKey(d => d.FkVvmTypeId)
                .HasConstraintName("FK_ARRIVAL_LINE_VVM_TYPE");

            entity.HasOne(d => d.FkVvmStage)
                .WithMany()
                .HasForeignKey(d => d.FkVvmStageId)
                .HasConstraintName("FK_ARRIVAL_LINE_VVM_STAGE");

            entity.HasOne(d => d.FkCommertialName)
                .WithMany()
                .HasForeignKey(d => d.FkCommertialNameId)
                .HasConstraintName("FK_ARRIVAL_LINE_COMMERTIAL_NAME");

            entity.HasOne(d => d.FkManufacturer)
                .WithMany()
                .HasForeignKey(d => d.FkManufacturerId)
                .HasConstraintName("FK_ARRIVAL_LINE_MANUFACTURER");
        });


        modelBuilder.Entity<TblArrivalRepConfig>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("tblArrivalRepConfig");

            entity.Property(e => e.Activity).HasColumnName("activity");
            entity.Property(e => e.ArrivalDate).HasColumnName("arrivalDate");
            entity.Property(e => e.ExpireDate).HasColumnName("expireDate");
            entity.Property(e => e.ItemClass).HasColumnName("itemClass");
            entity.Property(e => e.ItemCode).HasColumnName("itemCode");
            entity.Property(e => e.ItemName).HasColumnName("itemName");
            entity.Property(e => e.OrderPurchaseNumber).HasColumnName("orderPurchaseNumber");
            entity.Property(e => e.ReportNumber).HasColumnName("reportNumber");
            entity.Property(e => e.Source).HasColumnName("source");
            entity.Property(e => e.TotalPrice).HasColumnName("totalPrice");
            entity.Property(e => e.TotalVolume).HasColumnName("totalVolume");
            entity.Property(e => e.UnitPrice).HasColumnName("unitPrice");
            entity.Property(e => e.UnitVolume).HasColumnName("unitVolume");
        });

        modelBuilder.Entity<TblAuthenticationMode>(entity =>
        {
            entity.HasKey(e => e.AuthenticationModeId);

            entity.ToTable("tblAuthenticationMode");

            entity.Property(e => e.AuthenticationModeId).HasColumnName("AuthenticationModeID");
            entity.Property(e => e.AuthenticationModeName).HasMaxLength(10);
            entity.Property(e => e.LdapPath).HasColumnName("LDAP_Path");
        });

        modelBuilder.Entity<TblBundle>(entity =>
        {
            entity.HasKey(e => e.BundleId);

            entity.ToTable("tblBundles");

            entity.Property(e => e.BundleId).HasColumnName("BundleID");
            entity.Property(e => e.Code).HasMaxLength(100);
            entity.Property(e => e.DosePerVialId).HasColumnName("DosePerVialID");
            entity.Property(e => e.EntryDate).HasColumnType("datetime");
            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.ItemId).HasColumnName("ItemID");
            entity.Property(e => e.ReceiverId).HasColumnName("ReceiverID");
            entity.Property(e => e.RecipientId).HasColumnName("RecipientID");
            entity.Property(e => e.UseId).HasColumnName("UseID");
        });

        modelBuilder.Entity<TblBundleDetail>(entity =>
        {
            entity.HasKey(e => e.BundleDetailId);

            entity.ToTable("tblBundleDetails");

            entity.Property(e => e.BundleDetailId).HasColumnName("BundleDetailID");
            entity.Property(e => e.BundleId).HasColumnName("BundleID");
            entity.Property(e => e.EntryDate).HasColumnType("datetime");
            entity.Property(e => e.ItemId).HasColumnName("ItemID");

            entity.HasOne(d => d.Bundle).WithMany(p => p.TblBundleDetails)
                .HasForeignKey(d => d.BundleId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_tblBundleDetails_tblBundles");
        });

        modelBuilder.Entity<TblCountryCode>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("tblCountryCode");

            entity.Property(e => e.Code)
                .HasMaxLength(255)
                .HasColumnName("code");
            entity.Property(e => e.Country)
                .HasMaxLength(255)
                .HasColumnName("country");
        });

        modelBuilder.Entity<TblCulture>(entity =>
        {
            entity.HasKey(e => e.Abr);

            entity.ToTable("tblCultures");

            entity.Property(e => e.Abr).HasMaxLength(2);
            entity.Property(e => e.CssFile).HasMaxLength(100);
            entity.Property(e => e.Decsep)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("decsep");
            entity.Property(e => e.Direction).HasMaxLength(4);
            entity.Property(e => e.IsActive).HasColumnName("isActive");
            entity.Property(e => e.Title).HasMaxLength(100);
        });

        modelBuilder.Entity<TblDispatch>(entity =>
        {
            entity.HasKey(e => e.DispatchId);

            entity.ToTable("tblDispatches");

            entity.Property(e => e.DispatchId).HasColumnName("DispatchID");

            entity.Property(e => e.SeqPreview)
                .HasColumnName("seq_preview")
                .HasDefaultValueSql("NEXT VALUE FOR Dispatch_Seq_Preview")
                .ValueGeneratedOnAdd();

            entity.Property(e => e.ArrivalId).HasColumnName("ArrivalID");
            entity.Property(e => e.ConfirmationTime).HasColumnType("datetime");
            entity.Property(e => e.Date).HasColumnType("datetime");
            entity.Property(e => e.EnteredById).HasColumnName("EnteredByID");
            entity.Property(e => e.EntryDate).HasColumnType("datetime");
            entity.Property(e => e.FiscalPeriodId).HasColumnName("FiscalPeriodID");
            entity.Property(e => e.IsDel)
                .HasDefaultValue(false)
                .HasAnnotation("Relational:DefaultConstraintName", "DF_tblDispatches_IsDel");
            entity.Property(e => e.ReceiverId).HasColumnName("ReceiverID");
            entity.Property(e => e.ReceivingMode).HasColumnName("ReceivingMode");
            entity.Property(e => e.SenderId).HasColumnName("SenderID");
            entity.Property(e => e.TransportMode).HasColumnName("TransportMode");

            entity.HasOne(d => d.Arrival)
                .WithMany(p => p.TblDispatches)
                .HasForeignKey(d => d.ArrivalId)
                .HasConstraintName("FK_tblDispatches_tblArrivals");

            entity.HasOne(d => d.EnteredBy).WithMany(p => p.TblDispatches)
                .HasForeignKey(d => d.EnteredById)
                .HasConstraintName("FK_tblDispatches_tblUsers");

            entity.HasOne(d => d.FiscalPeriod).WithMany(p => p.TblDispatches)
                .HasForeignKey(d => d.FiscalPeriodId)
                .HasConstraintName("FK_tblDispatches_tblFiscalPeriods");

            entity.HasOne(d => d.Receiver).WithMany(p => p.TblDispatchReceivers)
                .HasForeignKey(d => d.ReceiverId)
                .HasConstraintName("FK_tblDispatches_tblRecipients1");

            entity.HasOne(d => d.Sender).WithMany(p => p.TblDispatchSenders)
                .HasForeignKey(d => d.SenderId)
                .HasConstraintName("FK_tblDispatches_tblRecipients");

            entity.Property(e => e.FkReceiveDispatchId)
                .HasColumnName("FK_RECEIVER_ID");
        });

        modelBuilder.Entity<TblDispatchLine>(entity =>
        {
            entity.HasKey(e => e.DispatchLineId);

            entity.ToTable("tblDispatchLines");

            entity.Property(e => e.DispatchLineId).HasColumnName("DispatchLineID");
            entity.Property(e => e.DispatchId).HasColumnName("DispatchID");
            entity.Property(e => e.DosePerVialId).HasColumnName("DosePerVialID");
            entity.Property(e => e.EntryDate).HasColumnType("datetime");
            entity.Property(e => e.ExpiryDate).HasColumnType("datetime");
            entity.Property(e => e.ItemDetailStoreId).HasColumnName("ItemDetailStoreID");
            entity.Property(e => e.ItemId).HasColumnName("ItemID");
            entity.Property(e => e.ManufacturerId).HasColumnName("ManufacturerID");
            entity.Property(e => e.StoreId).HasColumnName("StoreID");


            entity.Property(e => e.FkUseBaseInfo).HasColumnName("FK_USE_BASE_INFO");
            entity.Property(e => e.FkVvmTypeBaseInfo).HasColumnName("FK_VVM_TYPE_BASE_INFO");
            entity.Property(e => e.FkVvmStageBaseInfo).HasColumnName("FK_VVM_STAGE_BASE_INFO");

            entity.HasOne(d => d.UseBaseInfo)
                .WithMany()
                .HasForeignKey(d => d.FkUseBaseInfo)
                .HasConstraintName("FK_tblDispatchLines_tblBaseInfo_Use")
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(d => d.VvmTypeBaseInfo)
                .WithMany()
                .HasForeignKey(d => d.FkVvmTypeBaseInfo)
                .HasConstraintName("FK_tblDispatchLines_tblBaseInfo_VvmType")
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(d => d.VvmStageBaseInfo)
                .WithMany()
                .HasForeignKey(d => d.FkVvmStageBaseInfo)
                .HasConstraintName("FK_tblDispatchLines_tblBaseInfo_VvmStage")
                .OnDelete(DeleteBehavior.NoAction);


            entity.HasOne(d => d.Dispatch).WithMany(p => p.TblDispatchLines)
                .HasForeignKey(d => d.DispatchId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_tblDispatchLines_tblDispatches");

            entity.HasOne(d => d.DosePerVial).WithMany()
                .HasForeignKey(d => d.DosePerVialId)
                .HasConstraintName("FK_tblDispatchLines_tblBaseInfo_dosePerVials");

            entity.HasOne(d => d.ItemDetailStore).WithMany(p => p.TblDispatchLines)
                .HasForeignKey(d => d.ItemDetailStoreId)
                .HasConstraintName("FK_tblDispatchLines_tblItemDetailStores");

            entity.HasOne(d => d.Item).WithMany(p => p.TblDispatchLines)
                .HasForeignKey(d => d.ItemId)
                .HasConstraintName("FK_tblDispatchLines_tblItems");

            entity.HasOne(d => d.Manufacturer).WithMany()
                .HasForeignKey(d => d.ManufacturerId)
                .HasConstraintName("FK_tblDispatchLines_tblBaseInfo_manufacturers");

            entity.HasOne(d => d.Store).WithMany(p => p.TblDispatchLines)
                .HasForeignKey(d => d.StoreId)
                .HasConstraintName("FK_tblDispatchLines_tblStores");
        });

        modelBuilder.Entity<TblDispatchRepConfig>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("tblDispatchRepConfig");

            entity.Property(e => e.Activity).HasColumnName("activity");
            entity.Property(e => e.BatchNumber).HasColumnName("batchNumber");
            entity.Property(e => e.DispatchDate).HasColumnName("dispatchDate");
            entity.Property(e => e.DosePerVial).HasColumnName("dosePerVial");
            entity.Property(e => e.ExpDate).HasColumnName("expDate");
            entity.Property(e => e.ItemClass).HasColumnName("itemClass");
            entity.Property(e => e.ItemCode).HasColumnName("itemCode");
            entity.Property(e => e.ItemName).HasColumnName("itemName");
            entity.Property(e => e.NumberOfVial).HasColumnName("numberOfVial");
            entity.Property(e => e.Quantity).HasColumnName("quantity");
            entity.Property(e => e.Recipient).HasColumnName("recipient");
            entity.Property(e => e.RequestNumber).HasColumnName("requestNumber");
            entity.Property(e => e.Store).HasColumnName("store");
            entity.Property(e => e.VoucherNumber).HasColumnName("voucherNumber");
        });

        modelBuilder.Entity<TblFiscalPeriod>(entity =>
        {
            entity.HasKey(e => e.FiscalPeriodId);

            entity.ToTable("tblFiscalPeriods");

            entity.Property(e => e.FiscalPeriodId).HasColumnName("FiscalPeriodID");
            entity.Property(e => e.EndTime).HasColumnType("datetime");
            entity.Property(e => e.EntryDate).HasColumnType("datetime");
            entity.Property(e => e.FullName).HasMaxLength(200);
            entity.Property(e => e.StartDate).HasColumnType("datetime");
            entity.Property(e => e.SuggestedEndDate).HasColumnType("datetime");
        });

        modelBuilder.Entity<TblItem>(entity =>
        {
            entity.HasKey(e => e.ItemId);

            entity.ToTable("tblItems");

            entity.Property(e => e.ItemId).HasColumnName("ItemID");
            entity.Property(e => e.Code).HasMaxLength(50);
            entity.Property(e => e.DiluentId).HasColumnName("DiluentID");
            entity.Property(e => e.EntryDate).HasColumnType("datetime");
            entity.Property(e => e.FullName).HasMaxLength(200);
            entity.Property(e => e.OrderId).HasColumnName("OrderID");
            entity.Property(e => e.Osvol).HasColumnName("OSVOL");
            entity.Property(e => e.PresentationId).HasColumnName("PresentationID");
            entity.Property(e => e.UniCode)
                .HasMaxLength(150)
                .HasColumnName("uniCode");


            entity.Property(e => e.FkClassId).HasColumnName("FK_CLASS_ID");


            entity.HasOne(d => d.FkClass)
                .WithMany()
                .HasForeignKey(d => d.FkClassId)
                .HasConstraintName("FK_CLASS_ID")
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);

        });

        modelBuilder.Entity<TblItemDetail>(entity =>
        {
            entity.HasKey(e => e.ItemDetailId);

            entity.ToTable("tblItemDetails");

            entity.Property(e => e.CommercialNameId).HasColumnName("CommercialNameID");
            entity.Property(e => e.VvmTypeId).HasColumnName("VVMTypeID");
            entity.Property(e => e.ManufacturerId).HasColumnName("ManufacturerID");

            entity.Property(e => e.ItemDetailId).HasColumnName("ItemDetailID");
            entity.Property(e => e.BatchNumber).HasMaxLength(50);
            entity.Property(e => e.DosePerVialId).HasColumnName("DosePerVialID");
            entity.Property(e => e.EntryDate).HasColumnType("datetime");
            entity.Property(e => e.ExpiryDate).HasColumnType("datetime");
            entity.Property(e => e.FiscalPeriodId).HasColumnName("FiscalPeriodID");
            entity.Property(e => e.IsConfirmed)
                .HasDefaultValue(false)
                .HasAnnotation("Relational:DefaultConstraintName", "DF_tblItemDetails_IsConfirm");
            entity.Property(e => e.ItemId).HasColumnName("ItemID");
            entity.Property(e => e.ManufacturerId).HasColumnName("ManufacturerID");
            entity.Property(e => e.ProductionDate).HasColumnType("datetime");

            entity.HasOne(d => d.CommercialName).WithMany()
                .HasForeignKey(d => d.CommercialNameId)
                .HasConstraintName("FK_tblItemDetails_tblBaseInfo_commercialNames");

            entity.HasOne(d => d.DosePerVial).WithMany()
                .HasForeignKey(d => d.DosePerVialId)
                .HasConstraintName("FK_tblItemDetails_tblBaseInfo_dosePerVials");

            entity.HasOne(d => d.FiscalPeriod).WithMany(p => p.TblItemDetails)
                .HasForeignKey(d => d.FiscalPeriodId)
                .HasConstraintName("FK_tblItemDetails_tblFiscalPeriods");

            entity.HasOne(d => d.Item).WithMany(p => p.TblItemDetails)
                .HasForeignKey(d => d.ItemId)
                .HasConstraintName("FK_tblItemDetails_tblItems");

            entity.HasOne(d => d.Manufacturer).WithMany()
                .HasForeignKey(d => d.ManufacturerId)
                .HasConstraintName("FK_tblItemDetails_tblBaseInfo_manufacturers");

            entity.HasOne(d => d.Vvmtype).WithMany()
                .HasForeignKey(d => d.VvmTypeId)
                .HasConstraintName("FK_tblItemDetails_tblBaseInfo_VVMTypes");
        });



        modelBuilder.Entity<TblItemDetailStore>(entity =>
        {
            entity.HasKey(e => e.PKItemDetailStoreId);

            entity.ToTable("tblItemDetailStores");
            entity.Property(e => e.PKItemDetailStoreId).HasColumnName("PK_ITEM_DETAIL_STORE_ID");
            entity.Property(e => e.EntryDate).HasColumnType("datetime").HasColumnName("ENTRY_DATE");
            entity.Property(e => e.ItemDetailId).HasColumnName("ITEM_DETAIL_ID");
            entity.Property(e => e.StoreId).HasColumnName("STORE_ID");
            entity.Property(e => e.CurrentStock).HasColumnName("CURRENT_STOCK");
            entity.Property(e => e.FkVvmStageId).HasColumnName("FK_VVM_STAGE_ID");

            entity.HasOne(d => d.ItemDetail)
                .WithMany(p => p.TblItemDetailStores)
                .HasForeignKey(d => d.ItemDetailId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_tblItemDetailStores_tblItemDetails");

            entity.HasOne(d => d.Store)
                .WithMany(p => p.TblItemDetailStores)
                .HasForeignKey(d => d.StoreId)
                .HasConstraintName("FK_tblItemDetailStores_tblStores");

            entity.HasOne(d => d.FkVvmStage)
                .WithMany()
                .HasForeignKey(d => d.FkVvmStageId)
                .HasConstraintName("FK_tblItemDetailStores_tblBaseInfo_VvmStage");
        });

        modelBuilder.Entity<TblItemDoseAssignment>(entity =>
        {
            entity.HasKey(e => e.ItemDoseAssignmentId);

            entity.ToTable("tblItemDoseAssignments");

            entity.Property(e => e.ItemDoseAssignmentId).HasColumnName("ItemDoseAssignmentID");
            entity.Property(e => e.DoseId).HasColumnName("DoseID");
            entity.Property(e => e.EntryDate).HasColumnType("datetime");
            entity.Property(e => e.ItemId).HasColumnName("ItemID");

            entity.HasOne(d => d.Dose).WithMany()
                .HasForeignKey(d => d.DoseId)
                .HasConstraintName("FK_tblItemDoseAssignments_tblBaseInfo_dosePerVials");

            entity.HasOne(d => d.Item).WithMany(p => p.TblItemDoseAssignments)
                .HasForeignKey(d => d.ItemId)
                .HasConstraintName("FK_tblItemDoseAssignments_tblItems");
        });

        modelBuilder.Entity<TblItemsClass>(entity =>
        {
            entity.HasKey(e => e.ItemClassId);

            entity.ToTable("tblItemsClass");

            entity.Property(e => e.ItemClassId).HasColumnName("ItemClassID");
            entity.Property(e => e.Code).HasMaxLength(50);
            entity.Property(e => e.EntryDate).HasColumnType("datetime");
            entity.Property(e => e.FullName).HasMaxLength(200);
            entity.Property(e => e.ReqBatchNo).HasColumnName("Req_BatchNo");
            entity.Property(e => e.ReqDosePerVial).HasColumnName("Req_DosePerVial");
            entity.Property(e => e.ReqExpiryDate).HasColumnName("Req_ExpiryDate");
            entity.Property(e => e.ReqManufacturer).HasColumnName("Req_Manufacturer");
            entity.Property(e => e.ReqVvmtype).HasColumnName("Req_VVMType");
            entity.Property(e => e.VolumeType).HasMaxLength(10);
        });

        modelBuilder.Entity<TblBaseInfo>(entity =>
        {
            entity.ToTable("tblBaseInfo");

            entity.HasKey(e => e.BaseInfoId);
            entity.Property(e => e.BaseInfoId)
                .HasColumnName("PK_BASE_INFO_ID")
                .UseIdentityColumn();

            entity.Property(e => e.Code)
                .HasColumnName("Code")
                .HasMaxLength(100);

            entity.Property(e => e.Title)
                .HasColumnName("Title")
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(e => e.ParentId)
                .HasColumnName("FK_Parent_Id");

            entity.Property(e => e.Order)
                .HasColumnName("Order");

            entity.Property(e => e.CreateDate)
                .HasColumnName("Create_date")
                .HasColumnType("datetime")
                .HasDefaultValueSql("GETDATE()");

            entity.Property(e => e.CreateBy)
                .HasColumnName("Create_by")
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(e => e.UpdateBy)
                .HasColumnName("Update_by")
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(e => e.UpdateDate)
                .HasColumnName("Update_date")
                .HasColumnType("datetime");

            entity.HasOne(e => e.Parent)
                .WithMany(e => e.Children)
                .HasForeignKey(e => e.ParentId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<TblLevel>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("tblLevels");

            entity.Property(e => e.LevelId).HasColumnName("LevelID");
        });

        modelBuilder.Entity<TblManItemClass>(entity =>
        {
            entity.HasKey(e => e.ManItemClassId);

            entity.ToTable("tblManItemClass");

            entity.Property(e => e.ManItemClassId).HasColumnName("Man_ItemClassID");
            entity.Property(e => e.ItemClassId).HasColumnName("ItemClassID");
            entity.Property(e => e.ManufacturerId).HasColumnName("ManufacturerID");

            entity.HasOne(d => d.ItemClass).WithMany(p => p.TblManItemClasses)
                .HasForeignKey(d => d.ItemClassId)
                .HasConstraintName("FK_tblManItemClass_tblItemsClass");

            entity.HasOne(d => d.Manufacturer).WithMany()
                .HasForeignKey(d => d.ManufacturerId)
                .HasConstraintName("FK_tblManItemClass_tblBaseInfo_manufacturers");
        });

        modelBuilder.Entity<TblMinMaxItem>(entity =>
        {
            entity.HasKey(e => e.MinMaxItemId);

            entity.ToTable("tblMinMaxItem");

            entity.Property(e => e.MinMaxItemId).HasColumnName("MinMaxItemID");
            entity.Property(e => e.EntryDate).HasColumnType("datetime");
            entity.Property(e => e.ItemId).HasColumnName("ItemID");
            entity.Property(e => e.RecipientId).HasColumnName("RecipientID");

            entity.HasOne(d => d.Item).WithMany(p => p.TblMinMaxItems)
                .HasForeignKey(d => d.ItemId)
                .HasConstraintName("FK_tblMinMaxItem_tblItems");

            entity.HasOne(d => d.Recipient).WithMany(p => p.TblMinMaxItems)
                .HasForeignKey(d => d.RecipientId)
                .HasConstraintName("FK_tblMinMaxItem_tblRecipients");
        });

        modelBuilder.Entity<TblRecipient>(entity =>
        {
            entity.HasKey(e => e.RecipientId);

            entity.ToTable("tblRecipients");

            entity.HasIndex(e => e.RecipientId, "IX_tblRecipients");

            entity.HasIndex(e => e.FullName, "IX_tblRecipients_1");

            entity.Property(e => e.RecipientId).HasColumnName("RecipientID");
            entity.Property(e => e.Address1).HasMaxLength(500);
            entity.Property(e => e.Address2).HasMaxLength(500);
            entity.Property(e => e.City).HasMaxLength(100);
            entity.Property(e => e.ClimateId).HasColumnName("ClimateID");
            entity.Property(e => e.Code).HasMaxLength(50);
            entity.Property(e => e.Email)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.EntryDate).HasColumnType("datetime");
            entity.Property(e => e.FullName).HasMaxLength(200);
            entity.Property(e => e.OwnershipId).HasColumnName("OwnershipID");
            entity.Property(e => e.ParentId).HasColumnName("ParentID");
            entity.Property(e => e.Phone)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.PostalCode).HasMaxLength(50);
            entity.Property(e => e.PowerSourceId).HasColumnName("PowerSourceID");
            entity.Property(e => e.Province).HasMaxLength(100);
            entity.Property(e => e.ReceivingModeId).HasColumnName("ReceivingModeID");
            entity.Property(e => e.StartUp).HasColumnType("datetime");
            entity.Property(e => e.TransportModeId).HasColumnName("TransportModeID");
            entity.Property(e => e.TypeOfRoadId).HasColumnName("TypeOfRoadID");

            entity.HasOne(d => d.Climate).WithMany()
                .HasForeignKey(d => d.ClimateId)
                .HasConstraintName("FK_tblRecipients_tblBaseInfo_climate");

            entity.HasOne(d => d.Ownership).WithMany()
                .HasForeignKey(d => d.OwnershipId)
                .HasConstraintName("FK_tblRecipients_tblBaseInfo_Ownership");

            entity.HasOne(d => d.PowerSource).WithMany()
                .HasForeignKey(d => d.PowerSourceId)
                .HasConstraintName("FK_tblRecipients_tblBaseInfo_PowerSource");

            entity.HasOne(d => d.ReceivingMode).WithMany()
                .HasForeignKey(d => d.ReceivingModeId)
                .HasConstraintName("FK_tblRecipients_tblBaseInfo_ReceivingMode");

            entity.HasOne(d => d.TransportMode).WithMany()
                .HasForeignKey(d => d.TransportModeId)
                .HasConstraintName("FK_tblRecipients_tblBaseInfo_TransportMode");

            entity.HasOne(d => d.TypeOfRoad).WithMany()
                .HasForeignKey(d => d.TypeOfRoadId)
                .HasConstraintName("FK_tblRecipients_tblBaseInfo_TypeOfRoad");
        });

        modelBuilder.Entity<TblRecipientConfiguration>(entity =>
        {
            entity.HasKey(e => e.RecipientConfigurationId).HasName("PK_tblRecipientConfiguration");

            entity.ToTable("tblRecipientConfigurations");

            entity.Property(e => e.RecipientConfigurationId).HasColumnName("RecipientConfigurationID");
            entity.Property(e => e.CurrentLang).HasMaxLength(2);
            entity.Property(e => e.EntryDate).HasColumnType("datetime");
            entity.Property(e => e.LastLogin).HasColumnType("datetime");
            entity.Property(e => e.RecipientId).HasColumnName("RecipientID");

            entity.HasOne(d => d.Recipient).WithMany(p => p.TblRecipientConfigurations)
                .HasForeignKey(d => d.RecipientId)
                .HasConstraintName("FK_tblRecipientConfigurations_tblRecipients");
        });

        modelBuilder.Entity<TblRecipientMaxItem>(entity =>
        {
            entity.HasKey(e => e.RecipientMaxItemId);

            entity.ToTable("tblRecipientMaxItems");

            entity.Property(e => e.RecipientMaxItemId).HasColumnName("RecipientMaxItemID");
            entity.Property(e => e.EntryDate).HasColumnType("datetime");
            entity.Property(e => e.ItemId).HasColumnName("ItemID");
            entity.Property(e => e.ReceiverId).HasColumnName("ReceiverID");
            entity.Property(e => e.RecipientId).HasColumnName("RecipientID");

            entity.HasOne(d => d.Item).WithMany(p => p.TblRecipientMaxItems)
                .HasForeignKey(d => d.ItemId)
                .HasConstraintName("FK_tblRecipientMaxItems_tblItems");

            entity.HasOne(d => d.Receiver).WithMany(p => p.TblRecipientMaxItemReceivers)
                .HasForeignKey(d => d.ReceiverId)
                .HasConstraintName("FK_tblRecipientMaxItems_tblRecipients1");

            entity.HasOne(d => d.Recipient).WithMany(p => p.TblRecipientMaxItemRecipients)
                .HasForeignKey(d => d.RecipientId)
                .HasConstraintName("FK_tblRecipientMaxItems_tblRecipients");
        });

        modelBuilder.Entity<TblRequest>(entity =>
        {
            entity.ToTable("tblRequests");

            entity.Property(e => e.Id)
                .HasColumnName("PK_REQUEST_ID");

            entity.Property(e => e.SenderId)
                .HasColumnName("FK_SENDER_ID");

            entity.Property(e => e.ReceiverId)
                .HasColumnName("FK_RECEIVER_ID");

            entity.Property(e => e.EnteredBy)
                .HasColumnName("FK_ENTERED_BY");

            entity.Property(e => e.FiscalPeriodId)
                .HasColumnName("FK_FISCAL_PERIOD_ID");


            entity.Property(e => e.Status)
                .HasColumnName("STATUS");

            entity.Property(e => e.SendDate)
                .HasColumnName("SEND_DATE");

            entity.Property(e => e.ConfirmDate)
                .HasColumnName("CONFIRM_DATE");

            entity.Property(e => e.RequestNumber)
                .HasColumnName("REQUEST_NUMBER")
                .HasMaxLength(100);


            entity.HasOne(d => d.EnteredByNavigation)
                .WithMany(p => p.TblRequests)
                .HasForeignKey(d => d.EnteredBy)
                .HasConstraintName("FK_tblRequest_tblUsers");

            entity.HasOne(d => d.FiscalPeriod)
                .WithMany(p => p.TblRequests)
                .HasForeignKey(d => d.FiscalPeriodId)
                .HasConstraintName("FK_tblRequest_tblFiscalPeriods");

            entity.HasOne(d => d.Receiver)
                .WithMany(p => p.TblRequestReceivers)
                .HasForeignKey(d => d.ReceiverId)
                .HasConstraintName("FK_tblRequest_tblRecipients2");

            entity.HasOne(d => d.Sender)
                .WithMany(p => p.TblRequestSenders)
                .HasForeignKey(d => d.SenderId)
                .HasConstraintName("FK_tblRequest_tblRecipients");
        });

        modelBuilder.Entity<TblSource>(entity =>
        {
            entity.HasKey(e => e.SourceId);

            entity.ToTable("tblSources");

            entity.Property(e => e.SourceId).HasColumnName("SourceID");
            entity.Property(e => e.Code).HasMaxLength(50);
            entity.Property(e => e.EntryDate).HasColumnType("datetime");
            entity.Property(e => e.FullName).HasMaxLength(200);
        });

        modelBuilder.Entity<TblStore>(entity =>
        {
            entity.HasKey(e => e.StoreId);

            entity.ToTable("tblStores");

            entity.HasIndex(e => e.Name, "IX_tblStores");

            entity.Property(e => e.StoreId).HasColumnName("StoreID");
            entity.Property(e => e.Barcode).HasMaxLength(100);
            entity.Property(e => e.Code).HasMaxLength(50);
            entity.Property(e => e.EntryDate).HasColumnType("datetime");
            entity.Property(e => e.Manufacturer).HasMaxLength(200);
            entity.Property(e => e.Model).HasMaxLength(200);
            entity.Property(e => e.Name).HasMaxLength(200);
            entity.Property(e => e.Pqsno)
                .HasMaxLength(100)
                .HasColumnName("PQSNo");
            entity.Property(e => e.RecipientId).HasColumnName("RecipientID");
            entity.Property(e => e.RefrigerantGas).HasMaxLength(200);
            entity.Property(e => e.RoomBox).HasMaxLength(100);
            entity.Property(e => e.TypeId).HasColumnName("StoreTypeId");

            entity.HasOne(d => d.Recipient).WithMany(p => p.TblStores)
                .HasForeignKey(d => d.RecipientId)
                .HasConstraintName("FK_tblStores_tblRecipients");

            entity.HasOne(d => d.Type).WithMany()
                .HasForeignKey(d => d.TypeId)
                .HasConstraintName("FK_tblStores_tblBaseInfo_store");
        });


        modelBuilder.Entity<TblUser>(entity =>
        {
            entity.HasKey(e => e.UserId);

            entity.ToTable("tblUsers");

            entity.Property(e => e.JobId).HasColumnName("JOBID");
            entity.Property(e => e.EduId).HasColumnName("EDUID");
            entity.Property(e => e.UserId).HasColumnName("UserID");
            entity.Property(e => e.Course).HasMaxLength(200);
            entity.Property(e => e.CourseDate).HasColumnType("datetime");
            entity.Property(e => e.Email).HasMaxLength(200);
            entity.Property(e => e.EnteredById).HasColumnName("EnteredByID");
            entity.Property(e => e.EntryDate).HasColumnType("datetime");
            entity.Property(e => e.FirstName).HasMaxLength(100);
            entity.Property(e => e.FreeNote).HasMaxLength(4000);
            entity.Property(e => e.Idnumber)
                .HasMaxLength(50)
                .HasColumnName("IDNumber");
            entity.Property(e => e.IsAdmin).HasColumnName("isAdmin");
            entity.Property(e => e.LastName).HasMaxLength(100);
            entity.Property(e => e.Lastlogin)
                .HasColumnType("datetime")
                .HasColumnName("lastlogin");
            entity.Property(e => e.Phone1)
                .HasMaxLength(50)
                .HasColumnName("phone1");
            entity.Property(e => e.Phone2)
                .HasMaxLength(50)
                .HasColumnName("phone2");
            entity.Property(e => e.RecipientId).HasColumnName("RecipientID");
            entity.Property(e => e.Storewarrning).HasColumnName("storewarrning");
            entity.Property(e => e.UserName).HasMaxLength(50);

            entity.HasOne(d => d.CodeJobNavigation).WithMany()
                .HasForeignKey(d => d.JobId)
                .HasConstraintName("FK_tblUsers_tblBaseInfo_job");

            entity.HasOne(d => d.CodeEduNavigation).WithMany()
                .HasForeignKey(d => d.EduId)
                .HasConstraintName("FK_tblUsers_tblBaseInfo_edu");

            entity.HasOne(d => d.EnteredBy).WithMany(p => p.InverseEnteredBy)
                .HasForeignKey(d => d.EnteredById)
                .HasConstraintName("FK_tblUsers_tblUsers");

            entity.HasOne(d => d.Role).WithMany(p => p.TblUsers)
                .HasForeignKey(d => d.RoleId)
                .HasConstraintName("FK_tblUsers_Roles");
        });

        modelBuilder.Entity<TblVoucherConfig>(entity =>
        {
            entity.HasKey(e => e.VoucherConfigurationId);

            entity.ToTable("tblVoucherConfig");

            entity.Property(e => e.VoucherConfigurationId).HasColumnName("VoucherConfigurationID");
            entity.Property(e => e.ShowSource).HasColumnName("showSource");
            entity.Property(e => e.ShowVvmstage).HasColumnName("ShowVVMStage");
            entity.Property(e => e.ShowVvmtype).HasColumnName("ShowVVMType");
            entity.Property(e => e.SignPlaceHolder1).HasMaxLength(200);
            entity.Property(e => e.SignPlaceHolder2).HasMaxLength(200);
            entity.Property(e => e.SignPlaceHolder3).HasMaxLength(200);
            entity.Property(e => e.SignPlaceHolder4).HasMaxLength(200);
        });


        modelBuilder.Entity<TblOrderInfo>(entity =>
        {
            entity.HasKey(e => e.PkOrderInfo);

            entity.ToTable("tblOrderInfo");

            entity.Property(e => e.PkOrderInfo)
                .HasColumnName("PK_ORDER_INFO");

            entity.Property(e => e.NumberOrder)
                .HasColumnName("NUMBER_ORDER");

            entity.Property(e => e.ManufactureDate)
                .HasColumnName("MANUFACTURE_DATE")
                .HasColumnType("date");

            entity.Property(e => e.ExpirationDate)
                .HasColumnName("EXPIRATION_DATE")
                .HasColumnType("date");

            entity.Property(e => e.Irc)
                .HasColumnName("IRC")
                .HasMaxLength(50);

            entity.Property(e => e.BatchNumber)
                .HasColumnName("BATCH_NUMBER")
                .HasMaxLength(50);

            entity.Property(e => e.DistributorCompanyCode)
                .HasColumnName("DISTRIBUTOR_COMPANY_CODE")
                .HasMaxLength(20);

            entity.Property(e => e.OrderCompanyCode)
                .HasColumnName("ORDER_COMPANY_CODE")
                .HasMaxLength(20);

            entity.Property(e => e.LicenseCode)
                .HasColumnName("LICENSE_CODE")
                .HasMaxLength(50);

            entity.Property(e => e.Prefix)
                .HasColumnName("PREFIX")
                .HasMaxLength(20);

            entity.Property(e => e.CreationTime)
                .HasColumnName("CREATION_TIME")
                .HasColumnType("datetime")
                .HasDefaultValueSql("GETDATE()");

            entity.Property(e => e.CreatedBy)
                .HasColumnName("CREATED_BY");

            entity.Property(e => e.Status)
                .HasColumnName("STATUS")
                .HasMaxLength(50);

            entity.Property(e => e.FileName)
                .HasColumnName("FILE_NAME")
                .HasMaxLength(255);

            entity.HasMany(d => d.TblUidInfos)
                .WithOne(p => p.OrderInfo)
                .HasForeignKey(p => p.FkOrderInfo)
                .HasConstraintName("FK_tblUidInfo_InfoOrder");


            entity.Property(e => e.FkArrivalId)
                .HasColumnName("FK_ARRIVAL_ID");

            entity.Property(e => e.FkItemId)
                .HasColumnName("FK_ITEM_ID");

            entity.Property(e => e.FkDosePerVialId)
                .HasColumnName("FK_DOSE_PER_VIAL_ID");


            entity.HasOne(d => d.Item)
                .WithMany()
                .HasForeignKey(d => d.FkItemId)
                .HasConstraintName("FK_tblOrderInfo_Item");


            entity.HasOne(d => d.DosePerVial)
                .WithMany()
                .HasForeignKey(d => d.FkDosePerVialId)
                .HasConstraintName("FK_tblOrderInfo_tblBaseInfo_dosePerVial");


            entity.HasOne(d => d.Arrival)
                .WithMany()
                .HasForeignKey(d => d.FkArrivalId)
                .HasConstraintName("FK_tblOrderInfo_Arrival");
        });


        modelBuilder.Entity<TblUidError>(entity =>
        {
            entity.HasKey(e => e.PkUidError);

            entity.ToTable("tblUidError");

            entity.Property(e => e.PkUidError)
                .HasColumnName("PK_UID_ERROR");

            entity.Property(e => e.FkUidInfo)
                .HasColumnName("FK_UID_INFO")
                .HasMaxLength(100);

            entity.Property(e => e.ErrorCode)
                .HasColumnName("ERROR_CODE")
                .HasMaxLength(100);

            entity.Property(e => e.ErrorMessage)
                .HasColumnName("[ERROR_MESSAGE]")
                .HasMaxLength(100);

            entity.Property(e => e.FkOrderInfo)
                .HasColumnName("FK_ORDER_INFO");

            entity.HasOne(d => d.OrderInfo)
                .WithMany(p => p.TblUidErrors)
                .HasForeignKey(d => d.FkOrderInfo)
                .HasConstraintName("FK_tblUidError_InfoOrder");
        });

        modelBuilder.Entity<TblDispatchUid>(entity =>
        {
            entity.HasKey(e => e.PKDispatchUid);

            entity.ToTable("tblDispatchUid");

            entity.Property(e => e.PKDispatchUid)
                .HasColumnName("PK_DISPATCH_UID");

            entity.Property(e => e.FKDispatchId)
                .HasColumnName("FK_DISPATCHES");

            entity.HasOne(d => d.Dispatch)
                .WithMany()
                .HasForeignKey(d => d.FKDispatchId)
                .HasConstraintName("FK_tblDispatchUid_Dispatches");

            entity.Property(e => e.FKUidInfo)
                .HasColumnName("FK_UID_INFO");

            entity.HasOne(d => d.UidInfo)
                .WithMany()
                .HasForeignKey(d => d.FKUidInfo)
                .HasConstraintName("FK_tblDispatchUid_UidInfo");
        });

        modelBuilder.Entity<TblUidInfo>(entity =>
        {
            entity.HasKey(e => e.PkUidInfo);

            entity.ToTable("tblUidInfo");

            entity.Property(e => e.PkUidInfo)
                .HasColumnName("PK_UID_INFO");

            entity.Property(e => e.PatchBarcode)
                .HasColumnName("PATCH_BARCODE")
                .HasMaxLength(100);

            entity.Property(e => e.Barcode)
                .HasColumnName("BARCODE")
                .HasMaxLength(100);

            entity.Property(e => e.HashCode)
                .HasColumnName("HASH_CODE")
                .HasMaxLength(100);

            entity.Property(e => e.FirstLevelUid)
                .HasColumnName("FIRST_LEVEL_UID")
                .HasMaxLength(50);

            entity.Property(e => e.SecLevelUid)
                .HasColumnName("SEC_LEVEL_UID")
                .HasMaxLength(50);

            entity.Property(e => e.ParentUid)
                .HasColumnName("PARENT_UID")
                .HasMaxLength(50);

            entity.Property(e => e.IsActive)
                .HasColumnName("IS_ACTIVE");

            entity.Property(e => e.Used)
                .HasColumnName("USED");

            entity.Property(e => e.ConsumedCount)
                .HasColumnName("CONSUMED_COUNT")
                .IsRequired(false);

            entity.Property(e => e.CurrentStore)
                .HasColumnName("CURRENT_STORE")
                .IsRequired(false);

            entity.Property(e => e.FkOrderInfo)
                .HasColumnName("FK_ORDER_INFO");

            entity.HasOne(d => d.OrderInfo)
                .WithMany(p => p.TblUidInfos)
                .HasForeignKey(d => d.FkOrderInfo)
                .HasConstraintName("FK_tblUidInfo_InfoOrder");


            entity.Property(e => e.CurrentStore)
                .HasColumnName("CURRENT_STORE");

            entity.Property(e => e.DoseCount)
                .HasColumnName("DOSE_COUNT");


            entity.Property(e => e.FKDispatchUid)
                .HasColumnName("FK_DISPATCH_UID");

            entity.HasOne(d => d.DispatchUid)
                .WithMany()
                .HasForeignKey(d => d.FKDispatchUid)
                .HasConstraintName("FK_tblUidInfo_tblDispatchUid");


            entity.Property(e => e.FkReceiveDispatchId)
                  .HasColumnName("FK_RECEIVER_ID");

            entity.Property(e => e.FkDestroyId)
                  .HasColumnName("FK_DESTROY_ID")
                  .IsRequired(false);

            entity.HasOne(e => e.DestroyUid)
                  .WithOne(d => d.UidInfo)
                  .HasForeignKey<TblUidInfo>(e => e.FkDestroyId)
                  .IsRequired(false)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<TblConsumerPerson>(entity =>
        {
            entity.HasKey(e => e.PkConsumerPersonId);

            entity.ToTable("tblConsumerPerson");

            entity.Property(e => e.PkConsumerPersonId)
                .HasColumnName("PK_CONSUMER_PERSON_ID");

            entity.Property(e => e.ConsumerPerson)
                .HasColumnName("CONSUMER_PERSON");

            entity.Property(e => e.Vaccinator)
                .HasColumnName("VACCINATOR");

            entity.Property(e => e.VaccinationRank)
                .HasColumnName("VACCINATION_RANK");

            entity.Property(e => e.EntryDate)
                .HasColumnName("ENTRYDATE");

            entity.Property(e => e.FkUid)
                .HasColumnName("FK_UID");

            entity.HasOne(d => d.UidInfo)
                .WithMany()
                .HasForeignKey(d => d.FkUid)
                .HasConstraintName("FK_tblConsumerPerson_tblUidInfo");

            entity.Property(e => e.FkConsumerRecipientId)
                .HasColumnName("FK_CONSUMER_RECIPIENT_ID");

            entity.HasOne(d => d.Recipient)
                .WithMany()
                .HasForeignKey(d => d.FkConsumerRecipientId)
                .HasConstraintName("FK_tblConsumerPerson_tblRecipients");
        });

        modelBuilder.Entity<TblReceiveDispatches>(entity =>
        {
            entity.ToTable("tblReceiveDispatches");

            entity.HasKey(e => e.PkReceiveDispatchesId);

            entity.Property(e => e.PkReceiveDispatchesId)
                  .HasColumnName("PK_RECEIVE_DISPATCHES_ID")
                  .ValueGeneratedOnAdd();

            entity.Property(e => e.ReceiveDate)
                  .HasColumnName("RECEIVE_DATE")
                  .HasColumnType("date")
                  .IsRequired();

            entity.Property(e => e.ReceiveNumber)
                  .HasColumnName("RECEIVE_NUMBER")
                  .IsRequired()
                  .HasDefaultValueSql("NEXT VALUE FOR SEQ_RECEIVE_DISPATCH");

            entity.Property(e => e.CreationTime)
                  .HasColumnName("CREATION_TIME")
                  .HasColumnType("datetime")
                  .IsRequired();

            entity.Property(e => e.CreatedBy)
                  .HasColumnName("CREATED_BY")
                  .IsRequired();

            entity.HasOne(e => e.CreatedByUser)
                  .WithMany()
                  .HasForeignKey(e => e.CreatedBy)
                  .HasConstraintName("FK_tblReceiveDispatches_tblUsers");

            entity.HasMany(e => e.UidInfos)
                  .WithOne(e => e.ReceiveDispatch)
                  .HasForeignKey(e => e.FkReceiveDispatchId)
                  .IsRequired(false)
                  .HasConstraintName("FK_tblUidInfo_tblReceiveDispatches");

            entity.HasMany(e => e.Dispatches)
                  .WithOne(e => e.ReceiveDispatch)
                  .HasForeignKey(e => e.FkReceiveDispatchId)
                  .IsRequired(false)
                  .HasConstraintName("FK_tblDispatches_tblReceiveDispatches");
        });





        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}