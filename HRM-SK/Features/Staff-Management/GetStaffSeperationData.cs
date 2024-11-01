using Carter;
using HRM_BACKEND_VSA.Domains.Staffs.Staff_Bio;
using HRM_SK.Database;
using HRM_SK.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static HRM_SK.Features.Staff_Management.GetStaffSeperationData;

namespace HRM_SK.Features.Staff_Management
{
    public class GetStaffSeperationData
    {
        public class GetStaffSeperationDataRequest : IRequest<HRM_SK.Shared.Result<List<staffTransferHistoryDto>>>
        {
            public string staffIdentificationNumber { get; set; }
        }

        internal sealed class Handler(DatabaseContext dbContext) : IRequestHandler<GetStaffSeperationDataRequest, HRM_SK.Shared.Result<List<staffTransferHistoryDto>>>
        {
            public async Task<Result<List<staffTransferHistoryDto>>> Handle(GetStaffSeperationDataRequest request, CancellationToken cancellationToken)
            {
                var existingStaff = await dbContext.Staff
                    .Where(st => st.staffIdentificationNumber == request.staffIdentificationNumber)
                    .FirstOrDefaultAsync();

                if (existingStaff is null)
                {
                    return HRM_SK.Shared.Result.Failure<List<staffTransferHistoryDto>>(Error.CreateNotFoundError("Staff data was not found"));
                };

                var response = await dbContext
                    .StaffPostingHistory
                    .Where(entry => entry.staffId == existingStaff.Id)
                    .Include(entry => entry.department)
                    .Include(entry => entry.directorate)
                    .Include(entry => entry.unit)
                    .OrderByDescending(entry => entry.createdAt)
                    .IgnoreAutoIncludes()
                    .Select(entry => new staffTransferHistoryDto
                    {
                        Id = entry.Id,
                        staffId = entry.staffId,
                        createdAt = entry.createdAt,
                        updatedAt = entry.updatedAt,
                        postingOption = entry.postingOption,
                        postingDate = entry.postingDate,
                        unit = entry.unit != null ? new UnitDto
                        {
                            id = entry.unit.Id,
                            unitName = entry.unit.unitName
                        } : null,
                        department = entry.department != null ? new DepartmentDto
                        {
                            id = entry.department.Id,
                            departmentName = entry.department.departmentName
                        } : null,
                        directorate = entry.directorate != null ? new DirectorateDto
                        {
                            id = entry.directorate.Id,
                            directorateName = entry.directorate.directorateName
                        } : null,
                    })
                    .ToListAsync()

                    ;

                return Shared.Result<List<staffTransferHistoryDto>>.Success(response);
            }
        }
    }
}
public class GetStaffSeperationDataRequestEnpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("api/staff-transfer-history/{staffIdentificationNumber}", async (ISender sender, string staffIdentificationNumber) =>
        {
            var result = await sender.Send(new GetStaffSeperationDataRequest
            {
                staffIdentificationNumber = staffIdentificationNumber
            });

            if (result.IsFailure)
            {
                return Results.NotFound(result?.Error);
            }
            if (result.IsSuccess)
            {
                return Results.Ok(result?.Value);
            }

            return Results.BadRequest();
        }).WithMetadata(new ProducesResponseTypeAttribute(typeof(Error), StatusCodes.Status400BadRequest))
          .WithMetadata(new ProducesResponseTypeAttribute(typeof(List<staffTransferHistoryDto>), StatusCodes.Status200OK))
          .WithTags("Staff-Posting-Transfer")
            ;
    }
}
