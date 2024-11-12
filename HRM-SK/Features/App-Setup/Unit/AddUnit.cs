using Carter;
using FluentValidation;
using HRM_SK.Database;
using HRM_SK.Extensions;
using HRM_SK.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static App_Setup.Unit.AddUnit;

namespace App_Setup.Unit
{
    public static class AddUnit
    {
        public class AddUnitRequest : IRequest<HRM_SK.Shared.Result<Guid>>
        {

            public Guid departmentId { get; set; }
            public Guid? unitHeadId { get; set; }
            public Guid directorateId { get; set; }
            public string unitName { get; set; }
        }
        public class Validator : AbstractValidator<AddUnitRequest>
        {
            private readonly IServiceScopeFactory _scopeFactory;
            public Validator(IServiceScopeFactory scopeFactory)
            {
                _scopeFactory = scopeFactory;
                RuleFor(c => c.unitName).NotEmpty()
                    .MustAsync(async (name, cancellationToken) =>
                    {
                        using (var scope = _scopeFactory.CreateScope())
                        {
                            var dbContext = scope.ServiceProvider.GetService<DatabaseContext>();
                            var exist = await dbContext.Unit.AnyAsync(s => s.unitName == name, cancellationToken);
                            return !exist;
                        }
                    }).WithMessage("Unit Name Already Exist")
                    ;
                RuleFor(c => c.unitName)
                    .NotEmpty();
                RuleFor(c => c.directorateId)
                    .NotEmpty();
                RuleFor(c => c.departmentId)
                    .NotEmpty();

            }
        }

        public class Handler : IRequestHandler<AddUnitRequest, HRM_SK.Shared.Result<Guid>>
        {
            private readonly DatabaseContext _dbContext;
            private readonly IValidator<AddUnitRequest> _validator;
            public Handler(DatabaseContext dbContext, IValidator<AddUnitRequest> validator)
            {


                _dbContext = dbContext;
                _validator = validator;

            }
            public async Task<Result<Guid>> Handle(AddUnitRequest request, CancellationToken cancellationToken)
            {
                var validationResponse = await _validator.ValidateAsync(request);

                if (validationResponse.IsValid is false)
                {
                    return HRM_SK.Shared.Result.Failure<Guid>(Error.ValidationError(validationResponse));
                }

                var departmentHeadExist = await _dbContext.Department.AnyAsync(x => x.Id == request.departmentId);
                if (departmentHeadExist is false) return HRM_SK.Shared.Result.Failure<Guid>(Error.CreateNotFoundError("Department Not Found"));

                var newEntry = new HRM_SK.Entities.Unit
                {
                    createdAt = DateTime.UtcNow,
                    updatedAt = DateTime.UtcNow,
                    unitName = request.unitName,
                    departmentId = request.departmentId,
                    directorateId = request.directorateId,
                    unitHeadId = request.unitHeadId
                };
                _dbContext.Add(newEntry);
                await _dbContext.SaveChangesAsync();

                return HRM_SK.Shared.Result.Success(newEntry.Id);
            }
        }

    }
}

public class MapAddUnitEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("api/unit", async (ISender sender, AddUnitRequest request) =>
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

        }).WithTags("Setup-Unit")
              .WithMetadata(new ProducesResponseTypeAttribute(StatusCodes.Status200OK))
              .WithMetadata(new ProducesResponseTypeAttribute(typeof(Error), StatusCodes.Status400BadRequest))
              .WithGroupName(SwaggerEndpointDefintions.Setup)
          ;
    }
}
