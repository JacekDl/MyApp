namespace MyApp.Domain.Reviews;

public record ConversationDto(string Number, bool Completed, IReadOnlyList<EntryDto> Entries);
