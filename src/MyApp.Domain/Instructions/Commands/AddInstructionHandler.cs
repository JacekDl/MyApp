using MediatR;
using Microsoft.EntityFrameworkCore;
using MyApp.Domain.Common;
using MyApp.Domain.Data;
using MyApp.Model;

namespace MyApp.Domain.Instructions.Commands
{
    public sealed record AddInstructionCommand(string Code, string Text) : IRequest<Result<bool>>;

    public sealed class AddInstructionHandler(ApplicationDbContext db) : IRequestHandler<AddInstructionCommand, Result<bool>>
    {
        public async Task<Result<bool>> Handle(AddInstructionCommand request, CancellationToken ct)
        {
            var code = (request.Code ?? "").Trim().ToUpper();
            var text = (request.Text ?? "").Trim();
            text = text.Length switch
            {
                0 => "",
                1 => text.ToUpper(),
                _ => char.ToUpper(text[0]) + text[1..].ToLower()
            };

            if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(text))
                return Result<bool>.Fail("Kod i dawkowanie są wymagane.");

            var exists = await db.Set<Instruction>()
                .AnyAsync(i => i.Code == code, ct);

            if (exists)
                return Result<bool>.Fail($"Kod '{code}' jest już używany.");

            db.Add(new Instruction { Code = code, Text = text });
            await db.SaveChangesAsync(ct);
            return Result<bool>.Ok(true);
        }
    }
}
