namespace Gajyun.Core.Dictionary;

public sealed record DictionaryField(string Name, string SqlType, bool IsRequired, bool IsPrimaryKey,
    string ForeignKey, string Description, string DefaultValue, string Example, string Version);

public sealed record DictionaryTable(string Name, string Purpose, string Version, bool IsView,
    IReadOnlyList<DictionaryField> Fields, string Relationships, string Constraints);

public sealed record DictionaryCatalog(string SchemaVersion, IReadOnlyList<DictionaryTable> Tables);

public interface IDataDictionaryService
{
    DictionaryCatalog Load();
}
