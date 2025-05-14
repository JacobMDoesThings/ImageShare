namespace ImageShare.WebApi.Services;

// Interface contract matching production specifications
public interface IUserContextProvider
{
    /// <summary>
    /// Gets the unique identifier of the currently authenticated user.
    /// </summary>
    string GetUserId();
}
