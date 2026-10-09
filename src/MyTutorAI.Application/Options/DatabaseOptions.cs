using System.ComponentModel.DataAnnotations;

namespace MyTutorAI.Application.Options;

/// <summary>
/// EN: SQL Server connection settings bound from the <c>Persistence:SqlServer</c> configuration section.
/// PL: Ustawienia połączenia z SQL Server wiązane z sekcji konfiguracji <c>Persistence:SqlServer</c>.
/// </summary>
public sealed class DatabaseOptions
{
    /// <summary>
    /// EN: Name of the configuration section bound to this type (<c>Persistence:SqlServer</c>).
    /// PL: Nazwa sekcji konfiguracji wiązanej z tym typem (<c>Persistence:SqlServer</c>).
    /// </summary>
    public const string SectionName = "Persistence:SqlServer";

    /// <summary>
    /// EN: SQL Server connection string; required.
    /// PL: Connection string SQL Server; wymagany.
    /// </summary>
    [Required]
    public string ConnectionString { get; set; } = string.Empty;
}
