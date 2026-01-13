namespace MyApp.Domain.Users;

public record CreateTreatmentPlanMedicineDTO(
    string MedicineName, 
    string MedicineDosage, 
    string MedicineFrequency
    );
