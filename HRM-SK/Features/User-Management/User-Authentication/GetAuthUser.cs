using Carter;
using HRM_SK.Extensions;
using HRM_SK.Providers;
using HRM_SK.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static HRM_BACKEND_VSA.Domains.HR_Management.User.User_Authentication.GetAuthUser;

namespace HRM_BACKEND_VSA.Domains.HR_Management.User.User_Authentication
{
    public static class GetAuthUser
    {
        public class GetAuthUserRequest : IRequest<HRM_SK.Shared.Result<HRM_SK.Entities.User>>
        {

        }

        internal sealed class Handler : IRequestHandler<GetAuthUserRequest, HRM_SK.Shared.Result<HRM_SK.Entities.User>>
        {
            private readonly Authprovider _authProvider;
            public Handler(Authprovider authProvider)
            {
                _authProvider = authProvider;
            }
            public async Task<Result<HRM_SK.Entities.User>> Handle(GetAuthUserRequest request, CancellationToken cancellationToken)
            {

                var authUser = await _authProvider.GetAuthUser();

                if (authUser is null) { return HRM_SK.Shared.Result.Failure<HRM_SK.Entities.User>(Error.CreateNotFoundError("Auth User Not Found")); }

                return HRM_SK.Shared.Result.Success(authUser);
            }
        }
    }
}

public class MapGetAuhUserEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("api/auth/user",
        [Authorize]
        async (ISender sender) =>
        {
            var response = await sender.Send(new GetAuthUserRequest { });

            if (response.IsSuccess)
            {
                return Results.Ok(response.Value);
            }

            return Results.Unauthorized();
        })
        .WithMetadata(new ProducesResponseTypeAttribute(typeof(HRM_SK.Entities.User), StatusCodes.Status200OK))
        .WithMetadata(new ProducesResponseTypeAttribute(StatusCodes.Status401Unauthorized))
        .WithMetadata(new ProducesResponseTypeAttribute(typeof(Error), StatusCodes.Status422UnprocessableEntity))
        .WithTags("User Authentication")
        .WithGroupName(SwaggerEndpointDefintions.UserManagement)
            ;
    }
}
