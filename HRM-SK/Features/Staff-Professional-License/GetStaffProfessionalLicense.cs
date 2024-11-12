using AutoMapper;
using Carter;
using HRM_SK.Database;
using HRM_SK.Extensions;
using HRM_SK.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static HRM_SK.Contracts.StaffContracts;
using static HRM_SK.Features.Staff_Professional_License.GetStaffProfessionalLicense;

namespace HRM_SK.Features.Staff_Professional_License
{
    public class GetStaffProfessionalLicense
    {
        public class GetPLRequest : IRequest<Result<StaffProfessionalLicenseDto>>
        {
            public Guid staffId { get; set; }
        }

        internal sealed class Handler(DatabaseContext dbContext, IMapper mapper) : IRequestHandler<GetPLRequest, Result<StaffProfessionalLicenseDto>>
        {
            public async Task<Result<StaffProfessionalLicenseDto>> Handle(GetPLRequest request, CancellationToken cancellationToken)
            {
                var response = dbContext
                    .StaffProfessionalLincense
                    .Where(entry => entry.staffId == request.staffId)
                    .FirstOrDefaultAsync();

                if (response is null)
                {
                    return Shared.Result.Success<StaffProfessionalLicenseDto>(null);
                }

                var responseData = mapper.Map<StaffProfessionalLicenseDto>(response);

                return Shared.Result.Success<StaffProfessionalLicenseDto>(responseData);
            }
        }
    }

}
public class MapGetStaffProfessionalLicenseEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("api/staff-professional-license/{staffId}",
        async (ISender sender, Guid staffId) =>
        {
            var response = await sender.Send(new GetPLRequest
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
        .WithTags("Staff Professional License Record")
        .WithMetadata(new ProducesResponseTypeAttribute(typeof(StaffProfessionalLicenseDto), StatusCodes.Status200OK))
        .WithGroupName(SwaggerEndpointDefintions.Planning)
        ;
        
    }
}