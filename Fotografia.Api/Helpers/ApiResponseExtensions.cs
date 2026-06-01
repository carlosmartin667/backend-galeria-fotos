using Fotografia.Application.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace Fotografia.Api.Helpers;

public static class ApiResponseExtensions
{
    public static IActionResult ToActionResult<T>(this ControllerBase controller, ApiResponse<T> result)
    {
        if (result.Success)
        {
            return controller.Ok(result);
        }

        return result.StatusCode switch
        {
            StatusCodes.Status401Unauthorized => controller.Unauthorized(result),
            StatusCodes.Status403Forbidden => controller.Forbid(),
            StatusCodes.Status404NotFound => controller.NotFound(result),
            _ => controller.BadRequest(result)
        };
    }
}
