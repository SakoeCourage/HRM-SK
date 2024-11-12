using Carter;
using HRM_SK.Database;
using HRM_SK.Extensions;
using HRM_SK.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static HRM_BACKEND_VSA.Domains.HR_Management.User.GetUser;

namespace HRM_BACKEND_VSA.Domains.HR_Management.User
{
    public static class GetUser
    {
        public class GetUserRequest : IRequest<HRM_SK.Shared.Result<HRM_SK.Entities.User>>
        {
            public Guid id { get; set; }
        }

        public class Handler : IRequestHandler<GetUserRequest, HRM_SK.Shared.Result<HRM_SK.Entities.User>>
        {
            private readonly DatabaseContext _dbContext;
            public Handler(DatabaseContext dbContext)
            {
                _dbContext = dbContext;
            }
            public async Task<HRM_SK.Shared.Result<HRM_SK.Entities.User>> Handle(GetUserRequest request, CancellationToken cancellationToken)
            {
                var user = await _dbContext.User.FirstOrDefaultAsync(u => u.Id == request.id);
                if (user == null) { return HRM_SK.Shared.Result.Failure<HRM_SK.Entities.User>(Error.CreateNotFoundError("User Not Found")); }

                return HRM_SK.Shared.Result.Success<HRM_SK.Entities.User>(user);
            }
        }
    }
}

public class MapGetUserEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("api/user/{id}", async (ISender sender, Guid id) =>
        {
            var response = await sender.Send(new GetUserRequest
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
        }).WithTags("User Management")
        .WithMetadata(new ProducesResponseTypeAttribute(typeof(HRM_SK.Entities.User), StatusCodes.Status200OK))
        .WithGroupName(SwaggerEndpointDefintions.UserManagement)
        ;
        
    }
}
