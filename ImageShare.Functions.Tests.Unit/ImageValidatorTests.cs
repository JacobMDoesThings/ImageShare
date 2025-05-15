using System.Text;
using ImageShare.Functions.Services;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace ImageShare.Functions.Tests.Unit;

[TestClass]
public class ImageValidatorTests
{
    private ImageValidator _imageValidator;
    [TestInitialize]
    public void Setup()
    {
        _imageValidator = new ImageValidator();
    }
    
    [TestMethod]
    public async Task Test_ImageValidation()
    {
        // 1. Text File Stream
        var textStream = new MemoryStream();
        var textData = Encoding.UTF8.GetBytes("This is plain text.");
        textStream.Write(textData, 0, textData.Length);
        textStream.Position = 0;

        // Assert throws exception for text file
        await Assert.ThrowsExceptionAsync<ImageValidationException>(() =>
            _imageValidator.Validate(textStream));

        // Reset stream position for next test case
        await textStream.DisposeAsync();

        // 2. PDF Stream
        var pdfStream = new MemoryStream();
        var pdfHeader = Encoding.ASCII.GetBytes("%PDF-1.4\n%âãÏÓ");
        pdfStream.Write(pdfHeader, 0, pdfHeader.Length);
        var dummyData = new byte[1024]; // Simulate PDF content
        pdfStream.Write(dummyData, 0, dummyData.Length);
        pdfStream.Position = 0;

        // Assert throws exception for PDF file
        await Assert.ThrowsExceptionAsync<ImageValidationException>(() =>
            _imageValidator.Validate(pdfStream));
        await pdfStream.DisposeAsync();

        // 3. DOCX Stream (ZIP archive header)
        var docxStream = new MemoryStream();
        byte[] zipHeader = "PK"u8.ToArray(); // PK header for ZIP archives
        docxStream.Write(zipHeader, 0, zipHeader.Length);
        var dummyDataDocx = new byte[1024];
        docxStream.Write(dummyDataDocx, 0, dummyDataDocx.Length);
        docxStream.Position = 0;

        // Assert throws exception for DOCX file
        await Assert.ThrowsExceptionAsync<ImageValidationException>(() =>_imageValidator.Validate(docxStream));
        await docxStream.DisposeAsync();

        // 4. EXE Stream (MZ header)
        var exeStream = new MemoryStream();
        var mzHeader = "MZ"u8.ToArray(); // MZ header for EXE files
        exeStream.Write(mzHeader, 0, mzHeader.Length);
        var dummyDataExe = new byte[1024];
        exeStream.Write(dummyDataExe, 0, dummyDataExe.Length);
        exeStream.Position = 0;

        // Assert throws exception for EXE file
        await Assert.ThrowsExceptionAsync<ImageValidationException>(() => _imageValidator.Validate(exeStream));
        await exeStream.DisposeAsync();

        // 5. Malicious PDF Stream (disguised JavaScript payload)
        var maliciousPdfStream = new MemoryStream();
        var maliciousHeaderAndJs = Encoding.ASCII.GetBytes("%PDF-1.4\n%âãÏÓ\n/JS << /S /JavaScript >>");
        maliciousPdfStream.Write(maliciousHeaderAndJs, 0, maliciousHeaderAndJs.Length);
        var dummyDataMalPdf = new byte[1024];
        maliciousPdfStream.Write(dummyDataMalPdf, 0, dummyDataMalPdf.Length);
        maliciousPdfStream.Position = 0;

        // Assert throws exception for malicious PDF file
        await Assert.ThrowsExceptionAsync<ImageValidationException>(() => _imageValidator.Validate(maliciousPdfStream));
        await maliciousPdfStream.DisposeAsync();

        // 6. Malicious EXE Disguised as BMP Stream
        var maliciousExeAsStream = new MemoryStream();
        var bmpHeader = "BM"u8.ToArray(); // BMP header
        var mzHeaderAfterBmp = "MZ"u8.ToArray(); // MZ header disguising EXE
        maliciousExeAsStream.Write(bmpHeader, 0, bmpHeader.Length);
        maliciousExeAsStream.Write(mzHeaderAfterBmp, 0, mzHeaderAfterBmp.Length);
        var dummyDataMalExe = new byte[1024];
        maliciousExeAsStream.Write(dummyDataMalExe, 0, dummyDataMalExe.Length);
        maliciousExeAsStream.Position = 0;

        // Assert throws exception for malicious EXE-as-BMP file
        await Assert.ThrowsExceptionAsync<ImageValidationException>(() => _imageValidator.Validate(maliciousExeAsStream));
        await maliciousExeAsStream.DisposeAsync();

        // 7. Corrupted File Stream (Incomplete header)
        var corruptedStream = new MemoryStream();
        byte[] incompleteHeader = [0x42]; // Only part of BMP header
        corruptedStream.Write(incompleteHeader, 0, incompleteHeader.Length);
        var dummyDataCorrupted = new byte[1024];
        corruptedStream.Write(dummyDataCorrupted, 0, dummyDataCorrupted.Length);
        corruptedStream.Position = 0;

        // Assert throws exception for corrupted file
        await Assert.ThrowsExceptionAsync<ImageValidationException>(() => _imageValidator.Validate(corruptedStream));
        
        using var jpegImage = new Image<Rgba32>(50, 50);
        jpegImage.Mutate(ctx => ctx.Fill(Color.Red)); 
        var inputBlobStream = new MemoryStream();
        await jpegImage.SaveAsJpegAsync(inputBlobStream);
        inputBlobStream.Position = 0;
        await _imageValidator.Validate(inputBlobStream);
        
        
        using var pngImage = new Image<Rgba32>(50, 50);
        pngImage.Mutate(ctx => ctx.Fill(Color.Red)); 
        inputBlobStream = new MemoryStream();
        await pngImage.SaveAsPngAsync(inputBlobStream);
        inputBlobStream.Position = 0;
        await _imageValidator.Validate(inputBlobStream);
        
        using var pbmImage = new Image<Rgba32>(50, 50);
        pbmImage.Mutate(ctx => ctx.Fill(Color.Red)); 
        inputBlobStream = new MemoryStream();
        await pbmImage.SaveAsPbmAsync(inputBlobStream);
        inputBlobStream.Position = 0;
        await _imageValidator.Validate(inputBlobStream);
        
        using var gifImage = new Image<Rgba32>(50, 50);
        gifImage.Mutate(ctx => ctx.Fill(Color.Red)); 
        inputBlobStream = new MemoryStream();
        await gifImage.SaveAsGifAsync(inputBlobStream);
        inputBlobStream.Position = 0;
        await _imageValidator.Validate(inputBlobStream);
        
        using var tiffImage = new Image<Rgba32>(50, 50);
        tiffImage.Mutate(ctx => ctx.Fill(Color.Red)); 
        inputBlobStream = new MemoryStream();
        await tiffImage.SaveAsTiffAsync(inputBlobStream);
        inputBlobStream.Position = 0;
        await _imageValidator.Validate(inputBlobStream);
        
        using var bmpImage = new Image<Rgba32>(50, 50);
        bmpImage.Mutate(ctx => ctx.Fill(Color.Red)); 
        inputBlobStream = new MemoryStream();
        await bmpImage.SaveAsBmpAsync(inputBlobStream);
        inputBlobStream.Position = 0;
        await _imageValidator.Validate(inputBlobStream);
        
        using var qoiImage = new Image<Rgba32>(50, 50);
        qoiImage.Mutate(ctx => ctx.Fill(Color.Red)); 
        inputBlobStream = new MemoryStream();
        await qoiImage.SaveAsQoiAsync(inputBlobStream);
        inputBlobStream.Position = 0;
        await _imageValidator.Validate(inputBlobStream);
        
        using var tgaImage = new Image<Rgba32>(50, 50);
        tgaImage.Mutate(ctx => ctx.Fill(Color.Red)); 
        inputBlobStream = new MemoryStream();
        await tgaImage.SaveAsTgaAsync(inputBlobStream);
        inputBlobStream.Position = 0;
        await _imageValidator.Validate(inputBlobStream);
        
        using var webpImage = new Image<Rgba32>(50, 50);
        webpImage.Mutate(ctx => ctx.Fill(Color.Red)); 
        inputBlobStream = new MemoryStream();
        await webpImage.SaveAsWebpAsync(inputBlobStream);
        inputBlobStream.Position = 0;
        await _imageValidator.Validate(inputBlobStream);
    }
}