using Carter;
using FluentValidation;
using HRM_SK.Database;
using HRM_SK.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static App_Setup.GradeStep.UpdateGradeStep;

namespace App_Setup.GradeStep
{
    public static class UpdateGradeStep
    {

        public class UpdateGradeStepRequest : IRequest<HRM_SK.Shared.Result>
        {
            public Guid gradeId { get; set; }
            public Guid stepId { get; set; }
            public Double salary { get; set; }
            public Double marketPreBaseSalary { get; set; }
        }

        public class UpdateRequestData
        {
            public Double salary { get; set; }
            public Double marketPreBaseSalary { get; set; }
        }
        public class Validator : AbstractValidator<UpdateGradeStepRequest>
        {
            public Validator()
            {
                RuleFor(c => c.salary).NotEmpty();
                RuleFor(c => c.marketPreBaseSalary).NotEmpty();
            }
        }

        internal sealed class Handler : IRequestHandler<UpdateGradeStepRequest, HRM_SK.Shared.Result>
        {
            private readonly DatabaseContext _dbContext;
            private readonly IValidator<UpdateGradeStepRequest> _validator;
            public Handler(DatabaseContext dbContext, IValidator<UpdateGradeStepRequest> validator)
            {

                _dbContext = dbContext;
                _validator = validator;

            }

            public async Task<HRM_SK.Shared.Result> Handle(UpdateGradeStepRequest request, CancellationToken cancellationToken)
            {

                var validationResponse = await _validator.ValidateAsync(request);

                if (validationResponse.IsValid is false)
                {
                    return HRM_SK.Shared.Result.Failure<Guid>(HRM_SK.Shared.Error.ValidationError(validationResponse));
                }

                var affectedRows = await _dbContext.GradeStep.Where(ent => ent.Id == request.stepId && ent.gradeId == request.gradeId)
                    .ExecuteUpdateAsync(setter => setter
                    .SetProperty(c => c.salary, request.salary)
                    .SetProperty(c => c.marketPreBaseSalary, request.marketPreBaseSalary)
                    .SetProperty(c => c.updatedAt, DateTime.UtcNow)
                    );
                if (affectedRows == 0) return HRM_SK.Shared.Result.Failure(Error.CreateNotFoundError("Grade Step To Update Not Found"));

                return HRM_SK.Shared.Result.Success();

            }
        }

    }
}

public class MapUpdateGradeStepsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPatch("api/grade/{gradeId}/steps/{stepId}", async (ISender sender, Guid gradeId, Guid stepId, UpdateRequestData request) =>
        {
            var response = await sender.Send(
            new UpdateGradeStepRequest
            {
                gradeId = gradeId,
                stepId = stepId,
                salary = request.salary,
                marketPreBaseSalary = request.marketPreBaseSalary
            }
                ); ;
            if (response.IsFailure)
            {
                return Results.UnprocessableEntity(response.Error);
            }

            if (response.IsSuccess)
            {
                return Results.NoContent();
            }
            return Results.BadRequest();

        }).WithTags("Setup-Grade-Step")
              .WithMetadata(new ProducesResponseTypeAttribute(StatusCodes.Status204NoContent))
              .WithMetadata(new ProducesResponseTypeAttribute(typeof(Error), StatusCodes.Status400BadRequest))

          ;
    }
}
