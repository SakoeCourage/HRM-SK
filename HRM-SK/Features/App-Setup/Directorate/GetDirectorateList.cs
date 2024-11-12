using Carter;
using HRM_SK.Database;
using HRM_SK.Entities;
using HRM_SK.Extensions;
using HRM_SK.Shared;
using HRM_SK.Utilities;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static App_Setup.Directorate.GetDirectorateList;
using static HRM_SK.Contracts.UrlNavigation;

namespace App_Setup.Directorate
{
    public static class GetDirectorateList
    {
        public class GetDirectorateListRequest : IFilterableSortableRoutePageParam, IRequest<Result<object>>
        {
            public string? search { get; set; }
            public string? sort { get; set; }
            public int? pageSize { get; set; }
            public int? pageNumber { get; set; }
        }

        public class Handler : IRequestHandler<GetDirectorateListRequest, Result<object>>
        {
            private readonly DatabaseContext _dBContext;
            public Handler(DatabaseContext dbContext)
            {
                _dBContext = dbContext;
            }
            public async Task<Result<object>> Handle(GetDirectorateListRequest request, CancellationToken cancellationToken)
            {
                var query = _dBContext.Directorate
                    .Include(dr => dr.director)
                    .Include(dr => dr.depDirector)
                    .AsQueryable();

                var queryBuilder = new QueryBuilder<HRM_SK.Entities.Directorate>(query)
                        .WithSearch(request?.search, "directorateName")
                        .WithSort(request?.sort)
                        .Paginate(request?.pageNumber, request?.pageSize);

                var response = await queryBuilder.BuildAsync();

                return HRM_SK.Shared.Result.Success(response);
            }
        }
    }
}

public class GetGetDirectorateListEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("api/directorate/all", async (ISender sender, [FromQuery] int? pageNumber, [FromQuery] int? pageSize, [FromQuery] string? search, [FromQuery] string? sort) =>
        {

            var response = await sender.Send(new GetDirectorateListRequest
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
          .WithMetadata(new ProducesResponseTypeAttribute(typeof(Paginator.PaginatedData<Directorate>), StatusCodes.Status200OK))
          .WithGroupName(SwaggerEndpointDefintions.Setup)
          .WithTags("Setup-Directorate");
    }

}
