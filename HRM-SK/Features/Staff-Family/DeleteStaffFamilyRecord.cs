using Carter;
using HRM_SK.Database;
using HRM_SK.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static HRM_SK.Features.Staff_Family.DeleteStaffFamilyRecord;

namespace HRM_SK.Features.Staff_Family
{
    public class DeleteStaffFamilyRecord
    {
        public class DeleteStaffFamilyRecordRequest : IRequest<Result<string>>
        {
            public Guid staffId { get; set; }
        }

        internal sealed class Handler(DatabaseContext dbContext) : IRequestHandler<DeleteStaffFamilyRecordRequest, Result<string>>
        {
            public async Task<Result<string>> Handle(DeleteStaffFamilyRecordRequest request, CancellationToken cancellationToken)
            {
                var affectedRows = await dbContext.StaffFamilyDetail
                    .Where(e => e.staffId == request.staffId)
                    .ExecuteDeleteAsync(cancellationToken);

                if (affectedRows == 0)
                {
                    return Shared.Result.Failure<string>(Error.CreateNotFoundError("Staff Family Record Was Not Found"));
                }

                return Shared.Result.Success("Staff Family Record Deleted");

            }
        }
    }

    public class MapDeleteStaffFamilyRecordEndpoint : ICarterModule
    {

        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapDelete("api/staff-family/{staffId}", async (ISender sender, Guid staffId) =>
            {
                var response = await sender.Send(new DeleteStaffFamilyRecordRequest { staffId = staffId });

                if (response.IsFailure)
                {
                    return Results.NotFound(response.Error);
                }

                return Results.Ok(response?.Value);

            }).WithTags("Staff Family Record")
                  .WithMetadata(new ProducesResponseTypeAttribute(StatusCodes.Status204NoContent))
                  .WithMetadata(new ProducesResponseTypeAttribute(typeof(Error), StatusCodes.Status400BadRequest))
              ;
        }
    }
}
