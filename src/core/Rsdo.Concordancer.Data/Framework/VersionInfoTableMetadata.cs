using FluentMigrator.Runner.VersionTableInfo;

namespace Rsdo.Concordancer.Data.Framework;

public class VersionInfoTableMetadata : IVersionTableMetaData
{
    public string ColumnName => "version";

    public string TableName => "versioninfo";

    public string DescriptionColumnName => "description";

    public string AppliedOnColumnName => "appliedon";

    public string UniqueIndexName => "uc_version";

    public bool CreateWithPrimaryKey => false;

    public object ApplicationContext { get; set; }

    public bool OwnsSchema => true;

    public string SchemaName => "public";
}