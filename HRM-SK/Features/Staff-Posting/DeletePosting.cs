using Carter;
using HRM_SK.Database;
using HRM_SK.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;
using static HRM_SK.Features.Staff_Posting.DeletePosting;

namespace HRM_SK.Features.Staff_Posting
{
    public class DeletePosting
    {
        public class DeletePostingRequest : IRequest<Shared.Result<string>>
        {
            public Guid staffId { get; set; }
        }

        internal sealed class Handler(DatabaseContext _dbContext) : IRequestHandler<DeletePostingRequest, Result<string>>
        {
            public async Task<Result<string>> Handle(DeletePostingRequest request, CancellationToken cancellationToken)
            {
                var affectedRows = await _dbContext
                    .StaffPosting
                    .Where(stap => stap.staffId == request.staffId)
                    .ExecuteDeleteAsync(cancellationToken);

                if (affectedRows == 0)
                {
                    return Shared.Result.Failure<string>(Error.CreateNotFoundError("Posting Data Was Not Found"));
                }

                return Shared.Result.Success<string>("Saff Posting Data Has Been Deleted");
            }
        }
    }
}


public class MapDeletePostingEndpoin : ICarterModule
{

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("api/staff-posting/{staffId}", async (ISender sender, Guid staffId) =>
        {
            var result = await sender.Send(new DeletePostingRequest
            {
                staffId = staffId
            });

            if (result.IsSuccess)
            {
                return Results.Ok(result.Value);
            }

            if (result.IsFailure)
            {
                return Results.BadRequest(result?.Error);
            }

            return Results.BadRequest();
        }).WithTags("Staff-Posting-Transfer");
    }
}
