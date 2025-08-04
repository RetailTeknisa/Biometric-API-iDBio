using Microsoft.AspNetCore.Mvc;
using ControliD;

namespace BiometricApi.Controllers;
using BiometricApi.Services;

[ApiController]
[Route("api/biometric")] // Rota simplificada
public class BiometricController : ControllerBase
{
    private readonly BiometricService _biometricService;

    public BiometricController(BiometricService biometricService)
    {
        _biometricService = biometricService;
    }

    [HttpGet("status")]
    public string GetStatus() => "API funcionando!";
    
    // =================================================================
    // Endpoint para captura de imagem e template biométrico
    // =================================================================
    [HttpPost("capture-template")]
    public IActionResult CaptureWithImage()
    {
        try
        {
            var (code, template) = _biometricService.CaptureTemplateAndImage();

            if (code < RetCode.SUCCESS)
            {
                string errorMessage = $"Falha na operação: {CIDBio.GetErrorMessage(code)}";
                return BadRequest(new { Status = (int)code, Message = errorMessage });
            }

            return Ok(new
            {
                Status = (int)code,
                Message = "Captura com imagem realizada com sucesso.",
                template
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "Ocorreu um erro interno inesperado.", Details = ex.Message });
        }
    }
}