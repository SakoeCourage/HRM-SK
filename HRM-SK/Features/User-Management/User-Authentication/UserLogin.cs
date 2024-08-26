using AutoMapper;
using Carter;
using FluentValidation;
using HRM_SK.Database;
using HRM_SK.Providers;
using HRM_SK.Serivices.Mail_Service;
using HRM_SK.Services.SMS_Service;
using HRM_SK.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static HRM_BACKEND_VSA.Domains.HR_Management.User.User_Authentication.UserLogin;
using static HRM_SK.Contracts.UserContracts;

namespace HRM_BACKEND_VSA.Domains.HR_Management.User.User_Authentication
{
    public class UserLogin
    {

        public class UserLoginRequest : IRequest<HRM_SK.Shared.Result<UserLoginResponse>>
        {
            public string email { get; set; }
            public string password { get; set; }
        }

        public class Validator : AbstractValidator<UserLoginRequest>
        {
            public Validator()
            {
                RuleFor(c => c.email).NotEmpty();
                RuleFor(c => c.password).NotEmpty();
            }
        }

        internal sealed class Handler : IRequestHandler<UserLoginRequest, HRM_SK.Shared.Result<UserLoginResponse>>
        {
            private readonly DatabaseContext _dbContext;
            private readonly IMapper _mapper;
            private readonly SMSService _smsService;
            private readonly MailService _mailService;
            private readonly IValidator<UserLoginRequest> _validator;
            private readonly JWTProvider _jwtProvider;
            public Handler(DatabaseContext dbContext, IMapper mapper, SMSService smsService, MailService mailService, IValidator<UserLoginRequest> validator, JWTProvider jwtProvider)
            {
                _dbContext = dbContext;
                _mapper = mapper;
                _smsService = smsService;
                _mailService = mailService;
                _validator = validator;
                _jwtProvider = jwtProvider;
            }

            public async Task<Result<UserLoginResponse>> Handle(UserLoginRequest request, CancellationToken cancellationToken)
            {
                var validationResponse = _validator.Validate(request);

                if (validationResponse.IsValid is false)
                {
                    return HRM_SK.Shared.Result.Failure<UserLoginResponse>(Error.ValidationError(validationResponse));
                }

                var userEmail = request.email;
                //Checking if user Exist
                var user = await _dbContext.User.FirstOrDefaultAsync(s => s.email == userEmail);
                if (user == null) { return HRM_SK.Shared.Result.Failure<UserLoginResponse>(Error.BadRequest("Invalid Email or Password")); }
                if (BCrypt.Net.BCrypt.Verify(request.password, user.password) is not true)
                {
                    return HRM_SK.Shared.Result.Failure<UserLoginResponse>(Error.BadRequest("Invalid Email or Password"));
                }

                var unreadCount = await _dbContext.Notification.Where(entry => entry.notifiableType == typeof(HRM_SK.Entities.User).Name
                    && entry.notifiableId == user.Id
                    && entry.readAt == null
                     ).CountAsync();

                var response = _mapper.Map<UserLoginResponse>(user);
                response.unreadCount = unreadCount;
                response.expiry = DateTime.UtcNow.AddHours(6);

                response.accessToken = _jwtProvider.GenerateAccessToken(response.Id, AuthorizationDecisionType.HRMUser);

                return HRM_SK.Shared.Result.Success(response);
            }
        }
    }
}

public class MappUserLoginEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("api/auth/user/login", async (UserLoginRequest request, ISender sender) =>
        {
            var result = await sender.Send(request);

            if (result.IsFailure)
            {
                return Results.UnprocessableEntity(result.Error);
            };

            if (result.IsSuccess)
            {
                return Results.Ok(result.Value);
            }
            return Results.BadRequest();
        })
        .RequireRateLimiting("fixed")
        .WithMetadata(new ProducesResponseTypeAttribute(typeof(UserLoginResponse), StatusCodes.Status200OK))
        .WithMetadata(new ProducesResponseTypeAttribute(typeof(Error), StatusCodes.Status422UnprocessableEntity))
        .WithMetadata(new ProducesResponseTypeAttribute(StatusCodes.Status400BadRequest))
        .WithTags("User Authentication");
    }
}
