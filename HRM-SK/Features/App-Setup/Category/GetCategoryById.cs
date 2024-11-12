using Carter;
using HRM_SK.Database;
using HRM_SK.Extensions;
using HRM_SK.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static App_Setup.Category.GetCategoryById;

namespace App_Setup.Category
{
    public static class GetCategoryById
    {
        public class GetCategoryByIdRequest : IRequest<HRM_SK.Shared.Result<HRM_SK.Entities.Category>>
        {
            public Guid id { get; set; }
        }

        internal sealed class Hanalder(DatabaseContext dbContext) : IRequestHandler<GetCategoryByIdRequest, HRM_SK.Shared.Result<HRM_SK.Entities.Category>>
        {
            public async Task<Result<HRM_SK.Entities.Category>> Handle(GetCategoryByIdRequest request, CancellationToken cancellationToken)
            {
                var response = await dbContext
                    .Category
                    .Include(entry => entry.grades)
                    .ThenInclude(gr => gr.steps)
                    .Include(entry => entry.specialities)
                    .FirstOrDefaultAsync(entry => entry.Id == request.id, cancellationToken);

                if (response is null)
                {
                    return HRM_SK.Shared.Result.Failure<HRM_SK.Entities.Category>(Error.CreateNotFoundError("Category Not Found"));
                }

                return HRM_SK.Shared.Result.Success(response);
            }
        }
    }
}

public class MapGetCategoryByIdEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("api/category/{id}", async (ISender sender, Guid id) =>
        {
            var response = await sender.Send(new GetCategoryByIdRequest
            {
                id = id
            });

            if (response.IsSuccess)
            {
                return Results.Ok(response.Value);
            }

            if (response.IsFailure)
            {
                return Results.UnprocessableEntity(response.Error);
            }

            return Results.BadRequest();
        }).WithTags("Setup-Category").
            WithMetadata(new ProducesResponseTypeAttribute(typeof(Error), StatusCodes.Status422UnprocessableEntity))
           .WithMetadata(new ProducesResponseTypeAttribute(typeof(HRM_SK.Entities.Category), StatusCodes.Status200OK))
           .WithGroupName(SwaggerEndpointDefintions.Setup)
            ;
    }
}