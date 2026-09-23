using GDB.App.Infrastructure.Repositories.Contracts;
using GDB.App.Infrastructure.Repositories.Implementations;

namespace GDB.App.Infrastructure.Repositories
{
    public static class TransactionRepositoryFactory
    {
        public static ITransactionRepository Create(string type)
        {
            if (type == "DB")
            {
                return new TransactionRepositoryDB();
            }

            else if (type == "InMemory")
            {
                return new TransactionRepositoryInMemory();
            }

            throw new Exception("Invalid transaction repository type");
        }
    }
}