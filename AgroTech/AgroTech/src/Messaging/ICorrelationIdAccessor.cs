namespace AgroTech.Messaging
{
    public interface ICorrelationIdAccessor
    {
        string GetCorrelationId();
    }
}