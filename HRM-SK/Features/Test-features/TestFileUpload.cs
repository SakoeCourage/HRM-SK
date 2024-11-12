using Carter;
using HRM_SK.Extensions;
using HRM_SK.Serivices.ImageKit;
using MediatR;

namespace HRM_SK.Features.Test_features
{
    public static class TestFileUpload
    {
        public class TestFileUploadRequest : IRequest<Shared.Result>
        {
            public IFormFile image { get; set; }
        }

        internal sealed class Handler : IRequestHandler<TestFileUploadRequest, Shared.Result>
        {

            private readonly ImageKit _imageKit;
            public Handler(ImageKit imageKit)
            {
                _imageKit = imageKit;
            }
            public async Task<Shared.Result> Handle(TestFileUploadRequest request, CancellationToken cancellationToken)
            {
                var response = await _imageKit.HandleNewFormFileUploadAsync(request.image);

                return Shared.Result.Success("done");
            }
        }
    }
    public class MapUploadFileUrlEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("api/test/file-upload", async (ISender sender, IFormFile image) =>
            {
                await sender.Send(new TestFileUpload.TestFileUploadRequest { image = image });
                return Results.Ok("");
            }).WithTags("Test Features")
            .DisableAntiforgery()
            .WithGroupName(SwaggerEndpointDefintions.TestFeature)
            ;
        }
    }
}
