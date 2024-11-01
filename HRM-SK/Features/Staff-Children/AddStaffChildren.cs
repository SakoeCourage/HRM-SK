using AutoMapper;
using Carter;
using FluentValidation;
using HRM_SK.Database;
using HRM_SK.Entities.Staff;
using HRM_SK.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static HRM_SK.Features.Staff_Children.AddStaffChildren;

namespace HRM_SK.Features.Staff_Children
{
    public class AddStaffChildren
    {

        public class AddStaffChildrenRequest : IRequest<Result<string>>
        {
            public Guid staffId { get; set; }
            public string childName { get; set; } = string.Empty;
            public DateOnly dateOfBirth { get; set; }
            public string gender { get; set; } = string.Empty;
        }

        public class Validator : AbstractValidator<AddStaffChildrenRequest>
        {
            public Validator()
            {
                RuleFor(c => c.staffId).NotEmpty();
                RuleFor(c => c.childName).NotEmpty();
                RuleFor(c => c.dateOfBirth).NotEmpty();
                RuleFor(c => c.gender).NotEmpty();

            }
        }

        internal sealed class Handler(DatabaseContext dbContext, IValidator<AddStaffChildrenRequest> validator, IMapper mapper) : IRequestHandler<AddStaffChildrenRequest, Result<string>>
        {
            public async Task<Result<string>> Handle(AddStaffChildrenRequest request, CancellationToken cancellationToken)
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

                var duplicateChildren = await dbContext
                    .StaffChildrenDetail
                    .AnyAsync(staff => staff.staffId == request.staffId && staff.childName.ToLower() == request.childName.ToLower());

                if (duplicateChildren is true)
                {
                    return Shared.Result.Failure<string>(Error.BadRequest("Staff Child Already Exist"));
                }

                using (var dbTransaction = await dbContext.Database.BeginTransactionAsync())
                {
                    try
                    {


                        var newRecord = new StaffChildrenDetail
                        {
                            staffId = request.staffId,
                            childName = request.childName,
                            dateOfBirth = request.dateOfBirth,
                            gender = request.gender
                        };
                        dbContext.Add(newRecord);

                        var updateHistory = mapper.Map<StaffChildrenUpdateHistory>(newRecord);

                        await dbContext.SaveChangesAsync();
                        await dbTransaction.CommitAsync();

                        return Shared.Result.Success("Staff Child Record Saved");

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

public class MapStaffAddChildEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("api/staff-children",
        async (ISender sender, AddStaffChildrenRequest request) =>
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

        })
            .WithTags("Staff Children Record")
            .WithMetadata(new ProducesResponseTypeAttribute(typeof(Error), StatusCodes.Status422UnprocessableEntity))
            .WithMetadata(new ProducesResponseTypeMetadata(StatusCodes.Status200OK));
    }
}
