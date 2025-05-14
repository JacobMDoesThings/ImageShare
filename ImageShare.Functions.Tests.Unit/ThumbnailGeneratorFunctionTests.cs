using Azure;
using Azure.Storage.Files.DataLake;
using Azure.Storage.Files.DataLake.Models;
using ImageShare.Functions.Configuration;
using ImageShare.Functions.Services;
using Microsoft.Extensions.Logging;
using Moq;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Azure.Core;
using ImageShare.Functions.Data;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace ImageShare.Functions.Tests.Unit;

[TestClass]
public sealed class ThumbnailGeneratorFunctionTests
{
    private Mock<ILogger<ThumbnailGenerationFunction>> _loggerMock;
    private ThumbnailGenerationFunction _thumbnailGenerationFunction;
    private ThumbnailGenerationConfiguration _thumbNailConfig;
    private BlobUrlParser _blobUrlParser;
    private DataLakeServiceClientFactory _dataLakeServiceClientFactory;
    private ImageValidator _imageValidator;
    private readonly string _thumbnailPathPrivate = "privatePath";
    private readonly string _thumbnailPathPublic = "publicPath";
    private readonly string _thumbnailConnectionString = "FakeConnectionString";
    private readonly string[] _thumnailAllowedMimeTypes = ["mime-type1", "mime-type2"];

    [TestInitialize]
    public void Setup()
    {
        // Initialize mocks before each test
        _loggerMock = new Mock<ILogger<ThumbnailGenerationFunction>>();
        _thumbNailConfig = new ThumbnailGenerationConfiguration()
        {
            AllowedMimeTypes = _thumnailAllowedMimeTypes,
            ConnectionString = _thumbnailConnectionString,
            ThumbnailPathPrivate = _thumbnailPathPrivate,
            ThumbnailPathPublic = _thumbnailPathPublic,
            MaxHeight = 240,
            JpegQuality = 50
        };
        _blobUrlParser = new BlobUrlParser();
        _dataLakeServiceClientFactory = new DataLakeServiceClientFactory();
        _imageValidator = new ImageValidator();

        _thumbnailGenerationFunction = new ThumbnailGenerationFunction(
            _thumbNailConfig, _blobUrlParser, _dataLakeServiceClientFactory,
            _imageValidator, _loggerMock.Object);
    }

    // internal async Task ValidateImageStructureAsync(MemoryStream stream)
    // {
    //     try
    //     {
    //         await imageValidator.Validate(stream);
    //     }
    //     catch (Exception ex)
    //     {
    //         logger.LogWarning("Invalid image structure detected: {Error}", ex.Message);
    //         throw;
    //     }
    // }
    [TestMethod]
    public void Test_ValidateImageStructure_Log_Warning_On_Validation_Failure_And_Call_ImageValidator_Validate()
    {
        // Arrange
        // var mockFunction = new Mock<ThumbnailGenerationFunction>();
        var loggerMock = new Mock<ILogger>(); // Assuming ILogger is used

        // mockFunction.Setup(x => x.ValidateImageStructureAsync(It.IsAny<MemoryStream>()))
        //     .Throws(new ImageValidationException("Invalid image structure", It.IsAny<Exception>()));

       // var imageValidator = new ImageValidator();
    
        // Act & Assert
        // Assert.ThrowsExceptionAsync<ImageValidationException>(async () =>
        //      await imageValidator.Validate(new MemoryStream()));
        
    
        loggerMock.Verify(
            x => x.LogWarning("Invalid image structure detected: {Error}", It.IsAny<string>()),
            Times.Once());
    }
    // TODO: Test should be for ImageValidator.
    // [TestMethod]
    // public void Test_ImageValidation()
    // {
    //     // 1. Text File Stream
    //     var textStream = new MemoryStream();
    //     var textData = Encoding.UTF8.GetBytes("This is plain text.");
    //     textStream.Write(textData, 0, textData.Length);
    //     textStream.Position = 0;
    //
    //     // Assert throws exception for text file
    //     Assert.ThrowsException<Exception>(() =>
    //         _thumbnailGenerationFunction.ValidateImageStructureAsync
    //             (textStream));
    //
    //     // Reset stream position for next test case
    //     textStream.Dispose();
    //
    //     // 2. PDF Stream
    //     var pdfStream = new MemoryStream();
    //     byte[] pdfHeader = Encoding.ASCII.GetBytes("%PDF-1.4\n%âãÏÓ");
    //     pdfStream.Write(pdfHeader, 0, pdfHeader.Length);
    //     byte[] dummyData = new byte[1024]; // Simulate PDF content
    //     pdfStream.Write(dummyData, 0, dummyData.Length);
    //     pdfStream.Position = 0;
    //
    //     // Assert throws exception for PDF file
    //     Assert.ThrowsException<ImageValidationException>(() =>
    //         _thumbnailGenerationFunction.ValidateImageStructureAsync
    //             (pdfStream));
    //     pdfStream.Dispose();
    //
    //     // 3. DOCX Stream (ZIP archive header)
    //     var docxStream = new MemoryStream();
    //     byte[] zipHeader = { 0x50, 0x4B }; // PK header for ZIP archives
    //     docxStream.Write(zipHeader, 0, zipHeader.Length);
    //     byte[] dummyDataDocx = new byte[1024];
    //     docxStream.Write(dummyDataDocx, 0, dummyDataDocx.Length);
    //     docxStream.Position = 0;
    //
    //     // Assert throws exception for DOCX file
    //     Assert.ThrowsException<ImageValidationException>(() => _thumbnailGenerationFunction.ValidateImageStructureAsync
    //         (docxStream));
    //     docxStream.Dispose();
    //
    //     // 4. EXE Stream (MZ header)
    //     var exeStream = new MemoryStream();
    //     byte[] mzHeader = { 0x4D, 0x5A }; // MZ header for EXE files
    //     exeStream.Write(mzHeader, 0, mzHeader.Length);
    //     byte[] dummyDataExe = new byte[1024];
    //     exeStream.Write(dummyDataExe, 0, dummyDataExe.Length);
    //     exeStream.Position = 0;
    //
    //     // Assert throws exception for EXE file
    //     Assert.ThrowsException<ImageValidationException>(() => _thumbnailGenerationFunction.ValidateImageStructureAsync
    //         (exeStream));
    //     exeStream.Dispose();
    //
    //     // 5. Malicious PDF Stream (disguised JavaScript payload)
    //     var maliciousPdfStream = new MemoryStream();
    //     byte[] maliciousHeaderAndJs = Encoding.ASCII.GetBytes("%PDF-1.4\n%âãÏÓ\n/JS << /S /JavaScript >>");
    //     maliciousPdfStream.Write(maliciousHeaderAndJs, 0, maliciousHeaderAndJs.Length);
    //     byte[] dummyDataMalPdf = new byte[1024];
    //     maliciousPdfStream.Write(dummyDataMalPdf, 0, dummyDataMalPdf.Length);
    //     maliciousPdfStream.Position = 0;
    //
    //     // Assert throws exception for malicious PDF file
    //     Assert.ThrowsException<ImageValidationException>(() => _thumbnailGenerationFunction.ValidateImageStructureAsync
    //         (maliciousPdfStream));
    //     maliciousPdfStream.Dispose();
    //
    //     // 6. Malicious EXE Disguised as BMP Stream
    //     var maliciousExeAsStream = new MemoryStream();
    //     byte[] bmpHeader = { 0x42, 0x4D }; // BMP header
    //     byte[] mzHeaderAfterBmp = { 0x4D, 0x5A }; // MZ header disguising EXE
    //     maliciousExeAsStream.Write(bmpHeader, 0, bmpHeader.Length);
    //     maliciousExeAsStream.Write(mzHeaderAfterBmp, 0, mzHeaderAfterBmp.Length);
    //     byte[] dummyDataMalExe = new byte[1024];
    //     maliciousExeAsStream.Write(dummyDataMalExe, 0, dummyDataMalExe.Length);
    //     maliciousExeAsStream.Position = 0;
    //
    //     // Assert throws exception for malicious EXE-as-BMP file
    //     Assert.ThrowsException<ImageValidationException>(() => _thumbnailGenerationFunction.ValidateImageStructureAsync
    //         (maliciousExeAsStream));
    //     maliciousExeAsStream.Dispose();
    //
    //     // 7. Corrupted File Stream (Incomplete header)
    //     var corruptedStream = new MemoryStream();
    //     byte[] incompleteHeader = { 0x42 }; // Only part of BMP header
    //     corruptedStream.Write(incompleteHeader, 0, incompleteHeader.Length);
    //     byte[] dummyDataCorrupted = new byte[1024];
    //     corruptedStream.Write(dummyDataCorrupted, 0, dummyDataCorrupted.Length);
    //     corruptedStream.Position = 0;
    //
    //     // Assert throws exception for corrupted file
    //     Assert.ThrowsException<ImageValidationException>(() => _thumbnailGenerationFunction.ValidateImageStructureAsync
    //         (corruptedStream));
    // }

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
        var outputBlobStream = await _thumbnailGenerationFunction.GenerateThumbnailAsync(inputBlobStream);

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
        var result = _thumbnailGenerationFunction.GetThumbnailFileClient(
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
        await _thumbnailGenerationFunction.UploadThumbnailAsync(
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
        await _thumbnailGenerationFunction.UploadThumbnailAsync(
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
        await _thumbnailGenerationFunction.SetThumbnailMetadataAsync(mockDataLakeFileClient.Object);

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