using Carter;
using FluentValidation;
using HRM_SK.Database;
using HRM_SK.Extensions;
using HRM_SK.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;
using static App_Setup.Bank.AddBank;

namespace App_Setup.Bank
{
    public static class AddBank
    {
        public class CreateAddBankDTO : IRequest<HRM_SK.Shared.Result<Guid>>
        {
            public String bankName { get; set; }
        }

        public class Validator : AbstractValidator<CreateAddBankDTO>
        {
            protected readonly IServiceScopeFactory _serviceScopeFactory;
            public Validator(IServiceScopeFactory scopeServiceFactory)
            {
                _serviceScopeFactory = scopeServiceFactory;

                RuleFor(x => x.bankName)
                    .NotEmpty()
                    .MustAsync(async (name, cancellationToken) =>
                    {
                        using (var scope = _serviceScopeFactory.CreateScope())
                        {
                            var dbContext = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
                            bool exist = await dbContext
                            .Bank
                            .AnyAsync(e => e.bankName.ToLower() == name.Trim().ToLower());
                            return !exist;
                        }

                    })
                    .WithMessage("Bank Name Already Exist")
                    ;
            }
        }

        internal sealed class HandleRequest : IRequestHandler<CreateAddBankDTO, HRM_SK.Shared.Result<Guid>>
        {
            protected readonly DatabaseContext _dbContext;
            private readonly IValidator<CreateAddBankDTO> _validator;
            public HandleRequest(DatabaseContext dbContext, IValidator<CreateAddBankDTO> validator)
            {
                _dbContext = dbContext;
                _validator = validator;
            }

            public async Task<HRM_SK.Shared.Result<Guid>> Handle(CreateAddBankDTO request, CancellationToken cancellationToken)
            {

                if (request == null) return HRM_SK.Shared.Result.Failure<Guid>(new Error(code: "Invalid Request", message: "Invalid Request body"));

                var validationResult = await _validator.ValidateAsync(request);

                if (!validationResult.IsValid)
                {
                    return HRM_SK.Shared.Result.Failure<Guid>(Error.ValidationError(validationResult));
                }

                var newBank = new HRM_SK.Entities.Bank
                {
                    bankName = request.bankName,
                    createdAt = DateTime.UtcNow,
                    updatedAt = DateTime.UtcNow
                };

                _dbContext.Add(newBank);
                await _dbContext.SaveChangesAsync(cancellationToken);
                return newBank.Id;
            }
        }

    }
}

public class MapAddBankEndpint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("api/bank",
            async (CreateAddBankDTO request, ISender sender) =>
            {

                var response = await sender.Send(request);
                if (response.IsFailure)
                {
                    return Results.UnprocessableEntity(response.Error);
                }
                return Results.Ok(response.Value);
            }
        ).WithTags("Setup-Bank")
        .WithGroupName(SwaggerEndpointDefintions.Setup)
        
            ;
    }
}
