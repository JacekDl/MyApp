using MyApp.Domain.Medicines;

namespace MyApp.Web.ViewModels;

public class MedicinesViewModel : PagedViewModel
{
    public IReadOnlyList<MedicineDto> Medicines { get; set; } = [];
}