using AgroTech.Infrastructure.Mongo.Documents;

namespace AgroTech.Application.Interfaces;

public interface ISensorReadingMongoRepository
{
    Task AddAsync(SensorReadingDocument document, CancellationToken cancellationToken = default);
}

