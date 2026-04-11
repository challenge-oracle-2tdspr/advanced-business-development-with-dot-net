using AgroTech.Worker.Alerts.Models;

namespace AgroTech.Worker.Alerts.Repositories
{
    public interface IAlertEventRepository
    {
        Task SaveAsync(AlertEventRecord alert, CancellationToken cancellationToken);
    }
}