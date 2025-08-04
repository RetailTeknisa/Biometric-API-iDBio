using ControliD;
using System.Threading;

namespace BiometricApi.Services;

public class BiometricService : IDisposable
{
    private readonly ILogger<BiometricService> _logger;
    private readonly CIDBio _idbio;
    
    // Controle para garantir que a inicialização só aconteça uma vez
    private static readonly object _initLock = new object();
    private static bool _isInitialized = false;

    // O construtor agora é super rápido, não faz nada pesado.
    public BiometricService(ILogger<BiometricService> logger)
    {
        _logger = logger;
        _idbio = new CIDBio();
    }

    // Este método privado faz o trabalho pesado, mas só é chamado quando necessário.
    private void InitializeReader()
    {
        // Garante que mesmo com múltiplas requisições simultâneas,
        // a inicialização só será tentada uma vez.
        lock (_initLock)
        {
            if (_isInitialized) return;

            _logger.LogInformation("Primeira requisição recebida. Inicializando o leitor biométrico...");
            try
            {
                var ret = CIDBio.Init();
                if (ret >= RetCode.SUCCESS)
                {
                    _isInitialized = true;
                    _logger.LogInformation("Leitor biométrico inicializado com sucesso.");
                }
                else
                {
                    _isInitialized = false;
                    _logger.LogError("Falha ao inicializar leitor. Código: {ret}, Mensagem: {msg}", ret, CIDBio.GetErrorMessage(ret));
                }
            }
            catch (Exception ex)
            {
                _isInitialized = false;
                _logger.LogError(ex, "Exceção fatal ao inicializar o leitor biométrico.");
            }
        }
    }
    
    // Os métodos públicos agora garantem que o leitor está inicializado antes de agir.
    public (RetCode code, object? data) CaptureTemplateAndImage()
    {
        InitializeReader(); // Garante a inicialização na primeira chamada

        if (!_isInitialized)
        {
            return (RetCode.ERROR_UNKNOWN, null);
        }
        
        // O restante do método de captura continua igual, mas sem o Init/Terminate
        try
        {
            var ret = _idbio.CaptureImageAndTemplate(out string? template, out byte[]? image, out uint width, out uint height, out int quality);

            if (ret < RetCode.SUCCESS)
            {
                _logger.LogWarning("Falha na captura direta: {msg}", CIDBio.GetErrorMessage(ret));
                return (ret, null);
            }
            
            return (ret, template);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exceção durante o processo de captura com imagem.");
            return (RetCode.ERROR_UNKNOWN, null);
        }
    }
    
    public void Dispose()
    {
        // O Dispose agora só precisa se preocupar em finalizar, se foi inicializado.
        if (_isInitialized)
        {
            CIDBio.Terminate();
            _logger.LogInformation("Leitor biométrico finalizado na conclusão da API.");
        }
    }
}