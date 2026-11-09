using System.Security.Claims;

namespace Library.API.Extensions;

public static class ClaimsPrincipalExtensions
{
    // The JWT stores the user id in the "sub" claim
    public static int GetUserId(this ClaimsPrincipal user)
    {
        var value = user.FindFirstValue("sub");
        return int.TryParse(value, out var id) ? id : throw new UnauthorizedAccessException("Invalid token.");
    }
}
