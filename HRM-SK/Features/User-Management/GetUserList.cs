using Carter;
using HRM_SK.Database;
using HRM_SK.Extensions;
using HRM_SK.Shared;
using HRM_SK.Utilities;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static HRM_BACKEND_VSA.Domains.HR_Management.User.GetUserList;
using static HRM_SK.Contracts.UrlNavigation;
using static HRM_SK.Contracts.UserContracts;

namespace HRM_BACKEND_VSA.Domains.HR_Management.User
{
    public static class GetUserList
    {
        public class GetUserListRequest : IFilterableSortableRoutePageParam, IRequest<HRM_SK.Shared.Result<object>>
        {
            public string? search { get; set; }
            public string? sort { get; set; }
            public int? pageSize { get; set; }
            public int? pageNumber { get; set; }
        }
        public class Handler : IRequestHandler<GetUserListRequest, HRM_SK.Shared.Result<object>>
        {
            private readonly DatabaseContext _dBContext;
            public Handler(DatabaseContext dbContext)
            {
                _dBContext = dbContext;
            }
            public async Task<HRM_SK.Shared.Result<object>> Handle(GetUserListRequest request, CancellationToken cancellationToken)
            {
                var query = _dBContext.User
                    .Include(u => u.staff)
                    .Include(u => u.role)
                    .AsQueryable()
                    .IgnoreAutoIncludes();

                if (!string.IsNullOrWhiteSpace(request?.search))
                {
                    string searchLower = request.search.ToLower();

                    query = query.Where(u =>
                        EF.Functions.Like(u.email.ToLower(), $"%{searchLower}%") ||
                        (u.staff != null &&
                            (EF.Functions.Like(u.staff.firstName.ToLower(), $"%{searchLower}%") ||
                             EF.Functions.Like(u.staff.lastName.ToLower(), $"%{searchLower}%"))
                        )
                    );
                }

                var queryBuilder = new QueryBuilder<HRM_SK.Entities.User>(query)
                        .WithSort(request?.sort)
                        .Paginate(request?.pageNumber, request?.pageSize);

                var response = await queryBuilder.BuildAsync((user) =>
                  new UserStaffRoleDto
                  {
                      Id = user.Id,
                      createdAt = user.createdAt,
                      updatedAt = user.updatedAt,
                      email = user.email,
                      lastSeen = user.lastSeen,
                      hasResetPassword = user.hasResetPassword,
                      isAccountActive = user.isAccountActive,
                      role = new RoleDto
                      {
                          Id = user.role.Id,
                          name = user.role.name
                      },
                      staff = user.staff != null ? new UserStaffDto
                      {
                          title = user.staff.title,
                          firstName = user.staff.firstName,
                          lastName = user.staff.lastName,
                          otherNames = user.staff?.otherNames
                      } : null

                  });

                return HRM_SK.Shared.Result.Success(response);
            }
        }
    }
}
public class MapGetClubListEnpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("api/user/all", async (ISender sender, [FromQuery] int? pageNumber, [FromQuery] int? pageSize, [FromQuery] string? search, [FromQuery] string? sort) =>
        {
            var response = await sender.Send(new GetUserListRequest
            {
                pageSize = pageSize,
                pageNumber = pageNumber,
                search = search,
                sort = sort
            });

            if (response is null)
            {
                return Results.BadRequest("Empty Result");
            }

            if (response.IsSuccess)
            {
                return Results.Ok(response.Value);
            }
            return Results.BadRequest("Empty Result");
        }).WithMetadata(new ProducesResponseTypeAttribute(typeof(Error), StatusCodes.Status400BadRequest))
          .WithMetadata(new ProducesResponseTypeAttribute(typeof(Paginator.PaginatedData<UserStaffRoleDto>), StatusCodes.Status200OK))
          .WithTags("User Management")
          .WithGroupName(SwaggerEndpointDefintions.UserManagement)
          ;
    }
}
