using Carter;
using HRM_SK.Database;
using HRM_SK.Extensions;
using HRM_SK.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static HRM_SK.Features.Staff_Bank.RemoveStaffBank;

namespace HRM_SK.Features.Staff_Bank
{
    public class RemoveStaffBank
    {
        public class RemoveStaffBankRequest : IRequest<Result<string>>
        {
            public Guid staffId { get; set; }
        }

        internal sealed class Handler(DatabaseContext dbContext) : IRequestHandler<RemoveStaffBankRequest, Result<string>>
        {
            public async Task<Result<string>> Handle(RemoveStaffBankRequest request, CancellationToken cancellationToken)
            {
                var affectedRows = await dbContext.StaffBankDetail.Where(e => e.staffId == request.staffId).ExecuteDeleteAsync(cancellationToken);

                if (affectedRows == 0)
                {
                    return Shared.Result.Failure<string>(Error.CreateNotFoundError("Staff Bank Record Was Not Found"));
                }

                return Shared.Result.Success("Staff Bank Record Deleted");

            }
        }
    }

    public class DeleteStaffBankndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapDelete("api/staff-bank/{staffId}", async (ISender sender, Guid staffId) =>
            {
                var response = await sender.Send(new RemoveStaffBankRequest { staffId = staffId });

                if (response.IsFailure)
                {
                    return Results.NotFound(response.Error);
                }

                return Results.Ok(response?.Value);

            }).WithTags("Staff Bank Record")
                  .WithMetadata(new ProducesResponseTypeAttribute(StatusCodes.Status204NoContent))
                  .WithMetadata(new ProducesResponseTypeAttribute(typeof(Error), StatusCodes.Status400BadRequest))
                  .WithGroupName(SwaggerEndpointDefintions.Planning)
              ;
        }
    }
}
