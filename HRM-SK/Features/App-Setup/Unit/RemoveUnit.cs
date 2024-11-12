using Carter;
using HRM_SK.Database;
using HRM_SK.Extensions;
using HRM_SK.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static App_Setup.Unit.RemoveUnit;

namespace App_Setup.Unit
{
    public static class RemoveUnit
    {

        public class DeleteUnitRequest : IRequest<HRM_SK.Shared.Result>
        {
            public Guid Id { get; set; }
        }

        internal sealed class Handler : IRequestHandler<DeleteUnitRequest, HRM_SK.Shared.Result>
        {
            private readonly DatabaseContext _dbContext;
            public Handler(DatabaseContext dbContext)
            {

                _dbContext = dbContext;

            }

            public async Task<HRM_SK.Shared.Result> Handle(DeleteUnitRequest request, CancellationToken cancellationToken)
            {
                var affectedRows = await _dbContext
                    .Unit
                    .Where(s => s.Id == request.Id)
                    .ExecuteDeleteAsync();

                if (affectedRows == 0) return HRM_SK.Shared.Result.Failure(Error.CreateNotFoundError("Unit Not Found"));
                return HRM_SK.Shared.Result.Success();
            }
        }
    }
}
public class MapDeleteUnitEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("api/unit/{Id}", async (ISender sender, Guid Id) =>
        {
            var response = await sender.Send(new DeleteUnitRequest { Id = Id });

            if (response.IsFailure)
            {
                return Results.NotFound(response.Error);
            }

            return Results.NoContent();

        }).WithTags("Setup-Unit")
              .WithMetadata(new ProducesResponseTypeAttribute(StatusCodes.Status204NoContent))
              .WithMetadata(new ProducesResponseTypeAttribute(typeof(Error), StatusCodes.Status400BadRequest))
              .WithGroupName(SwaggerEndpointDefintions.Setup)
          ;
    }
}
