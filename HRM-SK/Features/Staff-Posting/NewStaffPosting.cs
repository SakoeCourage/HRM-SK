using AutoMapper;
using Carter;
using FluentValidation;
using HRM_SK.Database;
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
            public Guid directorateId { get; set; }
            public Guid departmentId { get; set; }
            public Guid unitId { get; set; }
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
                RuleFor(c => c.departmentId)
                    .NotEmpty();
                RuleFor(c => c.unitId)
                    .NotEmpty();
                RuleFor(c => c.postingDate)
                    .NotEmpty();
            }
        }
        internal sealed class Handler : IRequestHandler<NewStaffPostingRequest, Shared.Result<string>>
        {
            private readonly DatabaseContext _dbContext;
            private readonly IValidator<NewStaffPostingRequest> _validator;
            private readonly MailService _mailService;
            private readonly IMapper _mapper;
            public Handler(IMapper mapper, MailService mailService, DatabaseContext dbContext, IValidator<NewStaffPostingRequest> validator)
            {
                _dbContext = dbContext;
                _validator = validator;
                _mailService = mailService;
                _mapper = mapper;
            }

            public async Task<Result<string>> Handle(NewStaffPostingRequest request, CancellationToken cancellationToken)
            {
                var validationResult = _validator.Validate(request);

                if (validationResult.IsValid is false)
                {
                    return Shared.Result.Failure<string>(Error.ValidationError(validationResult));
                }

                var existingStaff = await _dbContext.Staff
                    .Include(s => s.currentAppointment).FirstOrDefaultAsync(s => s.Id == request.staffId);

                if (existingStaff is null)
                {
                    return Shared.Result.Failure<string>(Error.CreateNotFoundError("Staff Data Not Found"));
                }

                if (existingStaff?.currentAppointment is null)
                {
                    return Shared.Result.Failure<string>(Error.CreateNotFoundError("Staff Appointment Data Not Found"));
                }

                var unit = await _dbContext.Unit.Include(u => u.directorate).Include(u => u.department).FirstOrDefaultAsync(u => u.Id == request.unitId);

                if (unit is null)
                {
                    return Shared.Result.Failure<string>(Error.CreateNotFoundError("Unit Data Not Found"));
                }

                var alreadyPosted = await _dbContext.StaffPosting.AnyAsync(sp => sp.staffId == request.staffId);

                if (alreadyPosted)
                {
                    return Shared.Result.Failure<string>(Error.BadRequest("Staff Already Posted"));
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
                            directorateId = request.directorateId,
                            createdAt = DateTime.UtcNow,
                            updatedAt = DateTime.UtcNow
                        };

                        //var emailDTO = new EmailDTO
                        //{
                        //    Subject = "Staff Posting",
                        //    Body = EmailContracts.generateStaffPostingEmailBodyTemplate(new StaffPostingRecord(
                        //    firstName: existingStaff.firstName,
                        //    lastName: existingStaff.lastName,
                        //    staffType: existingStaff.currentAppointment?.staffType ?? "",
                        //    staffId: existingStaff.staffIdentificationNumber,
                        //    unitName: unit.unitName,
                        //    departmentName: unit.department.departmentName,
                        //    directorateName: unit.directorate.directorateName,
                        //    notionalDate: existingStaff.currentAppointment.notionalDate
                        //    )),
                        //    ToEmail = existingStaff.email,
                        //    ToName = $"{existingStaff.firstName} {existingStaff.lastName}"
                        //};

                        //_mailService.SendMail(emailDTO);

                        _dbContext.StaffPosting.Add(newPostingData);
                        _dbContext.StaffPostingHistory.Add(newStaffPostingHistory);

                        await _dbContext.SaveChangesAsync();
                        await dbTransaction.CommitAsync();
                        return Shared.Result.Success<string>("Staff Posting Success");
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
        }).WithTags("Staff-Posting");
    }
}