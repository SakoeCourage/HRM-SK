using Carter;
using HRM_SK.Database;
using HRM_SK.Shared;
using HRM_SK.Utilities;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using static App_Setup.Directorate.GetDepartmentFromDirectorateId;
using static HRM_SK.Contracts.UrlNavigation;

namespace App_Setup.Directorate
{
    public static class GetDepartmentFromDirectorateId
    {
        public class GetDepartmentFromDirectorateIdRequest : IRequest<HRM_SK.Shared.Result<object>>, IFilterableSortableRoutePageParam
        {
            public Guid Id { get; set; }
            public string? search { get; set; }
            public string? sort { get; set; }
            public int? pageSize { get; set; }
            public int? pageNumber { get; set; }

        }

        internal sealed class Handler : IRequestHandler<GetDepartmentFromDirectorateIdRequest, HRM_SK.Shared.Result<object>>
        {
            private readonly DatabaseContext _dbContext;

            public Handler(DatabaseContext dbContext)
            {
                _dbContext = dbContext;
            }
            public async Task<Result<object>> Handle(GetDepartmentFromDirectorateIdRequest request, CancellationToken cancellationToken)
            {
                var responseQuery = _dbContext.Department.Where(u => u.directorateId == request.Id).AsQueryable();

                var queryBuilder = new QueryBuilder<HRM_SK.Entities.Department>(responseQuery)
                        .WithSearch(request?.search, "departmentName")
                        .WithSort(request?.sort)
                        .Paginate(request?.pageNumber, request?.pageSize);

                var response = await queryBuilder.BuildAsync();

                return HRM_SK.Shared.Result.Success(response);
            }
        }
    }
}


public class MapGetDepartmentFromDirectorateIdEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("api/directorate/{directorateId}/department/all", async (ISender sender, Guid directorateId, [FromQuery] int? pageNumber, [FromQuery] int? pageSize, [FromQuery] string? search, [FromQuery] string? sort) =>
        {

            var response = await sender.Send(new GetDepartmentFromDirectorateIdRequest
            {
                Id = directorateId,
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
         .WithMetadata(new ProducesResponseTypeAttribute(typeof(Paginator.PaginatedData<HRM_SK.Entities.Department>), StatusCodes.Status200OK))
         .WithTags("Setup-Directorate");
    }
}