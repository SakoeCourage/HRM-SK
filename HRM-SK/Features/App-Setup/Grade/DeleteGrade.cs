using Carter;
using HRM_SK.Database;
using HRM_SK.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static App_Setup.Grade.DeleteGrade;

namespace App_Setup.Grade
{
    public static class DeleteGrade
    {

        public class DeleteGradeRequest : IRequest<HRM_SK.Shared.Result>
        {
            public Guid Id { get; set; }
        }

        public class Handler : IRequestHandler<DeleteGradeRequest, HRM_SK.Shared.Result>
        {
            private readonly DatabaseContext _dbContext;
            public Handler(DatabaseContext dbContext)
            {

                _dbContext = dbContext;

            }

            public async Task<HRM_SK.Shared.Result> Handle(DeleteGradeRequest request, CancellationToken cancellationToken)
            {
                var affectedRows = await _dbContext
                    .Grade
                    .Where(s => s.Id == request.Id)
                    .ExecuteDeleteAsync();

                if (affectedRows == 0) return HRM_SK.Shared.Result.Failure(Error.CreateNotFoundError("Requested Grade Was Not Found"));
                return HRM_SK.Shared.Result.Success();
            }
        }

    }
}


public class MapDeleteGradeEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("api/grade/{Id}", async (ISender sender, Guid Id) =>
        {
            var response = await sender.Send(new DeleteGradeRequest { Id = Id });

            if (response.IsFailure)
            {
                return Results.NotFound(response.Error);
            }

            if (response.IsSuccess)
            {

                return Results.NoContent();
            }
            return Results.BadRequest();
        }).WithTags("Setup-Grade")
              .WithMetadata(new ProducesResponseTypeAttribute(StatusCodes.Status204NoContent))
              .WithMetadata(new ProducesResponseTypeAttribute(typeof(Error), StatusCodes.Status400BadRequest))

          ;
    }
}