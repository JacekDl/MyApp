namespace MyApp.Application.Reviews;

public record ConversationDto(string Number, IReadOnlyList<EntryDto> Entries);
