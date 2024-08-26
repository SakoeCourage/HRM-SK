using Carter;
using HRM_SK.Database;
using HRM_SK.Entities;
using HRM_SK.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static App_Setup.Grade.GetSpecialityFromGradeId;

namespace App_Setup.Grade
{
    public static class GetSpecialityFromGradeId
    {
        public class GetSpecialityFromGradeIdRequest : IRequest<HRM_SK.Shared.Result<ICollection<HRM_SK.Entities.Speciality>>>
        {
            public Guid gradeId { get; set; }
        }

        internal sealed class Handler(DatabaseContext dbContext) : IRequestHandler<GetSpecialityFromGradeIdRequest, HRM_SK.Shared.Result<ICollection<HRM_SK.Entities.Speciality>>>
        {
            async Task<Result<ICollection<Speciality>>> IRequestHandler<GetSpecialityFromGradeIdRequest, Result<ICollection<Speciality>>>.Handle(GetSpecialityFromGradeIdRequest request, CancellationToken cancellationToken)
            {
                Console.WriteLine(request.gradeId);

                var grade = await dbContext.Grade
                    .FirstOrDefaultAsync(g => g.Id == request.gradeId);


                if (grade == null)
                {
                    return HRM_SK.Shared.Result.Failure<ICollection<HRM_SK.Entities.Speciality>>(Error.CreateNotFoundError("Grade Not Found"));
                }

                var response = await dbContext.Speciality.Where(s => s.categoryId == grade.categoryId).ToListAsync(cancellationToken);

                return HRM_SK.Shared.Result.Success<ICollection<HRM_SK.Entities.Speciality>>(response);

            }
        }
    }
}

public class MapGetSpecialityFromGradeId : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("api/grade/speciality-from/{gradeId}/all", async (ISender sender, Guid gradeId) =>
        {
            var response = await sender.Send(
                    new GetSpecialityFromGradeIdRequest
                    {
                        gradeId = gradeId
                    }
                );

            if (response.IsFailure)
            {
                return Results.UnprocessableEntity(response.Error);
            }

            if (response.IsSuccess)
            {
                return Results.Ok(response.Value);
            };

            return Results.BadRequest();

        }).WithTags("Setup-Staff-Speciality")
               .WithDescription("Get Specialities list from grade id")
              .WithMetadata(new ProducesResponseTypeAttribute(typeof(Guid), StatusCodes.Status200OK))
              .WithMetadata(new ProducesResponseTypeAttribute(typeof(Error), StatusCodes.Status400BadRequest));
    }
}