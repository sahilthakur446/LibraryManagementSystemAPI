using LibraryManagementSystem.Enums;
using LibraryManagementSystem.Models;
using LibraryManagementSystem.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Repositories.Implementations
{
    public class FineRepository : IFineRepository
    {
        private readonly LibraryDbContext _context;

        public FineRepository(LibraryDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Fine?>> GetAll()
        {
            return await _context.Fines
                .Include(f => f.User) 
                .Include(f => f.FineStatus) 
                .ToListAsync();
        }

        public async Task<IEnumerable<Fine?>> GetAllUnPaidFine()
        {
            return await _context.Fines
                .Where(f => f.FineStatusId == (int) FineStatusEnum.Unpaid)
                .Include(f => f.User)
                .Include(f => f.FineStatus)
                .ToListAsync();
        }

        public async Task<Fine?> GetById(int id)
        {
            return await _context.Fines
                .Include(f => f.User)
                .Include(f => f.FineStatus)
                .FirstOrDefaultAsync(f => f.FineId == id);
        }

        public async Task<Fine?> GetUnpaidFineByUser(int userId)
        {
            return await _context.Fines
                .Where(f => f.UserId == userId && f.FineStatusId == (int)FineStatusEnum.Unpaid).FirstOrDefaultAsync();
        }

        public async Task<bool> Insert(Fine entity)
        {
            _context.Fines.Add(entity);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> Update(Fine entity)
        {
            _context.Fines.Update(entity);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> Delete(int id)
        {
            var fine = await _context.Fines.FindAsync(id);
            if (fine == null)
                return false;

            _context.Fines.Remove(fine);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
