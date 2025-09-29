using System.ComponentModel.DataAnnotations;

namespace MyApp.Domain;

public class Review
{
    public int Id { get; set; }

    [Required, MaxLength(128)]
    public string Number { get; set; } = default!;
    public DateTime DateCreated { get; set; } //will not need that - get DateCreated from first Entry 
    public bool Completed { get; set; }

    [Required]
    public string PharmacistId { get; set; } = default!;
    public User Pharmacist { get; set; } = default!;

    public string? PatientId { get; set; }
    public User? Patient { get; set; } = default!;

    public List<Entry> Entries { get; set; } = new();
  
    public static Review Create(string pharmacistId, string initialTxt, string number, string? patientId = null)
    {
        var review =  new Review
        {
            PharmacistId = pharmacistId,
            PatientId = patientId,
            Number = number,
            DateCreated = DateTime.UtcNow,
            Completed = false
        };
        review.Entries.Add(Entry.Create(pharmacistId, initialTxt));
        return review;
    }
}
