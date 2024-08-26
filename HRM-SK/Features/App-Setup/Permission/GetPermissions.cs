﻿using Carter;
using HRM_BACKEND_VSA.Features.Permission;
using HRM_SK.Database;
using HRM_SK.Entities;
using HRM_SK.Shared;
using HRM_SK.Utilities;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using static HRM_SK.Contracts.UrlNavigation;

namespace HRM_BACKEND_VSA.Features.Permission
{
    public static class GetPermissions
    {
        public class getPermissionsRequest : IFilterableSortableRoutePageParam, IRequest<HRM_SK.Shared.Result<object>>
        {
            public string? search { get; set; }
            public string? sort { get; set; }
            public int? pageSize { get; set; }
            public int? pageNumber { get; set; }
        }

        internal sealed class Handler : IRequestHandler<getPermissionsRequest, HRM_SK.Shared.Result<object>>
        {

            private readonly DatabaseContext _dbContext;
            public Handler(DatabaseContext dbContext)
            {
                _dbContext = dbContext;
            }

            public async Task<HRM_SK.Shared.Result<object>> Handle(getPermissionsRequest request, CancellationToken cancellationToken)
            {
                var permissionQuery = _dbContext.Permission.AsQueryable();

                var queryBuilder = new QueryBuilder<HRM_SK.Entities.Permission>(permissionQuery)
                    .WithSearch(request?.search, "name")
                    .WithSort(request?.sort)
                    .Paginate(request?.pageNumber, request?.pageSize)
                    ;

                var result = await queryBuilder.BuildAsync();

                return HRM_SK.Shared.Result.Success(result);
            }
        }
    }
}

public class getPermissionsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {

        app.MapGet("api/permission/all",
        async (ISender sender, [FromQuery] int? pageNumber, [FromQuery] int? pageSize, [FromQuery] string? search, [FromQuery] string? sort) =>
        {

            var response = await sender.Send(new GetPermissions.getPermissionsRequest
            {
                pageSize = pageSize,
                pageNumber = pageNumber,
                search = search,
                sort = sort
            });

            if (response is null)
            {
                return Results.BadRequest("Empty Result");
            }

            if (response.IsSuccess)
            {
                return Results.Ok(response.Value);
            }
            return Results.BadRequest("Empty Result");
        })
            .WithMetadata(new ProducesResponseTypeAttribute(typeof(Error), StatusCodes.Status400BadRequest))
            .WithMetadata(new ProducesResponseTypeAttribute(typeof(Paginator.PaginatedData<Permission>), StatusCodes.Status200OK))
            .WithTags("Setup-Permission")
            ;
    }
}
