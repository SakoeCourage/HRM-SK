using AutoMapper;
using Carter;
using HRM_SK.Database;
using HRM_SK.Extensions;
using HRM_SK.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static HRM_SK.Contracts.StaffContracts;
using static HRM_SK.Features.Staff_Bank.GetStaffBank;

namespace HRM_SK.Features.Staff_Bank
{
    public class GetStaffBank
    {
        public class GetStaffBankRequest : IRequest<Result<StaffBankResponseDto>>
        {
            public Guid staffId { get; set; }
        }

        internal sealed class Handler(DatabaseContext dbContext, IMapper mapper) : IRequestHandler<GetStaffBankRequest, Result<StaffBankResponseDto>>
        {
            public async Task<Result<StaffBankResponseDto>> Handle(GetStaffBankRequest request, CancellationToken cancellationToken)
            {
                var staffBankData = await dbContext
                    .StaffBankDetail
                    .Where(bank => bank.staffId == request.staffId)
                    .FirstOrDefaultAsync();

                if (staffBankData is null)
                {
                    return Shared.Result.Success<StaffBankResponseDto>(null);
                }

                var response = mapper.Map<StaffBankResponseDto>(staffBankData);

                return Shared.Result.Success<StaffBankResponseDto>(response);
            }
        }
    }

    public class MapGetBankRequestEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("api/staff-bank/{staffId}",
            async (ISender sender, Guid staffId) =>
            {
                var response = await sender.Send(new GetStaffBankRequest { staffId = staffId });

                if (response.IsSuccess)
                {
                    return Results.Ok(response.Value);
                }

                if (response.IsFailure)
                {
                    return Results.UnprocessableEntity(response.Error);
                }

                return Results.BadRequest("Something Went Wrong");

            }).WithMetadata(new ProducesResponseTypeAttribute(typeof(StaffBankResponseDto), StatusCodes.Status200OK))
                .WithTags("Staff Bank Record")
                .WithGroupName(SwaggerEndpointDefintions.Planning)
                ;
        }
    }

}
