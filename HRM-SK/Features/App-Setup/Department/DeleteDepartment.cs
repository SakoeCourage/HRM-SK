using Carter;
using HRM_SK.Database;
using HRM_SK.Extensions;
using HRM_SK.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static App_Setup.Department.DeleteDepartment;

namespace App_Setup.Department
{
    public static class DeleteDepartment
    {
        public class DeleteDepartmentRequest : IRequest<HRM_SK.Shared.Result>
        {
            public Guid Id { get; set; }
        }

        internal sealed class Handler : IRequestHandler<DeleteDepartmentRequest, HRM_SK.Shared.Result>
        {
            private readonly DatabaseContext _dbContext;
            public Handler(DatabaseContext dbContext)
            {
                _dbContext = dbContext;
            }

            public async Task<HRM_SK.Shared.Result> Handle(DeleteDepartmentRequest request, CancellationToken cancellationToken)
            {
                var affectedRows = await _dbContext
                    .Department
                    .Where(s => s.Id == request.Id)
                    .ExecuteDeleteAsync();

                if (affectedRows == 0) return HRM_SK.Shared.Result.Failure(Error.CreateNotFoundError("Department Not Found"));
                return HRM_SK.Shared.Result.Success();
            }
        }
    }
}

public class DeleteDepartmentEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("api/department/{Id}", async (ISender sender, Guid Id) =>
        {
            var response = await sender.Send(new DeleteDepartmentRequest { Id = Id });

            if (response.IsFailure)
            {
                return Results.NotFound(response.Error);
            }

            return Results.NoContent();

        }).WithTags("Setup-Department")
              .WithMetadata(new ProducesResponseTypeAttribute(StatusCodes.Status204NoContent))
              .WithMetadata(new ProducesResponseTypeAttribute(typeof(Error), StatusCodes.Status400BadRequest))
              .WithGroupName(SwaggerEndpointDefintions.Setup)
          ;
    }
}
