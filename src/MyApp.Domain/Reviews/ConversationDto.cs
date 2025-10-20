namespace MyApp.Domain.Reviews;

public record ConversationDto(string Number, IReadOnlyList<EntryDto> Entries);
