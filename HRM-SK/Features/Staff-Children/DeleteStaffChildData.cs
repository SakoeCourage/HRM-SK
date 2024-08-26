using Carter;
using HRM_SK.Database;
using HRM_SK.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static HRM_SK.Features.Staff_Children.DeleteStaffChildData;

namespace HRM_SK.Features.Staff_Children
{
    public class DeleteStaffChildData
    {

        public class DeleteStaffChildDataRequest : IRequest<Shared.Result<string>>
        {
            public Guid Id { get; set; }
        }

        internal sealed class Handler(DatabaseContext dbContext) : IRequestHandler<DeleteStaffChildDataRequest, Shared.Result<string>>
        {
            public async Task<Result<string>> Handle(DeleteStaffChildDataRequest request, CancellationToken cancellationToken)
            {
                var affectedRows = await dbContext.StaffChildrenDetail
                    .Where(child => child.Id == request.Id)
                    .ExecuteDeleteAsync(cancellationToken);

                if (affectedRows == 0)
                {
                    return Shared.Result.Failure<string>(Error.CreateNotFoundError("Child Record Not Found"));
                }

                return Shared.Result.Success<string>("Staff Child Record Deleted");
            }
        }
    }

    public class MapDeleteStaffChildEnpoint : ICarterModule
    {

        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapDelete("api/children/{id}", async (ISender sender, Guid id) =>
            {
                var response = await sender.Send(new DeleteStaffChildDataRequest
                {
                    Id = id,
                });


                if (response.IsFailure)
                {
                    return Results.NotFound(response.Error);
                }

                return Results.Ok(response?.Value);

            }).WithTags("Staff Children Record")
                  .WithMetadata(new ProducesResponseTypeAttribute(StatusCodes.Status204NoContent))
                  .WithMetadata(new ProducesResponseTypeAttribute(typeof(Error), StatusCodes.Status400BadRequest))
              ;
        }
    }
}
