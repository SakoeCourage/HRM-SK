using Carter;
using HRM_SK.Database;
using HRM_SK.Entities;
using HRM_SK.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static App_Setup.Grade.GetGrade;

namespace App_Setup.Grade
{
    public static class GetGrade
    {
        public class getGradeRequest : IRequest<HRM_SK.Shared.Result<HRM_SK.Entities.Grade>>
        {
            public Guid id { get; set; }
        }

        internal sealed class Handler(DatabaseContext dbContext) : IRequestHandler<getGradeRequest, HRM_SK.Shared.Result<HRM_SK.Entities.Grade>>
        {
            public async Task<Result<HRM_SK.Entities.Grade>> Handle(getGradeRequest request, CancellationToken cancellationToken)
            {
                var response = await dbContext.Grade
                    .Include(g => g.steps)
                    .FirstOrDefaultAsync(x => x.Id == request.id);

                if (response == null)
                {
                    return HRM_SK.Shared.Result.Failure<HRM_SK.Entities.Grade>(Error.CreateNotFoundError("Grade Not Found"));
                }

                return HRM_SK.Shared.Result.Success(response);
            }
        }
    }
}

public class MapGetGradeEnpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("api/grade/{id}", async (ISender sender, Guid id) =>
        {
            var response = await sender.Send(
                    new getGradeRequest
                    {
                        id = id
                    }
                );

            if (response.IsFailure)
            {
                return Results.UnprocessableEntity(response.Error);
            }

            if (response.IsSuccess)
            {
                return Results.Ok(response.Value);
            };

            return Results.BadRequest();

        }).WithTags("Setup-Grade")
           .WithMetadata(new ProducesResponseTypeAttribute(typeof(Grade), StatusCodes.Status200OK))
           .WithMetadata(new ProducesResponseTypeAttribute(typeof(Error), StatusCodes.Status400BadRequest));
    }
}
