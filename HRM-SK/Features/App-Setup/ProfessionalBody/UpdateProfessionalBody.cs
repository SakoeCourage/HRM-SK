using App_Setup.ProfessionalBody;
using Carter;
using FluentValidation;
using HRM_SK.Database;
using HRM_SK.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static App_Setup.ProfessionalBody.UpdateProfessionalBody;

namespace App_Setup.ProfessionalBody
{
    public static class UpdateProfessionalBody
    {
        public class UpdateProfessionalBodyRequest : IRequest<HRM_SK.Shared.Result>
        {
            public Guid Id { get; set; }
            public string name { get; set; }
        }

        public class Validator : AbstractValidator<UpdateProfessionalBodyRequest>
        {
            private readonly IServiceScopeFactory _scopeFactory;
            public Validator(IServiceScopeFactory scopeFactory)
            {
                _scopeFactory = scopeFactory;
                RuleFor(c => c.name)
                    .NotEmpty()
                    .MustAsync(async (model, name, concellationToken) =>
                    {
                        using (var scope = _scopeFactory.CreateScope())
                        {
                            var dbContext = scope.ServiceProvider.GetService<DatabaseContext>();
                            var exist = await dbContext.ProfessionalBody.AnyAsync(pb => pb.name == name && pb.Id != model.Id, concellationToken);
                            return !exist;
                        }

                    }).WithMessage("Professional Body Already Exist");

            }
        }

        public class Handler : IRequestHandler<UpdateProfessionalBodyRequest, HRM_SK.Shared.Result>
        {
            private readonly DatabaseContext _dbContext;
            private readonly IValidator<UpdateProfessionalBodyRequest> _validator;
            public Handler(DatabaseContext dbContext, IValidator<UpdateProfessionalBodyRequest> validator)
            {
                _dbContext = dbContext;
                _validator = validator;
            }
            public async Task<HRM_SK.Shared.Result> Handle(UpdateProfessionalBodyRequest request, CancellationToken cancellationToken)
            {
                var validationResult = await _validator.ValidateAsync(request);

                if (validationResult.IsValid is false)
                {
                    return HRM_SK.Shared.Result.Failure(Error.ValidationError(validationResult));
                }

                var affectedRows = await _dbContext.ProfessionalBody.ExecuteUpdateAsync(setters =>
                   setters.SetProperty(c => c.name, request.name)
                   .SetProperty(c => c.updatedAt, DateTime.UtcNow)
                );
                if (affectedRows == 0) return HRM_SK.Shared.Result.Failure(Error.CreateNotFoundError("Proffesional Body Not Found"));

                return HRM_SK.Shared.Result.Success();
            }
        }
    }
}

public class MapUpdateProfessionalBodyEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPatch("/api/profesional-body/{Id}", async (ISender sender, Guid Id, AddProfessionalBody.AddProfessionalBodyRequest request) =>
        {
            var response = await sender.Send(new UpdateProfessionalBodyRequest
            {
                Id = Id,
                name = request.name,

            });

            if (response.IsSuccess)
            {
                return Results.NoContent();
            }
            if (response.IsFailure)
            {
                return Results.UnprocessableEntity(response.Error);
            }

            return Results.BadRequest();

        }).WithTags("Setup-ProfessionalBody")
        .WithMetadata(new ProducesResponseTypeAttribute(StatusCodes.Status204NoContent));

    }
}
