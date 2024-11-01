using Carter;
using HRM_SK.Database;
using HRM_SK.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static HRM_SK.Features.Staff_Professional_License.DeleteStaffProfessionalLicenseRecord;

namespace HRM_SK.Features.Staff_Professional_License
{
    public class DeleteStaffProfessionalLicenseRecord
    {
        public class DeletePLRecordRequest : IRequest<Result<string>>
        {
            public Guid staffId { get; set; }
        }

        internal sealed class Handler(DatabaseContext dbContext) : IRequestHandler<DeletePLRecordRequest, Result<string>>
        {
            public async Task<Result<string>> Handle(DeletePLRecordRequest request, CancellationToken cancellationToken)
            {
                var affectedRows = await dbContext.StaffProfessionalLincense
                    .Where(e => e.staffId == request.staffId)
                    .ExecuteDeleteAsync(cancellationToken);

                if (affectedRows == 0)
                {
                    return Shared.Result.Failure<string>(Error.CreateNotFoundError("Staff Professional License  Record Was Not Found"));
                }

                return Shared.Result.Success("Staff Professional License Record Deleted");

            }
        }
    }


}

public class mapDeleteAccodationRecordEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("api/staff-professional-license/{staffId}", async (ISender sender, Guid staffId) =>
        {
            var response = await sender.Send(new DeletePLRecordRequest { staffId = staffId });

            if (response.IsFailure)
            {
                return Results.NotFound(response.Error);
            }

            return Results.Ok(response?.Value);

        }).WithTags("Staff Professional License Record")
              .WithMetadata(new ProducesResponseTypeAttribute(StatusCodes.Status204NoContent))
              .WithMetadata(new ProducesResponseTypeAttribute(typeof(Error), StatusCodes.Status400BadRequest))
          ;
    }
}
