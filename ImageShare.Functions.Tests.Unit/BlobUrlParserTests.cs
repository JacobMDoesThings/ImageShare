using ImageShare.Functions.Data;
using ImageShare.Functions.Services;

namespace ImageShare.Functions.Tests.Unit;

[TestClass]
public class BlobUrlParserTests
{
    private BlobUrlParser _parser;
    [TestInitialize]
    public void Setup()
    {
        _parser = new BlobUrlParser();
    }

    [TestMethod]
    public void ParseBlob_Correctly_Parses_Well_Formed_Path()
    {
        const string container = "container-name";
        const string user = "user-name";
        const string name = "file-name";
        const string publicPath = $"https://account.dfs.core.windows.net/{container}/public/images/{user}/{name}";
        const string privatePath = $"https://account.dfs.core.windows.net/{container}/{user}/images/{name}";
        
        var pubExpectedBlobData = new BlobData
        {
            BlobUrlString = $"/public/images/{user}/{name}",
            ContainerName = container,
            IsPublic = true,
            Name = name,
            User = user
        };
        
        var privExpectedBlobData = new BlobData
        {
            BlobUrlString = $"/{user}/images/{name}",
            ContainerName = container,
            IsPublic = false,
            Name = name,
            User = user
        };
        
        var outputBlobData = _parser.ParseBlob(new Uri(publicPath));

        Assert.AreEqual(pubExpectedBlobData.BlobUrlString, outputBlobData.BlobUrlString);
        Assert.AreEqual(pubExpectedBlobData.ContainerName, outputBlobData.ContainerName);
        Assert.AreEqual(pubExpectedBlobData.IsPublic, outputBlobData.IsPublic);
        Assert.AreEqual(pubExpectedBlobData.Name, outputBlobData.Name);
        Assert.AreEqual(pubExpectedBlobData.User, outputBlobData.User);
        
        outputBlobData = _parser.ParseBlob(new Uri(privatePath));
        
        Assert.AreEqual(privExpectedBlobData.BlobUrlString, outputBlobData.BlobUrlString);
        Assert.AreEqual(privExpectedBlobData.ContainerName, outputBlobData.ContainerName);
        Assert.AreEqual(privExpectedBlobData.IsPublic, outputBlobData.IsPublic);
        Assert.AreEqual(privExpectedBlobData.Name, outputBlobData.Name);
        Assert.AreEqual(privExpectedBlobData.User, outputBlobData.User);
    }
}