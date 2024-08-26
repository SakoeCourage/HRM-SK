using Carter;
using HRM_SK.Database;
using HRM_SK.Shared;
using HRM_SK.Utilities;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static App_Setup.Unit.GetUnitList;
using static HRM_SK.Contracts.UrlNavigation;

namespace App_Setup.Unit
{
    public class GetUnitList
    {
        public class GetUnitListRequest : IFilterableSortableRoutePageParam, IRequest<Result<object>>
        {
            public string? search { get; set; }
            public string? sort { get; set; }
            public int? pageSize { get; set; }
            public int? pageNumber { get; set; }
        }

        public class Handler : IRequestHandler<GetUnitListRequest, Result<object>>
        {
            private readonly DatabaseContext _dBContext;
            public Handler(DatabaseContext dbContext)
            {
                _dBContext = dbContext;
            }
            public async Task<Result<object>> Handle(GetUnitListRequest request, CancellationToken cancellationToken)
            {
                var query = _dBContext.Unit
                    .Include(un => un.unitHead)
                    .Include(un => un.directorate)
                    .Include(un => un.department)
                    .AsQueryable();

                var queryBuilder = new QueryBuilder<HRM_SK.Entities.Unit>(query)
                        .WithSearch(request?.search, "unitname")
                        .WithSort(request?.sort)
                        .Paginate(request?.pageNumber, request?.pageSize);

                var response = await queryBuilder.BuildAsync();

                return HRM_SK.Shared.Result.Success(response);
            }
        }
    }
}

public class MapGetUnitListEnpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("api/unit/all", async (ISender sender, [FromQuery] int? pageNumber, [FromQuery] int? pageSize, [FromQuery] string? search, [FromQuery] string? sort) =>
        {

            var response = await sender.Send(new GetUnitListRequest
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
          .WithMetadata(new ProducesResponseTypeAttribute(typeof(Paginator.PaginatedData<HRM_SK.Entities.Unit>), StatusCodes.Status200OK))
          .WithTags("Setup-Unit");
    }
}
