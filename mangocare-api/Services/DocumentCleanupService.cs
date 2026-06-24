using mangocare_api.Data;
using Microsoft.EntityFrameworkCore;

namespace mangocare_api.Services
{
    public class DocumentCleanupService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<DocumentCleanupService> _logger;

        public DocumentCleanupService(
            IServiceScopeFactory scopeFactory,
            IWebHostEnvironment environment,
            ILogger<DocumentCleanupService> logger)
        {
            _scopeFactory = scopeFactory;
            _environment = environment;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(
            CancellationToken stoppingToken)
        {
            // Run once immediately whenever the API starts.
            await CleanupExpiredDocumentsAsync(stoppingToken);

            // Then run once every 24 hours.
            using var timer = new PeriodicTimer(TimeSpan.FromHours(24));

            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                await CleanupExpiredDocumentsAsync(stoppingToken);
            }
        }

        private async Task CleanupExpiredDocumentsAsync(
            CancellationToken stoppingToken)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();

                var db = scope.ServiceProvider
                    .GetRequiredService<MangoCareDbContext>();

                var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);

                var expiredDocuments = await db.MedicalDocuments
                    .Where(document =>
                        document.IsDeleted &&
                        document.DeletedAt.HasValue &&
                        document.DeletedAt.Value <= thirtyDaysAgo)
                    .ToListAsync(stoppingToken);

                foreach (var document in expiredDocuments)
                {
                    var filePath = Path.Combine(
                        _environment.ContentRootPath,
                        "Uploads",
                        document.StoredFileName
                    );

                    try
                    {
                        if (System.IO.File.Exists(filePath))
                        {
                            System.IO.File.Delete(filePath);
                        }

                        db.MedicalDocuments.Remove(document);

                        _logger.LogInformation(
                            "Permanently removed expired document {DocumentId}",
                            document.Id
                        );
                    }
                    catch (Exception exception)
                    {
                        _logger.LogError(
                            exception,
                            "Could not permanently remove document {DocumentId}",
                            document.Id
                        );
                    }
                }

                await db.SaveChangesAsync(stoppingToken);
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Document cleanup service failed."
                );
            }
        }
    }
}