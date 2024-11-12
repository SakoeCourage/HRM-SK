using Carter;
using FluentValidation;
using HRM_SK.Database;
using HRM_SK.Extensions;
using HRM_SK.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static App_Setup.Department.AddDepartment;
using static App_Setup.Department.UpdateDepartment;

namespace App_Setup.Department
{
    public class UpdateDepartment
    {
        public class UpdateDepartmentRequest : IRequest<HRM_SK.Shared.Result>
        {
            public Guid Id { get; set; }
            public Guid directorateId { get; set; }
            public string departmentName { get; set; }
            public Guid? headOfDepartmentId { get; set; }
            public Guid? depHeadOfDepartmentId { get; set; }
        }

        public class Validator : AbstractValidator<UpdateDepartmentRequest>
        {
            private readonly IServiceScopeFactory _scopeFactory;
            public Validator(IServiceScopeFactory scopefactory)
            {
                _scopeFactory = scopefactory;
                RuleFor(c => c.departmentName)
                    .NotEmpty()
                    .MustAsync(async (model, name, cancellationToken) =>
                    {
                        using (var scope = _scopeFactory.CreateScope())
                        {
                            var dbContext = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
                            var exist = await dbContext.Department.AnyAsync(c => c.departmentName.ToLower() == name.ToLower() && c.Id != model.Id);
                            return !exist;
                        }
                    })
                    .WithMessage("Department Name Is Already Taken");
                RuleFor(c => c.directorateId)
                  .NotEmpty();
            }
        }

        internal sealed class Handler : IRequestHandler<UpdateDepartmentRequest, HRM_SK.Shared.Result>
        {
            private readonly DatabaseContext _dbContext;
            private readonly IValidator<UpdateDepartmentRequest> _validator;
            public Handler(DatabaseContext dbContext, IValidator<UpdateDepartmentRequest> validator)
            {

                _dbContext = dbContext;
                _validator = validator;

            }
            public async Task<HRM_SK.Shared.Result> Handle(UpdateDepartmentRequest request, CancellationToken cancellationToken)
            {
                var validationResponse = await _validator.ValidateAsync(request);

                if (validationResponse.IsValid is false)
                {
                    return HRM_SK.Shared.Result.Failure(Error.ValidationError(validationResponse));
                }

                var affectedRows = await _dbContext.Department.Where(x => x.Id == request.Id).ExecuteUpdateAsync(setters =>
               setters.SetProperty(c => c.departmentName, request.departmentName)
               .SetProperty(c => c.directorateId, request.directorateId)
               .SetProperty(c => c.updatedAt, DateTime.UtcNow)
           );

                if (affectedRows >= 1) return HRM_SK.Shared.Result.Success();

                return HRM_SK.Shared.Result.Failure(Error.CreateNotFoundError("Category To Update Not Found"));
            }
        }

    }
}


public class MapUpdateDepartmentEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPatch("api/department/{Id}", async (ISender sender, Guid Id, AddDepartmentRequest request) =>
        {
            var response = await sender.Send(
                new UpdateDepartmentRequest
                {
                    Id = Id,
                    directorateId = request.directorateId,
                    departmentName = request.departmentName,
                    headOfDepartmentId = request.headOfDepartmentId,
                    depHeadOfDepartmentId = request.depHeadOfDepartmentId
                }
                );

            if (response.IsFailure)
            {
                return Results.UnprocessableEntity(response.Error);
            }
            if (response.IsSuccess)
            {
                return Results.NoContent();
            }

            return Results.BadRequest();

        }).WithTags("Setup-Department")
              .WithMetadata(new ProducesResponseTypeAttribute(StatusCodes.Status204NoContent))
              .WithMetadata(new ProducesResponseTypeAttribute(typeof(Error), StatusCodes.Status400BadRequest))
              .WithGroupName(SwaggerEndpointDefintions.Setup)
              ;
    }
}
