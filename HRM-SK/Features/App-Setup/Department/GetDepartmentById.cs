using Carter;
using HRM_SK.Database;
using HRM_SK.Extensions;
using HRM_SK.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static App_Setup.Department.GetDepartmentById;

namespace App_Setup.Department
{
    public static class GetDepartmentById
    {
        public class GetDepartmentByIdRequest : IRequest<HRM_SK.Shared.Result<HRM_SK.Entities.Department>>
        {
            public Guid Id { get; set; }
        }

        internal sealed class Handler : IRequestHandler<GetDepartmentByIdRequest, HRM_SK.Shared.Result<HRM_SK.Entities.Department>>
        {
            private readonly DatabaseContext _dbContext;
            public Handler(DatabaseContext dbContext)
            {
                _dbContext = dbContext;
            }
            public async Task<HRM_SK.Shared.Result<HRM_SK.Entities.Department>> Handle(GetDepartmentByIdRequest request, CancellationToken cancellationToken)
            {
                var response = await _dbContext.Department
                    .Include(x => x.headOfDepartment)
                    .Include(x => x.depHeadOfDepartment)
                    .FirstOrDefaultAsync(x => x.Id == request.Id);
                if (response is null)
                {
                    return HRM_SK.Shared.Result.Failure<HRM_SK.Entities.Department>(Error.NotFound);
                }
                return HRM_SK.Shared.Result.Success(response);
            }
        }
    }
}

public class GetDepartmentByIdEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {

        app.MapGet("api/department/{Id}",
        async (ISender sender, Guid id) =>
        {

            var response = await sender.Send(new GetDepartmentByIdRequest
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
            .WithMetadata(new ProducesResponseTypeAttribute(typeof(HRM_SK.Entities.Department), StatusCodes.Status200OK))
            .WithTags("Setup-Department")
            .WithGroupName(SwaggerEndpointDefintions.Setup)
            ;
    }
}

