using AutoMapper;
using Carter;
using HRM_SK.Database;
using HRM_SK.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static HRM_SK.Contracts.StaffContracts;
using static HRM_SK.Features.Staff_Family.GetStaffFamily;

namespace HRM_SK.Features.Staff_Family
{

    public class GetStaffFamily
    {
        public class GetStaffFamilyRequest : IRequest<Result<StaffFamilyResponseDto>>
        {
            public Guid staffId { get; set; }
        }

        internal sealed class Handler(DatabaseContext dbContext, IMapper mapper) : IRequestHandler<GetStaffFamilyRequest, Result<StaffFamilyResponseDto>>
        {
            public async Task<Result<StaffFamilyResponseDto>> Handle(GetStaffFamilyRequest request, CancellationToken cancellationToken)
            {
                var familyData = await dbContext
                    .StaffFamilyDetail
                    .Where(entry => entry.staffId == request.staffId)
                    .FirstOrDefaultAsync();

                if (familyData is null)
                {
                    return Shared.Result.Success<StaffFamilyResponseDto>(null);
                }

                var response = mapper.Map<StaffFamilyResponseDto>(familyData);

                return Shared.Result.Success<StaffFamilyResponseDto>(response);

            }
        }
    }


}

public class MapGetStaffFamilyEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("api/staff-family/{staffId}",
        async (ISender sender, Guid staffId) =>
        {
            var response = await sender.Send(new GetStaffFamilyRequest
            {
                staffId = staffId
            });

            if (response.IsSuccess)
            {
                return Results.Ok(response.Value);
            }

            if (response.IsFailure)
            {
                return Results.UnprocessableEntity(response.Error);
            }

            return Results.BadRequest("Something Went Wrong");

        })
        .WithTags("Staff Family Record")
        .WithMetadata(new ProducesResponseTypeAttribute(typeof(StaffFamilyResponseDto), StatusCodes.Status200OK))
        ;
    }
}
