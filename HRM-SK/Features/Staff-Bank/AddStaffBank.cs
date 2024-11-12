using AutoMapper;
using Carter;
using FluentValidation;
using HRM_SK.Database;
using HRM_SK.Entities.Staff;
using HRM_SK.Extensions;
using HRM_SK.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;
using static HRM_SK.Features.Staff_Bank.AddStaffBank;

namespace HRM_SK.Features.Staff_Bank
{
    public static class AddStaffBank
    {
        public class NewBankRequestData : IRequest<Shared.Result<string>>
        {
            public Guid staffId { get; set; }
            public Guid bankId { get; set; }
            public string accountType { get; set; }
            public string branch { get; set; }
            public string accountNumber { get; set; }
        }

        public class Validator : AbstractValidator<NewBankRequestData>
        {
            public Validator()
            {
                RuleFor(c => c.bankId).NotEmpty();
                RuleFor(c => c.accountType).NotEmpty();
                RuleFor(c => c.branch).NotEmpty();
                RuleFor(c => c.accountNumber).NotEmpty();
                RuleFor(c => c.staffId).NotEmpty();
            }
        }
        internal sealed class Handler(DatabaseContext dbContext, IValidator<NewBankRequestData> validator, IMapper mapper) : IRequestHandler<NewBankRequestData, Shared.Result<string>>
        {
            public async Task<Result<string>> Handle(NewBankRequestData request, CancellationToken cancellationToken)
            {
                var validationResult = validator.Validate(request);

                if (validationResult.IsValid is false)
                {
                    return Shared.Result.Failure<string>(Error.ValidationError(validationResult));
                }

                var staff = await dbContext.Staff.AnyAsync(s => s.Id == request.staffId);

                if (staff is false)
                {
                    return Shared.Result.Failure<string>(Error.CreateNotFoundError("Staff Not Found"));
                }

                var existingData = await dbContext.StaffBankDetail.FirstOrDefaultAsync(s => s.staffId == request.staffId);


                using (var dbTransaction = await dbContext.Database.BeginTransactionAsync())
                {
                    try
                    {


                        if (existingData is null)
                        {
                            var newStaffBioData = new StaffBankDetail
                            {
                                staffId = request.staffId,
                                bankId = request.bankId,
                                accountType = request.accountType,
                                branch = request.branch,
                                accountNumber = request.accountNumber
                            };

                            dbContext.Add(newStaffBioData);
                            var bankUpdateHistory = mapper.Map<StaffBankUpdateHistory>(newStaffBioData);
                            dbContext.Add(bankUpdateHistory);

                        }
                        else
                        {
                            existingData.bankId = request.bankId;
                            existingData.branch = request.branch;
                            existingData.accountType = request.accountType;
                            existingData.accountNumber = request.accountNumber;
                            existingData.updatedAt = DateTime.UtcNow;

                            dbContext.Update(existingData);
                            var bankUpdateHistory = mapper.Map<StaffBankUpdateHistory>(existingData);

                            dbContext.Add(bankUpdateHistory);

                        }

                        await dbContext.SaveChangesAsync();
                        await dbTransaction.CommitAsync();

                        return Shared.Result.Success("Staff Bank Record Saved");

                    }
                    catch (Exception ex)
                    {
                        await dbTransaction.RollbackAsync();
                        return Shared.Result.Failure<string>(Error.BadRequest(ex.Message));
                    }

                }

            }
        }
    }


}

public class MapNewBankRequestEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("api/staff-bank/update-or-new",
        async (ISender sender, NewBankRequestData request) =>
        {
            var response = await sender.Send(request);

            if (response.IsSuccess)
            {
                return Results.Ok(response.Value);
            }

            if (response.IsFailure)
            {
                return Results.UnprocessableEntity(response.Error);
            }

            return Results.BadRequest("Something Went Wrong");

        }).WithTags("Staff Bank Record")
            .WithGroupName(SwaggerEndpointDefintions.Planning)
            ;
    }
}
