using Carter;
using HRM_SK.Database;
using HRM_SK.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;
using static HRM_BACKEND_VSA.Domains.HR_Management.User.DeleteUser;

namespace HRM_BACKEND_VSA.Domains.HR_Management.User
{
    public static class DeleteUser
    {
        public class DeleteUserRequest : IRequest<HRM_SK.Shared.Result>
        {
            public Guid id { get; set; }
        }


        public class Handler : IRequestHandler<DeleteUserRequest, HRM_SK.Shared.Result>
        {
            private readonly DatabaseContext _dbContext;

            public Handler(DatabaseContext dbContext)
            {
                _dbContext = dbContext;
            }
            public async Task<HRM_SK.Shared.Result> Handle(DeleteUserRequest request, CancellationToken cancellationToken)
            {
                var affectedRows = await _dbContext.User.Where(u => u.Id == request.id).ExecuteDeleteAsync(cancellationToken);

                if (affectedRows == 0) return HRM_SK.Shared.Result.Failure(Error.CreateNotFoundError("Requested User Not Found"));

                return HRM_SK.Shared.Result.Success();
            }
        }
    }
}

public class MapDeleteUserEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("api/user/{id}", async (ISender sender, Guid id) =>
        {
            var response = await sender.Send(new DeleteUserRequest
            {
                id = id
            });

            if (response.IsSuccess)
            {
                return Results.NoContent();
            }
            if (response.IsFailure)
            {
                return Results.UnprocessableEntity(response.Error);
            }

            return Results.BadRequest();
        }).WithTags("User Management");
    }
}
