using Carter;
using HRM_SK.Database;
using HRM_SK.Extensions;
using HRM_SK.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static App_Setup.Directorate.GetDirectorateById;

namespace App_Setup.Directorate
{
    public static class GetDirectorateById
    {
        public class GetDirectorateByIdRequest : IRequest<HRM_SK.Shared.Result<HRM_SK.Entities.Directorate>>
        {
            public Guid Id { get; set; }
        }

        internal sealed class Handler : IRequestHandler<GetDirectorateByIdRequest, HRM_SK.Shared.Result<HRM_SK.Entities.Directorate>>
        {
            private readonly DatabaseContext _dbContext;
            public Handler(DatabaseContext dbContext)
            {
                _dbContext = dbContext;
            }
            public async Task<HRM_SK.Shared.Result<HRM_SK.Entities.Directorate>> Handle(GetDirectorateByIdRequest request, CancellationToken cancellationToken)
            {
                var response = await _dbContext.Directorate
                    .Include(x => x.director)
                    .Include(x => x.depDirector)
                    .FirstOrDefaultAsync(x => x.Id == request.Id);

                if (response is null)
                {
                    return HRM_SK.Shared.Result.Failure<HRM_SK.Entities.Directorate>(Error.NotFound);
                }
                return HRM_SK.Shared.Result.Success(response);
            }
        }
    }
}

public class GetDirectorateByIdEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {

        app.MapGet("api/directorate/{Id}",
        async (ISender sender, Guid id) =>
        {

            var response = await sender.Send(new GetDirectorateByIdRequest
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
            .WithMetadata(new ProducesResponseTypeAttribute(typeof(HRM_SK.Entities.Directorate), StatusCodes.Status200OK))
            .WithGroupName(SwaggerEndpointDefintions.Setup)
            .WithTags("Setup-Directorate")
            ;
    }
}

