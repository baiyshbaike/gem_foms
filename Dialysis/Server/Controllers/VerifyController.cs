using Dialysis.Server.Domain.Services;
using Dialysis.Shared.Dto;
using Dialysis.Server.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Memory;

namespace Dialysis.Server.Controllers;
using Microsoft.AspNetCore.Mvc;
[ApiController]
[Route("api/verify")] 
public class VerifyController : Controller
{
    private readonly IMemoryCache _cache;
    private readonly IHubContext<VerificationHub> _hubContext;
    private readonly string _secretKey;
    private readonly IVerifyService _verifyService;
    
    public VerifyController(IMemoryCache cache, IHubContext<VerificationHub> hubContext, IConfiguration configuration,IVerifyService verifyService)
    {
        _cache = cache;
        _hubContext = hubContext;
        _secretKey = configuration.GetValue<string>("VerificationSecretKey") ?? "b020985d3861b4f77ec233e1795b8de0";
        _verifyService = verifyService;
    }
    [HttpPost("request")]
    public ActionResult<RequestVerificationResponseDto> RequestVerification()
    {
        var sessionId = Guid.NewGuid().ToString();
        var expiryTime = DateTime.Now.AddMinutes(5);

        var response = new RequestVerificationResponseDto
        {
            SessionId = sessionId,
            Expires = expiryTime,
        };

        var cacheEntryOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromMinutes(5)); 
        
        _cache.Set(sessionId, expiryTime, cacheEntryOptions);

        return Ok(response);
    }
    [HttpGet("tunduk_status")]
    public async Task<ActionResult<bool>> TundukStatus()
    {
        var result = await _verifyService.GetTundukStatus();
        return Ok(result);
    }

    [HttpPost("set_tunduk_status")]
    public async Task<ActionResult<bool>> SetTundukStatus([FromBody] bool status)
    {
        var result = await _verifyService.SetTundukStatus(status);
        return Ok(result);
    }
    [HttpPost("confirm")]
    public IActionResult ConfirmVerification([FromBody] ConfirmDto dto)
    {
        if (dto.SecretKey != _secretKey)
        {
            return BadRequest(new { status = false, message = "Неверный секретный ключ." });
        }

        // 2. Session ID'nin Geçerliliğini Kontrol Et
        if (!_cache.TryGetValue(dto.SessionId, out DateTime serverExpiryTime))
        {
            // Session ID ya hiç oluşturulmadı ya da süresi geçip cache'ten silindi.
            return NotFound(new { status = false, message = "Недействительный или просроченный сеанс." });
        }

        // 3. Sürenin Dolup Dolmadığını Kontrol Et
        if (DateTime.Now > serverExpiryTime)
        {
            // Ekstra bir kontrol, cache'ten silinmemiş olsa bile süresi dolmuş olabilir.
            _cache.Remove(dto.SessionId); // Temizlik
            return BadRequest(new { status = false, message = "Сессия истекла." });
        }

        // Her şey başarılı! Doğrulama tamamlandı.
        _cache.Remove(dto.SessionId); // Tekrar kullanılmasını önle

        // SignalR ile ilgili istemciye bildir gönder
        _hubContext.Clients.Group(dto.SessionId).SendAsync("VerificationCompleted", dto.Status);

        return Ok(new { status = dto.Status, message = dto.Status ? "Проверка прошла успешно." : "Проверка не пройдена." });
    }
}