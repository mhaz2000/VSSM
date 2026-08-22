using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VSSM.Application.DTOs.BaseEntities.Consumer;
using VSSM.Application.Interfaces.BaseEntities;

namespace VSSM.Presentation.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ConsumerController : ControllerBase
{
    private readonly IConsumerService _consumerService;

    public ConsumerController(IConsumerService consumerService)
    {
        _consumerService = consumerService;
    }

    [HttpPost("consume")]
    [Authorize(AuthenticationSchemes = "Basic")]
    public async Task<IActionResult> ConsumeUid([FromBody] ConsumeUidRequest request, CancellationToken token)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new ConsumeUidResponse
            {
                StatusCode = "100",
                StatusDesc = "Ù¾Ø§Ø±Ø§Ù…ØªØ±Ù‡Ø§ÛŒ ÙˆØ±ÙˆØ¯ÛŒ Ù†Ø§Ù…Ø¹ØªØ¨Ø± Ø§Ø³Øª"
            });
        }

        var result = await _consumerService.ConsumeUidAsync(request, token);

        return Ok(result);
    }
}
