﻿using Carter;
using FluentValidation;
using HRM_BACKEND_VSA.Features.Permission;
using HRM_SK.Database;
using HRM_SK.Extensions;
using HRM_SK.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HRM_BACKEND_VSA.Features.Permission
{
    public class CreatePermission
    {
        public class CreatePermissionDTO : IRequest<HRM_SK.Shared.Result<Guid>>
        {
            public String name { get; set; }
        }

        public class Validator : AbstractValidator<CreatePermissionDTO>
        {
            protected readonly IServiceScopeFactory _serviceScopeFactory;
            public Validator(IServiceScopeFactory scopeServiceFactory)
            {
                _serviceScopeFactory = scopeServiceFactory;

                RuleFor(x => x.name)
                    .NotEmpty()
                    .MustAsync(async (name, cancellationToken) =>
                    {
                        using (var scope = _serviceScopeFactory.CreateScope())
                        {
                            var dbContext = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
                            bool exist = await dbContext
                            .Permission
                            .AnyAsync(e => e.name.ToLower() == name.Trim().ToLower());
                            return !exist;
                        }

                    })
                    .WithMessage("Permission Already Exist")
                    ;
            }
        }

        internal sealed class HandleRequest : IRequestHandler<CreatePermissionDTO, HRM_SK.Shared.Result<Guid>>
        {
            protected readonly DatabaseContext _dbContext;
            private readonly IValidator<CreatePermissionDTO> _validator;
            public HandleRequest(DatabaseContext dbContext, IValidator<CreatePermissionDTO> validator)
            {
                _dbContext = dbContext;
                _validator = validator;
            }

            public async Task<HRM_SK.Shared.Result<Guid>> Handle(CreatePermissionDTO request, CancellationToken cancellationToken)
            {

                if (request == null) return HRM_SK.Shared.Result.Failure<Guid>(new Error(code: "Invalid Request", message: "Invalid Request body"));

                var validationResult = await _validator.ValidateAsync(request);

                if (!validationResult.IsValid)
                {
                    return HRM_SK.Shared.Result.Failure<Guid>(new Error(StatusCodes.Status422UnprocessableEntity.ToString(), validationResult));
                }


                var newPermission = new HRM_SK.Entities.Permission
                {
                    name = request.name,
                    createdAt = DateTime.UtcNow,
                    updatedAt = DateTime.UtcNow
                };

                _dbContext.Add(newPermission);

                await _dbContext.SaveChangesAsync(cancellationToken);

                return newPermission.Id;
            }
        }
    }
}

public class CreatePermissionEndpint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("api/permission",
            async (CreatePermission.CreatePermissionDTO request, ISender sender) =>
            {

                var response = await sender.Send(request);
                if (response.IsFailure)
                {
                    return Results.UnprocessableEntity(response.Error);
                }
                return Results.Ok(response.Value);
            }
        ).WithTags("Setup-Permission")
        .WithGroupName(SwaggerEndpointDefintions.Setup)
            ;
    }
}



