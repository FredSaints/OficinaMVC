using Microsoft.EntityFrameworkCore;
using OficinaMVC.Data.Entities;
using OficinaMVC.Models.Mechanics;

namespace OficinaMVC.Data.Repositories
{
    public class MechanicRepository : IMechanicRepository
    {
        private readonly DataContext _context;

        public MechanicRepository(DataContext context)
        {
            _context = context;
        }

        public async Task<User> GetByIdWithDetailsAsync(string mechanicId)
        {
            return await _context.Users
                .Include(u => u.UserSpecialties)
                    .ThenInclude(us => us.Specialty)
                .Include(u => u.Schedules)
                .FirstOrDefaultAsync(u => u.Id == mechanicId);
        }

        public async Task<(bool Success, string ErrorMessage)> UpdateMechanicAsync(MechanicEditViewModel model)
        {
            // --- START OF VALIDATION LOGIC ---
            if (model.Schedules != null && model.Schedules.Any())
            {
                var schedulesByDay = model.Schedules
                    .GroupBy(s => s.DayOfWeek)
                    .ToList();

                foreach (var dayGroup in schedulesByDay)
                {
                    var timeSlots = dayGroup.OrderBy(s => s.StartTime).ToList();

                    // Check for basic invalid slots where end time is before start time
                    foreach (var slot in timeSlots)
                    {
                        if (slot.EndTime <= slot.StartTime)
                        {
                            return (false, $"On {slot.DayOfWeek}, the end time must be after the start time.");
                        }
                    }

                    // Check for overlaps within the same day
                    for (int i = 0; i < timeSlots.Count - 1; i++)
                    {
                        // Classic overlap check: if the end time of the current slot
                        // is after the start time of the next slot, they overlap.
                        if (timeSlots[i].EndTime > timeSlots[i + 1].StartTime)
                        {
                            return (false, $"The schedule for {dayGroup.Key} has overlapping time slots.");
                        }
                    }
                }
            }
            // --- END OF VALIDATION LOGIC ---

            var mechanic = await _context.Users.FirstOrDefaultAsync(u => u.Id == model.UserId);
            if (mechanic == null)
            {
                // Returning a specific error for this case is better practice.
                return (false, "Mechanic not found.");
            }

            var oldSpecialties = _context.UserSpecialties.Where(us => us.UserId == mechanic.Id);
            _context.UserSpecialties.RemoveRange(oldSpecialties);

            var oldSchedules = _context.Schedules.Where(s => s.UserId == mechanic.Id);
            _context.Schedules.RemoveRange(oldSchedules);

            var selectedSpecialtyIds = model.SelectedSpecialtyIds ?? new List<int>();
            foreach (var specialtyId in selectedSpecialtyIds)
            {
                _context.UserSpecialties.Add(new UserSpecialty { UserId = mechanic.Id, SpecialtyId = specialtyId });
            }

            if (model.Schedules != null && model.Schedules.Any())
            {
                foreach (var sched in model.Schedules)
                {
                    _context.Schedules.Add(new Schedule
                    {
                        DayOfWeek = sched.DayOfWeek,
                        StartTime = sched.StartTime,
                        EndTime = sched.EndTime,
                        UserId = mechanic.Id
                    });
                }
            }

            await _context.SaveChangesAsync();
            return (true, null); // Return success with no error message
        }
    }
}
