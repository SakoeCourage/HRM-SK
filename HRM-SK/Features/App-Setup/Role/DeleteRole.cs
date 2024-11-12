using Carter;
using HRM_BACKEND_VSA.Features.Role;
using HRM_SK.Database;
using HRM_SK.Extensions;
using HRM_SK.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HRM_BACKEND_VSA.Features.Role
{
    public static class DeleteRole
    {

        public class DeleteRoleRequest : IRequest<HRM_SK.Shared.Result>
        {
            public Guid Id { get; set; }
        }

        internal sealed class Hanlder : IRequestHandler<DeleteRoleRequest, HRM_SK.Shared.Result>
        {
            DatabaseContext _dbContext;
            public Hanlder(DatabaseContext dbContext)
            {
                _dbContext = dbContext;
            }
            public async Task<HRM_SK.Shared.Result> Handle(DeleteRoleRequest request, CancellationToken cancellationToken)
            {
                var role = await _dbContext.Role.FindAsync(request.Id, cancellationToken);

                if (role is null)
                {
                    return HRM_SK.Shared.Result.Failure(Error.NotFound);
                }

                _dbContext.Role.Remove(role);
                try
                {
                    await _dbContext.SaveChangesAsync();
                    return HRM_SK.Shared.Result.Success("Role Deleted Succesfully");
                }
                catch (DbUpdateException ex)
                {
                    return HRM_SK.Shared.Result.Failure(Error.BadRequest(ex.Message));
                }
            }
        }
    }
}

public class MapDeleteUserEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("api/role/{Id}", async (Guid Id, ISender sender) =>
        {
            var response = await sender.Send(new DeleteRole.DeleteRoleRequest
            {
                Id = Id
            });

            if (response.IsFailure)
            {
                return Results.BadRequest(response.Error);
            }
            if (response.IsSuccess)
            {
                return Results.Ok("Role Deleted Successfully");
            }
            return Results.BadRequest(response.Error);

        }).WithTags("Setup-Role")
              .WithMetadata(new ProducesResponseTypeAttribute(StatusCodes.Status200OK))
              .WithMetadata(new ProducesResponseTypeAttribute(typeof(Error), StatusCodes.Status400BadRequest))
              .WithGroupName(SwaggerEndpointDefintions.Setup)

        ;
    }
}