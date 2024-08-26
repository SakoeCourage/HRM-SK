using Carter;
using HRM_SK.Database;
using HRM_SK.Entities;
using HRM_SK.Shared;
using HRM_SK.Utilities;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using static App_Setup.GradeStep.GetGradeStepList;
using static HRM_SK.Contracts.UrlNavigation;

namespace App_Setup.GradeStep
{
    public static class GetGradeStepList
    {

        public class GetGradeStePListRequest : IFilterableSortableRoutePageParam, IRequest<Result<object>>
        {
            public Guid gradeId { get; set; }
            public string? search { get; set; }
            public string? sort { get; set; }
            public int? pageSize { get; set; }
            public int? pageNumber { get; set; }
        }
    }

    public class Handler : IRequestHandler<GetGradeStePListRequest, HRM_SK.Shared.Result<object>>
    {
        private readonly DatabaseContext _dBContext;
        public Handler(DatabaseContext dbContext)
        {
            _dBContext = dbContext;
        }
        public async Task<HRM_SK.Shared.Result<object>> Handle(GetGradeStePListRequest request, CancellationToken cancellationToken)
        {
            var query = _dBContext.GradeStep.Where(x => x.gradeId == request.gradeId).AsQueryable();

            var queryBuilder = new QueryBuilder<HRM_SK.Entities.GradeStep>(query)
                    .WithSearch(request?.search, "stepIndex")
                    .WithSort(request?.sort)
                    .Paginate(request?.pageNumber, request?.pageSize);

            var response = await queryBuilder.BuildAsync();

            return HRM_SK.Shared.Result.Success(response);
        }
    }
}


public class MapGetGradeStepListEnpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("api/grade/{gradeId}/steps", async (ISender sender, Guid gradeId, [FromQuery] int? pageNumber, [FromQuery] int? pageSize, [FromQuery] string? search, [FromQuery] string? sort) =>
        {

            var response = await sender.Send(new GetGradeStePListRequest
            {
                gradeId = gradeId,
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
          .WithMetadata(new ProducesResponseTypeAttribute(typeof(Paginator.PaginatedData<GradeStep>), StatusCodes.Status200OK))
          .WithTags("Setup-Grade-Step");
    }
}
