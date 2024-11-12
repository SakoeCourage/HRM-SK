﻿using Carter;
using FluentValidation;
using HRM_BACKEND_VSA.Features.Role;
using HRM_SK.Database;
using HRM_SK.Entities;
using HRM_SK.Extensions;
using HRM_SK.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HRM_BACKEND_VSA.Features.Role
{
    public static class SyncRoleToPermissions
    {

        public class RequestData : IRequest<HRM_SK.Shared.Result>
        {
            public List<string> PermissionNames { get; set; }
        }

        public class DataToProcess : IRequest<HRM_SK.Shared.Result>
        {

            public Guid RoleId { get; set; }
            public List<string> PermissionNames { get; set; }

        }
        public class Validator : AbstractValidator<DataToProcess>
        {
            public Validator()
            {
                RuleFor(x => x.RoleId)
                .NotEmpty();

                RuleFor(c => c.PermissionNames)
                .NotNull().WithMessage("Permissions list cannot be null.")
                .Must(permissions => permissions != null && permissions.Count > 0)
                .WithMessage("At least one permission ID must be provided.");
            }

        }


        internal sealed class Handler : IRequestHandler<DataToProcess, HRM_SK.Shared.Result>
        {
            private readonly DatabaseContext _dbContext;
            private readonly IValidator<DataToProcess> _validator;
            public Handler(DatabaseContext dbContext, IValidator<DataToProcess> validator)
            {
                _dbContext = dbContext;
                _validator = validator;

            }
            public async Task UpdateRolePermissions(Guid roleId, List<string> permissionNames)
            {
                var existingPermissions = await _dbContext.RoleHasPermissions
                                                        .Include(rp => rp.permission)
                                                        .Where(rp => rp.roleId == roleId)
                                                        .ToListAsync();

                var existingPermissionNames = existingPermissions.Select(rp => rp.permission.name).ToList();

                var permissionIds = await _dbContext.Permission
                                                .Where(p => permissionNames.Contains(p.name))
                                                .Select(p => p.Id)
                                                .ToListAsync();

                var newPermissionIds = permissionIds.Except(existingPermissions.Select(rp => rp.permissionId));

                foreach (var newPermissionId in newPermissionIds)
                {
                    _dbContext.RoleHasPermissions.Add(new RoleHasPermissions
                    {
                        roleId = roleId,
                        permissionId = newPermissionId
                    });
                }

                var removedPermissions = existingPermissions.Where(rp => !permissionNames.Contains(rp.permission.name)).ToList();

                _dbContext.RoleHasPermissions.RemoveRange(removedPermissions);

                await _dbContext.SaveChangesAsync();
            }



            public async Task<HRM_SK.Shared.Result> Handle(DataToProcess request, CancellationToken cancellation)
            {

                var validationResponse = _validator.Validate(request);

                if (!validationResponse.IsValid)
                {
                    return HRM_SK.Shared.Result.Failure(new Error(StatusCodes.Status422UnprocessableEntity.ToString(), validationResponse.Errors));
                }

                try
                {
                    await UpdateRolePermissions(request.RoleId, request.PermissionNames);
                }
                catch (Exception ex)
                {
                    return HRM_SK.Shared.Result.Failure(new Error(StatusCodes.Status422UnprocessableEntity.ToString(), ex.Message));
                }

                return HRM_SK.Shared.Result.Success();
            }

        }
    }
}

public class SyncRolePermissionEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("api/role/sync-permissions/{roleId}", async (Guid roleId, SyncRoleToPermissions.RequestData request, ISender sender) =>
        {

            var response = await sender.Send(
                new SyncRoleToPermissions.DataToProcess
                {
                    RoleId = roleId,
                    PermissionNames = request.PermissionNames
                }
                );

            if (response.IsFailure)
            {
                return Results.UnprocessableEntity(response.Error);
            }

            return Results.NoContent();
        }).WithTags("Setup-Role")
        .WithGroupName(SwaggerEndpointDefintions.Setup)
            ;
    }
}



