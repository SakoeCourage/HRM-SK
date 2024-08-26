using AutoMapper;
using Carter;
using FluentValidation;
using HRM_SK.Database;
using HRM_SK.Entities.Staff;
using HRM_SK.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;
using static HRM_SK.Features.Staff_Accomodation.NewAccomodation;

namespace HRM_SK.Features.Staff_Accomodation
{
    public class NewAccomodation
    {
        public class NewAccomodationRequestData : IRequest<Shared.Result<string>>
        {
            public string source { get; set; } = String.Empty;
            public string gpsAddress { get; set; } = String.Empty;
            public string accomodationType { get; set; } = String.Empty;
            public string flatNumber { get; set; } = String.Empty;
            public DateOnly? allocationDate { get; set; }
            public Guid staffId { get; set; }
        }

        public class validator : AbstractValidator<NewAccomodationRequestData>
        {
            public validator()
            {
                RuleFor(c => c.staffId).NotEmpty();
                RuleFor(c => c.source).NotEmpty();
                RuleFor(c => c.gpsAddress).NotEmpty();
                RuleFor(c => c.accomodationType).NotEmpty();
            }

            internal sealed class Handler(DatabaseContext dbContext, IValidator<NewAccomodationRequestData> validator, IMapper mapper) : IRequestHandler<NewAccomodationRequestData, Shared.Result<string>>
            {
                public async Task<Result<string>> Handle(NewAccomodationRequestData request, CancellationToken cancellationToken)
                {
                    var validationResult = validator.Validate(request);

                    if (validationResult.IsValid is false)
                    {
                        return Shared.Result.Failure<string>(Error.ValidationError(validationResult));
                    }

                    var staff = await dbContext.Staff.AnyAsync(s => s.Id == request.staffId);

                    if (staff is false)
                    {
                        return Shared.Result.Failure<string>(Error.CreateNotFoundError("Staff Record Not Found"));
                    }

                    var existingData = await dbContext.StaffAccomodationDetail.FirstOrDefaultAsync(s => s.staffId == request.staffId);


                    using (var dbTransaction = await dbContext.Database.BeginTransactionAsync())
                    {
                        try
                        {
                            if (existingData is null)
                            {
                                var newRecord = new StaffAccomodationDetail
                                {
                                    staffId = request.staffId,
                                    source = request.source,
                                    gpsAddress = request.gpsAddress,
                                    accomodationType = request.accomodationType,
                                    allocationDate = request.allocationDate,
                                    flatNumber = request.flatNumber
                                };

                                dbContext.Add(newRecord);

                                var newHistory = mapper.Map<StaffAccomodationUpdateHistory>(newRecord);
                                dbContext.Add(newHistory);

                            }
                            else
                            {
                                existingData.source = request.source;
                                existingData.gpsAddress = request.gpsAddress;
                                existingData.accomodationType = request.accomodationType;
                                existingData.allocationDate = request.allocationDate;
                                existingData.flatNumber = request.flatNumber;
                                existingData.updatedAt = DateTime.UtcNow;

                                dbContext.Update(existingData);
                                var newHistory = mapper.Map<StaffAccomodationUpdateHistory>(existingData);
                                dbContext.Add(newHistory);
                            }

                            await dbContext.SaveChangesAsync();
                            await dbTransaction.CommitAsync();

                            return Shared.Result.Success("Staff Accomodation Record Saved");

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

    public class MapAddOrUpdateAccodationRecordEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("api/staff-accommodation/update-or-new",
            async (ISender sender, NewAccomodationRequestData request) =>
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

                return Results.BadRequest("Something Went Wrong");

            }).WithTags("Staff Accommodation Record");
        }
    }
}
