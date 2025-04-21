using LibraryManagementSystem.Models;

namespace LibraryManagementSystem.Repositories.Interfaces
{
    public interface IReservationRepository
    {
        Task<IEnumerable<Reservation>> GetAllReservations();
        Task<Reservation?> GetReservationById(int id);
        Task<Reservation?> GetNextPendingReservationForBook(int bookId);
        Task<bool> InsertReservation(Reservation reservation);
        Task<bool> UpdateReservation(Reservation reservation);
        Task<bool> DeleteReservation(int id);
        Task<IEnumerable<Reservation>> GetReservationsByUserId(int userId);
        Task<IEnumerable<Reservation>> GetReservationsByBookId(int bookId);
    }
}
