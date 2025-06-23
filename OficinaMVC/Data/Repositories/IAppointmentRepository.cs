using OficinaMVC.Data.Entities;

namespace OficinaMVC.Data.Repositories
{
    public interface IAppointmentRepository : IGenericRepository<Appointment>
    {
        //TODO: Adicionar métodos específicos para o repositório de agendamentos, (slots disponiveis, etc)
    }
}
