using Carter;
using HRM_SK.Database;
using HRM_SK.Extensions;
using HRM_SK.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static App_Setup.Specialty.DeleteSpeciality;

namespace App_Setup.Specialty
{
    public static class DeleteSpeciality
    {
        public class DeleteSpecialityRequest : IRequest<HRM_SK.Shared.Result>
        {
            public Guid Id { get; set; }
        }

        internal sealed class Hander : IRequestHandler<DeleteSpecialityRequest, HRM_SK.Shared.Result>
        {
            protected readonly DatabaseContext _dbContext;
            public Hander(DatabaseContext dBContext)
            {
                _dbContext = dBContext;
            }
            public async Task<HRM_SK.Shared.Result> Handle(DeleteSpecialityRequest request, CancellationToken cancellationToken)
            {
                var deletedRow = await _dbContext.Speciality.Where(ent => ent.Id == request.Id)
                    .ExecuteDeleteAsync(cancellationToken);
                ;
                if (deletedRow == 0) return HRM_SK.Shared.Result.Failure(Error.NotFound);

                return HRM_SK.Shared.Result.Success();
            }
        }
    }
}

public class DeletePlayerEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("api/speciality/{Id}", async (ISender sender, Guid Id) =>
        {
            var result = await sender.Send(new DeleteSpecialityRequest { Id = Id });

            if (result.IsFailure)
            {
                return Results.BadRequest(result?.Error);
            }
            if (result.IsSuccess)
            {
                return Results.NoContent();
            }

            return Results.BadRequest();
        })
        .WithMetadata(new ProducesResponseTypeAttribute(typeof(Error), StatusCodes.Status422UnprocessableEntity))
        .WithTags("Setup-Staff-Speciality")
        .WithGroupName(SwaggerEndpointDefintions.Setup)
          ;
    }
}
