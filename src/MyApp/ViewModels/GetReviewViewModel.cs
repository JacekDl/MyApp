using System.ComponentModel.DataAnnotations;

namespace MyApp.ViewModels;

public class GetReviewViewModel
{
    [Required, MaxLength(128)]
    public string Number { get; set; } = string.Empty;
}
