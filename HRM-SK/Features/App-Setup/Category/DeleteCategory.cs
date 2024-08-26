using Carter;
using HRM_SK.Database;
using HRM_SK.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static App_Setup.Category.DeleteCategory;

namespace App_Setup.Category
{
    public static class DeleteCategory
    {

        public class DeleteCategoryRequest : IRequest<HRM_SK.Shared.Result>
        {
            public Guid Id { get; set; }
        }

        public class Handler : IRequestHandler<DeleteCategoryRequest, HRM_SK.Shared.Result>
        {
            private readonly DatabaseContext _dbContext;
            public Handler(DatabaseContext dbContext)
            {

                _dbContext = dbContext;

            }

            public async Task<HRM_SK.Shared.Result> Handle(DeleteCategoryRequest request, CancellationToken cancellationToken)
            {
                var affectedRows = await _dbContext
                    .Category
                    .Where(s => s.Id == request.Id)
                    .ExecuteDeleteAsync();

                if (affectedRows == 0) return HRM_SK.Shared.Result.Failure(Error.NotFound);
                return HRM_SK.Shared.Result.Success();
            }
        }
    }
}


public class MapDeleteCategoryEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("api/category/{Id}", async (ISender sender, Guid Id) =>
        {
            var response = await sender.Send(new DeleteCategoryRequest { Id = Id });

            if (response.IsFailure)
            {
                return Results.NotFound(response.Error);
            }

            return Results.NoContent();

        }).WithTags("Setup-Category")
              .WithMetadata(new ProducesResponseTypeAttribute(StatusCodes.Status204NoContent))
              .WithMetadata(new ProducesResponseTypeAttribute(typeof(Error), StatusCodes.Status400BadRequest))
          ;
    }
}