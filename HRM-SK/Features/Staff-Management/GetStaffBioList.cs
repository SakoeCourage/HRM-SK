﻿using Carter;
using HRM_BACKEND_VSA.Domains.Staffs.Staff_Bio;
using HRM_SK.Contracts;
using HRM_SK.Database;
using HRM_SK.Entities.Staff;
using HRM_SK.Shared;
using HRM_SK.Utilities;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Text.Json.Serialization;
using static HRM_BACKEND_VSA.Domains.Staffs.Staff_Bio.GetStaffBioList;
using static HRM_SK.Contracts.UrlNavigation;

namespace HRM_BACKEND_VSA.Domains.Staffs.Staff_Bio
{
    public static class StaffListFilters
    {
        public const string bankDetail = "BANKDETAIL";
        public const string currentAppointment = "CURRENTAPPOINTMENT";
        public const string familyDetail = "FAMILYDETAIL";
        public const string professionalLincense = "PROFESSIONALLICENSE";
        public const string staffChildren = "STAFFCHILDREN";
        public const string staffAccomodation = "STAFFACCOMMODATION";
        public const string staffPosting = "STAFFPOSTING";
    }


    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum StaffListFilter
    {
        BANKDETAIL,
        CURRENTAPPOINTMENT,
        FAMILYDETAIL,
        PROFESSIONALLICENSE,
        STAFFCHILDREN,
        STAFFACCOMMODATION,
        STAFFPOSTING
    }

    public static class GetStaffBioList
    {
        public class GetStaffBioListListRequest : IFilterableSortableRoutePageParam, IRequest<Result<object>>
        {
            /// <summary>
            /// Search by 'StaffIdentification Number', 'First Name', 'Last Name', 'Email'
            /// </summary>
            public string? search { get; set; }
            public string? sort { get; set; }
            public int? pageSize { get; set; }
            public int? pageNumber { get; set; }
            public Guid? directorateId { get; set; }
            public Guid? departmentId { get; set; }
            public Guid? unitId { get; set; }

            /// <summary>
            ///  Filter to apply to staff list where has not the valid filters:
            /// 'BankDetail', 'CurrentAppointment', 'FamilyDetail', 
            /// 'ProfessionalLicense', 'StaffChildren', 'StaffAccommodation'.
            /// </summary>
            public string? filter { get; set; }
        }

        public class Handler : IRequestHandler<GetStaffBioListListRequest, Result<object>>
        {
            private readonly DatabaseContext _dBContext;
            public Handler(DatabaseContext dbContext)
            {
                _dBContext = dbContext;
            }
            public async Task<Result<object>> Handle(GetStaffBioListListRequest request, CancellationToken cancellationToken)
            {
                var query = _dBContext
                    .Staff
                    .Include(st => st.currentAppointment)
                    .ThenInclude(st => st.speciality)
                    .Include(st => st.bankDetail)
                    .Include(st => st.familyDetail)
                    .Include(st => st.staffChildren)
                    .Include(st => st.professionalLincense)
                    .Include(st => st.staffAccomodation)
                    .Include(st => st.staffPosting)
                    .ThenInclude(sp => sp.unit)
                    .ThenInclude(sp => sp.department)
                    .ThenInclude(sp => sp.directorate)
                    .AsQueryable();

                var filterDictionary = new Dictionary<string, Expression<Func<Staff, bool>>>
                {
                { StaffListFilters.bankDetail, entry => entry.bankDetail == null },
                { StaffListFilters.currentAppointment, entry => entry.currentAppointment == null },
                { StaffListFilters.familyDetail, entry => entry.familyDetail == null },
                { StaffListFilters.staffChildren, entry => entry.staffChildren.Count() == 0 },
                { StaffListFilters.staffAccomodation, entry => entry.staffAccomodation == null },
                { StaffListFilters.staffPosting, entry => entry.staffPosting == null }
                };

                if (request.filter != null && filterDictionary.TryGetValue(request.filter, out var filterExpression))
                {
                    query = query.Where(filterExpression);
                }


                if (request.departmentId is not null)
                {
                    query = query.Where(entry => entry.staffPosting.departmentId == request.departmentId);
                }

                if (request.unitId is not null)
                {
                    query = query.Where(entry => entry.staffPosting.unitId == request.unitId);
                }

                if (request.directorateId is not null)
                {
                    query = query.Where(entry => entry.staffPosting.directorateId == request.directorateId);
                }

                if (String.IsNullOrWhiteSpace(request?.search) is false)
                {
                    query = query.Where(u =>
                    EF.Functions.Like(u.staffIdentificationNumber, $"%{request.search}%")
                    || EF.Functions.Like(u.firstName, $"%{request.search}%")
                    || EF.Functions.Like(u.lastName, $"%{request.search}%")
                     || EF.Functions.Like(u.email, $"%{request.search}%")
                    );
                }

                var queryBuilder = new QueryBuilder<Staff>(query)
                        .WithSort(request?.sort)
                        .Paginate(request?.pageNumber, request?.pageSize);

                var response = await queryBuilder.BuildAsync((staff) =>
                new StaffListResponseDto
                {
                    Id = staff.Id,
                    createdAt = staff.createdAt,
                    updatedAt = staff.updatedAt,
                    lastSeen = staff.lastSeen,
                    title = staff.title,
                    GPSAddress = staff.GPSAddress,
                    staffIdentificationNumber = staff.staffIdentificationNumber,
                    firstName = staff.firstName,
                    lastName = staff.lastName,
                    otherNames = staff.otherNames,
                    dateOfBirth = staff.dateOfBirth,
                    phone = staff.phone,
                    gender = staff.gender,
                    SNNITNumber = staff.SNNITNumber,
                    email = staff.email,
                    disability = staff.disability,
                    passportPicture = staff.passportPicture,
                    ECOWASCardNumber = staff.ECOWASCardNumber,
                    status = staff.status,
                    isApproved = staff.isApproved,
                    isAlterable = staff.isAlterable,
                    unit = (staff?.staffPosting != null) ? new UnitDto
                    {
                        id = staff.staffPosting.unit.Id,
                        unitName = staff.staffPosting.unit.unitName
                    } : null,
                    directorate = (staff?.staffPosting != null) ? new DirectorateDto
                    {
                        id = staff.staffPosting.unit.directorate.Id,
                        directorateName = staff.staffPosting.unit.directorate.directorateName
                    } : null,
                    department = (staff?.staffPosting != null) ? new DepartmentDto
                    {
                        id = staff.staffPosting.unit.department.Id,
                        departmentName = staff.staffPosting.unit.department.departmentName
                    } : null,
                    speciality = (staff?.currentAppointment != null) ? new SpecialityDto
                    {
                        id = staff.currentAppointment.speciality.Id,
                        categoryId = staff.currentAppointment.speciality.categoryId,
                        createdAt = staff.currentAppointment.speciality.createdAt,
                        updatedAt = staff.currentAppointment.speciality.updatedAt,
                        specialityName = staff.currentAppointment.speciality.specialityName

                    } : null,
                    hasBankDetail = staff.bankDetail != null,
                    hasFamilyDetail = staff.familyDetail != null,
                    hasProfessionalLincenseDetail = staff.professionalLincense != null,
                    hasChildrenDetail = staff.staffChildren?.Any() == true,
                    hasStaffAccomodationDetail = staff.staffAccomodation != null,
                    hasCurrentAppointmentDetail = staff.currentAppointment != null,
                    hasPostingDetail = staff.staffPosting != null,
                });

                return HRM_SK.Shared.Result.Success(response);
            }
        }

    }
}

public class GetStaffBioListListEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("api/staff/all", async (ISender sender, [FromQuery] int? pageNumber, [FromQuery] int? pageSize,
            [FromQuery] string? search, [FromQuery] string? sort, [FromQuery] StaffListFilter? filter,
            [FromQuery] Guid? departmentId, [FromQuery] Guid? directorateId, [FromQuery] Guid? unitId) =>
        {

            var response = await sender.Send(new GetStaffBioListListRequest
            {
                pageSize = pageSize,
                pageNumber = pageNumber,
                search = search,
                sort = sort,
                filter = filter.ToString(),
                departmentId = departmentId,
                unitId = unitId,
                directorateId = directorateId
            });

            if (response is null)
            {
                return Results.BadRequest("Empty Result");
            }

            if (response.IsSuccess)
            {
                return Results.Ok(response.Value);
            }


            return Results.BadRequest("Empty Result");
        }).WithMetadata(new ProducesResponseTypeAttribute(typeof(Error), StatusCodes.Status400BadRequest))
          .WithMetadata(new ProducesResponseTypeAttribute(typeof(Paginator.PaginatedData<StaffListResponseDto>), StatusCodes.Status200OK))
          .WithTags("Staff Management");
    }

}