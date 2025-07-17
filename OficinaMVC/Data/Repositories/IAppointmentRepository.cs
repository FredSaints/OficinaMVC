using OficinaMVC.Data.Entities;

namespace OficinaMVC.Data.Repositories
{
    public interface IAppointmentRepository : IGenericRepository<Appointment>
    {

        Task<Appointment> GetByIdWithDetailsAsync(int appointmentId);
        Task<IEnumerable<Appointment>> GetByClientIdAsync(string clientId, bool includeCompleted);
        Task<IEnumerable<User>> GetAvailableMechanicsAsync(DateTime appointmentDate);
        Task<IEnumerable<string>> GetUnavailableDaysAsync(int year, int month);
        Task<bool> IsMechanicAvailableAtTimeAsync(string mechanicId, DateTime appointmentDate);
        Task<Appointment> CreateAndReturnAsync(Appointment appointment);
    }
}
