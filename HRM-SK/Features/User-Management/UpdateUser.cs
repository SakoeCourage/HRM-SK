using Carter;
using FluentValidation;
using HRM_SK.Database;
using HRM_SK.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;
using static HRM_BACKEND_VSA.Domains.HR_Management.User.CreateUser;
using static HRM_BACKEND_VSA.Domains.HR_Management.User.UpdateUser;

namespace HRM_BACKEND_VSA.Domains.HR_Management.User
{
    public class UpdateUser
    {
        public class UpdateUserRequest : IRequest<HRM_SK.Shared.Result<Guid>>
        {
            public Guid id { get; set; }
            public Guid roleId { get; set; }
            public Guid staffId { get; set; }
            public string email { get; set; }

        }

        public class Validator : AbstractValidator<UpdateUserRequest>
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
                        var exist = await dbContext.User.AnyAsync(u => u.email.Trim().ToLower() == email.Trim().ToLower() && u.Id != model.id, cancellationToken);
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
                        var exist = await dbContext.User.AnyAsync(u => u.staffId != null && u.staffId == staffId && u.Id != model.id, cancellationToken);
                        return !exist;
                    }
                }).WithMessage("Staff  Is Already Taken");

            }
        }
    }
    internal sealed class RequestHandler : IRequestHandler<UpdateUserRequest, HRM_SK.Shared.Result<Guid>>
    {
        private readonly IValidator<UpdateUserRequest> _validator;
        private readonly DatabaseContext _dbContext;

        public RequestHandler(IValidator<UpdateUserRequest> validator, DatabaseContext dbContext)
        {
            _dbContext = dbContext;
            _validator = validator;
        }
        public async Task<Result<Guid>> Handle(UpdateUserRequest request, CancellationToken cancellationToken)
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

            var existingUser = await _dbContext.User.FirstOrDefaultAsync(s => s.Id == request.id);

            if (existingUser is null)
            {
                return HRM_SK.Shared.Result.Failure<Guid>(Error.CreateNotFoundError("User Data Not Found"));
            }

            using (var transaction = _dbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    existingUser.roleId = request.roleId;
                    existingUser.staffId = request.staffId;
                    existingUser.updatedAt = DateTime.UtcNow;
                    existingUser.email = request.email;

                    var existingUserRole = await _dbContext.UserHasRole.FirstOrDefaultAsync(x => x.userId == request.id);

                    if (existingUserRole is not null)
                    {
                        existingUserRole.roleId = request.roleId;
                    }
                    await _dbContext.SaveChangesAsync();
                    await transaction.Result.CommitAsync();
                    return HRM_SK.Shared.Result.Success(existingUser.Id);
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
public class MapUpdateUserEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPatch("api/user/{id}", async (ISender sender, Guid id, CreateUserRequest request) =>
        {
            var response = await sender.Send(new UpdateUserRequest
            {
                id = id,
                roleId = request.roleId,
                staffId = request.staffId,
                email = request.email,
            });

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
