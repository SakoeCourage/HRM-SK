using AutoMapper;
using Carter;
using FluentValidation;
using HRM_SK.Contracts;
using HRM_SK.Database;
using HRM_SK.Entities.HRMActivities;
using HRM_SK.Entities.Staff;
using HRM_SK.Serivices.Mail_Service;
using HRM_SK.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;
using static HRM_SK.Features.Staff_Posting.NewStaffPosting;

namespace HRM_SK.Features.Staff_Posting
{
    public static class NewStaffPosting
    {
        public class NewStaffPostingRequest : IRequest<Shared.Result<string>>
        {
            public Guid staffId { get; set; }
            public Guid? directorateId { get; set; }
            public Guid? departmentId { get; set; } = null;
            public Guid? unitId { get; set; } = null;
            public String? postingOption { get; set; } = "internal";
            public DateOnly postingDate { get; set; }
        }

        public class validator : AbstractValidator<NewStaffPostingRequest>
        {
            public validator()
            {
                RuleFor(c => c.staffId)
                   .NotEmpty();
                RuleFor(c => c.directorateId)
                    .NotEmpty();
                RuleFor(c => c.postingOption)
                    .NotEmpty();

                When(c => c.postingOption == "internal", () =>
                {
                    RuleFor(c => c.departmentId)
                        .NotEmpty().WithMessage("Department ID is required when the posting option is internal.");

                    RuleFor(c => c.unitId)
                        .NotEmpty().WithMessage("Unit ID is required when the posting option is internal.");
                });
            }
        }
        internal sealed class Handler : IRequestHandler<NewStaffPostingRequest, Shared.Result<string>>
        {
            private readonly DatabaseContext _dbContext;
            private readonly IValidator<NewStaffPostingRequest> _validator;
            private readonly MailService _mailService;
            private readonly IMapper _mapper;
            private readonly ISender _sender;
            public Handler(IMapper mapper, MailService mailService, DatabaseContext dbContext, IValidator<NewStaffPostingRequest> validator, ISender sender)
            {
                _dbContext = dbContext;
                _validator = validator;
                _mailService = mailService;
                _mapper = mapper;
                _sender = sender;
            }

            public async Task<Result<string>> Handle(NewStaffPostingRequest request, CancellationToken cancellationToken)
            {
                var validationResult = _validator.Validate(request);

                if (validationResult.IsValid is false)
                {
                    return Shared.Result.Failure<string>(Error.ValidationError(validationResult));
                }
                
             

                var validOption = StaffPostingOptions.parseOption(request.postingOption);
                if (validOption is null)
                {
                    return Shared.Result.Failure<string>(Error.CreateNotFoundError($"Invalid Request option accepted internal, external"));
                }

                var existingStaff = await _dbContext.Staff
                    .Include(s => s.currentAppointment).FirstOrDefaultAsync(s => s.Id == request.staffId);

                if (existingStaff is null)
                {
                    return Shared.Result.Failure<string>(Error.CreateNotFoundError("Staff Data Not Found"));
                }

                if (existingStaff.status == StaffStatusTypes.inActive)
                {
                    return Shared.Result.Failure<string>(Error.CreateNotFoundError("Cannot proceed with current staff"));
                }

                if (existingStaff?.currentAppointment is null)
                {
                    return Shared.Result.Failure<string>(Error.CreateNotFoundError("Staff appointment data was not found"));
                }

                var currentPostingData = await _dbContext.StaffPosting.FirstOrDefaultAsync(sp => sp.staffId == request.staffId);

                if (currentPostingData is not null)
                {
                    {
                        using (var dbTransaction = await _dbContext.Database.BeginTransactionAsync())
                            try
                            {
                                var res = Shared.Result.Success<string>("Staff posting data updated successfully");

                                if (validOption == "internal")
                                {
                                    if (currentPostingData.departmentId != Guid.Empty)
                                    {
                                        _dbContext.Entry(currentPostingData).State = EntityState.Detached;
                                        var isDepartmentChanged = currentPostingData.departmentId != request.departmentId;
                                        var isUnitChanged = currentPostingData.unitId != request.unitId;
                                        
                                        if (isDepartmentChanged || isUnitChanged)
                                        {
                                            var newStaffPostingHistory = new StaffPostingHistory
                                            {
                                                staffId = request.staffId,
                                                departmentId = request.departmentId,
                                                unitId = request.unitId,
                                                postingDate = request.postingDate,
                                                postingOption = validOption,
                                                directorateId = request.directorateId,
                                                createdAt = DateTime.UtcNow,
                                                updatedAt = DateTime.UtcNow
                                            };
                                            _dbContext.StaffPostingHistory.Add(newStaffPostingHistory);
                                        }
                                    }
                                }

                                if (validOption == "external")
                                {
                                    existingStaff.status = StaffStatusTypes.inActive;
                                    var separationResult = new Seperation
                                    {
                                        StaffId = existingStaff.Id,
                                        Reason = "Posting Separation",
                                        DateOfSeparation = DateOnly.FromDateTime(DateTime.UtcNow),
                                        comment = "Staff separated due to external posting"
                                    };
                                    
                                    var newStaffPostingHistory = new StaffPostingHistory
                                    {
                                        staffId = request.staffId,
                                        departmentId = request?.departmentId ?? null,
                                        unitId = request?.unitId ?? null,
                                        postingDate = request.postingDate,
                                        postingOption = validOption,
                                        directorateId = request.directorateId,
                                        createdAt = DateTime.UtcNow,
                                        updatedAt = DateTime.UtcNow
                                    };

                                    _dbContext.Seperation.Add(separationResult);
                                    _dbContext.StaffPostingHistory.Add(newStaffPostingHistory);

                                    res = Shared.Result.Success<string>("Staff Separation Successful");
                                }

                                currentPostingData.updatedAt = DateTime.UtcNow;
                                currentPostingData.departmentId = request.departmentId;
                                currentPostingData.postingDate = request.postingDate;
                                currentPostingData.postingOption = validOption;
                                currentPostingData.unitId = request?.unitId;
                                currentPostingData.directorateId = request?.directorateId;
                                
                                await _dbContext.SaveChangesAsync();
                                await dbTransaction.CommitAsync();
                                return res;
                            }
                            catch (Exception ex)
                            {
                                await dbTransaction.RollbackAsync();
                                return Shared.Result.Failure<string>(Error.BadRequest(ex.Message));
                            };

                    }

                }

                using (var dbTransaction = await _dbContext.Database.BeginTransactionAsync())
                {
                    try
                    {
                        // Creating New Posting Data
                        var newPostingData = new StaffPosting
                        {
                            staffId = request.staffId,
                            departmentId = request.departmentId,
                            unitId = request.unitId,
                            directorateId = request.directorateId,
                            postingDate = request.postingDate,
                            postingOption = validOption,
                            createdAt = DateTime.UtcNow,
                            updatedAt = DateTime.UtcNow
                        };

                        // Adding Posting Data To History
                        var newStaffPostingHistory = new StaffPostingHistory
                        {
                            staffId = request.staffId,
                            departmentId = request.departmentId,
                            unitId = request.unitId,
                            postingDate = request.postingDate,
                            postingOption = validOption,
                            directorateId = request.directorateId,
                            createdAt = DateTime.UtcNow,
                            updatedAt = DateTime.UtcNow
                        };


                        _dbContext.StaffPosting.Add(newPostingData);
                        _dbContext.StaffPostingHistory.Add(newStaffPostingHistory);

                        await _dbContext.SaveChangesAsync();
                        await dbTransaction.CommitAsync();
                        return Shared.Result.Success<string>("Staff posted successfully");
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

public class MapNewStaffPostingEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("api/staff-posting", async (ISender sender, NewStaffPostingRequest request) =>
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
            return Results.BadRequest();
        }).WithTags("Staff-Posting-Transfer");
    }
}