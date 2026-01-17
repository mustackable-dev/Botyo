using Botyo.Models;
using Microsoft.AspNetCore.Mvc;

namespace Botyo.Controllers;

[ApiController]
public class BaseController : ControllerBase
{
    protected IActionResult FromResult<T>(Result<T> result)
        => result.Error is null ?
            StatusCode(result.StatusCode, result.Payload) :
            StatusCode(result.StatusCode, string.Join(" - ", new[] {result.Error.Message, result.Error.InnerException?.Message}.Where(x=>x is not null)));
}