//using Carter;
//using FluentValidation;
//using HRM_SK.Database;
//using HRM_SK.Serivices.ImageKit;
//using HRM_SK.Shared;
//using MediatR;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using static HRM_BACKEND_VSA.Domains.Staffs.Staff_Bio.UpdateOrAddStaffPassport;

//namespace HRM_BACKEND_VSA.Domains.Staffs.Staff_Bio
//{
//    public static class UpdateOrAddStaffPassport
//    {
//        public class UpdateOrAddStaffPassportRequest : IRequest<HRM_SK.Shared.Result<string>>
//        {
//            public Guid staffId { get; set; }
//            public IFormFile passportPicture { get; set; }
//        }

//        public class Validator : AbstractValidator<UpdateOrAddStaffPassportRequest>
//        {
//            public Validator()
//            {
//                RuleFor(c => c.passportPicture)
//                    .NotEmpty();
//            }
//        }

//        internal sealed class Handler : IRequestHandler<UpdateOrAddStaffPassportRequest, HRM_SK.Shared.Result<string>>
//        {
//            private readonly IValidator<UpdateOrAddStaffPassportRequest> _validator;
//            private readonly DatabaseContext _dbContext;
//            private readonly ImageKit _imageKit;
//            public Handler(DatabaseContext dbContext, IValidator<UpdateOrAddStaffPassportRequest> validator, ImageKit imageKit)
//            {
//                _validator = validator;
//                _dbContext = dbContext;
//                _imageKit = imageKit;
//            }
//            public async Task<HRM_SK.Shared.Result<string>> Handle(UpdateOrAddStaffPassportRequest request, CancellationToken cancellationToken)
//            {
//                var validationResponse = _validator.Validate(request);
//                if (validationResponse.IsValid is false) return HRM_SK.Shared.Result.Failure<string>(Error.ValidationError(validationResponse));

//                var currentStaff = await _dbContext.Staff.FirstOrDefaultAsync(s => s.Id == request.staffId);
//                if (currentStaff is null) return HRM_SK.Shared.Result.Failure<string>(Error.CreateNotFoundError("Staff Not Found"));

//                try
//                {
//                    var imageUpladStatus = await _imageKit.HandleNewFormFileUploadAsync(request.passportPicture);
//                    if (imageUpladStatus.thumbnailUrl is not null)
//                    {
//                        currentStaff.passportPicture = imageUpladStatus.thumbnailUrl;
//                        await _dbContext.SaveChangesAsync();
//                        return HRM_SK.Shared.Result.Success(imageUpladStatus.thumbnailUrl);
//                    }
//                }
//                catch (Exception ex)
//                {
//                    return HRM_SK.Shared.Result.Failure<string>(Error.BadRequest(ex.Message));
//                }
//                return HRM_SK.Shared.Result.Failure<string>(Error.BadRequest("Failed Attach Passport Picture"));
//            }
//        }
//    }
//}

//public class MapUpdateOrAddStaffPassportEnpoint : ICarterModule
//{
//    public void AddRoutes(IEndpointRouteBuilder app)
//    {
//        app.MapPatch("api/staff/{Id}/update-passportpic", async (ISender sender, Guid Id, [FromForm] IFormFile passportPicture) =>
//        {
//            var response = await sender.Send(
//                new UpdateOrAddStaffPassportRequest
//                {
//                    staffId = Id,
//                    passportPicture = passportPicture
//                }
//                );

//            if (response.IsFailure)
//            {
//                return Results.UnprocessableEntity(response.Error);
//            }
//            if (response.IsSuccess)
//            {
//                return Results.NoContent();
//            }

//            return Results.BadRequest();

//        }).WithTags("Staff Management")
//              .WithMetadata(new ProducesResponseTypeAttribute(StatusCodes.Status204NoContent))
//              .WithMetadata(new ProducesResponseTypeAttribute(typeof(Error), StatusCodes.Status400BadRequest));
//    }
//}

