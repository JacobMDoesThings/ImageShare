namespace ImageShare.WebApi.Services.Testing;

// Mock implementation for development/testing purposes
internal class MockUserContextProvider : IUserContextProvider
{
    private readonly string _userId;

    /// <summary>
    /// Initializes a new instance with the specified user ID.
    /// </summary>
    internal MockUserContextProvider(string userId)
    {
        _userId = userId;
    }

    /// <summary>
    /// Returns the pre-configured user ID for testing purposes.
    /// </summary>
    public string GetUserId()
    {
        return _userId;
    }
}