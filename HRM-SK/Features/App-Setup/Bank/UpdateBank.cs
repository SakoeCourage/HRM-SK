﻿using App_Setup.Bank;
using Carter;
using FluentValidation;
using HRM_SK.Database;
using HRM_SK.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace App_Setup.Bank
{
    public static class UpdateBank
    {
        public class UpdateAddBankDTO : IRequest<HRM_SK.Shared.Result>
        {
            public Guid Id { get; set; }
            public String bankName { get; set; }
        }


        public class Validator : AbstractValidator<UpdateAddBankDTO>
        {
            protected readonly IServiceScopeFactory _serviceScopeFactory;
            public Validator(IServiceScopeFactory scopeServiceFactory)
            {
                _serviceScopeFactory = scopeServiceFactory;

                RuleFor(x => x.bankName)
                    .NotEmpty()
                    .MustAsync(async (model, name, cancellationToken) =>
                    {
                        using (var scope = _serviceScopeFactory.CreateScope())
                        {
                            var dbContext = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
                            bool exist = await dbContext
                            .Bank
                            .AnyAsync(e => e.bankName.ToLower() == name.Trim().ToLower() && e.Id != model.Id);
                            return !exist;
                        }

                    })
                    .WithMessage("Bank Name Already Exist")
                    ;
            }
        }

        internal sealed class Handler : IRequestHandler<UpdateAddBankDTO, HRM_SK.Shared.Result>
        {
            private readonly DatabaseContext _dbContext;
            private readonly IValidator<UpdateAddBankDTO> _validator;
            public Handler(DatabaseContext dbContext, IValidator<UpdateAddBankDTO> validator)
            {

                _dbContext = dbContext;
                _validator = validator;

            }
            public async Task<HRM_SK.Shared.Result> Handle(UpdateAddBankDTO request, CancellationToken cancellationToken)
            {
                var validationResponse = await _validator.ValidateAsync(request);

                if (validationResponse.IsValid is false)
                {
                    return HRM_SK.Shared.Result.Failure(Error.ValidationError(validationResponse));
                }

                var affectedRows = await _dbContext.Bank.Where(x => x.Id == request.Id).ExecuteUpdateAsync(setters =>
               setters.SetProperty(c => c.bankName, request.bankName)
               .SetProperty(c => c.updatedAt, DateTime.UtcNow)
           );

                if (affectedRows >= 1) return HRM_SK.Shared.Result.Success();

                return HRM_SK.Shared.Result.Failure(Error.CreateNotFoundError("Bank To Update Not Found"));
            }
        }

    }
}

public class MapUpdateBankEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPatch("api/bank/{Id}", async (ISender sender, Guid Id, AddBank.CreateAddBankDTO request) =>
        {
            var response = await sender.Send(
                new UpdateBank.UpdateAddBankDTO
                {
                    Id = Id,
                    bankName = request.bankName
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

        }).WithTags("Setup-Bank")
              .WithMetadata(new ProducesResponseTypeAttribute(StatusCodes.Status204NoContent))
              .WithMetadata(new ProducesResponseTypeAttribute(typeof(Error), StatusCodes.Status400BadRequest));
    }
}
