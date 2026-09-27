using System.ComponentModel;
using System.Text.Json.Serialization;

namespace MyTutorAI.Application.Llm.Contracts;

/// <summary>
/// EN: Single turn of a generated practice dialogue; element of <see cref="DialogueGenerationResponse.Turns"/>.
/// PL: Pojedyncza tura wygenerowanego dialogu ćwiczeniowego; element <see cref="DialogueGenerationResponse.Turns"/>.
/// </summary>
/// <remarks>
/// EN: Part of the structured output contract: <c>JsonPropertyName</c> and <c>Description</c> attributes go into the JSON schema imposed on the model, so changing them changes the schema.
/// PL: Część kontraktu structured output: atrybuty <c>JsonPropertyName</c> i <c>Description</c> trafiają do schematu JSON narzucanego modelowi, więc ich zmiana zmienia schemat.
/// </remarks>
/// <param name="Speaker">
/// EN: Person saying the line.
/// PL: Osoba wypowiadająca kwestię.
/// </param>
/// <param name="Text">
/// EN: Content of the line, in English.
/// PL: Treść kwestii, po angielsku.
/// </param>
public sealed record DialogueTurn(
    [property: JsonPropertyName("speaker")]
    [property: Description("Osoba wypowiadająca kwestię")]
    string Speaker,
    [property: JsonPropertyName("text")]
    [property: Description("Treść wypowiedzi")]
    string Text);
