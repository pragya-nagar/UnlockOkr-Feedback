using System.Threading.Tasks;

namespace OKRFeedbackService.EF
{
    public interface IUnitOfWorkAsync : IUnitOfWork
    {
        Task<IOperationStatus> SaveChangesAsync();
        IRepositoryAsync<TEntity> RepositoryAsync<TEntity>() where TEntity : class, IObjectState;
    }
}
