namespace MyApp.Domain.Medicines;

public sealed record MedicineDto(
    int Id, 
    string Code, 
    string Name
    );
