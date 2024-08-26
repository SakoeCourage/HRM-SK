using AutoMapper;
using FluentValidation;
using HRM_SK.Database;
using HRM_SK.Providers;
using HRM_SK.Serivices.Mail_Service;
using HRM_SK.Services.SMS_Service;
using HRM_SK.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;
using static HRM_SK.Contracts.UserContracts;

namespace HRM_BACKEND_VSA.Domains.HR_Management.User.User_Authentication
{
    public class ConfirmTwoFactorOtp
    {
        public class ConfirmTwoFactorOptRequest : IRequest<HRM_SK.Shared.Result<UserLoginResponse>>
        {
            public string email { get; set; }
            public string otp { get; set; }
        }
        public class Validator : AbstractValidator<ConfirmTwoFactorOptRequest>
        {
            public Validator()
            {
                RuleFor(c => c.email)
                    .NotEmpty()
                    .MinimumLength(5);
                RuleFor(c => c.otp)
                    .NotEmpty()
                    .MinimumLength(4);
            }

        }
        internal sealed class Handler : IRequestHandler<ConfirmTwoFactorOptRequest, HRM_SK.Shared.Result<UserLoginResponse>>
        {
            private readonly DatabaseContext _dbContext;
            private readonly IMapper _mapper;
            private readonly SMSService _smsService;
            private readonly MailService _mailService;
            private readonly IValidator<ConfirmTwoFactorOptRequest> _validator;
            private readonly JWTProvider _jwtProvider;
            public Handler(JWTProvider jwtProvider, DatabaseContext dbContext, IMapper mapper, SMSService smsService, MailService mailService, IValidator<ConfirmTwoFactorOptRequest> validator)
            {
                _jwtProvider = jwtProvider;
                _dbContext = dbContext;
                _mapper = mapper;
                _smsService = smsService;
                _mailService = mailService;
                _validator = validator;
            }

            public async Task<Result<UserLoginResponse>> Handle(ConfirmTwoFactorOptRequest request, CancellationToken cancellationToken)
            {
                var validationResult = _validator.Validate(request);

                if (validationResult.IsValid is false)
                {
                    return HRM_SK.Shared.Result.Failure<UserLoginResponse>(Error.ValidationError(validationResult));
                }
                var hasOTP = await _dbContext
                       .UserHasOTP
                       .FirstOrDefaultAsync(a => a.email == request.email && a.otp == request.otp);

                if (hasOTP == null) return HRM_SK.Shared.Result.Failure<UserLoginResponse>(Error.CreateNotFoundError("Failed To Confirm OTP"));
                DateTime OTPCreatedDate = hasOTP.updatedAt;
                bool hasExpired = DateTime.UtcNow - OTPCreatedDate > TimeSpan.FromMinutes(10);

                if (hasExpired) return HRM_SK.Shared.Result.Failure<UserLoginResponse>(Error.BadRequest("OTP Has Expired"));

                var user = await _dbContext.User.Include(u => u.staff).IgnoreAutoIncludes().FirstOrDefaultAsync(u => u.email == hasOTP.email);

                if (user is null) return HRM_SK.Shared.Result.Failure<UserLoginResponse>(Error.CreateNotFoundError("User Not Found"));

                user.lastSeen = DateTime.UtcNow;
                await _dbContext.SaveChangesAsync();

                // Getting users Notification Count
                var unreadCount = await _dbContext.Notification.Where(entry => entry.notifiableType == typeof(HRM_SK.Entities.User).Name
                   && entry.notifiableId == user.Id
                   && entry.readAt == null
                ).CountAsync();

                var response = _mapper.Map<UserLoginResponse>(user);
                response.unreadCount = unreadCount;
                response.expiry = DateTime.UtcNow.AddHours(6);

                response.accessToken = _jwtProvider.GenerateAccessToken(response.Id, AuthorizationDecisionType.HRMUser);

                await _dbContext.UserHasOTP.Where(x => x.email == hasOTP.email).ExecuteDeleteAsync();

                return HRM_SK.Shared.Result.Success(response);
            }

        }
    }
}

//public class MappConfirmTwoFactorOtpEndpoint : ICarterModule
//{
//    public void AddRoutes(IEndpointRouteBuilder app)
//    {
//        app.MapPost("api/auth/user/two-factor-confirmation", async (ConfirmTwoFactorOptRequest request, ISender sender) =>
//        {
//            var result = await sender.Send(request);

//            if (result.IsFailure)
//            {
//                return Results.UnprocessableEntity(result.Error);
//            };

//            if (result.IsSuccess)
//            {
//                return Results.Ok(result.Value);
//            }
//            return Results.BadRequest();
//        })
//         .WithMetadata(new ProducesResponseTypeAttribute(typeof(UserLoginResponse), StatusCodes.Status200OK))
//        .WithMetadata(new ProducesResponseTypeAttribute(typeof(Error), StatusCodes.Status422UnprocessableEntity))
//        .WithMetadata(new ProducesResponseTypeAttribute(StatusCodes.Status204NoContent))
//        .WithTags("User Authentication");
//    }
//}
