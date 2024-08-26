using Carter;
using FluentValidation;
using HRM_BACKEND_VSA.Features.Role;
using HRM_SK.Database;
using HRM_SK.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;
using static HRM_BACKEND_VSA.Features.Role.EditRole;


namespace HRM_BACKEND_VSA.Features.Role
{
    public static class EditRole
    {
        public class EitRoleRequestBody
        {
            public string Name { get; set; }
        }
        public class EditRoleRequest : IRequest<HRM_SK.Shared.Result<string>>
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
        }

        public class Validator : AbstractValidator<EditRoleRequest>
        {
            private readonly IServiceScopeFactory _scopeFactory;
            public Validator(IServiceScopeFactory scopeFactory)
            {

                _scopeFactory = scopeFactory;

                RuleFor(c => c.Name)
                    .NotEmpty()
                    .MustAsync(async (model, name, cancellation) =>
                    {
                        using (var scope = _scopeFactory.CreateScope())
                        {
                            var dbContext = scope.ServiceProvider.GetService<DatabaseContext>();
                            var exist = await dbContext.Role.AnyAsync(r => r.name.ToLower() == name.Trim().ToLower() && r.Id != model.Id);
                            return !exist;
                        }
                    }
                    )
                    .WithMessage("Role Name Already Exist");
                ;
            }
        }

        internal sealed class RequestHanlder : IRequestHandler<EditRoleRequest, HRM_SK.Shared.Result<string>>
        {
            private readonly DatabaseContext _dbContext;
            private readonly IValidator<EditRoleRequest> _validator;
            public RequestHanlder(DatabaseContext dbContext, IValidator<EditRoleRequest> validator)
            {
                _dbContext = dbContext;
                _validator = validator;
            }
            public async Task<HRM_SK.Shared.Result<string>> Handle(EditRoleRequest request, CancellationToken cancellationtoken)
            {

                if (request == null) return HRM_SK.Shared.Result.InvalidRequest<string>();

                var validationResponse = await _validator.ValidateAsync(request);

                if (!validationResponse.IsValid)
                {
                    return HRM_SK.Shared.Result.Failure<string>(new Error(StatusCodes.Status422UnprocessableEntity.ToString(), validationResponse));
                }

                var role = await _dbContext.Role.FindAsync(request.Id);
                if (role is null)
                {
                    return HRM_SK.Shared.Result.Failure<string>(Error.NotFound);
                }

                role.name = request.Name;
                role.updatedAt = DateTime.UtcNow;

                await _dbContext.SaveChangesAsync();

                return HRM_SK.Shared.Result.Success("Role Updated Successfully");

            }
        }
    }
}

public class EditRoleEndpooint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPatch("api/role/{Id}", async (Guid Id, EditRole.EitRoleRequestBody request, ISender sender) =>
        {
            var response = await sender.Send(new
            EditRoleRequest
            {
                Id = Id,
                Name = request.Name,
            });

            if (response.IsFailure)
            {
                return Results.UnprocessableEntity(response.Error);
            }

            return Results.Ok(response.Value);
        }).WithTags("Setup-Role");
    }
}
