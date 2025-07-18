namespace OficinaMVC.Services
{
    public interface ICommunicationService
    {
        Task<int> BulkCancelAppointmentsForMechanicAsync(string mechanicId, string connectionId, string baseUrl);
    }
}
