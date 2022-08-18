using AttendanceAPP.Model;

namespace AttendanceAPP.IRepository
{
    public interface IUnitOfWork : IDisposable
    {
        IGenericRepository<UserModel> Users { get; }


    Task Save();
}
}