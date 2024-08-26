using Carter;
using HRM_SK.Database;
using HRM_SK.Entities;
using HRM_SK.Shared;
using HRM_SK.Utilities;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using static App_Setup.Grade.GetGradeList;
using static HRM_SK.Contracts.UrlNavigation;

namespace App_Setup.Grade
{
    public static class GetGradeList
    {
        public class GetGradeListRequest : IFilterableSortableRoutePageParam, IRequest<Result<object>>
        {
            public string? search { get; set; }
            public string? sort { get; set; }
            public int? pageSize { get; set; }
            public int? pageNumber { get; set; }
        }

        public class Handler : IRequestHandler<GetGradeListRequest, HRM_SK.Shared.Result<object>>
        {
            private readonly DatabaseContext _dBContext;
            public Handler(DatabaseContext dbContext)
            {
                _dBContext = dbContext;
            }
            public async Task<HRM_SK.Shared.Result<object>> Handle(GetGradeListRequest request, CancellationToken cancellationToken)
            {
                var query = _dBContext.Grade.AsQueryable();

                var queryBuilder = new QueryBuilder<HRM_SK.Entities.Grade>(query)
                        .WithSearch(request?.search, "gradeName")
                        .WithSort(request?.sort)
                        .Paginate(request?.pageNumber, request?.pageSize);

                var response = await queryBuilder.BuildAsync();

                return HRM_SK.Shared.Result.Success(response);
            }
        }
    }
}

public class MapGetGradeListEnpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("api/grade/all", async (ISender sender, [FromQuery] int? pageNumber, [FromQuery] int? pageSize, [FromQuery] string? search, [FromQuery] string? sort) =>
        {

            var response = await sender.Send(new GetGradeListRequest
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
          .WithMetadata(new ProducesResponseTypeAttribute(typeof(Paginator.PaginatedData<Grade>), StatusCodes.Status200OK))
          .WithTags("Setup-Grade");
    }
}
