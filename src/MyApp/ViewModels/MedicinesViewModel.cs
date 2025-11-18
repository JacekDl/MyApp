using MyApp.Domain.Medicines;

namespace MyApp.Web.ViewModels;

public class MedicinesViewModel : ViewModelBase
{
    public IReadOnlyList<MedicineDto> Medicines { get; set; } = [];
}