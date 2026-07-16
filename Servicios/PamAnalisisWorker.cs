namespace NSIE.Servicios
{
    public sealed class PamAnalisisWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<PamAnalisisWorker> _logger;

        public PamAnalisisWorker(IServiceScopeFactory scopeFactory, ILogger<PamAnalisisWorker> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                PamAnalisisClaim claim = null;
                try
                {
                    using (var scope = _scopeFactory.CreateScope())
                    {
                        var service = scope.ServiceProvider.GetRequiredService<IPamAnalisisService>();
                        claim = await service.TomarSiguienteAsync(stoppingToken);
                    }

                    if (claim == null)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(3), stoppingToken);
                        continue;
                    }

                    using var processingScope = _scopeFactory.CreateScope();
                    var processor = processingScope.ServiceProvider.GetRequiredService<IPamAnalisisService>();
                    try
                    {
                        await processor.ProcesarAsync(claim.AnalisisId, claim.LeaseUid, stoppingToken);
                    }
                    catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                    {
                        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                        try { await processor.ReencolarAsync(claim.AnalisisId, claim.LeaseUid, timeout.Token); }
                        catch (Exception ex) { _logger.LogWarning(ex, "No fue posible reencolar el análisis PAM {AnalisisId} durante el cierre.", claim.AnalisisId); }
                        break;
                    }
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error en el worker de análisis PAM.");
                    try { await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken); }
                    catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { break; }
                }
            }
        }
    }
}
