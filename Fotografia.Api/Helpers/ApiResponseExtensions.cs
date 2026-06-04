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
            StatusCodes.Status403Forbidden => controller.StatusCode(StatusCodes.Status403Forbidden, result),
            StatusCodes.Status404NotFound => controller.NotFound(result),
            StatusCodes.Status409Conflict => controller.Conflict(result),
            StatusCodes.Status500InternalServerError => controller.StatusCode(StatusCodes.Status500InternalServerError, result),
            StatusCodes.Status502BadGateway => controller.StatusCode(StatusCodes.Status502BadGateway, result),
            StatusCodes.Status503ServiceUnavailable => controller.StatusCode(StatusCodes.Status503ServiceUnavailable, result),
            _ => controller.BadRequest(result)
        };
    }
}
