using AutoMapper;
using Carter;
using FluentValidation;
using HRM_SK.Database;
using HRM_SK.Entities.Staff;
using HRM_SK.Extensions;
using HRM_SK.Providers;
using HRM_SK.Serivices.ImageKit;
using HRM_SK.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;
using static HRM_BACKEND_VSA.Domains.Staffs.Staff_Bio.AddStaffBio;

namespace HRM_BACKEND_VSA.Domains.Staffs.Staff_Bio
{
    public static class AddStaffBio
    {
        public class AddStaffBioRequest : IRequest<HRM_SK.Shared.Result<Guid>>
        {
            public IFormFile? passportPicture { get; set; } = null;
            public string title { get; set; } = string.Empty;
            public string firstName { get; set; }
            public string lastName { get; set; }
            public string? otherNames { get; set; }
            public string gender { get; set; }
            public string email { get; set; }
            public string phone { get; set; }
            public string GPSAddress { get; set; } = string.Empty;
            public string nationality { get; set; } = string.Empty;
            public DateOnly dateOfBirth { get; set; }
            public string? SNNITNumber { get; set; } = null;
            public string disability { get; set; }
            public string? ECOWASCardNumber { get; set; } = null;
            public string staffIdentificationNumber { get; set; }
        }

        public class Validator : AbstractValidator<AddStaffBioRequest>
        {
            protected readonly IServiceScopeFactory _serviceScopeFactory;
            public Validator(IServiceScopeFactory scopeServiceFactory)
            {
                _serviceScopeFactory = scopeServiceFactory;

                RuleFor(x => x.passportPicture)
                .Must(file => file == null || FluentValidatorExtensions.IsImageFile(file))
                .WithMessage("Only image files are allowed.")
                .Must(file => file == null || file.Length <= 3 * 1024 * 1024)
                .WithMessage("File size must not exceed 3MB.");

                RuleFor(c => c.firstName).NotEmpty();
                RuleFor(c => c.lastName).NotEmpty();
                RuleFor(c => c.phone).NotEmpty().MustAsync(async (model, phone, cancelationToken) =>
                {
                    //if (phone is null) return true;

                    using (var scope = _serviceScopeFactory.CreateScope())
                    {
                        var dbContext = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
                        bool exist = await dbContext
                        .Staff
                        .AnyAsync(e => String.IsNullOrWhiteSpace(e.phone) == false && e.phone.ToLower().Trim() == phone.Trim().ToLower());
                        return !exist;
                    }
                })
               .WithMessage("Phone Number Already Taken");
                RuleFor(c => c.gender).NotEmpty();
                RuleFor(c => c.dateOfBirth).NotEmpty();
                RuleFor(c => c.phone).NotEmpty();
                RuleFor(c => c.SNNITNumber).MustAsync(async (model, snnit, cancelationToken) =>
                {
                    if (snnit is null) return true;

                    using (var scope = _serviceScopeFactory.CreateScope())
                    {
                        var dbContext = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
                        bool exist = await dbContext
                        .Staff
                        .AnyAsync(e => String.IsNullOrWhiteSpace(e.SNNITNumber) == false && e.SNNITNumber.Trim().ToLower() == snnit.Trim().ToLower());
                        return !exist;
                    }
                })
               .WithMessage("SNNIT Number Already Taken");

                RuleFor(c => c.staffIdentificationNumber).NotEmpty().MustAsync(async (model, idnumber, cancelationToken) =>
                {
                    if (idnumber is null) return true;

                    using (var scope = _serviceScopeFactory.CreateScope())
                    {
                        var dbContext = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
                        bool exist = await dbContext
                        .Staff
                        .AnyAsync(e => e.staffIdentificationNumber.Trim().ToLower() == idnumber.Trim().ToLower());
                        return !exist;
                    }
                })
                   .WithMessage("Staff With Identification Number Already Exist");
                RuleFor(c => c.email)
                    .NotEmpty()
                    .EmailAddress()
                    .MustAsync(async (model, email, cancelationToken) =>
                    {
                        using (var scope = _serviceScopeFactory.CreateScope())
                        {
                            var dbContext = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
                            bool exist = await dbContext
                            .Staff
                            .AnyAsync(e => e.email.ToLower() == email.Trim().ToLower());
                            return !exist;
                        }
                    })
                    .WithMessage("Email Already Exist");

                RuleFor(c => c.ECOWASCardNumber)
               .MustAsync(async (model, cardNo, cancelationToken) =>
               {
                   if (cardNo is null) return true;

                   using (var scope = _serviceScopeFactory.CreateScope())
                   {
                       var dbContext = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
                       bool exist = await dbContext
                       .Staff
                       .AnyAsync(e => e.ECOWASCardNumber.Trim().ToLower() == cardNo.Trim().ToLower() && String.IsNullOrWhiteSpace(e.ECOWASCardNumber) == false);
                       return !exist;
                   }
               })
               .WithMessage("ECOWAS Card Number Already Taken");
            }
        }

        internal sealed class Handler : IRequestHandler<AddStaffBioRequest, HRM_SK.Shared.Result<Guid>>
        {
            private readonly IValidator<AddStaffBioRequest> _validator;
            private readonly DatabaseContext _dbContext;
            private readonly Authprovider _authProvider;
            private readonly IMapper _mapper;
            private readonly ImageKit _imageKit;

            public Handler(DatabaseContext dbContext, IValidator<AddStaffBioRequest> validator, Authprovider authProvider, IMapper mapper, ImageKit imageKit)
            {
                _validator = validator;
                _dbContext = dbContext;
                _authProvider = authProvider;
                _mapper = mapper;
                _imageKit = imageKit;
            }
            public async Task<Result<Guid>> Handle(AddStaffBioRequest request, CancellationToken cancellationToken)
            {
                if (request == null) return HRM_SK.Shared.Result.Failure<Guid>(new Error(code: "Invalid Request", message: "Invalid Request body"));

                var validationResult = await _validator.ValidateAsync(request);

                if (!validationResult.IsValid)
                {
                    return HRM_SK.Shared.Result.Failure<Guid>(Error.ValidationError(validationResult));
                }

                using (var transactions = await _dbContext.Database.BeginTransactionAsync())
                {
                    try
                    {
                        var newStaffEntry = new HRM_SK.Entities.Staff.Staff
                        {
                            title = request.title,
                            GPSAddress = request.GPSAddress,
                            firstName = request.firstName,
                            lastName = request.lastName,
                            email = request.email,
                            phone = request.phone,
                            gender = request.gender,
                            SNNITNumber = request?.SNNITNumber,
                            disability = request.disability,
                            dateOfBirth = request.dateOfBirth,
                            createdAt = DateTime.UtcNow,
                            updatedAt = DateTime.UtcNow,
                            ECOWASCardNumber = request?.ECOWASCardNumber,
                            isApproved = true,
                            staffIdentificationNumber = request.staffIdentificationNumber,
                            nationality = request.nationality
                        };

                        if (request.passportPicture is not null)
                        {
                            Console.WriteLine("passport found");

                            var imageUpladStatus = await _imageKit.HandleNewFormFileUploadAsync(request.passportPicture);

                            if (imageUpladStatus.thumbnailUrl is not null)
                            {
                                Console.WriteLine($"Url found as {imageUpladStatus.thumbnailUrl}");
                                newStaffEntry.passportPicture = imageUpladStatus.thumbnailUrl;
                            }
                        }

                        _dbContext.Add(newStaffEntry);
                        var bioUpdateHistory = _mapper.Map<StaffBioUpdateHistory>(newStaffEntry);
                        bioUpdateHistory.staffId = newStaffEntry.Id;
                        _dbContext.StaffBioUpdateHistory.Add(bioUpdateHistory);

                        await _dbContext.SaveChangesAsync();
                        await transactions.CommitAsync();

                        return HRM_SK.Shared.Result.Success(newStaffEntry.Id);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                        await transactions.RollbackAsync();
                        return HRM_SK.Shared.Result.Failure<Guid>(Error.BadRequest(ex.Message));

                    }
                }
            }
        }
    }
}

public class MapAddStaffEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("api/staff",
            async (HttpContext context, ISender sender) =>
            {
                var form = await context.Request.ReadFormAsync();

                var passportPicture = form.Files.GetFile("passportPicture");
                var firstName = form["firstName"];
                var title = form["title"];
                var GPSAddress = form["GPSAddress"];
                var nationality = form["nationality"];
                var lastName = form["lastName"];
                var otherNames = form["otherNames"];
                var dateOfBirth = DateOnly.Parse(form["dateOfBirth"]);
                var phone = form["phone"];
                var gender = form["gender"];
                var SNNITNumber = form["SNNITNumber"];
                var email = form["email"];
                var disability = form["disability"];
                var ECOWASCardNumber = form["ECOWASCardNumber"];
                var staffIdentificationNumber = form["staffIdentificationNumber"];

                var requestModel = new AddStaffBioRequest
                {
                    passportPicture = passportPicture,
                    firstName = firstName,
                    title = title,
                    GPSAddress = GPSAddress,
                    nationality = nationality,
                    lastName = lastName,
                    otherNames = otherNames,
                    dateOfBirth = dateOfBirth,
                    phone = phone,
                    gender = gender,
                    SNNITNumber = SNNITNumber,
                    email = email,
                    disability = disability,
                    ECOWASCardNumber = ECOWASCardNumber,
                    staffIdentificationNumber = staffIdentificationNumber
                };


                var response = await sender.Send(requestModel);
                if (response.IsFailure)
                {
                    return Results.UnprocessableEntity(response.Error);
                }
                return Results.Ok(response.Value);
            })
            .WithTags("Staff Management")
            .Accepts<AddStaffBioRequest>("multipart/form-data")
            .DisableAntiforgery()
            .WithGroupName(SwaggerEndpointDefintions.Planning)
            ;
    }

}

