namespace ImageShare.WebApi.Services;

public class EntraUserContextProvider : IUserContextProvider
{
    public string GetUserId()
    {
        // Implementation to extract user ID from Entra ID token
        // This could involve HttpContext, ClaimsPrincipal, etc.
        throw new NotImplementedException("Entra ID integration not yet implemented");
    }
}
