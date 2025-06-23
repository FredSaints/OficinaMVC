using OficinaMVC.Data.Entities;

namespace OficinaMVC.Data.Repositories
{
    public class AppointmentRepository : GenericRepository<Appointment>, IAppointmentRepository
    {
        public AppointmentRepository(DataContext context) : base(context)
        {
        }
    }
}
