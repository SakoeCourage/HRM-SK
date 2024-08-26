using Carter;
using HRM_SK.Database;
using HRM_SK.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static App_Setup.ProfessionalBody.DeleteProfessionalBody;

namespace App_Setup.ProfessionalBody
{
    public static class DeleteProfessionalBody
    {
        public class DeleteProfessionalBodyRequest : IRequest<HRM_SK.Shared.Result>
        {
            public Guid Id { get; set; }
        }
        public class Handler : IRequestHandler<DeleteProfessionalBodyRequest, HRM_SK.Shared.Result>
        {
            private readonly DatabaseContext _dbContext;
            public Handler(DatabaseContext dbContext)
            {

                _dbContext = dbContext;

            }

            public async Task<HRM_SK.Shared.Result> Handle(DeleteProfessionalBodyRequest request, CancellationToken cancellationToken)
            {
                var affectedRows = await _dbContext
                    .ProfessionalBody
                    .Where(s => s.Id == request.Id)
                    .ExecuteDeleteAsync();

                if (affectedRows == 0) return HRM_SK.Shared.Result.Failure(Error.NotFound);
                return HRM_SK.Shared.Result.Success();
            }
        }
    }
}

public class MapDeleteProfessionalBodyEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("api/professional-body/{Id}", async (ISender sender, Guid Id) =>
        {
            var response = await sender.Send(new DeleteProfessionalBodyRequest { Id = Id });

            if (response.IsFailure)
            {
                return Results.NotFound(response.Error);
            }

            return Results.NoContent();

        }).WithTags("Setup-ProfessionalBody")
              .WithMetadata(new ProducesResponseTypeAttribute(StatusCodes.Status204NoContent))
              .WithMetadata(new ProducesResponseTypeAttribute(typeof(Error), StatusCodes.Status400BadRequest))

          ;
    }
}
