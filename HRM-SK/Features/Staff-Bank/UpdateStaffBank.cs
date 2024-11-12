using AutoMapper;
using Carter;
using FluentValidation;
using HRM_SK.Database;
using HRM_SK.Entities.Staff;
using HRM_SK.Extensions;
using HRM_SK.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;
using static HRM_SK.Features.Staff_Bank.UpdateStaffBank;

namespace HRM_SK.Features.Staff_Bank
{
    public class UpdateStaffBank
    {
        public class UpdateStaffBankRequestData : IRequest<Shared.Result<string>>
        {
            public Guid staffId { get; set; }
            public Guid bankId { get; set; }
            public string accountType { get; set; }
            public string branch { get; set; }
            public string accountNumber { get; set; }
        }

        public class UpdateStaffBankRequest
        {
            public Guid bankId { get; set; }
            public string accountType { get; set; }
            public string branch { get; set; }
            public string accountNumber { get; set; }
        }

        public class Validator : AbstractValidator<UpdateStaffBankRequestData>
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

        internal sealed class Handler(DatabaseContext dbContext, IValidator<UpdateStaffBankRequestData> validator, IMapper mapper) : IRequestHandler<UpdateStaffBankRequestData, Shared.Result<string>>
        {
            public async Task<Result<string>> Handle(UpdateStaffBankRequestData request, CancellationToken cancellationToken)
            {
                var validationResult = validator.Validate(request);

                if (validationResult.IsValid is false)
                {
                    return Shared.Result.Failure<string>(Error.ValidationError(validationResult));
                }

                var staff = await dbContext.Staff.AnyAsync(s => s.Id == request.staffId);

                if (staff is false)
                {
                    return Shared.Result.Failure<string>(Error.CreateNotFoundError("Staff Record Not Found"));
                }

                var existingData = await dbContext.StaffBankDetail.FirstOrDefaultAsync(s => s.staffId == request.staffId);

                if (existingData is null)
                {
                    return Shared.Result.Failure<string>(Error.CreateNotFoundError("Staff Bank Record Was Not Found"));
                }

                using (var dbTransaction = await dbContext.Database.BeginTransactionAsync())
                {
                    try
                    {
                        existingData.bankId = request.bankId;
                        existingData.branch = request.branch;
                        existingData.accountType = request.accountType;
                        existingData.accountNumber = request.accountNumber;

                        dbContext.Update(existingData);

                        var bankUpdateHistory = mapper.Map<StaffBankUpdateHistory>(existingData);

                        dbContext.Add(bankUpdateHistory);

                        await dbContext.SaveChangesAsync();
                        await dbTransaction.CommitAsync();

                        return Shared.Result.Success("Staff Bank Record Updated");

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
    public class MapUpdateBankRequestEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPatch("api/staff-bank/{staffId}",
            async (ISender sender, UpdateStaffBankRequest request, Guid staffId) =>
            {

                var requestModel = new UpdateStaffBankRequestData
                {
                    staffId = staffId,
                    bankId = request.bankId,
                    accountType = request.accountType,
                    branch = request.branch,
                    accountNumber = request.accountNumber
                };

                var response = await sender.Send(requestModel);

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
}
