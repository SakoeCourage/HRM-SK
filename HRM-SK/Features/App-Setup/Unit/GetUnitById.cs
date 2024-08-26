using Carter;
using HRM_SK.Database;
using HRM_SK.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static App_Setup.Unit.GetUnitById;

namespace App_Setup.Unit
{
    public static class GetUnitById
    {
        public class GetUnitByIdRequest : IRequest<HRM_SK.Shared.Result<HRM_SK.Entities.Unit>>
        {
            public Guid Id { get; set; }
        }

        internal sealed class Handler : IRequestHandler<GetUnitByIdRequest, HRM_SK.Shared.Result<HRM_SK.Entities.Unit>>
        {
            private readonly DatabaseContext _dbContext;
            public Handler(DatabaseContext dbContext)
            {
                _dbContext = dbContext;
            }
            public async Task<HRM_SK.Shared.Result<HRM_SK.Entities.Unit>> Handle(GetUnitByIdRequest request, CancellationToken cancellationToken)
            {
                var role = await _dbContext.Unit.Include(x => x.department)
                    .Include(x => x.directorate)
                    .Include(x => x.unitHead)
                    .FirstOrDefaultAsync(x => x.Id == request.Id);

                if (role is null)
                {
                    return HRM_SK.Shared.Result.Failure<HRM_SK.Entities.Unit>(Error.NotFound);
                }
                return HRM_SK.Shared.Result.Success(role);
            }
        }

    }
}

public class GetUnitByIdEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {

        app.MapGet("api/unit/{Id}",
        async (ISender sender, Guid id) =>
        {

            var response = await sender.Send(new GetUnitByIdRequest
            {
                Id = id
            });

            if (response.IsFailure)
            {
                return Results.BadRequest(response?.Error);
            }

            if (response.IsSuccess)
            {
                return Results.Ok(response.Value);
            }

            return Results.BadRequest(response?.Error);
        })
            .WithMetadata(new ProducesResponseTypeAttribute(typeof(Error), StatusCodes.Status400BadRequest))
            .WithMetadata(new ProducesResponseTypeAttribute(typeof(HRM_SK.Entities.Unit), StatusCodes.Status200OK))
            .WithTags("Setup-Unit")
            ;
    }
}
