using AutoMapper;
using Carter;
using HRM_SK.Database;
using HRM_SK.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static HRM_SK.Contracts.StaffContracts;
using static HRM_SK.Features.Staff_Accomodation.GetStaffAccomodation;

namespace HRM_SK.Features.Staff_Accomodation
{
    public class GetStaffAccomodation
    {
        public class GetStaffAccomodationRequest : IRequest<Result<staffAccomodationResponseDto>>
        {
            public Guid staffId { get; set; }
        }

        internal sealed class Handler(DatabaseContext dbContext, IMapper mapper) : IRequestHandler<GetStaffAccomodationRequest, Result<staffAccomodationResponseDto>>
        {
            public async Task<Result<staffAccomodationResponseDto>> Handle(GetStaffAccomodationRequest request, CancellationToken cancellationToken)
            {
                var accommodationData = await dbContext
                    .StaffAccomodationDetail
                    .Where(stacc => stacc.staffId == request.staffId)
                    .FirstOrDefaultAsync();

                if (accommodationData is null)
                {
                    return Shared.Result.Success<staffAccomodationResponseDto>(null);
                }

                var response = mapper.Map<staffAccomodationResponseDto>(accommodationData);

                return Shared.Result.Success<staffAccomodationResponseDto>(response);
            }
        }
    }

    public class MapGetStaffAccommodationEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("api/staff-accommodation/{staffId}", async (ISender sender, Guid staffId) =>
            {
                var response = await sender.Send(new GetStaffAccomodationRequest { staffId = staffId });

                if (response.IsFailure)
                {
                    return Results.NotFound(response.Error);
                }

                return Results.Ok(response?.Value);

            }).WithTags("Staff Accommodation Record")
                  .WithMetadata(new ProducesResponseTypeAttribute(typeof(staffAccomodationResponseDto), StatusCodes.Status200OK))
                  .WithMetadata(new ProducesResponseTypeAttribute(typeof(Error), StatusCodes.Status400BadRequest))
              ;
        }
    }
}
