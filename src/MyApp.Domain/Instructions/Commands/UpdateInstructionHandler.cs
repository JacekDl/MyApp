using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MyApp.Domain.Common;
using MyApp.Domain.Data;
using MyApp.Model;

namespace MyApp.Domain.Instructions.Commands
{
    public record UpdateInstructionCommand(int Id, string Code, string Text) : IRequest<Result<bool>>;

    public class UpdateInstructionHandler : IRequestHandler<UpdateInstructionCommand, Result<bool>>
    {
        private readonly ApplicationDbContext _db;

        public UpdateInstructionHandler(ApplicationDbContext db)
        {
            _db = db;
        }
        public async Task<Result<bool>> Handle(UpdateInstructionCommand request, CancellationToken ct)
        {
            var entity = await _db.Set<Instruction>().FirstOrDefaultAsync(m => m.Id == request.Id, ct);
            if (entity is null)
            {
                return Result<bool>.Fail("Nie znaleziono dawkowania.");
            }

            (entity.Code, entity.Text) = FormatStringHelper.FormatCodeAndText(request.Code, request.Text);

            await _db.SaveChangesAsync(ct);
            return Result<bool>.Ok(true);
        }
    }
}
