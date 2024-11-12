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
using static HRM_SK.Features.Staff_Management.UpdateStaffBio;

namespace HRM_SK.Features.Staff_Management
{
    public class UpdateStaffBio
    {

        public class UpdateStaffBioRequest : IRequest<Shared.Result<string>>
        {
            public IFormFile? passportPicture { get; set; } = null;
            public Guid staffId { get; set; }
            public string firstName { get; set; }
            public string title { get; set; } = string.Empty;
            public string GPSAddress { get; set; } = string.Empty;
            public string nationality { get; set; } = string.Empty;
            public string lastName { get; set; }
            public string? otherNames { get; set; }
            public DateOnly dateOfBirth { get; set; }
            public string? phone { get; set; } = null;
            public string gender { get; set; }
            public string? SNNITNumber { get; set; } = null;
            public string email { get; set; }
            public string disability { get; set; }
            public string? ECOWASCardNumber { get; set; } = null;
            public string staffIdentificationNumber { get; set; }
        }
        public class Validator : AbstractValidator<UpdateStaffBioRequest>
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
                        .AnyAsync(e => String.IsNullOrWhiteSpace(e.phone) == false && e.phone.ToLower().Trim() == phone.Trim().ToLower() && e.Id != model.staffId);
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
                        .AnyAsync(e => String.IsNullOrWhiteSpace(e.SNNITNumber) == false && e.SNNITNumber.Trim().ToLower() == snnit.Trim().ToLower() && e.Id != model.staffId);
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
                        .AnyAsync(e => e.staffIdentificationNumber.Trim().ToLower() == idnumber.Trim().ToLower() && e.Id != model.staffId);
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
                            .AnyAsync(e => e.email.Trim().ToLower() == email.Trim().ToLower() && e.Id != model.staffId);
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
                       .AnyAsync(e => String.IsNullOrWhiteSpace(e.ECOWASCardNumber) == false && e.ECOWASCardNumber.Trim().ToLower() == cardNo.Trim().ToLower()
                       && e.Id != model.staffId
                       );
                       return !exist;
                   }
               })
               .WithMessage("ECOWAS Card Number Already Taken");
            }
        }

        internal sealed class Handler : IRequestHandler<UpdateStaffBioRequest, HRM_SK.Shared.Result<string>>
        {
            private readonly IValidator<UpdateStaffBioRequest> _validator;
            private readonly DatabaseContext _dbContext;
            private readonly Authprovider _authProvider;
            private readonly IMapper _mapper;
            private readonly ImageKit _imageKit;

            public Handler(DatabaseContext dbContext, IValidator<UpdateStaffBioRequest> validator, Authprovider authProvider, IMapper mapper, ImageKit imageKit)
            {
                _validator = validator;
                _dbContext = dbContext;
                _authProvider = authProvider;
                _mapper = mapper;
                _imageKit = imageKit;
            }
            public async Task<Result<string>> Handle(UpdateStaffBioRequest request, CancellationToken cancellationToken)
            {
                if (request == null) return HRM_SK.Shared.Result.Failure<string>(new Error(code: "Invalid Request", message: "Invalid Request body"));

                var validationResult = await _validator.ValidateAsync(request);

                if (!validationResult.IsValid)
                {
                    return HRM_SK.Shared.Result.Failure<string>(Error.ValidationError(validationResult));
                }

                var staffCurrentData = await _dbContext.Staff.FirstOrDefaultAsync(entry => entry.Id == request.staffId);

                if (staffCurrentData is null)
                {
                    return HRM_SK.Shared.Result.Failure<string>(Error.CreateNotFoundError("Staff Was Not Found"));
                }

                using (var transactions = await _dbContext.Database.BeginTransactionAsync())
                {
                    try
                    {
                        staffCurrentData.nationality = request.nationality;
                        staffCurrentData.firstName = request.firstName;
                        staffCurrentData.lastName = request.lastName;
                        staffCurrentData.email = request.email;
                        staffCurrentData.phone = request.phone;
                        staffCurrentData.gender = request.gender;
                        staffCurrentData.SNNITNumber = request.SNNITNumber;
                        staffCurrentData.disability = request.disability;
                        staffCurrentData.title = request.title;
                        staffCurrentData.ECOWASCardNumber = request.ECOWASCardNumber;
                        staffCurrentData.GPSAddress = request.GPSAddress;
                        //staffCurrentData.dateOfBirth = request.dateOfBirth;
                        staffCurrentData.updatedAt = DateTime.UtcNow;
                        staffCurrentData.isApproved = true;
                        staffCurrentData.staffIdentificationNumber = request.staffIdentificationNumber;

                        if (request.passportPicture is not null)
                        {
                            var imageUpladStatus = staffCurrentData.passportPicture == null
                                ? await _imageKit.HandleNewFormFileUploadAsync(request.passportPicture)
                                : await _imageKit.ReplaceFileAsync(staffCurrentData.passportPicture, request.passportPicture);

                            if (imageUpladStatus.thumbnailUrl is not null)
                            {
                                staffCurrentData.passportPicture = imageUpladStatus.thumbnailUrl;
                            }
                        }
                        else
                        {
                            if (staffCurrentData.passportPicture != null && request.passportPicture == null)
                            {
                                await _imageKit.DeleteFileAsync(staffCurrentData.passportPicture);
                            }
                            staffCurrentData.passportPicture = null;
                        }

                        _dbContext.Staff.Update(staffCurrentData);

                        var bioUpdateHistory = _mapper.Map<StaffBioUpdateHistory>(staffCurrentData);
                        bioUpdateHistory.staffId = staffCurrentData.Id;

                        _dbContext.StaffBioUpdateHistory.Add(bioUpdateHistory);

                        await _dbContext.SaveChangesAsync();
                        await transactions.CommitAsync();
                        return HRM_SK.Shared.Result.Success("staff Bio Updated Successfully");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                        await transactions.RollbackAsync();
                        return HRM_SK.Shared.Result.Failure<string>(Error.BadRequest(ex.Message));

                    }
                }
            }
        }
    }
}

public class MapUpdateStaffioEndpint(IMapper mapper) : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPatch("api/staff/{staffId}",
            async (HttpContext context, Guid staffId, ISender sender) =>
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

                var requestModel = new UpdateStaffBioRequest
                {
                    staffId = staffId,
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
            }
        )
        .Accepts<AddStaffBioRequest>("multipart/form-data")
        .WithTags("Staff Management")
        .DisableAntiforgery()
        .WithGroupName(SwaggerEndpointDefintions.Planning)
        ;
    }
}
