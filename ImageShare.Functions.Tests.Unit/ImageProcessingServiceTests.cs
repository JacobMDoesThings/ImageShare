using System.Diagnostics.CodeAnalysis;
using System.Text;
using Azure;
using Azure.Core;
using Azure.Storage.Files.DataLake;
using Azure.Storage.Files.DataLake.Models;
using ImageShare.Functions.Configuration;
using ImageShare.Functions.Data;
using ImageShare.Functions.Interfaces;
using ImageShare.Functions.Services;
using Microsoft.Extensions.Logging;
using Moq;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace ImageShare.Functions.Tests.Unit;

[TestClass]
public class ImageProcessingServiceTests
{
    private Mock<ILogger<ImageProcessingService>> _loggerMock;
    private ImageProcessingService _imageProcessingService;
    private ThumbnailGenerationConfiguration _thumbNailConfig;
    private BlobUrlParser _blobUrlParser;
    private DataLakeServiceClientFactory _dataLakeServiceClientFactory;
    private ImageValidator _imageValidator;
    private readonly string _thumbnailPathPrivate = "privatePath";
    private readonly string _thumbnailPathPublic = "publicPath";
    private readonly string _thumbnailConnectionString = "name=FakeConnectionString";
    private readonly string[] _thumbnailAllowedMimeTypes = ["mime-type1", "mime-type2"];

    [TestInitialize]
    public void Setup()
    {
        // Initialize mocks before each test
        _loggerMock = new Mock<ILogger<ImageProcessingService>>();
        _thumbNailConfig = new ThumbnailGenerationConfiguration()
        {
            AllowedMimeTypes = _thumbnailAllowedMimeTypes,
            ConnectionString = _thumbnailConnectionString,
            ThumbnailPathPrivate = _thumbnailPathPrivate,
            ThumbnailPathPublic = _thumbnailPathPublic,
            MaxHeight = 240,
            JpegQuality = 50
        };
        _blobUrlParser = new BlobUrlParser();
        _dataLakeServiceClientFactory = new DataLakeServiceClientFactory();
        _imageValidator = new ImageValidator();

        _imageProcessingService = new ImageProcessingService(
            _loggerMock.Object,
            _thumbNailConfig, _blobUrlParser, _dataLakeServiceClientFactory,
            _imageValidator);
    }
    
    [TestMethod]
    public async Task ProcessImageAsync_Calls_All_Validation_And_Upload_Methods()
    {
        var fakeBlobData = new BlobData
        {
            BlobUrlString = "https://account.dfs.core.windows.net/api/service",
            ContainerName = "container",
            IsPublic = true,
            Name = "Name",
            User = "User"
        };
        
        var mockDataLakeServiceClientFactory = new Mock<IDataLakeServiceClientFactory>();
        var mockDataLakeServiceClient = new Mock<DataLakeServiceClient>();
        var mockBlobUrlParser = new Mock<IBlobUrlParser>();
        var mockDataLakeFileSystemClient = new Mock<DataLakeFileSystemClient>();
        var mockDataLakeFileClient = new Mock<DataLakeFileClient>();
        var mockThumbDataLakeFileClient = new Mock<DataLakeFileClient>();
        var fakeMemoryStream = new MemoryStream();
        var fakeThumbMemoryStream = new MemoryStream();
        
      
        mockDataLakeServiceClientFactory.Setup(x => x.Create(It.IsAny<string>()))
            .Returns(mockDataLakeServiceClient.Object);
        mockDataLakeServiceClient.Setup(x => x.GetFileSystemClient(fakeBlobData.ContainerName))
            .Returns(mockDataLakeFileSystemClient.Object);
        
        mockBlobUrlParser.Setup(x => x.ParseBlob(It.IsAny<Uri>())).Returns(fakeBlobData);

        var mockImageProcessingService = new Mock<ImageProcessingService>(
            Mock.Of<ILogger<ImageProcessingService>>(),
            _thumbNailConfig, mockBlobUrlParser.Object, mockDataLakeServiceClientFactory.Object,
            _imageValidator
        );

        mockImageProcessingService.Setup(x =>
                x.GetSourceFileClient(mockDataLakeFileSystemClient.Object, fakeBlobData))
            .Returns(mockDataLakeFileClient.Object);
        mockImageProcessingService.Setup(x =>
            x.ValidateMimeTypeAsync(mockDataLakeFileClient.Object));
        mockImageProcessingService.Setup(x =>
            x.DownloadFileStreamAsync(mockDataLakeFileClient.Object)).ReturnsAsync(fakeMemoryStream);
        mockImageProcessingService.Setup(x => x.ValidateImageStructureAsync(fakeMemoryStream));
        mockImageProcessingService.Setup(x => x.GenerateThumbnailAsync(fakeMemoryStream))
            .ReturnsAsync(fakeThumbMemoryStream);
        mockImageProcessingService.Setup(x => 
            x.GetThumbnailFileClient(mockDataLakeFileSystemClient.Object, fakeBlobData))
            .Returns(mockThumbDataLakeFileClient.Object);
        mockImageProcessingService.Setup(x =>
            x.UploadThumbnailAsync(mockThumbDataLakeFileClient.Object, fakeThumbMemoryStream));
       
        await mockImageProcessingService.Object.ProcessImageAsync(
            new Uri("https://account.dfs.core.windows.net/api/service"));
       
        mockImageProcessingService.Verify(x =>
            x.GetSourceFileClient(mockDataLakeFileSystemClient.Object, fakeBlobData), Times.Once);
        mockImageProcessingService.Verify(x => 
            x.ValidateMimeTypeAsync(mockDataLakeFileClient.Object), Times.Once);
        mockImageProcessingService.Verify(x =>
            x.DownloadFileStreamAsync(mockDataLakeFileClient.Object), Times.Once);
        mockImageProcessingService.Verify(x => 
            x.ValidateImageStructureAsync(fakeMemoryStream), Times.Once);
        mockImageProcessingService.Verify(x => 
            x.GenerateThumbnailAsync(fakeMemoryStream), Times.Once);
        mockImageProcessingService.Verify(x => 
            x.GetThumbnailFileClient(mockDataLakeFileSystemClient.Object, fakeBlobData), Times.Once);
        mockImageProcessingService.Verify(x => 
            x.UploadThumbnailAsync(mockThumbDataLakeFileClient.Object, fakeThumbMemoryStream), Times.Once);
    }

    [TestMethod]
    public void GetSourceFileClient_ReturnsCorrectFileClient()
    {
        // Arrange
        var blobData = new BlobData
        {
            BlobUrlString = "test-container/test-file.txt",
            ContainerName = "test-container",
            IsPublic = true,
            Name = "test-file.txt",
            User = "test-user"
        };

        var mockDataLakeFileSystemClient = new Mock<DataLakeFileSystemClient>();

        var returnsDataLakeFileClient =
            _imageProcessingService.GetSourceFileClient(mockDataLakeFileSystemClient.Object, blobData);

        mockDataLakeFileSystemClient.Verify(x =>
            x.GetFileClient(blobData.BlobUrlString), Times.Once);

        _loggerMock.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) =>
                    true),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()!),
            Times.Once);
    }

    [TestMethod]
    public async Task ValidateMimeTypeAsync_Logs_Bad_MimeTypes_Deletes_File()
    {
        var properMockDataLakeFileClient = new Mock<DataLakeFileClient>();
        var improperMockDataLakeFileClient = new Mock<DataLakeFileClient>();

        var fakeProperDataPathProperties = DataLakeModelFactory.PathProperties(
            DateTimeOffset.Now,
            DateTimeOffset.Now,
            new Dictionary<string, string>(),
            DateTimeOffset.Now, string.Empty, string.Empty,
            string.Empty, new Uri("https://fakeuri"), CopyStatus.Success, false, DataLakeLeaseDuration.Fixed,
            DataLakeLeaseState.Available, DataLakeLeaseStatus.Unlocked, 999, _thumbnailAllowedMimeTypes[0],
            new ETag(), new byte[0], string.Empty, string.Empty, string.Empty, string.Empty,
            string.Empty, false, string.Empty, string.Empty, string.Empty,
            DateTimeOffset.Now
        );

        var fakeImproperDataPathProperties = DataLakeModelFactory.PathProperties(
            DateTimeOffset.Now,
            DateTimeOffset.Now,
            new Dictionary<string, string>(),
            DateTimeOffset.Now, string.Empty, string.Empty,
            string.Empty, new Uri("https://fakeuri"), CopyStatus.Success, false, DataLakeLeaseDuration.Fixed,
            DataLakeLeaseState.Available, DataLakeLeaseStatus.Unlocked, 999, "BadMimeType",
            new ETag(), new byte[0], string.Empty, string.Empty, string.Empty, string.Empty,
            string.Empty, false, string.Empty, string.Empty, string.Empty,
            DateTimeOffset.Now
        );

        properMockDataLakeFileClient.Setup(x =>
                x.GetPropertiesAsync(null, CancellationToken.None))
            .ReturnsAsync(Response.FromValue(fakeProperDataPathProperties, new MockResponse()));

        improperMockDataLakeFileClient.Setup(x =>
                x.GetPropertiesAsync(null, CancellationToken.None))
            .ReturnsAsync(Response.FromValue(fakeImproperDataPathProperties, new MockResponse()));

        var ex = await Assert.ThrowsExceptionAsync<InvalidOperationException>(async () =>
            await _imageProcessingService.ValidateMimeTypeAsync(improperMockDataLakeFileClient.Object));

        Assert.IsNotNull(ex);

        _loggerMock.Verify(l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) =>
                    true),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()!),
            Times.Once);

        improperMockDataLakeFileClient.Verify(x =>
            x.DeleteAsync(It.IsAny<DataLakeRequestConditions>(),
                It.IsAny<CancellationToken>()), Times.Once);

        await _imageProcessingService.ValidateMimeTypeAsync(properMockDataLakeFileClient.Object);
        _loggerMock.Verify(l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) =>
                    true),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()!),
            Times.Once);
    }

    [TestMethod]
    public async Task DownloadFileStreamAsync_ReturnsCorrectStream()
    {
        var mockDataLakeFileClient = new Mock<DataLakeFileClient>();
        const string data = "test data";
        var bytes = Encoding.UTF8.GetBytes(data);
        var stream = new MemoryStream(bytes);

        mockDataLakeFileClient.Setup(x => x.ReadAsync()).ReturnsAsync(Response.FromValue(
            DataLakeModelFactory.FileDownloadInfo(stream.Length,
                content: stream, [], null), new MockResponse()));

        var output = await _imageProcessingService.DownloadFileStreamAsync(mockDataLakeFileClient.Object);
        output.Position = 0;
        stream.Position = 0;

        var outputBytes = new byte[output.Length];
        await output.ReadExactlyAsync(outputBytes, 0, outputBytes.Length);

        var streamBytes = new byte[stream.Length];
        await stream.ReadExactlyAsync(streamBytes, 0, streamBytes.Length);

        CollectionAssert.AreEqual(outputBytes, streamBytes);
    }

    [TestMethod]
    public async Task
        Test_ValidateImageStructure_Log_Warning_On_Validation_Failure_And_Throws_Calls_ImageValidator_Validate()
    {
        // Arrange
        var properMockImageValidator = new Mock<IImageValidator>();
        var improperMockImageValidator = new Mock<IImageValidator>();
        var properMockLogger = new Mock<ILogger<ImageProcessingService>>();
        var improperMockLogger = new Mock<ILogger<ImageProcessingService>>();

        var properThumbnailGenerationFunction = new ImageProcessingService(
            properMockLogger.Object, _thumbNailConfig, _blobUrlParser, _dataLakeServiceClientFactory,
            properMockImageValidator.Object);

        var improperThumbnailGenerationFunction = new ImageProcessingService(
            improperMockLogger.Object, _thumbNailConfig, _blobUrlParser, _dataLakeServiceClientFactory,
            improperMockImageValidator.Object);

        using var inputImage = new Image<Rgba32>(100, 100);
        using var properBlobStream = new MemoryStream();
        await inputImage.SaveAsJpegAsync(properBlobStream);
        using var improperBlobStream = new MemoryStream();

        // Reset stream position before passing to method
        properBlobStream.Position = 0;

        improperMockImageValidator.Setup(x => x.Validate(It.IsAny<MemoryStream>())).Throws<Exception>();

        // Act
        await properThumbnailGenerationFunction.ValidateImageStructureAsync(properBlobStream);
        await improperThumbnailGenerationFunction.ValidateImageStructureAsync(improperBlobStream);

        // Assert
        properMockImageValidator.Verify(x => x.Validate(It.IsAny<MemoryStream>()), Times.Once);
        improperMockImageValidator.Verify(x => x.Validate(It.IsAny<MemoryStream>()), Times.Once);
        improperMockLogger.Verify(l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) =>
                    true),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()!),
            Times.Once);
    }

    [DataTestMethod]
    [DataRow(480, 360)]
    [DataRow(680, 500)]
    [DataRow(1024, 1024)]
    [DataRow(240, 240)]
    [DataRow(120, 120)]
    public async Task Test_ValidImageResizingAsync(int width, int height)
    {
        // Arrange
        using var inputImage = new Image<Rgba32>(width, height);
        inputImage.Mutate(ctx => ctx.Fill(Color.Red));

        using var inputBlobStream = new MemoryStream();
        await inputImage.SaveAsJpegAsync(inputBlobStream);

        // Act
        var outputBlobStream = await _imageProcessingService.GenerateThumbnailAsync(inputBlobStream);

        using var outputImage = await Image.LoadAsync(outputBlobStream);
        var aspectRatio = outputImage.Width / (float)outputImage.Height;
        var targetWidth = Math.Min((int)(_thumbNailConfig.MaxHeight * aspectRatio), outputImage.Width);


        if (_thumbNailConfig.MaxHeight < inputImage.Height)
        {
            Assert.AreNotEqual(inputImage.Width, outputImage.Width);
            Assert.AreNotEqual(inputImage.Height, outputImage.Height);
            Assert.IsTrue(inputBlobStream.Length > outputBlobStream.Length);
            Assert.AreEqual(_thumbNailConfig.MaxHeight, outputImage.Height);
        }
        else
        {
            Assert.AreEqual(inputImage.Width, outputImage.Width);
            Assert.AreEqual(inputImage.Height, outputImage.Height);
            Assert.AreEqual(inputBlobStream.Length, outputBlobStream.Length);
        }

        Assert.AreEqual(targetWidth, outputImage.Width);
    }

    [DataTestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public void GetThumbnailFileClient_Returns_FileClient_With_Correct_Filename(bool isPublic)
    {
        const string thumbnailNameSuffix = "-thumb";

        // Mock setup
        var mockDataLakeFileSystemClient = new Mock<DataLakeFileSystemClient>();

        // Pass `isPublic` dynamically via constructor
        var mockBlobData = new Mock<BlobData>("0", "1", "user001", "blobName", isPublic) { CallBase = true };

        // Act
        var result = _imageProcessingService.GetThumbnailFileClient(
            mockDataLakeFileSystemClient.Object,
            mockBlobData.Object);

        // Dynamic path based on `isPublic`
        var expectedPath = isPublic
            ? $"{_thumbNailConfig.ThumbnailPathPublic}/" +
              $"{mockBlobData.Object.User}/{mockBlobData.Object.Name}{thumbnailNameSuffix}"
            : $"{_thumbNailConfig.ThumbnailPathPrivate.ReplacePlaceholder(mockBlobData.Object.User)}/" +
              $"{mockBlobData.Object.Name}{thumbnailNameSuffix}";

        // Assert
        mockDataLakeFileSystemClient.Verify(x =>
            x.GetFileClient(expectedPath), Times.Once);
    }

    [TestMethod]
    public async Task UploadThumbnailAsync_CallsAllMethods_WhenFileSystemNameContainsPublic()
    {
        // Arrange
        var mockDataLakeFileClient = new Mock<DataLakeFileClient>();
        mockDataLakeFileClient.SetupGet(x => x.Path).Returns("public/");

        var thumbnailBytes = new MemoryStream();

        // Setup UploadAsync
        mockDataLakeFileClient.Setup(x => x.UploadAsync(
                It.Is<Stream>(s => s == thumbnailBytes),
                true,
                CancellationToken.None))
            .ReturnsAsync(Response.FromValue(DataLakeModelFactory.PathInfo(new ETag(),
                DateTime.UtcNow), new MockResponse()));

        // Setup SetHttpHeadersAsync
        mockDataLakeFileClient.Setup(x => x.SetHttpHeadersAsync(
                It.Is<PathHttpHeaders>(h => h.ContentType == "image/jpeg"),
                null, CancellationToken.None))
            .ReturnsAsync(Response.FromValue(DataLakeModelFactory.PathInfo(new ETag(),
                DateTime.UtcNow), new MockResponse()));

        // Setup SetThumbnailMetadataAsync with metadata capture
        IDictionary<string, string>? capturedMetadata = null;
        mockDataLakeFileClient.Setup(m => m.SetMetadataAsync(
                It.IsAny<IDictionary<string, string>>(),
                null,
                CancellationToken.None))
            .Callback<IDictionary<string, string>, DataLakeRequestConditions?, CancellationToken>((metadata, _, _) =>
                capturedMetadata = metadata)
            .ReturnsAsync(Response.FromValue(DataLakeModelFactory.PathInfo(new ETag(), DateTime.UtcNow),
                new MockResponse()));

        // Act
        await _imageProcessingService.UploadThumbnailAsync(
            mockDataLakeFileClient.Object,
            thumbnailBytes);

        // Assert
        mockDataLakeFileClient.Verify(x => x.UploadAsync(
            It.Is<Stream>(s => s == thumbnailBytes),
            true,
            CancellationToken.None), Times.Once);

        mockDataLakeFileClient.Verify(x => x.SetHttpHeadersAsync(
            It.Is<PathHttpHeaders>(h => h.ContentType == "image/jpeg"),
            null, CancellationToken.None), Times.Once);

        mockDataLakeFileClient.Verify(x => x.SetMetadataAsync(
            It.IsAny<IDictionary<string, string>>(),
            null,
            CancellationToken.None), Times.Once);

        _loggerMock.Verify(x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) =>
                    v.ToString() == "Thumbnail uploaded successfully with MIME type: image/jpeg"),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()!),
            Times.Once);

        _loggerMock.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) =>
                    v.ToString() == "Thumbnail metadata updated for public, e.g. CreatedOn"),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()!),
            Times.Once);

        Assert.IsNotNull(capturedMetadata);
        Assert.IsTrue(capturedMetadata.ContainsKey("CreatedOn"));

        var createdOnValue = capturedMetadata["CreatedOn"];
        Assert.IsTrue(DateTimeOffset.TryParse(createdOnValue, out var parsed));
    }

    [TestMethod]
    public async Task UploadThumbnailAsync_SkipsMetadataCall_WhenFileSystemNameDoesNotContainPublicWithTrailingSlash()
    {
        // Arrange
        var mockDataLakeFileClient = new Mock<DataLakeFileClient>();
        mockDataLakeFileClient.SetupGet(x => x.Path).Returns("public");

        var thumbnailBytes = new MemoryStream();

        // Setup UploadAsync
        mockDataLakeFileClient.Setup(x => x.UploadAsync(
                It.Is<Stream>(s => s == thumbnailBytes),
                true,
                CancellationToken.None))
            .ReturnsAsync(Response.FromValue(DataLakeModelFactory.PathInfo(new ETag(), DateTime.UtcNow),
                new MockResponse()));

        // Setup SetHttpHeadersAsync
        mockDataLakeFileClient.Setup(x => x.SetHttpHeadersAsync(
                It.Is<PathHttpHeaders>(h => h.ContentType == "image/jpeg"), null,
                CancellationToken.None))
            .ReturnsAsync(Response.FromValue(DataLakeModelFactory.PathInfo(new ETag(), DateTime.UtcNow),
                new MockResponse()));

        IDictionary<string, string>? capturedMetadata = null;
        mockDataLakeFileClient.Setup(m => m.SetMetadataAsync(
                It.IsAny<IDictionary<string, string>>(),
                null,
                CancellationToken.None))
            .Callback<IDictionary<string, string>, DataLakeRequestConditions?, CancellationToken>((metadata, _, _) =>
                capturedMetadata = metadata)
            .ReturnsAsync(Response.FromValue(DataLakeModelFactory.PathInfo(new ETag(), DateTime.UtcNow),
                new MockResponse()));

        // Act
        await _imageProcessingService.UploadThumbnailAsync(
            mockDataLakeFileClient.Object,
            thumbnailBytes);

        // Assert
        mockDataLakeFileClient.Verify(x => x.UploadAsync(
            It.Is<Stream>(s => s == thumbnailBytes),
            true,
            CancellationToken.None), Times.Once);

        mockDataLakeFileClient.Verify(x => x.SetHttpHeadersAsync(
            It.Is<PathHttpHeaders>(h => h.ContentType == "image/jpeg"), null,
            CancellationToken.None), Times.Once);

        mockDataLakeFileClient.Verify(x => x.SetMetadataAsync(
            It.IsAny<IDictionary<string, string>>(),
            null,
            CancellationToken.None), Times.Never);

        _loggerMock.Verify(x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) =>
                    v.ToString() == "Thumbnail uploaded successfully with MIME type: image/jpeg"),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()!),
            Times.Once);

        _loggerMock.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) =>
                    v.ToString() == "Thumbnail metadata updated for public, e.g. CreatedOn"),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()!),
            Times.Never);


        Assert.IsNull(capturedMetadata);
    }


    [TestMethod]
    public async Task SetThumbnailMetadataAsync_ShouldSetCreatedOnTag()
    {
        // Arrange
        var mockDataLakeFileClient = new Mock<DataLakeFileClient>();
        IDictionary<string, string>? capturedMetadata = null;
        mockDataLakeFileClient.Setup(m => m.SetMetadataAsync(
                It.IsAny<IDictionary<string, string>>(),
                It.IsAny<DataLakeRequestConditions>(),
                CancellationToken.None))
            .Callback<IDictionary<string, string>, DataLakeRequestConditions,
                CancellationToken>((metadata, conditions, ct) => { capturedMetadata = metadata; })
            .ReturnsAsync(Response.FromValue(DataLakeModelFactory.PathInfo(new ETag(),
                DateTime.UtcNow), new MockResponse()));

        // Act
        await _imageProcessingService.SetThumbnailMetadataAsync(mockDataLakeFileClient.Object);

        // Assert
        mockDataLakeFileClient.Verify(m =>
                m.SetMetadataAsync(It.IsAny<IDictionary<string, string>>(), null, CancellationToken.None),
            Times.Once);

        Assert.IsNotNull(capturedMetadata);
        Assert.IsTrue(capturedMetadata.ContainsKey("CreatedOn"));

        var createdOnValue = capturedMetadata["CreatedOn"];
        Assert.IsTrue(DateTimeOffset.TryParse(createdOnValue, out var parsed));
    }

    private sealed class MockResponse : Response
    {
        public override int Status => throw new NotImplementedException();

        public override string ReasonPhrase => throw new NotImplementedException();

        public override Stream? ContentStream
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        public override string ClientRequestId
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        public override void Dispose() =>
            throw new NotImplementedException();

        protected override bool ContainsHeader(string name) =>
            throw new NotImplementedException();

        protected override IEnumerable<HttpHeader> EnumerateHeaders() =>
            throw new NotImplementedException();

        protected override bool TryGetHeader(
            string name,
            [NotNullWhen(true)] out string? value) =>
            throw new NotImplementedException();

        protected override bool TryGetHeaderValues(
            string name,
            [NotNullWhen(true)] out IEnumerable<string>? values) =>
            throw new NotImplementedException();
    }
}