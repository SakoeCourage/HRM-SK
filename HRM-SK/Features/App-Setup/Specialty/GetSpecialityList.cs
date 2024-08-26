using Carter;
using HRM_SK.Database;
using HRM_SK.Shared;
using HRM_SK.Utilities;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using static App_Setup.Specialty.GetSpecialityList;
using static HRM_SK.Contracts.UrlNavigation;

namespace App_Setup.Specialty
{
    public static class GetSpecialityList
    {

        public class GetSpecialityListRequest : IFilterableSortableRoutePageParam, IRequest<HRM_SK.Shared.Result<object>>
        {
            public string? search { get; set; }
            public string? sort { get; set; }
            public int? pageSize { get; set; }
            public int? pageNumber { get; set; }
        }

        public class Handler : IRequestHandler<GetSpecialityListRequest, HRM_SK.Shared.Result<object>>
        {
            private readonly DatabaseContext _dBContext;
            public Handler(DatabaseContext dbContext)
            {
                _dBContext = dbContext;
            }
            public async Task<HRM_SK.Shared.Result<object>> Handle(GetSpecialityListRequest request, CancellationToken cancellationToken)
            {
                var query = _dBContext.Speciality.AsQueryable();

                var schoolQueryBuilder = new QueryBuilder<HRM_SK.Entities.Speciality>(query)
                        .WithSearch(request?.search, "specialityName")
                        .WithSort(request?.sort)
                        .Paginate(request?.pageNumber, request?.pageSize);

                var response = await schoolQueryBuilder.BuildAsync();

                return HRM_SK.Shared.Result.Success(response);
            }
        }

    }
}

public class MapGetSpecialityListEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("api/speciality/all", async (ISender sender, [FromQuery] int? pageNumber, [FromQuery] int? pageSize, [FromQuery] string? search, [FromQuery] string? sort) =>
        {

            var response = await sender.Send(new GetSpecialityListRequest
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
          .WithMetadata(new ProducesResponseTypeAttribute(typeof(Paginator.PaginatedData<HRM_SK.Entities.Speciality>), StatusCodes.Status200OK))
          .WithTags("Setup-Staff-Speciality");
    }

}