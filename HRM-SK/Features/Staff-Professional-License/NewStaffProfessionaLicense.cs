using AutoMapper;
using Carter;
using FluentValidation;
using HRM_SK.Database;
using HRM_SK.Entities.Staff;
using HRM_SK.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;
using static HRM_SK.Features.Staff_Professional_License.NewStaffProfessionaLicense;

namespace HRM_SK.Features.Staff_Professional_License
{
    public class NewStaffProfessionaLicense
    {
        public class ProfessionalLIncenceRequest : IRequest<Shared.Result<string>>
        {
            public Guid staffId { get; set; }
            public Guid professionalBodyId { get; set; }
            public string pin { get; set; }
            public DateOnly issuedDate { get; set; }
            public DateOnly expiryDate { get; set; }
        }

        public class Validator : AbstractValidator<ProfessionalLIncenceRequest>
        {
            public Validator()
            {

                RuleFor(c => c.staffId).NotEmpty();
                RuleFor(c => c.issuedDate).NotEmpty();
                RuleFor(c => c.pin).NotEmpty();
                RuleFor(c => c.professionalBodyId).NotEmpty();
                RuleFor(c => c.expiryDate).NotEmpty();
            }
        }

        internal sealed class Handler(DatabaseContext dbContext, IValidator<ProfessionalLIncenceRequest> validator, IMapper mapper) : IRequestHandler<ProfessionalLIncenceRequest, Shared.Result<string>>
        {
            public async Task<Result<string>> Handle(ProfessionalLIncenceRequest request, CancellationToken cancellationToken)
            {
                var validationResult = validator.Validate(request);

                if (validationResult.IsValid is false)
                {
                    return Shared.Result.Failure<string>(Error.ValidationError(validationResult));
                }

                var staff = await dbContext.Staff.AnyAsync(s => s.Id == request.staffId);

                if (staff is false)
                {
                    return Shared.Result.Failure<string>(Error.CreateNotFoundError("Staff Record Not Found"));
                }

                var existingData = await dbContext.StaffProfessionalLincense.FirstOrDefaultAsync(s => s.staffId == request.staffId);


                using (var dbTransaction = await dbContext.Database.BeginTransactionAsync())
                {
                    try
                    {

                        if (existingData is null)
                        {
                            var newData = new StaffProfessionalLincense
                            {
                                staffId = request.staffId,
                                professionalBodyId = request.professionalBodyId,
                                pin = request.pin,
                                issuedDate = request.issuedDate,
                                expiryDate = request.expiryDate
                            };

                            dbContext.Add(newData);

                            var updateHistory = mapper.Map<StaffProfessionalLincenseUpdateHistory>(newData);
                            dbContext.Add(updateHistory);

                        }
                        else
                        {
                            existingData.professionalBodyId = request.professionalBodyId;
                            existingData.pin = request.pin;
                            existingData.issuedDate = request.issuedDate;
                            existingData.expiryDate = request.expiryDate;
                            existingData.updatedAt = DateTime.UtcNow;

                            dbContext.Update(existingData);

                            var updateHistory = mapper.Map<StaffProfessionalLincenseUpdateHistory>(existingData);
                            dbContext.Add(updateHistory);
                        }

                        await dbContext.SaveChangesAsync();
                        await dbTransaction.CommitAsync();

                        return Shared.Result.Success("Staff Professional License Record Saved");
                    }
                    catch (Exception ex)
                    {
                        await dbTransaction.RollbackAsync();
                        return Shared.Result.Failure<string>(Error.BadRequest(ex.Message));
                    }
                }

            }
        }
    }

    public class MappStaffPLEndpoint : ICarterModule
    {

        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("api/staff-professional-license/update-or-new",
            async (ISender sender, ProfessionalLIncenceRequest request) =>
            {
                var response = await sender.Send(request);

                if (response.IsSuccess)
                {
                    return Results.Ok(response.Value);
                }

                if (response.IsFailure)
                {
                    return Results.UnprocessableEntity(response.Error);
                }

                return Results.BadRequest("Something Went Wrong");

            }).WithTags("Staff Professional License Record");
        }
    }
}
