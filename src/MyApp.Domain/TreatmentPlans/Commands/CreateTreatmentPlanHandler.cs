using FluentValidation;
using MediatR;
using MyApp.Domain.Common;
using MyApp.Domain.Data;
using MyApp.Domain.Users;
using MyApp.Model;
using MyApp.Model.enums;
using System.Security.Cryptography;

namespace MyApp.Domain.TreatmentPlans.Commands
{
    public record class CreateTreatmentPlanCommand(
        string PharmacistId,
        IReadOnlyList<CreateTreatmentPlanMedicineDTO> Medicines,
        string? Advice) 
        : IRequest<CreateTreatmentPlanResult>;

    public record class CreateTreatmentPlanResult : Result<TreatmentPlan>;

    public class CreateTreatmentPlanHandler : IRequestHandler<CreateTreatmentPlanCommand, CreateTreatmentPlanResult>
    {
        private readonly ApplicationDbContext _db;

        public CreateTreatmentPlanHandler(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<CreateTreatmentPlanResult> Handle(CreateTreatmentPlanCommand request, CancellationToken ct)
        {
            var validator = new CreateTreatmentPlanValidator().Validate(request);
            if (!validator.IsValid)
            {
                return new() { ErrorMessage = string.Join(";", validator.Errors.Select(e => e.ErrorMessage)) };
            }

            var plan = new TreatmentPlan
            {
                DateCreated = DateTime.UtcNow,
                DateStarted = null,
                DateCompleted = null,
                IdPharmacist = request.PharmacistId,
                Status = TreatmentPlanStatus.Created
            };

            string number = GenerateDigits();
            plan.Number = number;

            _db.TreatmentPlans.Add(plan);
            await _db.SaveChangesAsync(ct);

            var medicinesToAdd = new List<TreatmentPlanMedicine>();

            foreach(var med in request.Medicines)
            {
                var times = ExpandFrequency(med.MedicineFrequency);

                foreach(var time in times)
                {
                    medicinesToAdd.Add(new TreatmentPlanMedicine
                    {
                        IdTreatmentPlan = plan.Id,
                        MedicineName = med.MedicineName,
                        Dosage = med.MedicineDosage,
                        TimeOfDay = time
                    });
                }
            }

            plan.AdviceFullText = BuildAdviceFullText(request.Medicines, request.Advice);

            if (medicinesToAdd.Count > 0)
            {
                _db.TreatmentPlanMedicines.AddRange(medicinesToAdd);
            }

            if (!string.IsNullOrWhiteSpace(request.Advice))
            {
                var advice = new TreatmentPlanAdvice
                {
                    IdTreatmentPlan = plan.Id,
                    AdviceText = request.Advice.Trim()
                };
                _db.TreatmentPlanAdvices.Add(advice);
            }

            await _db.SaveChangesAsync(ct);
            return new() { Value = plan };
        }

        //TODO: przeniesc do slownika
        private static IReadOnlyList<TimeOfDay> ExpandFrequency(string token) =>
            token switch
            {
                "raz_dziennie_rano" => new[] { TimeOfDay.Rano },
                "raz_dziennie_wieczorem" => new[] { TimeOfDay.Wieczor },
                "dwa_razy_dziennie" => new[] { TimeOfDay.Rano, TimeOfDay.Wieczor },
                "trzy_razy_dziennie" => new[] { TimeOfDay.Rano, TimeOfDay.Poludnie, TimeOfDay.Wieczor },
                "cztery_razy_dziennie" => new[] { TimeOfDay.Rano, TimeOfDay.Poludnie, TimeOfDay.Popoludnie, TimeOfDay.Wieczor },
                _ => Array.Empty<TimeOfDay>()
            };

        private static string BuildAdviceFullText(
            IEnumerable<CreateTreatmentPlanMedicineDTO> medicines,
            string? advice)
        {
            var lines = new List<string>();
            foreach (var m in medicines)
            {
                lines.Add($"{m.MedicineName} - {m.MedicineDosage} {m.MedicineFrequency.Replace('_', ' ')}.");
            }

            if (!string.IsNullOrWhiteSpace(advice))
            {
                lines.Add(advice.Trim());
            }

            return string.Join("\n", lines);
        }
        private static string GenerateDigits(int bytes = 16)
        {
            byte[] buffer = new byte[bytes];
            RandomNumberGenerator.Fill(buffer);
            var token = Convert.ToBase64String(buffer)
                .Replace("+", "")
                .Replace("/", "")
                .Replace("=", "")
                [..bytes];
            return token;
        }
    }

    public class CreateTreatmentPlanValidator : AbstractValidator<CreateTreatmentPlanCommand>
    {
        private static readonly string[] AllowedFrequencyTokens =
        {
            "raz_dziennie_rano",
            "raz_dziennie_wieczorem",
            "dwa_razy_dziennie",
            "trzy_razy_dziennie",
            "cztery_razy_dziennie",
        };

        public CreateTreatmentPlanValidator()
        {
            RuleFor(x => x.PharmacistId)
                .Must(id => !string.IsNullOrWhiteSpace(id))
                    .WithMessage("Id farmaceuty nie może być puste.");

            RuleFor(x => x.Medicines)
                .Must(m => m.Count > 0)
                    .WithMessage("Plan leczenia musi zawierać co najmniej jeden lek.");

            RuleForEach(x => x.Medicines)
                .SetValidator(new CreateTreatmentPlanMedicineDtoValidator());

            RuleFor(x => x.Advice)
                .MaximumLength(2000)
                    .WithMessage("Dodatkowa porada nie może przekraczać 2000 znaków.")
                    .When(x => x.Advice is not null);
        }

        private class CreateTreatmentPlanMedicineDtoValidator : AbstractValidator<CreateTreatmentPlanMedicineDTO>
        {
            public CreateTreatmentPlanMedicineDtoValidator()
            {
                RuleFor(x => x.MedicineName)
                    .Must(v => !string.IsNullOrWhiteSpace(v))
                        .WithMessage("Nazwa leku nie może być pusta.")
                    .MaximumLength(128)
                        .WithMessage("Nazwa leku nie może być dłuższa niż 128 znaków.");

                RuleFor(x => x.MedicineDosage)
                    .Must(v => !string.IsNullOrWhiteSpace(v))
                        .WithMessage("Dawkowanie leku nie może być puste.")
                    .MaximumLength(128)
                        .WithMessage("Dawkowanie leku nie może być dłuższe niż 128 znaków.");

                RuleFor(x => x.MedicineFrequency)
                    .Must(v => !string.IsNullOrWhiteSpace(v))
                        .WithMessage("Częstotliwość nie może być pusta.")
                    .Must(v => AllowedFrequencyTokens.Contains(v))
                        .WithMessage("Nieprawidłowa częstotliwość dawkowania.");
            }
        }
    }
}
