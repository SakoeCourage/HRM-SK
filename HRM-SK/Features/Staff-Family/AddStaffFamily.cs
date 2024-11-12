using AutoMapper;
using Carter;
using FluentValidation;
using HRM_SK.Database;
using HRM_SK.Entities.Staff;
using HRM_SK.Extensions;
using HRM_SK.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;
using static HRM_SK.Features.Staff_Family.AddStaffFamily;

namespace HRM_SK.Features.Staff_Family
{
    public class AddStaffFamily
    {

        public class AddStaffFamilyRequest : IRequest<Result<string>>
        {
            public Guid staffId { get; set; }
            public string fathersName { get; set; } = String.Empty;
            public string mothersName { get; set; } = String.Empty;
            public string? spouseName { get; set; } = String.Empty;
            public string? spousePhoneNumber { get; set; } = String.Empty;
            public string nextOfKIN { get; set; } = String.Empty;
            public string nextOfKINPhoneNumber { get; set; } = String.Empty;
            public string emergencyPerson { get; set; } = String.Empty;
            public string emergencyPersonPhoneNumber { get; set; } = String.Empty;
        }

        public class Validator : AbstractValidator<AddStaffFamilyRequest>
        {
            public Validator()
            {
                RuleFor(c => c.staffId).NotEmpty();
                RuleFor(c => c.fathersName).NotEmpty();
                RuleFor(c => c.mothersName).NotEmpty();
                RuleFor(c => c.nextOfKIN).NotEmpty();
                RuleFor(c => c.nextOfKINPhoneNumber).NotEmpty();
                RuleFor(c => c.emergencyPerson).NotEmpty();
                RuleFor(c => c.emergencyPersonPhoneNumber).NotEmpty();
            }

            internal sealed class Handler(DatabaseContext dbContext, IValidator<AddStaffFamilyRequest> validator, IMapper mapper) : IRequestHandler<AddStaffFamilyRequest, Result<string>>
            {
                public async Task<Result<string>> Handle(AddStaffFamilyRequest request, CancellationToken cancellationToken)
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

                    var existingData = await dbContext.StaffFamilyDetail.FirstOrDefaultAsync(s => s.staffId == request.staffId);

                    using (var dbTransaction = await dbContext.Database.BeginTransactionAsync())
                    {
                        try
                        {
                            if (existingData is null)
                            {
                                var newRecord = new StaffFamilyDetail
                                {
                                    staffId = request.staffId,
                                    fathersName = request.fathersName,
                                    mothersName = request.mothersName,
                                    spouseName = request.spouseName,
                                    spousePhoneNumber = request.spousePhoneNumber,
                                    nextOfKIN = request.nextOfKIN,
                                    nextOfKINPhoneNumber = request.nextOfKINPhoneNumber,
                                    emergencyPerson = request.emergencyPerson,
                                    emergencyPersonPhoneNumber = request.emergencyPersonPhoneNumber
                                };

                                dbContext.Add(newRecord);

                                var updateHistory = mapper.Map<StaffFamilyUpdatetHistory>(newRecord);
                                dbContext.Add(updateHistory);
                            }
                            else
                            {
                                existingData.fathersName = request.fathersName;
                                existingData.mothersName = request.mothersName;
                                existingData.spouseName = request.spouseName;
                                existingData.spousePhoneNumber = request.spousePhoneNumber;
                                existingData.nextOfKIN = request.nextOfKIN;
                                existingData.nextOfKINPhoneNumber = request.nextOfKINPhoneNumber;
                                existingData.emergencyPerson = request.emergencyPerson;
                                existingData.emergencyPersonPhoneNumber = request.emergencyPersonPhoneNumber;
                                existingData.updatedAt = DateTime.UtcNow;

                                dbContext.Update(existingData);

                                var updateHistory = mapper.Map<StaffFamilyUpdatetHistory>(existingData);
                                dbContext.Add(updateHistory);

                            }
                            await dbContext.SaveChangesAsync();
                            await dbTransaction.CommitAsync();

                            return Shared.Result.Success("Staff Family Record Saved");
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
    }


}

public class MapAddStaffFamilyEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("api/staff-family/update-or-new",
        async (ISender sender, AddStaffFamilyRequest request) =>
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

        }).WithTags("Staff Family Record")
            .WithGroupName(SwaggerEndpointDefintions.Planning)
            ;
    }
}
