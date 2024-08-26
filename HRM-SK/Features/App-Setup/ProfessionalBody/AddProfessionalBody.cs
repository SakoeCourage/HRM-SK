﻿using Carter;
using FluentValidation;
using HRM_SK.Database;
using HRM_SK.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data.Common;
using static App_Setup.ProfessionalBody.AddProfessionalBody;

namespace App_Setup.ProfessionalBody
{
    public static class AddProfessionalBody
    {

        public class AddProfessionalBodyRequest : IRequest<Result<Guid>>
        {
            public string name { get; set; }
        }

        public class Validator : AbstractValidator<AddProfessionalBodyRequest>
        {
            private readonly IServiceScopeFactory _scopeFactory;
            public Validator(IServiceScopeFactory scopeFactory)
            {
                _scopeFactory = scopeFactory;
                RuleFor(c => c.name)
                    .NotEmpty()
                    .MustAsync(async (name, concellationToken) =>
                    {
                        using (var scope = _scopeFactory.CreateScope())
                        {
                            var dbContext = scope.ServiceProvider.GetService<DatabaseContext>();
                            var exist = await dbContext.ProfessionalBody.AnyAsync(pb => pb.name == name, concellationToken);
                            return !exist;
                        }

                    }).WithMessage("Professional Body Already Exist");

            }
        }

        public class Handler : IRequestHandler<AddProfessionalBodyRequest, Result<Guid>>
        {
            private readonly DatabaseContext _dbContext;
            private readonly IValidator<AddProfessionalBodyRequest> _validator;
            public Handler(DatabaseContext dbContext, IValidator<AddProfessionalBodyRequest> validator)
            {

                _dbContext = dbContext;
                _validator = validator;

            }
            public async Task<Result<Guid>> Handle(AddProfessionalBodyRequest request, CancellationToken cancellationToken)
            {
                var validationResponse = await _validator.ValidateAsync(request, cancellationToken);

                if (validationResponse.IsValid is false)
                {
                    return HRM_SK.Shared.Result.Failure<Guid>(Error.ValidationError(validationResponse));
                }

                var newEntry = new HRM_SK.Entities.ProfessionalBody
                {
                    name = request.name

                };
                _dbContext.Add(newEntry);
                try
                {
                    await _dbContext.SaveChangesAsync();
                }
                catch (DbException ex)
                {
                    return HRM_SK.Shared.Result.Failure<Guid>(Error.BadRequest(ex.Message));
                }
                return HRM_SK.Shared.Result.Success<Guid>(newEntry.Id);
            }
        }


    }
}

public class MappAddProfessionalBodyRequest : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("api/professional-body", async (ISender sender, AddProfessionalBodyRequest request) =>
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

        }).WithTags("Setup-ProfessionalBody")
              .WithMetadata(new ProducesResponseTypeAttribute(StatusCodes.Status200OK))
              .WithMetadata(new ProducesResponseTypeAttribute(typeof(Error), StatusCodes.Status400BadRequest))

          ;
    }
}
