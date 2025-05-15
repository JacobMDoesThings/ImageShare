// using Azure;
// using Azure.Storage.Files.DataLake;
// using Azure.Storage.Files.DataLake.Models;
// using ImageShare.Functions.Configuration;
// using ImageShare.Functions.Services;
// using Microsoft.Extensions.Logging;
// using Moq;
// using System.Diagnostics.CodeAnalysis;
// using System.Net;
// using System.Text;
// using Azure.Core;
// using ImageShare.Functions.Data;
// using ImageShare.Functions.Interfaces;
// using Microsoft.AspNetCore.Http;
// using Microsoft.Extensions.Azure;
// using Microsoft.VisualStudio.TestPlatform.Utilities;
// using SixLabors.ImageSharp;
// using SixLabors.ImageSharp.Advanced;
// using SixLabors.ImageSharp.Drawing.Processing;
// using SixLabors.ImageSharp.Formats;
// using SixLabors.ImageSharp.Formats.Jpeg;
// using SixLabors.ImageSharp.PixelFormats;
// using SixLabors.ImageSharp.Processing;
//
// namespace ImageShare.Functions.Tests.Unit;
//
// [TestClass]
// public sealed class ThumbnailGeneratorFunctionTests
// {
//     private Mock<ILogger<ThumbnailGenerationFunction>> _loggerMock;
//     private ThumbnailGenerationFunction _thumbnailGenerationFunction;
//     private ThumbnailGenerationConfiguration _thumbNailConfig;
//     private BlobUrlParser _blobUrlParser;
//     private DataLakeServiceClientFactory _dataLakeServiceClientFactory;
//     private ImageValidator _imageValidator;
//     private readonly string _thumbnailPathPrivate = "privatePath";
//     private readonly string _thumbnailPathPublic = "publicPath";
//     private readonly string _thumbnailConnectionString = "FakeConnectionString";
//     private readonly string[] _thumnailAllowedMimeTypes = ["mime-type1", "mime-type2"];
//
//     [TestInitialize]
//     public void Setup()
//     {
//         // Initialize mocks before each test
//         _loggerMock = new Mock<ILogger<ThumbnailGenerationFunction>>();
//         _thumbNailConfig = new ThumbnailGenerationConfiguration()
//         {
//             AllowedMimeTypes = _thumnailAllowedMimeTypes,
//             ConnectionString = _thumbnailConnectionString,
//             ThumbnailPathPrivate = _thumbnailPathPrivate,
//             ThumbnailPathPublic = _thumbnailPathPublic,
//             MaxHeight = 240,
//             JpegQuality = 50
//         };
//         _blobUrlParser = new BlobUrlParser();
//         _dataLakeServiceClientFactory = new DataLakeServiceClientFactory();
//         _imageValidator = new ImageValidator();
//
//         _thumbnailGenerationFunction = new ThumbnailGenerationFunction(
//             _thumbNailConfig, _blobUrlParser, _dataLakeServiceClientFactory,
//             _imageValidator, _loggerMock.Object);
//     }
//
//     [TestMethod]
//     public async Task test()
//     {
//          // Arrange
//         var mockLogger = new Mock<ILogger<ThumbnailGenerationFunction>>();
//         var logMessages = new List<string>();
//
//         mockLogger.Setup(l => l.Log(
//                 It.Is<LogLevel>(ll => ll == LogLevel.Information),
//                 It.IsAny<EventId>(),
//                 It.IsAny<object>(),
//                 null,
//                 It.IsAny<Func<object, Exception, string>>()!))
//             .Callback((LogLevel logLevel, EventId eventId, object state, Exception exception, Func<object, Exception, string> formatter) =>
//             {
//                 logMessages.Add(state.ToString());
//             });
//
//         var thumbnailFunction = new Mock<ThumbnailGenerationFunction>(
//             _thumbNailConfig, _blobUrlParser, _dataLakeServiceClientFactory,
//             _imageValidator, mockLogger.Object
//             ) { CallBase = true };
//         //
//         // // Setup mocks for helper methods
//         // mockImageProcessor.Setup(m => m.GetSourceFileClient(It.IsAny<DataLakeFileSystemClient>(), It.IsAny<BlobData>()))
//         //     .Returns(Mock.Of<DataLakeFileClient>());
//         //
//         // mockImageProcessor.Setup(m => m.ValidateMimeTypeAsync(It.IsAny<DataLakeFileClient>()))
//         //     .Returns(Task.CompletedTask);
//         //
//         // // mockImageProcessor.Setup(m => m.DownloadFileStreamAsync(It.IsAny<DataLakeFileClient>()))
//         // //     .Returns(Task.FromResult(new MemoryStream()));
//         //
//         // mockImageProcessor.Setup(m => m.ValidateImageStructureAsync(It.IsAny<MemoryStream>()))
//         //     .Returns(Task.CompletedTask);
//         //
//         // mockImageProcessor.Setup(m => m.GenerateThumbnailAsync(It.IsAny<MemoryStream>()))
//         //     .Returns(Task.FromResult(new MemoryStream()));
//         //
//         // mockImageProcessor.Setup(m => m.GetThumbnailFileClient(It.IsAny<DataLakeFileSystemClient>(), It.IsAny<BlobData>()))
//         //     .Returns(Mock.Of<DataLakeFileClient>());
//         //
//         // mockImageProcessor.Setup(m => m.UploadThumbnailAsync(It.IsAny<DataLakeFileClient>(), It.IsAny<MemoryStream>()))
//         //     .Returns(Task.CompletedTask);
//
//         var blobData = new BlobData
//         {
//             BlobUrlString = "test-container/test-file.txt",
//             ContainerName = "test-container",
//             IsPublic = true,
//             Name = "test-file.txt",
//             User = "test-user"
//         };
//         
//         var mockFileSystemClient = Mock.Of<DataLakeFileSystemClient>();
//
//         // Act
//         await thumbnailFunction.Object.ProcessImagePipelineAsync(blobData, mockFileSystemClient);
//
//         // Assert helper methods are called once
//         thumbnailFunction.Verify(m => m.GetSourceFileClient(It.IsAny<DataLakeFileSystemClient>(), It.IsAny<BlobData>()), Times.Once);
//         thumbnailFunction.Verify(m => m.ValidateMimeTypeAsync(It.IsAny<DataLakeFileClient>()), Times.Once);
//         //mockImageProcessor.Verify(m => m.DownloadFileStreamAsync(It.IsAny<DataLakeFileClient>()), Times.Once);
//         thumbnailFunction.Verify(m => m.ValidateImageStructureAsync(It.IsAny<MemoryStream>()), Times.Once);
//         thumbnailFunction.Verify(m => m.GenerateThumbnailAsync(It.IsAny<MemoryStream>()), Times.Once);
//         thumbnailFunction.Verify(m => m.GetThumbnailFileClient(It.IsAny<DataLakeFileSystemClient>(), It.IsAny<BlobData>()), Times.Once);
//         thumbnailFunction.Verify(m => m.UploadThumbnailAsync(It.IsAny<DataLakeFileClient>(), It.IsAny<MemoryStream>()), Times.Once);
//
//         // Assert log messages are correct and in order
//         CollectionAssert.AreEqual(
//             new[] {
//                 "Validating MIME type...",
//                 "Downloading file...",
//                 "Validating image structure...",
//                 "Generating thumbnail...",
//                 "Uploading thumbnail...",
//                 "Processing completed successfully"
//             },
//             logMessages);
//     }
//
//     [TestMethod]
//     public void GetSourceFileClient_ReturnsCorrectFileClient()
//     {
//         // Arrange
//         var blobData = new BlobData
//         {
//             BlobUrlString = "test-container/test-file.txt",
//             ContainerName = "test-container",
//             IsPublic = true,
//             Name = "test-file.txt",
//             User = "test-user"
//         };
//         
//         var mockDataLakeFileSystemClient = new Mock<DataLakeFileSystemClient>();
//         
//         var returnsDataLakeFileClient = _thumbnailGenerationFunction.GetSourceFileClient(mockDataLakeFileSystemClient.Object, blobData);
//         
//         mockDataLakeFileSystemClient.Verify(x => 
//             x.GetFileClient(blobData.BlobUrlString), Times.Once);
//         
//         _loggerMock.Verify(l => l.Log(
//                 LogLevel.Information,
//                 It.IsAny<EventId>(),
//                 It.Is<It.IsAnyType>((v, t) =>
//                     true),
//                 It.IsAny<Exception>(),
//                 It.IsAny<Func<It.IsAnyType, Exception, string>>()!),
//             Times.Once);
//     }
//
//     [TestMethod]
//     public async Task ValidateMimeTypeAsync_Logs_Bad_MimeTypes_Deletes_File()
//     {
//         var properMockDataLakeFileClient = new Mock<DataLakeFileClient>();
//         var improperMockDataLakeFileClient = new Mock<DataLakeFileClient>();
//
//         var fakeProperDataPathProperties = DataLakeModelFactory.PathProperties(
//             DateTimeOffset.Now,
//             DateTimeOffset.Now,
//             new Dictionary<string, string>(),
//             DateTimeOffset.Now, string.Empty, string.Empty,
//             string.Empty, new Uri("https://fakeuri"), CopyStatus.Success, false, DataLakeLeaseDuration.Fixed,
//             DataLakeLeaseState.Available, DataLakeLeaseStatus.Unlocked, 999, _thumnailAllowedMimeTypes[0],
//             new ETag(), new byte[0], string.Empty, string.Empty, string.Empty, string.Empty,
//             string.Empty, false, string.Empty, string.Empty, string.Empty,
//             DateTimeOffset.Now
//         );
//
//         var fakeImproperDataPathProperties = DataLakeModelFactory.PathProperties(
//             DateTimeOffset.Now,
//             DateTimeOffset.Now,
//             new Dictionary<string, string>(),
//             DateTimeOffset.Now, string.Empty, string.Empty,
//             string.Empty, new Uri("https://fakeuri"), CopyStatus.Success, false, DataLakeLeaseDuration.Fixed,
//             DataLakeLeaseState.Available, DataLakeLeaseStatus.Unlocked, 999, "BadMimeType",
//             new ETag(), new byte[0], string.Empty, string.Empty, string.Empty, string.Empty,
//             string.Empty, false, string.Empty, string.Empty, string.Empty,
//             DateTimeOffset.Now
//         );
//
//         properMockDataLakeFileClient.Setup(x =>
//                 x.GetPropertiesAsync(null, CancellationToken.None))
//             .ReturnsAsync(Response.FromValue(fakeProperDataPathProperties, new MockResponse()));
//
//         improperMockDataLakeFileClient.Setup(x =>
//                 x.GetPropertiesAsync(null, CancellationToken.None))
//             .ReturnsAsync(Response.FromValue(fakeImproperDataPathProperties, new MockResponse()));
//
//         var ex = await Assert.ThrowsExceptionAsync<InvalidOperationException>(async () =>
//             await _thumbnailGenerationFunction.ValidateMimeTypeAsync(improperMockDataLakeFileClient.Object));
//
//         Assert.IsNotNull(ex);
//
//         _loggerMock.Verify(l => l.Log(
//                 LogLevel.Warning,
//                 It.IsAny<EventId>(),
//                 It.Is<It.IsAnyType>((v, t) =>
//                     true),
//                 It.IsAny<Exception>(),
//                 It.IsAny<Func<It.IsAnyType, Exception, string>>()!),
//             Times.Once);
//
//         improperMockDataLakeFileClient.Verify(x =>
//             x.DeleteAsync(It.IsAny<DataLakeRequestConditions>(),
//                 It.IsAny<CancellationToken>()), Times.Once);
//
//         await _thumbnailGenerationFunction.ValidateMimeTypeAsync(properMockDataLakeFileClient.Object);
//         _loggerMock.Verify(l => l.Log(
//                 LogLevel.Warning,
//                 It.IsAny<EventId>(),
//                 It.Is<It.IsAnyType>((v, t) =>
//                     true),
//                 It.IsAny<Exception>(),
//                 It.IsAny<Func<It.IsAnyType, Exception, string>>()!),
//             Times.Once);
//     }
//
//     [TestMethod]
//     public async Task DownloadFileStreamAsync_ReturnsCorrectStream()
//     {
//         var mockDataLakeFileClient = new Mock<DataLakeFileClient>();
//         const string data = "test data";
//         var bytes = Encoding.UTF8.GetBytes(data);
//         var stream = new MemoryStream(bytes);
//
//         mockDataLakeFileClient.Setup(x => x.ReadAsync()).ReturnsAsync(Response.FromValue(
//             DataLakeModelFactory.FileDownloadInfo(stream.Length,
//                 content: stream, [], null), new MockResponse()));
//
//         var output = await ThumbnailGenerationFunction.DownloadFileStreamAsync(mockDataLakeFileClient.Object);
//         output.Position = 0;
//         stream.Position = 0;
//
//         var outputBytes = new byte[output.Length];
//         await output.ReadExactlyAsync(outputBytes, 0, outputBytes.Length);
//
//         var streamBytes = new byte[stream.Length];
//         await stream.ReadExactlyAsync(streamBytes, 0, streamBytes.Length);
//
//         CollectionAssert.AreEqual(outputBytes, streamBytes);
//     }
//
//     [TestMethod]
//     public async Task
//         Test_ValidateImageStructure_Log_Warning_On_Validation_Failure_And_Throws_Calls_ImageValidator_Validate()
//     {
//         // Arrange
//         var properMockImageValidator = new Mock<IImageValidator>();
//         var improperMockImageValidator = new Mock<IImageValidator>();
//         var properMockLogger = new Mock<ILogger<ThumbnailGenerationFunction>>();
//         var improperMockLogger = new Mock<ILogger<ThumbnailGenerationFunction>>();
//
//         var properThumbnailGenerationFunction = new ThumbnailGenerationFunction(
//             _thumbNailConfig, _blobUrlParser, _dataLakeServiceClientFactory, properMockImageValidator.Object,
//             properMockLogger.Object);
//
//         var improperThumbnailGenerationFunction = new ThumbnailGenerationFunction(
//             _thumbNailConfig, _blobUrlParser, _dataLakeServiceClientFactory, improperMockImageValidator.Object,
//             improperMockLogger.Object);
//
//         using var inputImage = new Image<Rgba32>(100, 100);
//         using var properBlobStream = new MemoryStream();
//         await inputImage.SaveAsJpegAsync(properBlobStream);
//         using var improperBlobStream = new MemoryStream();
//
//         // Reset stream position before passing to method
//         properBlobStream.Position = 0;
//
//         improperMockImageValidator.Setup(x => x.Validate(It.IsAny<MemoryStream>())).Throws<Exception>();
//
//         // Act
//         await properThumbnailGenerationFunction.ValidateImageStructureAsync(properBlobStream);
//         await improperThumbnailGenerationFunction.ValidateImageStructureAsync(improperBlobStream);
//
//         // Assert
//         properMockImageValidator.Verify(x => x.Validate(It.IsAny<MemoryStream>()), Times.Once);
//         improperMockImageValidator.Verify(x => x.Validate(It.IsAny<MemoryStream>()), Times.Once);
//         improperMockLogger.Verify(l => l.Log(
//                 LogLevel.Warning,
//                 It.IsAny<EventId>(),
//                 It.Is<It.IsAnyType>((v, t) =>
//                     true),
//                 It.IsAny<Exception>(),
//                 It.IsAny<Func<It.IsAnyType, Exception, string>>()!),
//             Times.Once);
//     }

//     [DataTestMethod]
//     [DataRow(480, 360)]
//     [DataRow(680, 500)]
//     [DataRow(1024, 1024)]
//     [DataRow(240, 240)]
//     [DataRow(120, 120)]
//     public async Task Test_ValidImageResizingAsync(int width, int height)
//     {
//         // Arrange
//         using var inputImage = new Image<Rgba32>(width, height);
//         inputImage.Mutate(ctx => ctx.Fill(Color.Red));
//
//         using var inputBlobStream = new MemoryStream();
//         await inputImage.SaveAsJpegAsync(inputBlobStream);
//
//         // Act
//         var outputBlobStream = await _thumbnailGenerationFunction.GenerateThumbnailAsync(inputBlobStream);
//
//         using var outputImage = await Image.LoadAsync(outputBlobStream);
//         var aspectRatio = outputImage.Width / (float)outputImage.Height;
//         var targetWidth = Math.Min((int)(_thumbNailConfig.MaxHeight * aspectRatio), outputImage.Width);
//
//
//         if (_thumbNailConfig.MaxHeight < inputImage.Height)
//         {
//             Assert.AreNotEqual(inputImage.Width, outputImage.Width);
//             Assert.AreNotEqual(inputImage.Height, outputImage.Height);
//             Assert.IsTrue(inputBlobStream.Length > outputBlobStream.Length);
//             Assert.AreEqual(_thumbNailConfig.MaxHeight, outputImage.Height);
//         }
//         else
//         {
//             Assert.AreEqual(inputImage.Width, outputImage.Width);
//             Assert.AreEqual(inputImage.Height, outputImage.Height);
//             Assert.AreEqual(inputBlobStream.Length, outputBlobStream.Length);
//         }
//
//         Assert.AreEqual(targetWidth, outputImage.Width);
//     }
//
//     [DataTestMethod]
//     [DataRow(true)]
//     [DataRow(false)]
//     public void GetThumbnailFileClient_Returns_FileClient_With_Correct_Filename(bool isPublic)
//     {
//         const string thumbnailNameSuffix = "-thumb";
//
//         // Mock setup
//         var mockDataLakeFileSystemClient = new Mock<DataLakeFileSystemClient>();
//
//         // Pass `isPublic` dynamically via constructor
//         var mockBlobData = new Mock<BlobData>("0", "1", "user001", "blobName", isPublic) { CallBase = true };
//
//         // Act
//         var result = _thumbnailGenerationFunction.GetThumbnailFileClient(
//             mockDataLakeFileSystemClient.Object,
//             mockBlobData.Object);
//
//         // Dynamic path based on `isPublic`
//         var expectedPath = isPublic
//             ? $"{_thumbNailConfig.ThumbnailPathPublic}/" +
//               $"{mockBlobData.Object.User}/{mockBlobData.Object.Name}{thumbnailNameSuffix}"
//             : $"{_thumbNailConfig.ThumbnailPathPrivate.ReplacePlaceholder(mockBlobData.Object.User)}/" +
//               $"{mockBlobData.Object.Name}{thumbnailNameSuffix}";
//
//         // Assert
//         mockDataLakeFileSystemClient.Verify(x =>
//             x.GetFileClient(expectedPath), Times.Once);
//     }
//
//     [TestMethod]
//     public async Task UploadThumbnailAsync_CallsAllMethods_WhenFileSystemNameContainsPublic()
//     {
//         // Arrange
//         var mockDataLakeFileClient = new Mock<DataLakeFileClient>();
//         mockDataLakeFileClient.SetupGet(x => x.Path).Returns("public/");
//
//         var thumbnailBytes = new MemoryStream();
//
//         // Setup UploadAsync
//         mockDataLakeFileClient.Setup(x => x.UploadAsync(
//                 It.Is<Stream>(s => s == thumbnailBytes),
//                 true,
//                 CancellationToken.None))
//             .ReturnsAsync(Response.FromValue(DataLakeModelFactory.PathInfo(new ETag(),
//                 DateTime.UtcNow), new MockResponse()));
//
//         // Setup SetHttpHeadersAsync
//         mockDataLakeFileClient.Setup(x => x.SetHttpHeadersAsync(
//                 It.Is<PathHttpHeaders>(h => h.ContentType == "image/jpeg"),
//                 null, CancellationToken.None))
//             .ReturnsAsync(Response.FromValue(DataLakeModelFactory.PathInfo(new ETag(),
//                 DateTime.UtcNow), new MockResponse()));
//
//         // Setup SetThumbnailMetadataAsync with metadata capture
//         IDictionary<string, string>? capturedMetadata = null;
//         mockDataLakeFileClient.Setup(m => m.SetMetadataAsync(
//                 It.IsAny<IDictionary<string, string>>(),
//                 null,
//                 CancellationToken.None))
//             .Callback<IDictionary<string, string>, DataLakeRequestConditions?, CancellationToken>((metadata, _, _) =>
//                 capturedMetadata = metadata)
//             .ReturnsAsync(Response.FromValue(DataLakeModelFactory.PathInfo(new ETag(), DateTime.UtcNow),
//                 new MockResponse()));
//
//         // Act
//         await _thumbnailGenerationFunction.UploadThumbnailAsync(
//             mockDataLakeFileClient.Object,
//             thumbnailBytes);
//
//         // Assert
//         mockDataLakeFileClient.Verify(x => x.UploadAsync(
//             It.Is<Stream>(s => s == thumbnailBytes),
//             true,
//             CancellationToken.None), Times.Once);
//
//         mockDataLakeFileClient.Verify(x => x.SetHttpHeadersAsync(
//             It.Is<PathHttpHeaders>(h => h.ContentType == "image/jpeg"),
//             null, CancellationToken.None), Times.Once);
//
//         mockDataLakeFileClient.Verify(x => x.SetMetadataAsync(
//             It.IsAny<IDictionary<string, string>>(),
//             null,
//             CancellationToken.None), Times.Once);
//
//         _loggerMock.Verify(x => x.Log(
//                 LogLevel.Information,
//                 It.IsAny<EventId>(),
//                 It.Is<It.IsAnyType>((v, t) =>
//                     v.ToString() == "Thumbnail uploaded successfully with MIME type: image/jpeg"),
//                 It.IsAny<Exception>(),
//                 It.IsAny<Func<It.IsAnyType, Exception, string>>()!),
//             Times.Once);
//
//         _loggerMock.Verify(l => l.Log(
//                 LogLevel.Information,
//                 It.IsAny<EventId>(),
//                 It.Is<It.IsAnyType>((v, t) =>
//                     v.ToString() == "Thumbnail metadata updated for public, e.g. CreatedOn"),
//                 It.IsAny<Exception>(),
//                 It.IsAny<Func<It.IsAnyType, Exception, string>>()!),
//             Times.Once);
//
//         Assert.IsNotNull(capturedMetadata);
//         Assert.IsTrue(capturedMetadata.ContainsKey("CreatedOn"));
//
//         var createdOnValue = capturedMetadata["CreatedOn"];
//         Assert.IsTrue(DateTimeOffset.TryParse(createdOnValue, out var parsed));
//     }
//
//     [TestMethod]
//     public async Task UploadThumbnailAsync_SkipsMetadataCall_WhenFileSystemNameDoesNotContainPublicWithTrailingSlash()
//     {
//         // Arrange
//         var mockDataLakeFileClient = new Mock<DataLakeFileClient>();
//         mockDataLakeFileClient.SetupGet(x => x.Path).Returns("public");
//
//         var thumbnailBytes = new MemoryStream();
//
//         // Setup UploadAsync
//         mockDataLakeFileClient.Setup(x => x.UploadAsync(
//                 It.Is<Stream>(s => s == thumbnailBytes),
//                 true,
//                 CancellationToken.None))
//             .ReturnsAsync(Response.FromValue(DataLakeModelFactory.PathInfo(new ETag(), DateTime.UtcNow),
//                 new MockResponse()));
//
//         // Setup SetHttpHeadersAsync
//         mockDataLakeFileClient.Setup(x => x.SetHttpHeadersAsync(
//                 It.Is<PathHttpHeaders>(h => h.ContentType == "image/jpeg"), null,
//                 CancellationToken.None))
//             .ReturnsAsync(Response.FromValue(DataLakeModelFactory.PathInfo(new ETag(), DateTime.UtcNow),
//                 new MockResponse()));
//
//         IDictionary<string, string>? capturedMetadata = null;
//         mockDataLakeFileClient.Setup(m => m.SetMetadataAsync(
//                 It.IsAny<IDictionary<string, string>>(),
//                 null,
//                 CancellationToken.None))
//             .Callback<IDictionary<string, string>, DataLakeRequestConditions?, CancellationToken>((metadata, _, _) =>
//                 capturedMetadata = metadata)
//             .ReturnsAsync(Response.FromValue(DataLakeModelFactory.PathInfo(new ETag(), DateTime.UtcNow),
//                 new MockResponse()));
//
//         // Act
//         await _thumbnailGenerationFunction.UploadThumbnailAsync(
//             mockDataLakeFileClient.Object,
//             thumbnailBytes);
//
//         // Assert
//         mockDataLakeFileClient.Verify(x => x.UploadAsync(
//             It.Is<Stream>(s => s == thumbnailBytes),
//             true,
//             CancellationToken.None), Times.Once);
//
//         mockDataLakeFileClient.Verify(x => x.SetHttpHeadersAsync(
//             It.Is<PathHttpHeaders>(h => h.ContentType == "image/jpeg"), null,
//             CancellationToken.None), Times.Once);
//
//         mockDataLakeFileClient.Verify(x => x.SetMetadataAsync(
//             It.IsAny<IDictionary<string, string>>(),
//             null,
//             CancellationToken.None), Times.Never);
//
//         _loggerMock.Verify(x => x.Log(
//                 LogLevel.Information,
//                 It.IsAny<EventId>(),
//                 It.Is<It.IsAnyType>((v, t) =>
//                     v.ToString() == "Thumbnail uploaded successfully with MIME type: image/jpeg"),
//                 It.IsAny<Exception>(),
//                 It.IsAny<Func<It.IsAnyType, Exception, string>>()!),
//             Times.Once);
//
//         _loggerMock.Verify(l => l.Log(
//                 LogLevel.Information,
//                 It.IsAny<EventId>(),
//                 It.Is<It.IsAnyType>((v, t) =>
//                     v.ToString() == "Thumbnail metadata updated for public, e.g. CreatedOn"),
//                 It.IsAny<Exception>(),
//                 It.IsAny<Func<It.IsAnyType, Exception, string>>()!),
//             Times.Never);
//
//
//         Assert.IsNull(capturedMetadata);
//     }
//
//
//     [TestMethod]
//     public async Task SetThumbnailMetadataAsync_ShouldSetCreatedOnTag()
//     {
//         // Arrange
//         var mockDataLakeFileClient = new Mock<DataLakeFileClient>();
//         IDictionary<string, string>? capturedMetadata = null;
//         mockDataLakeFileClient.Setup(m => m.SetMetadataAsync(
//                 It.IsAny<IDictionary<string, string>>(),
//                 It.IsAny<DataLakeRequestConditions>(),
//                 CancellationToken.None))
//             .Callback<IDictionary<string, string>, DataLakeRequestConditions,
//                 CancellationToken>((metadata, conditions, ct) => { capturedMetadata = metadata; })
//             .ReturnsAsync(Response.FromValue(DataLakeModelFactory.PathInfo(new ETag(),
//                 DateTime.UtcNow), new MockResponse()));
//
//         // Act
//         await _thumbnailGenerationFunction.SetThumbnailMetadataAsync(mockDataLakeFileClient.Object);
//
//         // Assert
//         mockDataLakeFileClient.Verify(m =>
//                 m.SetMetadataAsync(It.IsAny<IDictionary<string, string>>(), null, CancellationToken.None),
//             Times.Once);
//
//         Assert.IsNotNull(capturedMetadata);
//         Assert.IsTrue(capturedMetadata.ContainsKey("CreatedOn"));
//
//         var createdOnValue = capturedMetadata["CreatedOn"];
//         Assert.IsTrue(DateTimeOffset.TryParse(createdOnValue, out var parsed));
//     }
//
//     private sealed class MockResponse : Response
//     {
//         public override int Status => throw new NotImplementedException();
//
//         public override string ReasonPhrase => throw new NotImplementedException();
//
//         public override Stream? ContentStream
//         {
//             get => throw new NotImplementedException();
//             set => throw new NotImplementedException();
//         }
//
//         public override string ClientRequestId
//         {
//             get => throw new NotImplementedException();
//             set => throw new NotImplementedException();
//         }
//
//         public override void Dispose() =>
//             throw new NotImplementedException();
//
//         protected override bool ContainsHeader(string name) =>
//             throw new NotImplementedException();
//
//         protected override IEnumerable<HttpHeader> EnumerateHeaders() =>
//             throw new NotImplementedException();
//
//         protected override bool TryGetHeader(
//             string name,
//             [NotNullWhen(true)] out string? value) =>
//             throw new NotImplementedException();
//
//         protected override bool TryGetHeaderValues(
//             string name,
//             [NotNullWhen(true)] out IEnumerable<string>? values) =>
//             throw new NotImplementedException();
//     }
// }