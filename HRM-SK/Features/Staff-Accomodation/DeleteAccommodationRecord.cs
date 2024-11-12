using Carter;
using HRM_SK.Database;
using HRM_SK.Extensions;
using HRM_SK.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static HRM_SK.Features.Staff_Accomodation.DeleteAccommodationRecord;

namespace HRM_SK.Features.Staff_Accomodation
{
    public class DeleteAccommodationRecord
    {
        public class DeleteAccommodationRecordRequest : IRequest<Result<string>>
        {
            public Guid staffId { get; set; }
        }

        internal sealed class Handler(DatabaseContext dbContext) : IRequestHandler<DeleteAccommodationRecordRequest, Result<string>>
        {
            public async Task<Result<string>> Handle(DeleteAccommodationRecordRequest request, CancellationToken cancellationToken)
            {
                var affectedRows = await dbContext.StaffAccomodationDetail
                    .Where(e => e.staffId == request.staffId)
                    .ExecuteDeleteAsync(cancellationToken);

                if (affectedRows == 0)
                {
                    return Shared.Result.Failure<string>(Error.CreateNotFoundError("Staff Accommodation Record Was Not Found"));
                }

                return Shared.Result.Success("Staff Accommodation Record Deleted");

            }
        }
    }

    public class DeleteStaffAccommodationEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapDelete("api/staff-accommodation/{staffId}", async (ISender sender, Guid staffId) =>
            {
                var response = await sender.Send(new DeleteAccommodationRecordRequest { staffId = staffId });

                if (response.IsFailure)
                {
                    return Results.NotFound(response.Error);
                }

                return Results.Ok(response?.Value);

            }).WithTags("Staff Accommodation Record")
                  .WithMetadata(new ProducesResponseTypeAttribute(StatusCodes.Status204NoContent))
                  .WithMetadata(new ProducesResponseTypeAttribute(typeof(Error), StatusCodes.Status400BadRequest))
                  .WithGroupName(SwaggerEndpointDefintions.Planning)
              ;
        }
    }
}
