
using AttendanceAPP.Context;
using AttendanceAPP.IRepository;
using AttendanceAPP.Model;

namespace AuthorsAPI.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
        private IGenericRepository<UserModel> _users;


        public UnitOfWork(AppDbContext context)
        {
            _context = context;
        }

        public IGenericRepository<UserModel> Users => _users ??= new GenericRepository<UserModel>(_context);

        public void Dispose()
        {
            _context.Dispose();
            GC.SuppressFinalize(this);
        }

        public async Task Save()
        {
            await _context.SaveChangesAsync();
        }
    }
}
