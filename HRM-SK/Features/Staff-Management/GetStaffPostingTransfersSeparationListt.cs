using Carter;
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
using static HRM_SK.Contracts.UrlNavigation;
using static HRM_SK.Features.Staff_Management.GetStaffPostingTransfersSeparationListt;

namespace HRM_SK.Features.Staff_Management
{
    public class GetStaffPostingTransfersSeparationListt
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public enum TransferListFilters
        {
            SEPARATED,
            TRANSFERED
        }

        public class GetStaffPostingTransfersSeparationListtRequest : IFilterableSortableRoutePageParam, IRequest<Result<object>>
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
            public string? status { get; set; }

            /// <summary>
            ///  Filter to apply to staff list where has not the valid filters:
            /// 'BankDetail', 'CurrentAppointment', 'FamilyDetail', 
            /// 'ProfessionalLicense', 'StaffChildren', 'StaffAccommodation'.
            /// </summary>
            public string? filter { get; set; }
        }

        internal sealed class Handler(DatabaseContext dbContext) : IRequestHandler<GetStaffPostingTransfersSeparationListtRequest, Result<object>>
        {
            public async Task<Result<object>> Handle(GetStaffPostingTransfersSeparationListtRequest request, CancellationToken cancellationToken)
            {
                var query = dbContext
                  .Staff
                  .Include(st => st.staffPosting)
                  .ThenInclude(sp => sp.unit)
                  .ThenInclude(sp => sp.department)
                  .ThenInclude(sp => sp.directorate)
                  .Include(st => st.separation)
                  .Include(st => st.transferHistory)
                  .AsSplitQuery();

                query = query.Where(st => st.transferHistory != null && st.transferHistory.Count() > 1);

                if (request.departmentId is not null)
                {
                    query = query.Where(entry => entry.staffPosting != null && entry.staffPosting.departmentId == request.departmentId);
                }

                if (request.unitId is not null)
                {
                    query = query.Where(entry => entry.staffPosting != null && entry.staffPosting.unitId == request.unitId);
                }

                if (request.directorateId is not null)
                {
                    query = query.Where(entry => entry.staffPosting != null && entry.staffPosting.directorateId == request.directorateId);
                }

                if (String.IsNullOrWhiteSpace(request.status) is false)
                {
                    query = query.Where(entry => entry.status == request.status);
                }

                var transferFilter = new Dictionary<string, Expression<Func<Staff, bool>>>
                {
                { "SEPARATED", entry => entry.staffPosting != null && entry.staffPosting.postingOption == "external"  },
                { "TRANSFERED", entry => entry.staffPosting != null && entry.staffPosting.postingOption == "internal"  }
                };

                if (request.filter != null && transferFilter.TryGetValue(request.filter, out var filterExpression))
                {
                    query = query.Where(filterExpression);
                }

                if (request.search is not null)
                {
                    query = query.Where(u =>
                    EF.Functions.Like(u.staffIdentificationNumber, $"%{request.search}%")
                    || EF.Functions.Like(u.firstName, $"%{request.search}%")
                    || EF.Functions.Like(u.lastName, $"%{request.search}%")
                     || EF.Functions.Like(u.email, $"%{request.search}%")
                    );
                }

                var queryBuilder = new QueryBuilder<Staff>(query.AsSingleQuery())
                        .WithSort(request?.sort)
                        .Paginate(request?.pageNumber, request?.pageSize);

                var response = await queryBuilder.BuildAsync((staff) =>
                    new StaffPostingListResponseDto
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
                        isSeparated = staff.separation != null ? true : false,
                        hasTransfers = staff.transferHistory.Count() > 1 ? true : false,
                        transferCount = staff.transferHistory.Count()

                    });


                return HRM_SK.Shared.Result.Success(response);
            }
        }
    }
}
public class GetStaffPostingTransfersSeparationListtEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("api/staff/posting-transfer/all", async (ISender sender,
            [FromQuery] int? pageNumber,
            [FromQuery] int? pageSize,
            [FromQuery] StaffStatus? status,
            [FromQuery] string? search,
            [FromQuery] string? sort,
            [FromQuery] TransferListFilters? filter,
            [FromQuery] Guid? departmentId,
            [FromQuery] Guid? directorateId,
            [FromQuery] Guid? unitId) =>
        {

            var response = await sender.Send(new GetStaffPostingTransfersSeparationListtRequest
            {
                pageSize = pageSize,
                pageNumber = pageNumber,
                search = search,
                sort = sort,
                filter = filter.ToString(),
                status = status.ToString(),
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
          .WithMetadata(new ProducesResponseTypeAttribute(typeof(Paginator.PaginatedData<StaffPostingListResponseDto>), StatusCodes.Status200OK))
          .WithTags("Staff-Posting-Transfer");
    }

}
