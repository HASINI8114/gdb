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

        public async Task<DepositResponseDto> DepositAsync(string accountNumber, decimal amount)
        {
            return await _transactionService.DepositAsync(accountNumber, amount);
        }

        public async Task<WithdrawResponseDto> WithdrawAsync(string accountNumber,string pin,decimal amount)
        {
           return await _transactionService.WithdrawAsync(accountNumber,pin,amount);

        }

        public async Task<TranferFundsResponseDto> TransferFundsAsync(
            string fromAccountNumber,
            string toAccountNumber,
            string pin,
            decimal amount)
        {
           return await _transactionService.TransferFundsAsync(
                                   fromAccountNumber,
                                   toAccountNumber,
                                   pin,
                                   amount
                               );
        }
        public async Task<List<ViewRecentTransactionsResponseDto>>
            GetRecentTransactionsAsync(string accountNumber)
        {
            return await _transactionService.GetRecentTransactionsAsync(
                accountNumber);
        }
    }
}
