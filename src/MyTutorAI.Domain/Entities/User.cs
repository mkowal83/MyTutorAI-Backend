namespace MyTutorAI.Domain.Users;

/// <summary>
/// EN: Immutable account of a single student; the root of the per-user data isolation. The e-mail is stored normalized (trimmed, lowercase), the display name trimmed.
/// PL: Niemutowalne konto pojedynczego ucznia; korzeń izolacji danych per użytkownik. E-mail jest przechowywany znormalizowany (przycięty, małymi literami), nazwa wyświetlana przycięta.
/// </summary>
public sealed class User
{
    /// <summary>
    /// EN: Maximum length of <see cref="Email"/>, in characters.
    /// PL: Maksymalna długość <see cref="Email"/> w znakach.
    /// </summary>
    public const int EmailMaxLength = 256;

    /// <summary>
    /// EN: Maximum length of <see cref="DisplayName"/>, in characters.
    /// PL: Maksymalna długość <see cref="DisplayName"/> w znakach.
    /// </summary>
    public const int DisplayNameMaxLength = 100;

    /// <summary>
    /// EN: Creates the account; the e-mail is normalized with <see cref="NormalizeEmail"/> and the display name is trimmed before the guards run.
    /// PL: Tworzy konto; e-mail jest normalizowany przez <see cref="NormalizeEmail"/>, a nazwa wyświetlana przycinana przed sprawdzeniem guardów.
    /// </summary>
    /// <param name="id">
    /// EN: Account identifier; must not be <see cref="Guid.Empty"/>.
    /// PL: Identyfikator konta; nie może być <see cref="Guid.Empty"/>.
    /// </param>
    /// <param name="email">
    /// EN: E-mail address; must not be empty and, after normalization, must not exceed <see cref="EmailMaxLength"/> characters.
    /// PL: Adres e-mail; nie może być pusty i po normalizacji nie może przekraczać <see cref="EmailMaxLength"/> znaków.
    /// </param>
    /// <param name="passwordHash">
    /// EN: Already hashed password; must not be empty. The plaintext password never reaches the domain.
    /// PL: Już zahaszowane hasło; nie może być puste. Hasło w postaci jawnej nigdy nie trafia do domeny.
    /// </param>
    /// <param name="displayName">
    /// EN: Name shown to the student; must not be empty and, after trimming, must not exceed <see cref="DisplayNameMaxLength"/> characters.
    /// PL: Nazwa pokazywana uczniowi; nie może być pusta i po przycięciu nie może przekraczać <see cref="DisplayNameMaxLength"/> znaków.
    /// </param>
    /// <param name="createdAt">
    /// EN: Moment the account was created (UTC).
    /// PL: Moment utworzenia konta (UTC).
    /// </param>
    /// <exception cref="ArgumentException">
    /// EN: Any argument violates the rules above.
    /// PL: Którykolwiek argument narusza powyższe reguły.
    /// </exception>
    public User(Guid id, string email, string passwordHash, string displayName, DateTimeOffset createdAt)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("User id must not be empty.", nameof(id));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        ArgumentException.ThrowIfNullOrWhiteSpace(passwordHash);
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);

        var normalizedEmail = NormalizeEmail(email);
        if (normalizedEmail.Length > EmailMaxLength)
        {
            throw new ArgumentException($"Email must not exceed {EmailMaxLength} characters.", nameof(email));
        }

        var trimmedDisplayName = displayName.Trim();
        if (trimmedDisplayName.Length > DisplayNameMaxLength)
        {
            throw new ArgumentException(
                $"Display name must not exceed {DisplayNameMaxLength} characters.", nameof(displayName));
        }

        Id = id;
        Email = normalizedEmail;
        PasswordHash = passwordHash;
        DisplayName = trimmedDisplayName;
        CreatedAt = createdAt;
    }

    /// <summary>
    /// EN: Account identifier.
    /// PL: Identyfikator konta.
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// EN: Normalized e-mail address, unique across all accounts.
    /// PL: Znormalizowany adres e-mail, unikalny wśród wszystkich kont.
    /// </summary>
    public string Email { get; }

    /// <summary>
    /// EN: Password hash in the format of the configured password hasher.
    /// PL: Hash hasła w formacie skonfigurowanego hashera haseł.
    /// </summary>
    public string PasswordHash { get; }

    /// <summary>
    /// EN: Trimmed display name.
    /// PL: Przycięta nazwa wyświetlana.
    /// </summary>
    public string DisplayName { get; }

    /// <summary>
    /// EN: Moment the account was created (UTC).
    /// PL: Moment utworzenia konta (UTC).
    /// </summary>
    public DateTimeOffset CreatedAt { get; }

    /// <summary>
    /// EN: Normalizes an e-mail address for storage and uniqueness checks: trim + lowercase (invariant culture).
    /// PL: Normalizuje adres e-mail do zapisu i sprawdzania unikalności: trim + małe litery (kultura niezmienna).
    /// </summary>
    /// <param name="email">
    /// EN: Raw e-mail address; must not be null.
    /// PL: Surowy adres e-mail; nie może być null.
    /// </param>
    /// <returns>
    /// EN: Normalized e-mail address.
    /// PL: Znormalizowany adres e-mail.
    /// </returns>
    public static string NormalizeEmail(string email)
    {
        ArgumentNullException.ThrowIfNull(email);
        return email.Trim().ToLowerInvariant();
    }
}
