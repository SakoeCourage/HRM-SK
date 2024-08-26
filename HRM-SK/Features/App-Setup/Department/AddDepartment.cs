﻿using Carter;
using FluentValidation;
using HRM_SK.Database;
using HRM_SK.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static App_Setup.Department.AddDepartment;

namespace App_Setup.Department
{
    public static class AddDepartment
    {
        public class AddDepartmentRequest : IRequest<HRM_SK.Shared.Result<Guid>>
        {
            public Guid directorateId { get; set; }
            public Guid? headOfDepartmentId { get; set; }
            public Guid? depHeadOfDepartmentId { get; set; }
            public string departmentName { get; set; }
        }

        public class Validator : AbstractValidator<AddDepartmentRequest>
        {
            private readonly IServiceScopeFactory _scopeFactory;
            public Validator(IServiceScopeFactory scopefactory)
            {
                _scopeFactory = scopefactory;
                RuleFor(c => c.departmentName)
                    .NotEmpty()
                    .MustAsync(async (name, cancellationToken) =>
                    {
                        using (var scope = _scopeFactory.CreateScope())
                        {
                            var dbContext = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
                            var exist = await dbContext.Department.AnyAsync(c => c.departmentName.ToLower() == name.ToLower());
                            return !exist;
                        }
                    })
                    .WithMessage("Department Name Is Already Taken");
                RuleFor(c => c.directorateId)
                  .NotEmpty();

            }
        }
        internal sealed class Handler : IRequestHandler<AddDepartmentRequest, HRM_SK.Shared.Result<Guid>>
        {
            private readonly DatabaseContext _dbContext;
            private readonly IValidator<AddDepartmentRequest> _validator;

            public Handler(DatabaseContext dbContext, IValidator<AddDepartmentRequest> validator)
            {
                _dbContext = dbContext;
                _validator = validator;
            }
            public async Task<Result<Guid>> Handle(AddDepartmentRequest request, CancellationToken cancellationToken)
            {

                var validationResult = await _validator.ValidateAsync(request, cancellationToken);

                if (validationResult.IsValid is false)
                {
                    return HRM_SK.Shared.Result.Failure<Guid>(Error.ValidationError(validationResult));
                }
                var directoroateHeadExist = await _dbContext.Directorate.AnyAsync(x => x.Id == request.directorateId);
                if (directoroateHeadExist is false) return HRM_SK.Shared.Result.Failure<Guid>(Error.CreateNotFoundError("Diretorate Not Found"));

                var newEntry = new HRM_SK.Entities.Department()
                {
                    createdAt = DateTime.UtcNow,
                    updatedAt = DateTime.UtcNow,
                    departmentName = request.departmentName,
                    directorateId = request.directorateId,
                    headOfDepartmentId = request.headOfDepartmentId,
                    depHeadOfDepartmentId = request.depHeadOfDepartmentId
                };

                _dbContext.Add(newEntry);

                await _dbContext.SaveChangesAsync();
                return HRM_SK.Shared.Result.Success(newEntry.Id);
            }
        }

    }
}

public class MapAddDepartmentEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("api/department", async (ISender sender, AddDepartmentRequest request) =>
        {
            var response = await sender.Send(request);
            if (response.IsFailure)
            {
                return Results.UnprocessableEntity(response.Error);
            }
            if (response.IsSuccess)
            {
                return Results.Ok(response.Value);
            }
            return Results.BadRequest();

        }).WithTags("Setup-Department")
              .WithMetadata(new ProducesResponseTypeAttribute(StatusCodes.Status200OK))
              .WithMetadata(new ProducesResponseTypeAttribute(typeof(Error), StatusCodes.Status400BadRequest));
    }
}
