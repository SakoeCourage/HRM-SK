using Carter;
using FluentValidation;
using HRM_SK.Database;
using HRM_SK.Entities;
using HRM_SK.Serivices.Mail_Service;
using HRM_SK.Services.SMS_Service;
using HRM_SK.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;
using static HRM_BACKEND_VSA.Domains.HR_Management.User.CreateUser;

namespace HRM_BACKEND_VSA.Domains.HR_Management.User
{
    public static class CreateUser
    {
        public class CreateUserRequest : IRequest<HRM_SK.Shared.Result<Guid>>
        {
            public Guid roleId { get; set; }
            public Guid staffId { get; set; }
            public string email { get; set; }

        }

        public class Validator : AbstractValidator<CreateUserRequest>
        {
            private readonly IServiceScopeFactory _scopeFactory;

            public Validator(IServiceScopeFactory scopeFactory)
            {
                _scopeFactory = scopeFactory;

                RuleFor(c => c.email).NotEmpty().EmailAddress()
                .MustAsync(async (model, email, cancellationToken) =>
                {
                    using (var scope = _scopeFactory.CreateScope())
                    {
                        var dbContext = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
                        var exist = await dbContext.User.AnyAsync(u => u.email.Trim().ToLower() == email.Trim().ToLower(), cancellationToken);
                        return !exist;
                    }
                }).WithMessage("User Email Is Already Taken");
                RuleFor(c => c.roleId).NotEmpty();

                RuleFor(c => c.staffId).NotEmpty()
                .MustAsync(async (model, staffId, cancellationToken) =>
                {
                    using (var scope = _scopeFactory.CreateScope())
                    {
                        var dbContext = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
                        var exist = await dbContext.User.AnyAsync(u => u.staffId != null && u.staffId == staffId, cancellationToken);
                        return !exist;
                    }
                }).WithMessage("Staff  Is Already Taken");
            }

        }
        internal sealed class RequestHandler : IRequestHandler<CreateUserRequest, HRM_SK.Shared.Result<Guid>>
        {
            private readonly IValidator<CreateUserRequest> _validator;
            private readonly DatabaseContext _dbContext;
            private readonly MailService _mailService;

            public RequestHandler(IValidator<CreateUserRequest> validator, DatabaseContext dbContext, MailService mailService)
            {
                _dbContext = dbContext;
                _validator = validator;
                _mailService = mailService;
            }
            public async Task<Result<Guid>> Handle(CreateUserRequest request, CancellationToken cancellationToken)
            {
                var validationResponse = await _validator.ValidateAsync(request, cancellationToken);

                if (validationResponse.IsValid is false)
                {
                    return HRM_SK.Shared.Result.Failure<Guid>(Error.ValidationError(validationResponse));
                }

                var userStaffData = await _dbContext.Staff.FirstOrDefaultAsync(s => s.Id == request.staffId);

                if (userStaffData is null)
                {
                    return HRM_SK.Shared.Result.Failure<Guid>(Error.CreateNotFoundError("User Staff Data Not Found"));
                }

                using (var transaction = _dbContext.Database.BeginTransactionAsync())
                {
                    try
                    {
                        var newUserEntry = new HRM_SK.Entities.User
                        {
                            roleId = request.roleId,
                            staffId = request.staffId,
                            createdAt = DateTime.UtcNow,
                            email = request.email,
                            updatedAt = DateTime.UtcNow,
                            password = BCrypt.Net.BCrypt.HashPassword(userStaffData.firstName),
                            isAccountActive = true,
                            hasResetPassword = false
                        };

                        var userRoleEntry = new UserHasRole
                        {
                            userId = newUserEntry.Id,
                            roleId = request.roleId
                        };

                        _dbContext.Add(newUserEntry);
                        await _dbContext.SaveChangesAsync();
                        await transaction.Result.CommitAsync();
                        var message = SMSMessages.generateUserOnboardingMessage($"{userStaffData.lastName} {userStaffData.firstName}");

                        //_mailService.SendMail(new EmailDTO
                        //{
                        //    ToEmail = request.email,
                        //    ToName = $"{userStaffData.lastName} {userStaffData.firstName}",
                        //    Subject = "User Account Created",
                        //    Body = message

                        //});
                        return HRM_SK.Shared.Result.Success(newUserEntry.Id);
                    }
                    catch (Exception ex)
                    {
                        await transaction.Result.RollbackAsync();
                        return HRM_SK.Shared.Result.Failure<Guid>(Error.BadRequest(ex.Message));
                    }
                }


            }
        }
    }
}

public class MapCreateUserEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("api/user", async (ISender sender, CreateUserRequest request) =>
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
        }).WithTags("User Management");
    }
}
