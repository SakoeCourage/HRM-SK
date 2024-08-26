using AutoMapper;
using Carter;
using FluentValidation;
using HRM_SK.Contracts;
using HRM_SK.Database;
using HRM_SK.Entities.Staff;
using HRM_SK.Serivices.Mail_Service;
using HRM_SK.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;
using static HRM_SK.Features.Staff_Appointment.NewStaffAppointment;

namespace HRM_SK.Features.Staff_Appointment
{
    public class NewStaffAppointment
    {
        public class StaffAppointmentRequest : IRequest<Shared.Result<string>>
        {
            public Guid staffId { get; set; } = Guid.Empty;
            public Guid gradeId { get; set; } = Guid.Empty;
            public string appointmentType { get; set; } = String.Empty;
            public string staffType { get; set; } = String.Empty;
            public string paymentSource { get; set; } = String.Empty;
            public DateOnly? endDate { get; set; }
            public DateOnly notionalDate { get; set; }
            public DateOnly substantiveDate { get; set; }
            public DateOnly? firstAppointmentNotionalDate { get; set; }
            public DateOnly? firstAppointmentSubstantiveDate { get; set; }
            public Guid? firstAppointmentGradeId { get; set; }
            public Guid staffSpecialityId { get; set; }
            public string step { get; set; }
        }

        public class Validator : AbstractValidator<StaffAppointmentRequest>
        {
            public Validator()
            {
                RuleFor(c => c.gradeId).NotEmpty();
                RuleFor(c => c.staffId).NotEmpty();
                RuleFor(c => c.appointmentType).NotEmpty();
                RuleFor(c => c.notionalDate).NotEmpty();
                RuleFor(c => c.substantiveDate).NotEmpty();
                RuleFor(c => c.staffSpecialityId).NotEmpty();
                RuleFor(c => c.step).NotEmpty();
                RuleFor(c => c.appointmentType).NotEmpty();
                RuleFor(c => c.paymentSource).NotEmpty();
            }
        }

        internal sealed class Handler : IRequestHandler<StaffAppointmentRequest, Shared.Result<string>>
        {

            private readonly DatabaseContext _dbContext;
            private readonly IValidator<StaffAppointmentRequest> _validator;
            private readonly MailService _mailService;
            private readonly IMapper _mapper;
            private readonly ILogger<object> _logger;
            public Handler(IMapper mapper, MailService mailService, DatabaseContext dbContext, IValidator<StaffAppointmentRequest> validator, ILogger<string> logger)
            {
                _dbContext = dbContext;
                _validator = validator;
                _mailService = mailService;
                _mapper = mapper;
                _logger = logger;
            }
            public async Task<Result<string>> Handle(StaffAppointmentRequest request, CancellationToken cancellationToken)
            {
                var validationResult = _validator.Validate(request);

                if (validationResult.IsValid is false)
                {
                    return Shared.Result.Failure<string>(Error.ValidationError(validationResult));
                }

                var paymentSourceInitial = PaymentSourceResponseInitials.getGetInitialsFromStaffRequestType(request.paymentSource);
                if (paymentSourceInitial is null) return Shared.Result.Failure<string>(Error.BadRequest("Failed to Process Payment Source"));

                var staffType = StaffTypesInitials.getGetInitialsFromStaffRequestType(request.staffType);
                if (staffType is null) return Shared.Result.Failure<string>(Error.BadRequest("Failed to Process Type of Staff"));

                var staffExist = await _dbContext.Staff.AnyAsync(st => st.Id == request.staffId);

                if (staffExist is false)
                {
                    return Shared.Result.Failure<string>(Error.CreateNotFoundError("Staff Was Not Found"));
                }

                //Staff Has already an appointment created
                var staffAppointed = await _dbContext.StaffAppointment.AnyAsync(st => st.staffId == request.staffId);

                if (staffAppointed)
                {
                    return Shared.Result.Failure<string>(Error.BadRequest("Staff appointment was already found"));
                }

                //creating first appointmentDetail
                var firstAppointmentDetail = new StaffAppointment
                {
                    staffId = request.staffId,
                    appointmentType = request.appointmentType,
                    notionalDate = request?.firstAppointmentNotionalDate ?? request.notionalDate,
                    substantiveDate = request?.firstAppointmentSubstantiveDate ?? request.substantiveDate,
                    staffType = request.staffType,
                    staffSpecialityId = request.staffSpecialityId,
                    paymentSource = request.paymentSource,
                    endDate = request.endDate,
                    step = request.step,
                    gradeId = request?.firstAppointmentGradeId ?? request.gradeId
                };

                // creating current appointment record
                var currentAppointmentDetail = new StaffAppointment
                {
                    staffId = request.staffId,
                    appointmentType = request.appointmentType,
                    notionalDate = request.notionalDate,
                    staffSpecialityId = request.staffSpecialityId,
                    substantiveDate = request.substantiveDate,
                    staffType = request.staffType,
                    paymentSource = request.paymentSource,
                    endDate = request.endDate,
                    step = request.step,
                    gradeId = request.gradeId
                };

                using (var transaction = await _dbContext.Database.BeginTransactionAsync())
                {
                    try
                    {
                        _dbContext.StaffAppointment.Add(currentAppointmentDetail);

                        var appointmentHistoryRecord = _mapper.Map<StaffAppointmentHistory>(firstAppointmentDetail);
                        _dbContext.StaffAppointmentHistory.Add(appointmentHistoryRecord);

                        await _dbContext.SaveChangesAsync();
                        await transaction.CommitAsync();

                        return Shared.Result.Success<string>("Staff Appointment Success");
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        return Shared.Result.Failure<string>(Error.BadRequest(ex.Message));

                    }

                }


            }
        }
    }
}

public class MapNewStaffAppointmentEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("api/staff-appointment", async (ISender sender, StaffAppointmentRequest request) =>
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
        }).WithTags("Staff-Appointment")
            ;
    }
}
