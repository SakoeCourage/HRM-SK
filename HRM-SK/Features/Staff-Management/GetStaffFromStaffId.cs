using AutoMapper;
using Carter;
using HRM_BACKEND_VSA.Domains.Staffs.Staff_Bio;
using HRM_SK.Database;
using HRM_SK.Entities.Staff;
using HRM_SK.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static HRM_BACKEND_VSA.Domains.Staffs.GetStaffData.GetStaffFromStaffId;

namespace HRM_BACKEND_VSA.Domains.Staffs.GetStaffData
{

    public static class GetStaffFromStaffId
    {
        public class GetStaffFromIdRequest : IRequest<HRM_SK.Shared.Result<StaffDto>>
        {
            public string staffIdentificationNumber { get; set; }
        }
        internal sealed class Handler(DatabaseContext dbContext, IMapper mapper) : IRequestHandler<GetStaffFromIdRequest, HRM_SK.Shared.Result<StaffDto>>
        {
            public async Task<Result<StaffDto>> Handle(GetStaffFromIdRequest request, CancellationToken cancellationToken)
            {
                var response = await dbContext.Staff
                .Where(s => s.staffIdentificationNumber == request.staffIdentificationNumber)
                .Include(s => s.staffPosting)
                .ThenInclude(s => s.department)
                .ThenInclude(s => s.directorate)
                .ThenInclude(s => s.units)
                .Include(s => s.currentAppointment)
                .ThenInclude(s => s.speciality)
                .ThenInclude(s => s.category)
                .Include(s => s.currentAppointment)
                .ThenInclude(ap => ap.grade)
                .Include(s => s.appointmentHistory)
                .ThenInclude(sah => sah.grade)
                .Select(s => new StaffDto
                {
                    id = s.Id,
                    createdAt = s.createdAt,
                    updatedAt = s.updatedAt,
                    title = s.title,
                    gpsAddress = s.GPSAddress,
                    staffIdentificationNumber = s.staffIdentificationNumber,
                    firstName = s.firstName,
                    lastName = s.lastName,
                    otherNames = s.otherNames,
                    dateOfBirth = s.dateOfBirth,
                    phone = s.phone,
                    gender = s.gender,
                    SNNITNumber = s.SNNITNumber,
                    email = s.email,
                    disability = s.disability,
                    passportPicture = s.passportPicture,
                    ECOWASCardNumber = s.ECOWASCardNumber,
                    status = s.status,
                    isApproved = s.isApproved,
                    isAlterable = s.isAlterable,
                    staffPosting = s.staffPosting != null ? new StaffPostingDto
                    {
                        id = s.staffPosting.Id,
                        staffId = s.staffPosting.staffId,
                        createdAt = s.staffPosting.createdAt,
                        updatedAt = s.staffPosting.updatedAt,
                        directorateId = s.staffPosting.directorateId,
                        unitId = s.staffPosting.unitId,
                        departmentId = s.staffPosting.departmentId,
                        postingDate = s.staffPosting.postingDate
                    } : null,
                    unit = s.staffPosting != null ? new UnitDto
                    {
                        id = s.staffPosting.unit.Id,
                        unitName = s.staffPosting.unit.unitName
                    } : null,
                    department = s.staffPosting != null ? new DepartmentDto
                    {
                        id = s.staffPosting.department.Id,
                        departmentName = s.staffPosting.department.departmentName
                    } : null,
                    directorate = s.staffPosting != null ? new DirectorateDto
                    {
                        id = s.staffPosting.directorate.Id,
                        directorateName = s.staffPosting.directorate.directorateName
                    } : null,
                    category = s.currentAppointment.speciality != null ? new CategoryDto
                    {
                        id = s.currentAppointment.speciality.category.Id,
                        createdAt = s.currentAppointment.speciality.category.createdAt,
                        updatedAt = s.currentAppointment.speciality.category.updatedAt,
                        categoryName = s.currentAppointment.speciality.category.categoryName
                    } : null,
                    speciality = s.currentAppointment != null ? new SpecialityDto
                    {
                        id = s.currentAppointment.speciality.Id,
                        createdAt = s.currentAppointment.speciality.createdAt,
                        updatedAt = s.currentAppointment.speciality.updatedAt,
                        specialityName = s.currentAppointment.speciality.specialityName
                    } : null,
                    currentAppointment = s.currentAppointment,
                    firstAppointment = s.appointmentHistory.Count() > 0 ?
                     s.appointmentHistory
                    .OrderBy(c => c.createdAt)
                    .Select(entry => new StaffAppointment
                    {
                        Id = entry.Id,
                        gradeId = entry.gradeId,
                        staffId = entry.staffId,
                        staffSpecialityId = entry.staffSpecialityId,
                        createdAt = entry.createdAt,
                        updatedAt = entry.updatedAt,
                        appointmentType = entry.appointmentType,
                        staffType = entry.staffType,
                        endDate = entry.endDate,
                        paymentSource = entry.paymentSource,
                        notionalDate = entry.notionalDate,
                        substantiveDate = entry.substantiveDate,
                        step = entry.step,
                        grade = entry.grade
                    }).
                    FirstOrDefault()
                    : null
                })
                 .FirstOrDefaultAsync();

                if (response is null)
                {
                    return HRM_SK.Shared.Result.Failure<StaffDto>(Error.CreateNotFoundError("Staff Was Not Found"));
                }


                return HRM_SK.Shared.Result.Success(response);
            }
        }
    }
}

public class MapGetStaffFromStaffIDRequest : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("api/staff/{staffIdentificationNumber}", async (ISender sender, string staffIdentificationNumber) =>
        {
            var result = await sender.Send(new GetStaffFromIdRequest
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
          .WithMetadata(new ProducesResponseTypeAttribute(typeof(StaffDto), StatusCodes.Status200OK))
          .WithTags("Staff Management")
            ;
    }
}
