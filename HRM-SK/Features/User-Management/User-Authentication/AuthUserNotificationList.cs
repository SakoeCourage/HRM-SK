using Carter;
using HRM_SK;
using HRM_SK.Database;
using HRM_SK.Providers;
using HRM_SK.Shared;
using HRM_SK.Utilities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static HRM_BACKEND_VSA.Domains.HR_Management.User.User_Authentication.AuthUserNotificationList;
using static HRM_SK.Contracts.UrlNavigation;

namespace HRM_BACKEND_VSA.Domains.HR_Management.User.User_Authentication
{
    public static class AuthUserNotificationList
    {

        public class AuthUserNotificationRequest : IFilterableSortableRoutePageParam, IRequest<Result<object>>
        {
            public string? sort { get; set; }
            public int? pageSize { get; set; }
            public int? pageNumber { get; set; }
            public string? search { get; set; }
        }

        internal sealed class Handler
            (
              DatabaseContext _dbContext,
              Authprovider _authProvider
            )
            : IRequestHandler<AuthUserNotificationRequest, Result<object>>
        {
            public async Task<Result<object>> Handle(AuthUserNotificationRequest request, CancellationToken cancellationToken)
            {
                var authUser = await _authProvider.GetAuthUser();

                if (authUser is null)
                {
                    return HRM_SK.Shared.Result.Failure<ICollection<Notification>>(Error.CreateNotFoundError("auth user not found"));
                }

                var notificationQuery = _dbContext
                    .Notification
                    .Where(entry => entry.notifiableType == typeof(HRM_SK.Entities.User).Name && entry.notifiableId == authUser.Id)
                    .AsQueryable();

                var queryBuilder = new QueryBuilder<Notification>(notificationQuery)
                    .WithSort(request?.sort)
                    .Paginate(request?.pageNumber, request?.pageSize);

                var response = await queryBuilder.BuildAsync();

                return HRM_SK.Shared.Result.Success<object>(response);

            }
        }
    }
}

public class MapGetAuthUserNotificationList : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("api/auth/user/notification",
            [Authorize()]
        async (ISender sender, [FromQuery] int? pageNumber, [FromQuery] int? pageSize, [FromQuery] string? sort) =>
            {
                var response = await sender.Send(
                    new AuthUserNotificationRequest
                    {
                        pageNumber = pageNumber,
                        pageSize = pageSize,
                        sort = sort
                    }
                    );
                if (response.IsSuccess)
                {
                    return Results.Ok(response.Value);
                }
                if (response.IsFailure)
                {
                    return Results.UnprocessableEntity(response.Error);
                }

                return Results.BadRequest();
            })
        .WithMetadata(new ProducesResponseTypeAttribute(typeof(List<Notification>), StatusCodes.Status200OK))
        .WithMetadata(new ProducesResponseTypeAttribute(StatusCodes.Status401Unauthorized))
        .WithMetadata(new ProducesResponseTypeAttribute(typeof(Error), StatusCodes.Status422UnprocessableEntity))
        .WithTags("User Authentication")
            ;
    }
}

