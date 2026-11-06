using System.Globalization;
using System.Text.Json;
using Gajyun.Core.Dictionary;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Gajyun.Data.Metadata;

public sealed class DataDictionaryService(IDbContextFactory<GajyunDbContext> contextFactory) : IDataDictionaryService
{
    public DictionaryCatalog Load()
    {
        using var stream = typeof(DataDictionaryService).Assembly.GetManifestResourceStream("Gajyun.Dictionary.v1.3.json")
            ?? throw new InvalidOperationException("The embedded schema dictionary is missing.");
        var documentation = JsonSerializer.Deserialize<Documentation>(stream, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
            ?? throw new InvalidOperationException("The embedded schema dictionary is empty.");
        using var db = contextFactory.CreateDbContext();
        var model = db.GetService<IDesignTimeModel>().Model;
        var tables = new List<DictionaryTable>();
        foreach (var entity in model.GetEntityTypes().OrderBy(x => x.GetViewName() ?? x.GetTableName(), StringComparer.Ordinal))
        {
            var isView = entity.GetViewName() is not null;
            var name = entity.GetViewName() ?? entity.GetTableName()
                ?? throw new InvalidOperationException("A mapped entity has no table or view name.");
            if (!documentation.Tables.TryGetValue(name, out var tableDocumentation))
                throw new InvalidOperationException($"Dictionary metadata is missing for {name}.");
            var store = isView ? StoreObjectIdentifier.View(name, entity.GetViewSchema())
                : StoreObjectIdentifier.Table(name, entity.GetSchema());
            var fields = entity.GetProperties().Select(property =>
            {
                var column = property.GetColumnName(store) ?? property.Name;
                if (!tableDocumentation.Fields.TryGetValue(column, out var description))
                    throw new InvalidOperationException($"Dictionary description is missing for {name}.{column}.");
                var foreignKeys = property.GetContainingForeignKeys().Select(fk =>
                    $"{fk.PrincipalEntityType.GetTableName()}({string.Join(", ", fk.PrincipalKey.Properties.Select(x => x.Name))})");
                var computed = property.GetComputedColumnSql();
                var defaultValue = computed is not null ? $"Computed: {computed}"
                    : property.GetDefaultValueSql() ?? (property.TryGetDefaultValue(out var value)
                        ? Convert.ToString(value, CultureInfo.InvariantCulture) ?? "" : "");
                return new DictionaryField(column, property.GetColumnType() ?? property.GetRelationalTypeMapping().StoreType,
                    !property.IsNullable, property.IsPrimaryKey(), string.Join("; ", foreignKeys), description,
                    defaultValue, tableDocumentation.Examples?.GetValueOrDefault(column) ?? "",
                    name == "Room" ? (column is "Capacity" or "DefaultTaxPolicyID" ? "1.1" : "1.0")
                        : name == "Reservation" && column == "SourcePaymentMethodID" || name == "ReservationGroup" && column == "LogoUrl" ? "1.3"
                        : name == "ImportBatch" ? (column is "SourceFileHash" or "SourceEncoding" or "SourceFileContent" or "ParserVersion" ? "1.2" : "1.0")
                        : name == "ImportRow" ? (column == "SourceRowHash" ? "1.2" : "1.0") : tableDocumentation.Version);
            }).OrderByDescending(x => x.IsPrimaryKey).ThenBy(x => x.Name, StringComparer.Ordinal).ToArray();
            var relationships = entity.GetForeignKeys().Select(fk =>
                $"({string.Join(", ", fk.Properties.Select(x => x.Name))}) -> {fk.PrincipalEntityType.GetTableName()}({string.Join(", ", fk.PrincipalKey.Properties.Select(x => x.Name))}); delete: {fk.DeleteBehavior}");
            var constraints = entity.GetKeys().Select(key => $"{(key.IsPrimaryKey() ? "Primary" : "Unique")} key: {string.Join(", ", key.Properties.Select(x => x.Name))}")
                .Concat(entity.GetIndexes().Where(x => x.IsUnique).Select(index => $"Unique: {string.Join(", ", index.Properties.Select(x => x.Name))}{(index.GetFilter() is string filter ? $" WHERE {filter}" : "")}"))
                .Concat(entity.GetCheckConstraints().Select(check => $"{check.Name}: {check.Sql}"));
            tables.Add(new DictionaryTable(name, tableDocumentation.Purpose, tableDocumentation.Version, isView,
                fields, string.Join(Environment.NewLine, relationships), string.Join(Environment.NewLine, constraints)));
        }
        if (tables.Count(x => !x.IsView) != 27 || tables.Count != documentation.Tables.Count)
            throw new InvalidOperationException("The dictionary and implemented schema do not contain the same 27 tables and financial view.");
        return new DictionaryCatalog(documentation.SchemaVersion, tables.AsReadOnly());
    }

    private sealed record Documentation(string SchemaVersion, Dictionary<string, TableDocumentation> Tables);
    private sealed record TableDocumentation(string Purpose, string Version, Dictionary<string, string> Fields, Dictionary<string, string>? Examples);
}
