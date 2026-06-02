using AutoCheckAML.Api.Data.Repository;
using AutoCheckAML.Api.Entity;

namespace AutoCheckAML.Api.Data.UnitOfWork
{
    /// <summary>
    /// Unit of Work Pattern - Coordina múltiples repositorios en una transacción
    /// </summary>
    public interface IUnitOfWork : IAsyncDisposable
    {
        IRepository<User> Users { get; }
        IRepository<FormSubmission> FormSubmissions { get; }

        Task<int> SaveChangesAsync();
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }

    /// <summary>
    /// Implementación del Unit of Work
    /// </summary>
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AutoCheckAMLContext _context;
        private IRepository<User> _userRepository;
        private IRepository<FormSubmission> _formSubmissionRepository;

        public UnitOfWork(AutoCheckAMLContext context)
        {
            _context = context;
        }

        public IRepository<User> Users 
        {
            get
            {
                _userRepository ??= new Repository<User>(_context);
                return _userRepository;
            }
        }

        public IRepository<FormSubmission> FormSubmissions 
        {
            get
            {
                _formSubmissionRepository ??= new Repository<FormSubmission>(_context);
                return _formSubmissionRepository;
            }
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task BeginTransactionAsync()
        {
            await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            try
            {
                await _context.SaveChangesAsync();
                await _context.Database.CommitTransactionAsync();
            }
            catch
            {
                await RollbackTransactionAsync();
                throw;
            }
        }

        public async Task RollbackTransactionAsync()
        {
            await _context.Database.RollbackTransactionAsync();
        }

        public async ValueTask DisposeAsync()
        {
            await _context.DisposeAsync();
        }
    }
}
