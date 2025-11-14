using MediatR;
using Microsoft.EntityFrameworkCore;
using MyApp.Domain.Common;
using MyApp.Domain.Data;
using MyApp.Model;

namespace MyApp.Domain.Instructions.Commands
{
    public record class AddInstructionCommand(string Code, string Text) : IRequest<AddInstructionResult>;

    public record class AddInstructionResult : Result;

    public class AddInstructionHandler : IRequestHandler<AddInstructionCommand, AddInstructionResult>
    {
        private readonly ApplicationDbContext _db;

        public AddInstructionHandler(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<AddInstructionResult> Handle(AddInstructionCommand request, CancellationToken ct)
        {

            var (code, text) = FormatStringHelper.FormatCodeAndText(request.Code, request.Text);

            var exists = await _db.Set<Instruction>()
                .AnyAsync(i => i.Code == code, ct);

            if (exists)
                return new() { ErrorMessage = $"Kod '{code}' jest już używany." };

            _db.Add(new Instruction { Code = code, Text = text });
            await _db.SaveChangesAsync(ct);
            return new();
        }
    }
}
