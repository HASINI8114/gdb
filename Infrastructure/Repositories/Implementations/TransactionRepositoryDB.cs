using GDB.App.Application.Dtos;
using GDB.App.Domain.Enums;
using GDB.App.Infrastructure.Repositories.Contracts;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace GDB.App.Infrastructure.Repositories.Implementations
{
    public class TransactionRepositoryDB : ITransactionRepository
    {
        public List<ViewRecentTransactionsResponseDto> GetRecentTransactions(
            string accountNumber)
        {
            List<ViewRecentTransactionsResponseDto> transactions =
                new List<ViewRecentTransactionsResponseDto>();

            using (DbConnection connection =
                   DataBaseConnectionManager.GetConnection())
            {
                connection.Open();

                string query = @"
                    SELECT TOP 10

                        t.TransactionId,

                        fromAccount.AccountNumber AS FromAccountNumber,

                        toAccount.AccountNumber AS ToAccountNumber,

                        t.Amount,

                        tt.Code AS TransactionType,

                        ts.Code AS TransactionStatus,

                        t.Timestamp,

                        t.BalanceAfterFrom,

                        t.BalanceAfterTo

                    FROM Transactions t

                    LEFT JOIN Accounts fromAccount
                        ON t.FromAccountId = fromAccount.AccountId

                    LEFT JOIN Accounts toAccount
                        ON t.ToAccountId = toAccount.AccountId

                    INNER JOIN TransactionTypes tt
                        ON t.TransactionTypeId = tt.TransactionTypeId

                    INNER JOIN TransactionStatuses ts
                        ON t.TransactionStatusId = ts.TransactionStatusId

                    WHERE
                        t.FromAccountId =
                        (
                            SELECT AccountId
                            FROM Accounts
                            WHERE AccountNumber = @AccountNumber
                        )

                        OR

                        t.ToAccountId =
                        (
                            SELECT AccountId
                            FROM Accounts
                            WHERE AccountNumber = @AccountNumber
                        )

                    ORDER BY t.Timestamp DESC";

                using (DbCommand command =
                       connection.CreateCommand())
                {
                    command.CommandText = query;

                    DbParameter accountNumberParameter =
                        command.CreateParameter();

                    accountNumberParameter.ParameterName =
                        "@AccountNumber";

                    accountNumberParameter.Value =
                        accountNumber;

                    command.Parameters.Add(
                        accountNumberParameter);

                    using (DbDataReader reader =
                           command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ViewRecentTransactionsResponseDto transaction =
                                new ViewRecentTransactionsResponseDto();

                            transaction.TransactionId =
                                Convert.ToInt32(
                                    reader["TransactionId"]);

                            transaction.FromAccountNumber =
                                reader["FromAccountNumber"] == DBNull.Value
                                    ? null
                                    : reader["FromAccountNumber"].ToString();

                            transaction.ToAccountNumber =
                                reader["ToAccountNumber"] == DBNull.Value
                                    ? null
                                    : reader["ToAccountNumber"].ToString();

                            transaction.Amount =
                                Convert.ToDecimal(
                                    reader["Amount"]);

                            transaction.TransactionType =
                                (TransactionType)Enum.Parse(
                                    typeof(TransactionType),
                                    reader["TransactionType"].ToString(),
                                    true);

                            transaction.TransactionStatus =
                                (TransactionStatus)Enum.Parse(
                                    typeof(TransactionStatus),
                                    reader["TransactionStatus"].ToString(),
                                    true);

                            transaction.Timestamp =
                                Convert.ToDateTime(
                                    reader["Timestamp"]);

                            transaction.BalanceAfterFrom =
                                reader["BalanceAfterFrom"] == DBNull.Value
                                    ? null
                                    : Convert.ToDecimal(
                                        reader["BalanceAfterFrom"]);

                            transaction.BalanceAfterTo =
                                reader["BalanceAfterTo"] == DBNull.Value
                                    ? null
                                    : Convert.ToDecimal(
                                        reader["BalanceAfterTo"]);

                            transactions.Add(transaction);
                        }
                    }
                }
            }

            return transactions;
        }

        public void SaveTransaction(
            string fromAccountNumber,
            string toAccountNumber,
            TransactionType transactionType,
            decimal amount,
            TransactionStatus transactionStatus,
            decimal balanceAfterFrom,
            decimal balanceAfterTo)
        {
            using (DbConnection connection =
                   DataBaseConnectionManager.GetConnection())
            {
                connection.Open();

                string query = @"
                    INSERT INTO Transactions
                    (
                        TransactionTypeId,
                        FromAccountId,
                        ToAccountId,
                        Amount,
                        TransactionStatusId,
                        Timestamp,
                        BalanceAfterFrom,
                        BalanceAfterTo
                    )
                    VALUES
                    (
                        (
                            SELECT TransactionTypeId
                            FROM TransactionTypes
                            WHERE Code = @TransactionType
                        ),

                        (
                            SELECT AccountId
                            FROM Accounts
                            WHERE AccountNumber = @FromAccountNumber
                        ),

                        (
                            SELECT AccountId
                            FROM Accounts
                            WHERE AccountNumber = @ToAccountNumber
                        ),

                        @Amount,

                        (
                            SELECT TransactionStatusId
                            FROM TransactionStatuses
                            WHERE Code = @TransactionStatus
                        ),

                        GETDATE(),

                        @BalanceAfterFrom,
                        @BalanceAfterTo
                    )";

                using (DbCommand command =
                       connection.CreateCommand())
                {
                    command.CommandText = query;

                    DbParameter transactionTypeParameter =
                        command.CreateParameter();

                    transactionTypeParameter.ParameterName =
                        "@TransactionType";

                    transactionTypeParameter.Value =
                        transactionType.ToString().ToUpper();

                    command.Parameters.Add(
                        transactionTypeParameter);


                    DbParameter fromAccountParameter =
                        command.CreateParameter();

                    fromAccountParameter.ParameterName =
                        "@FromAccountNumber";

                    fromAccountParameter.Value =
                        string.IsNullOrEmpty(fromAccountNumber)
                            ? (object)DBNull.Value
                            : fromAccountNumber;

                    command.Parameters.Add(
                        fromAccountParameter);


                    DbParameter toAccountParameter =
                        command.CreateParameter();

                    toAccountParameter.ParameterName =
                        "@ToAccountNumber";

                    toAccountParameter.Value =
                        string.IsNullOrEmpty(toAccountNumber)
                            ? (object)DBNull.Value
                            : toAccountNumber;

                    command.Parameters.Add(
                        toAccountParameter);


                    DbParameter amountParameter =
                        command.CreateParameter();

                    amountParameter.ParameterName =
                        "@Amount";

                    amountParameter.Value =
                        amount;

                    command.Parameters.Add(
                        amountParameter);


                    DbParameter transactionStatusParameter =
                        command.CreateParameter();

                    transactionStatusParameter.ParameterName =
                        "@TransactionStatus";

                    transactionStatusParameter.Value =
                        transactionStatus.ToString().ToUpper();

                    command.Parameters.Add(
                        transactionStatusParameter);


                    DbParameter balanceAfterFromParameter =
                        command.CreateParameter();

                    balanceAfterFromParameter.ParameterName =
                        "@BalanceAfterFrom";

                    balanceAfterFromParameter.Value =
                        balanceAfterFrom;

                    command.Parameters.Add(
                        balanceAfterFromParameter);


                    DbParameter balanceAfterToParameter =
                        command.CreateParameter();

                    balanceAfterToParameter.ParameterName =
                        "@BalanceAfterTo";

                    balanceAfterToParameter.Value =
                        balanceAfterTo;

                    command.Parameters.Add(
                        balanceAfterToParameter);


                    command.ExecuteNonQuery();
                }
            }
        }
    }
}