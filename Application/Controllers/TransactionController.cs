using GDB.App.Application.Services.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GDB.App.Domain.Enums;
using GDB.App.Application.Services;
using GDB.App.Application.Dtos;

namespace GDB.App.Application.Controllers
{
    internal class TransactionController
    {
        private readonly ITransactionService _transactionService;

        public TransactionController()
        {
            _transactionService = TransactionServiceFactory.Create();
        }

        public DepositResponseDto Deposit(string accountNumber, decimal amount)
        {
            return _transactionService.Deposit(accountNumber, amount);
        }

        public WithdrawResponseDto Withdraw(string accountNumber,string pin,decimal amount)
        {
           return _transactionService.Withdraw(accountNumber,pin,amount);

        }

        public TranferFundsResponseDto TransferFunds(
            string fromAccountNumber,
            string toAccountNumber,
            string pin,
            decimal amount)
        {
           return _transactionService.TransferFunds(
                                   fromAccountNumber,
                                   toAccountNumber,
                                   pin,
                                   amount
                               );
        }
        public List<ViewRecentTransactionsResponseDto>
            GetRecentTransactions(string accountNumber)
        {
            return _transactionService.GetRecentTransactions(
                accountNumber);
        }
    }
}
