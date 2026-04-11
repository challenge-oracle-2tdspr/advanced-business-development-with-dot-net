using AgroTech.Worker.Recommendations.Models;

namespace AgroTech.Worker.Recommendations.Repositories
{
    public interface IRecommendationEventRepository
    {
        Task SaveAsync(RecommendationEventRecord recommendation, CancellationToken cancellationToken);
    }
}