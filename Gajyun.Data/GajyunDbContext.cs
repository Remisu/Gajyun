using Gajyun.Core.Models;
using Gajyun.Data.Configurations;
using Microsoft.EntityFrameworkCore;

namespace Gajyun.Data;

public sealed class GajyunDbContext(DbContextOptions<GajyunDbContext> options) : DbContext(options)
{
    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<Bed> Beds => Set<Bed>();
    public DbSet<OTA> OTAs => Set<OTA>();
    public DbSet<BookingStatus> BookingStatuses => Set<BookingStatus>();
    public DbSet<Country> Countries => Set<Country>();
    public DbSet<PaymentMethod> PaymentMethods => Set<PaymentMethod>();
    public DbSet<DocumentType> DocumentTypes => Set<DocumentType>();
    public DbSet<ExtraService> ExtraServices => Set<ExtraService>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<CustomerDocument> CustomerDocuments => Set<CustomerDocument>();
    public DbSet<ReservationGroup> ReservationGroups => Set<ReservationGroup>();
    public DbSet<Reservation> Reservations => Set<Reservation>();
    public DbSet<ReservationCustomer> ReservationCustomers => Set<ReservationCustomer>();
    public DbSet<ReservationAccommodation> ReservationAccommodations => Set<ReservationAccommodation>();
    public DbSet<ReservationCharge> ReservationCharges => Set<ReservationCharge>();
    public DbSet<ReservationPayment> ReservationPayments => Set<ReservationPayment>();
    public DbSet<OTAStatementItem> OTAStatementItems => Set<OTAStatementItem>();
    public DbSet<PaymentAllocation> PaymentAllocations => Set<PaymentAllocation>();
    public DbSet<StayMemo> StayMemos => Set<StayMemo>();
    public DbSet<Stay> Stays => Set<Stay>();
    public DbSet<StayReservation> StayReservations => Set<StayReservation>();
    public DbSet<OTARoomMapping> OTARoomMappings => Set<OTARoomMapping>();
    public DbSet<TaxPolicy> TaxPolicies => Set<TaxPolicy>();
    public DbSet<TaxBracket> TaxBrackets => Set<TaxBracket>();
    public DbSet<ReservationTax> ReservationTaxes => Set<ReservationTax>();
    public DbSet<ImportBatch> ImportBatches => Set<ImportBatch>();
    public DbSet<ImportRow> ImportRows => Set<ImportRow>();
    public DbSet<ReservationFinancialSummary> FinancialSummaries => Set<ReservationFinancialSummary>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("dbo");
        LookupConfiguration.Configure(modelBuilder);
        ReservationConfiguration.Configure(modelBuilder);
        FinancialConfiguration.Configure(modelBuilder);
        TaxAndImportConfiguration.Configure(modelBuilder);
        Seeding.LookupSeedData.Configure(modelBuilder);

        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            if (entity.GetViewName() is null)
                entity.SetTableName(entity.ClrType.Name);

            foreach (var property in entity.GetProperties())
            {
                var type = Nullable.GetUnderlyingType(property.ClrType) ?? property.ClrType;
                if (type == typeof(decimal) && property.GetPrecision() is null)
                {
                    property.SetPrecision(12);
                    property.SetScale(2);
                }
                if (type == typeof(DateTime))
                    property.SetColumnType("datetime2(0)");
                if (property.Name == "CurrencyCode")
                    property.SetColumnType("char(3)");
            }

            foreach (var foreignKey in entity.GetForeignKeys())
                foreignKey.DeleteBehavior = DeleteBehavior.NoAction;
        }
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        UpdateReservationTimestamps();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        UpdateReservationTimestamps();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private void UpdateReservationTimestamps()
    {
        var japanNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow,
            TimeZoneInfo.FindSystemTimeZoneById("Tokyo Standard Time"));
        foreach (var entry in ChangeTracker.Entries<Reservation>().Where(x => x.State == EntityState.Modified))
            entry.Entity.UpdatedAt = japanNow;
    }
}
