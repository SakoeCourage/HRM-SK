using Carter;
using HRM_BACKEND_VSA.Features.Permission;
using HRM_SK.Database;
using HRM_SK.Entities;
using HRM_SK.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace HRM_BACKEND_VSA.Features.Permission
{
    public static class GetPermission
    {
        public class GetPermissionRequest : IRequest<HRM_SK.Shared.Result<HRM_SK.Entities.Permission?>>
        {
            public Guid Id { get; set; }
        }

        internal sealed class Hanlder : IRequestHandler<GetPermissionRequest, HRM_SK.Shared.Result<HRM_SK.Entities.Permission?>>
        {
            private readonly DatabaseContext _dbContext;
            public Hanlder(DatabaseContext dBContext)
            {
                _dbContext = dBContext;
            }
            public async Task<HRM_SK.Shared.Result<HRM_SK.Entities.Permission?>> Handle(GetPermissionRequest request, CancellationToken cancellationToken)
            {
                var permission = await _dbContext.Permission.FindAsync(request.Id);

                if (permission is null) return HRM_SK.Shared.Result.Failure<HRM_SK.Entities.Permission?>(Error.NotFound);

                return HRM_SK.Shared.Result.Success<HRM_SK.Entities.Permission?>(permission);

            }
        }
    }
}

public class CreateGetPermissionEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("api/permission/{Id}", async (ISender sender, Guid Id) =>
        {
            var result = await sender.Send(new GetPermission.GetPermissionRequest
            {
                Id = Id
            });
            if (result is null)
            {
                return Results.BadRequest(result?.Error);
            }
            if (result.IsSuccess)
            {
                return Results.Ok(result.Value);
            }

            return Results.BadRequest(result?.Error);

        }).WithMetadata(new ProducesResponseTypeAttribute(typeof(Error), StatusCodes.Status400BadRequest))
            .WithMetadata(new ProducesResponseTypeAttribute(typeof(Permission), StatusCodes.Status200OK))
            .WithTags("Setup-Permission")
            ;
    }
}
