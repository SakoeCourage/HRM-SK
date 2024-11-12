using AutoMapper;
using Carter;
using FluentValidation;
using HRM_SK.Database;
using HRM_SK.Entities.Staff;
using HRM_SK.Extensions;
using HRM_SK.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static HRM_SK.Features.Staff_Children.AddStaffChildren;
using static HRM_SK.Features.Staff_Children.UpdateStaffChildRecord;

namespace HRM_SK.Features.Staff_Children
{
    public class UpdateStaffChildRecord
    {
        public class UpdateStaffChildRequest : IRequest<Result<string>>
        {
            public Guid id { get; set; }
            public Guid staffId { get; set; }
            public string childName { get; set; } = string.Empty;
            public DateOnly dateOfBirth { get; set; }
            public string gender { get; set; } = string.Empty;
        }

        public class Validator : AbstractValidator<UpdateStaffChildRequest>
        {
            public Validator()
            {
                RuleFor(c => c.staffId).NotEmpty();
                RuleFor(c => c.childName).NotEmpty();
                RuleFor(c => c.dateOfBirth).NotEmpty();
                RuleFor(c => c.gender).NotEmpty();
            }
        }

        internal sealed class Handler(DatabaseContext dbContext, IValidator<UpdateStaffChildRequest> validator, IMapper mapper) : IRequestHandler<UpdateStaffChildRequest, Result<string>>
        {
            public async Task<Result<string>> Handle(UpdateStaffChildRequest request, CancellationToken cancellationToken)
            {

                var duplicateChildren = await dbContext
                   .StaffChildrenDetail
                   .AnyAsync(data => data.staffId == request.staffId && data.childName.ToLower() == request.childName.ToLower() && data.Id != request.id);

                if (duplicateChildren is true)
                {
                    return Shared.Result.Failure<string>(Error.BadRequest("Staff Child Already Exist"));
                }

                using (var dbTransaction = await dbContext.Database.BeginTransactionAsync())
                {

                    try
                    {

                        var affectedRows = await dbContext.StaffChildrenDetail
                               .Where(entry => entry.Id == request.id && entry.staffId == request.staffId)
                               .ExecuteUpdateAsync((setters) => setters
                               .SetProperty(entry => entry.childName, request.childName)
                               .SetProperty(entry => entry.dateOfBirth, request.dateOfBirth)
                               .SetProperty(entry => entry.gender, request.gender)
                               .SetProperty(entry => entry.updatedAt, DateTime.UtcNow)
                               );

                        if (affectedRows == 0)
                        {
                            return Shared.Result.Failure<string>(Error.CreateNotFoundError("Child Record Not Found"));
                        }

                        var bioupdateHidstory = new StaffChildrenUpdateHistory
                        {
                            childName = request.childName,
                            dateOfBirth = request.dateOfBirth,
                            gender = request.gender,
                            staffId = request.staffId,
                        };
                        dbContext.Add(bioupdateHidstory);

                        await dbContext.SaveChangesAsync();
                        await dbTransaction.CommitAsync();
                        return Shared.Result.Success<string>("Staff Child Record Saved");

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

public class MapUpdateChildRecordEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPatch("api/children/{id}", async (ISender sender, AddStaffChildrenRequest request, Guid id) =>
        {
            var response = await sender.Send(new UpdateStaffChildRequest
            {
                staffId = request.staffId,
                id = id,
                dateOfBirth = request.dateOfBirth,
                gender = request.gender,
                childName = request.childName
            });

            if (response.IsFailure)
            {
                return Results.NotFound(response.Error);
            }

            return Results.Ok(response?.Value);

        }).WithTags("Staff Children Record")
              .WithMetadata(new ProducesResponseTypeAttribute(StatusCodes.Status204NoContent))
              .WithMetadata(new ProducesResponseTypeAttribute(typeof(Error), StatusCodes.Status400BadRequest))
              .WithGroupName(SwaggerEndpointDefintions.Planning)
          ;
    }
}
