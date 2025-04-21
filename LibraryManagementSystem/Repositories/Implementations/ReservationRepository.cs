using LibraryManagementSystem.Models;
using LibraryManagementSystem.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using LibraryManagementSystem.Enums;

namespace LibraryManagementSystem.Repositories.Implementations
{
    public class ReservationRepository : IReservationRepository
    {
        private readonly LibraryDbContext _context;

        public ReservationRepository(LibraryDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Reservation>> GetAllReservations()
        {
            return await _context.Reservations
                .Include(r => r.User)
                .Include(r => r.Book)
                .Include(r => r.ReservationStatus)
                .ToListAsync();
        }

        public async Task<Reservation?> GetReservationById(int id)
        {
            return await _context.Reservations
                .Include(r => r.User)
                .Include(r => r.Book)
                .Include(r => r.ReservationStatus)
                .FirstOrDefaultAsync(r => r.ReservationId == id);
        }

        public async Task<Reservation?> GetNextPendingReservationForBook(int bookId)
        {
            try
            {
                return await _context.Reservations
                    .Where(r => r.BookId == bookId && r.ReservationStatusId == (int)ReservationStatusEnum.Pending)
                    .OrderBy(r => r.ReservedDate)
                    .Include(r => r.User)
                    .Include(r => r.Book)
                    .Include(r => r.ReservationStatus)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                // Log the exception here if needed
                throw;
            }
        }

        public async Task<IEnumerable<Reservation>> GetReservationsByBookId(int bookId)
        {
            return await _context.Reservations
                .Where(r => r.BookId == bookId)
                .Include(r => r.User)
                .Include(r => r.ReservationStatus)
                .ToListAsync();
        }

        public async Task<IEnumerable<Reservation>> GetReservationsByUserId(int userId)
        {
            return await _context.Reservations
                .Where(r => r.UserId == userId)
                .Include(r => r.Book)
                .Include(r => r.ReservationStatus)
                .ToListAsync();
        }

        public async Task<bool> InsertReservation(Reservation reservation)
        {
            await _context.Reservations.AddAsync(reservation);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateReservation(Reservation reservation)
        {
            _context.Reservations.Update(reservation);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteReservation(int id)
        {
            var reservation = await _context.Reservations.FindAsync(id);
            if (reservation == null) return false;

            _context.Reservations.Remove(reservation);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
