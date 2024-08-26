using Carter;
using HRM_SK.Database;
using HRM_SK.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;
using static HRM_SK.Features.Staff_Appointment.DeleteStaffAppointment;

namespace HRM_SK.Features.Staff_Appointment
{
    public static class DeleteStaffAppointment
    {
        public class DeleteStaffAppointmentRequest : IRequest<Result<string>>
        {
            public Guid StaffId { get; set; }
        }

        internal sealed class Handler(DatabaseContext _dbContext) : IRequestHandler<DeleteStaffAppointmentRequest, Result<string>>
        {
            public async Task<Result<string>> Handle(DeleteStaffAppointmentRequest request, CancellationToken cancellationToken)
            {
                var affectedRows = await _dbContext
                    .StaffAppointment
                    .Where(stap => stap.staffId == request.StaffId)
                    .ExecuteDeleteAsync(cancellationToken);

                if (affectedRows == 0)
                {
                    return Shared.Result.Failure<string>(Error.CreateNotFoundError("Appointment Data Was Not Found"));
                }

                return Shared.Result.Success<string>("Saff Appointment Data Has Been Deleted");
            }
        }

    }
}


public class MapDeleteStaffAppointmentEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("api/staff-appointment/{staffId}", async (ISender sender, Guid staffId) =>
        {
            var result = await sender.Send(new DeleteStaffAppointmentRequest
            {
                StaffId = staffId
            });

            if (result.IsSuccess)
            {
                return Results.Ok(result.Value);
            }

            if (result.IsFailure)
            {
                return Results.BadRequest(result?.Error);
            }

            return Results.BadRequest();
        }).WithTags("Staff-Appointment");
    }
}

