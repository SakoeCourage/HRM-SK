using Carter;
using HRM_SK.Database;
using HRM_SK.Extensions;
using HRM_SK.Shared;
using HRM_SK.Utilities;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using static App_Setup.Bank.GetBankList;
using static HRM_SK.Contracts.UrlNavigation;

namespace App_Setup.Bank
{
    public static class GetBankList
    {
        public class GetGetBankListRequest : IFilterableSortableRoutePageParam, IRequest<Result<object>>
        {
            public string? search { get; set; }
            public string? sort { get; set; }
            public int? pageSize { get; set; }
            public int? pageNumber { get; set; }
        }

        public class Handler : IRequestHandler<GetGetBankListRequest, Result<object>>
        {
            private readonly DatabaseContext _dBContext;
            public Handler(DatabaseContext dbContext)
            {
                _dBContext = dbContext;
            }
            public async Task<Result<object>> Handle(GetGetBankListRequest request, CancellationToken cancellationToken)
            {
                var query = _dBContext.Bank.AsQueryable();

                var queryBuilder = new QueryBuilder<HRM_SK.Entities.Bank>(query)
                        .WithSearch(request?.search, "bankName")
                        .WithSort(request?.sort)
                        .Paginate(request?.pageNumber, request?.pageSize);

                var response = await queryBuilder.BuildAsync();

                return HRM_SK.Shared.Result.Success(response);
            }
        }

    }
}

public class GetBankListEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("api/bank/all", async (ISender sender, [FromQuery] int? pageNumber, [FromQuery] int? pageSize, [FromQuery] string? search, [FromQuery] string? sort) =>
        {

            var response = await sender.Send(new GetGetBankListRequest
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
          .WithMetadata(new ProducesResponseTypeAttribute(typeof(Paginator.PaginatedData<HRM_SK.Entities.Bank>), StatusCodes.Status200OK))
          .WithTags("Setup-Bank")
          .WithGroupName(SwaggerEndpointDefintions.Setup)
          ;
    }

}
