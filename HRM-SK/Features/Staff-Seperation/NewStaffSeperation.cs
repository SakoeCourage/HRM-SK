using Carter;
using FluentValidation;
using HRM_SK.Contracts;
using HRM_SK.Database;
using HRM_SK.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;
using static HRM_SK.Features.Staff_Seperation.NewStaffSeperation;

namespace HRM_SK.Features.Staff_Seperation
{
    public class NewStaffSeperation
    {

        public class NewSeperationRequestData : IRequest<Shared.Result<string>>
        {
            public Guid StaffId { get; set; }
            public string Reason { get; set; }
            public DateOnly DateOfSeparation { get; set; }
            public string? Comment { get; set; }
        }

        public class NewSeperationRequestDataBody
        {
            public string Reason { get; set; }
            public DateOnly DateOfSeparation { get; set; }
            public string? Comment { get; set; }
        }
    }

    public class Validator : AbstractValidator<NewSeperationRequestData>
    {
        public Validator()
        {
            RuleFor(c => c.Reason).NotEmpty();
            RuleFor(c => c.DateOfSeparation).NotEmpty();
        }

    }


    internal sealed class Handler(DatabaseContext dbContext, IValidator<NewSeperationRequestData> validator) : IRequestHandler<NewSeperationRequestData, Shared.Result<string>>
    {
        public async Task<Result<string>> Handle(NewSeperationRequestData request, CancellationToken cancellationToken)
        {

            var validationResult = validator.Validate(request);

            if (validationResult.IsValid is false)
            {
                return Shared.Result.Failure<string>(Error.ValidationError(validationResult));
            }

            var existingStaff = await dbContext.Staff.FirstOrDefaultAsync(s => s.Id == request.StaffId);

            if (existingStaff is null)
            {
                return Shared.Result.Failure<string>(Error.CreateNotFoundError("Staff's data was not found"));
            }

            if (existingStaff.status == StaffStatusTypes.inActive)
            {
                return Shared.Result.Failure<string>(Error.BadRequest("Cannot proceed with current staff"));
            }

            using (var dbTransaction = await dbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    existingStaff.status = StaffStatusTypes.inActive;
                    dbContext.Seperation.Add(new Entities.HRMActivities.Seperation
                    {
                        createdAt = DateTime.UtcNow,
                        updatedAt = DateTime.UtcNow,
                        Reason = request.Reason,
                        StaffId = request.StaffId,
                        DateOfSeparation = request.DateOfSeparation,
                        comment = request.Comment
                    });

                    await dbContext.SaveChangesAsync();
                    await dbTransaction.CommitAsync();
                    return Shared.Result.Success<string>("Staff Separation Successful");
                }
                catch (Exception ex)
                {
                    await dbTransaction.RollbackAsync();
                    return Shared.Result.Failure<string>(Error.BadRequest(ex.Message));

                }

            }
        }
    }

    public class MapStaffSeperationEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("api/staff-separation/{staffId}", async (ISender sender, Guid staffId, NewSeperationRequestDataBody request) =>
            {
                var response = await sender.Send(new NewSeperationRequestData
                {
                    Comment = request.Comment,
                    StaffId = staffId,
                    Reason = request.Reason,
                    DateOfSeparation = request.DateOfSeparation
                });

                if (response.IsSuccess)
                {
                    return Results.Ok(response.Value);
                }

                if (response.IsFailure)
                {
                    return Results.UnprocessableEntity(response.Error);
                }
                return Results.BadRequest();
            }).WithTags("Staff-Separation");
        }
    }
}
