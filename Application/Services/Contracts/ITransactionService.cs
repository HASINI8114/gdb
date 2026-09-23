using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GDB.App.Application.Dtos;
using GDB.App.Domain.Enums;
using GDB.App.Application.Dtos;

namespace GDB.App.Application.Services.Contracts
{
    public interface ITransactionService
    {
        List<ViewRecentTransactionsResponseDto> GetRecentTransactions(
           string accountNumber);
        DepositResponseDto Deposit(string accountNumber, decimal amount);

        WithdrawResponseDto Withdraw(string accountNumber, string pin, decimal amount);

        TranferFundsResponseDto TransferFunds(
            string fromAccountNumber,
            string toAccountNumber,
            string pin,
            decimal amount);
    }
}
