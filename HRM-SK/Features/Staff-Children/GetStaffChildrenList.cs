using Carter;
using HRM_SK.Database;
using HRM_SK.Extensions;
using HRM_SK.Features.Staff_Children;
using HRM_SK.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static HRM_SK.Features.Staff_Children.GetStaffChildrenList;

namespace HRM_SK.Features.Staff_Children
{
    public class staffChildDto
    {
        public Guid Id { get; set; }
        public DateTime createdAt { get; set; } = DateTime.UtcNow;
        public DateTime? updatedAt { get; set; } = DateTime.UtcNow;
        public string childName { get; set; } = string.Empty;
        public DateOnly dateOfBirth { get; set; }
        public string gender { get; set; } = string.Empty;
    }
    public class GetStaffChildrenList
    {
        public class GetStaffchildrenRequest : IRequest<Result<List<staffChildDto>>>
        {
            public Guid staffId { get; set; }
        }

        internal sealed class Handler(DatabaseContext dbContext) : IRequestHandler<GetStaffchildrenRequest, Result<List<staffChildDto>>>
        {
            public async Task<Result<List<staffChildDto>>> Handle(GetStaffchildrenRequest request, CancellationToken cancellationToken)
            {
                var childrenList = await dbContext.StaffChildrenDetail.Where(child => child.staffId == request.staffId)
                    .Select(entry => new staffChildDto
                    {
                        Id = entry.Id,
                        createdAt = entry.createdAt,
                        updatedAt = entry.updatedAt,
                        childName = entry.childName,
                        gender = entry.gender,
                        dateOfBirth = entry.dateOfBirth
                    })
                    .ToListAsync();

                return Shared.Result.Success(childrenList);

            }
        }
    }
}

public class MapGetChildrenRecordEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("api/children/{staffId}", async (ISender sender, Guid staffId) =>
        {
            var response = await sender.Send(new GetStaffchildrenRequest
            {
                staffId = staffId

            });

            if (response.IsFailure)
            {
                return Results.NotFound(response.Error);
            }

            return Results.Ok(response?.Value);

        }).WithTags("Staff Children Record")
              .WithMetadata(new ProducesResponseTypeAttribute(typeof(List<staffChildDto>), StatusCodes.Status200OK))
              .WithMetadata(new ProducesResponseTypeAttribute(typeof(Error), StatusCodes.Status400BadRequest))
              .WithGroupName(SwaggerEndpointDefintions.Planning)
          ;
    }
}