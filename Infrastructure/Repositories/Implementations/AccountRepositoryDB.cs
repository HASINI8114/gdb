using GDB.App.Domain.Enums;
using GDB.App.Domain.Models;
using GDB.App.Infrastructure.Repositories.Contracts;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GDB.App.Infrastructure.Repositories.Implementations
{
    internal class AccountRepositoryDB : IAccountRepository
    {
        public IAccount GetAccount(string accountNumber)
        {
            using (DbConnection connection =
                   DataBaseConnectionManager.GetConnection())
            {
                connection.Open();

                string query = @"
                    SELECT 
                        a.AccountId, 
                        a.AccountNumber, 
                        a.Name, 
                        a.Age, 
                        a.Balance, 
                        a.Pin, 

                        at.Code AS AccountType, 
                        ast.Code AS AccountStatus, 
                        ap.Code AS AccountPrivilege, 

                        sa.InterestRate AS SavingsInterestRate, 
                        sa.MinimumBalance AS SavingsMinimumBalance, 

                        ca.OverdraftLimit, 

                        fda.InterestRate AS FixedDepositInterestRate, 
                        fda.TenureMonths, 

                        sya.EmployerName, 
                        sya.InactiveMonths 

                    FROM Accounts a 

                    INNER JOIN AccountTypes at 
                        ON a.AccountTypeId = at.AccountTypeId 

                    INNER JOIN AccountStatuses ast 
                        ON a.AccountStatusId = ast.AccountStatusId 

                    INNER JOIN AccountPrivileges ap 
                        ON a.AccountPrivilegeId = ap.AccountPrivilegeId 

                    LEFT JOIN SavingsAccounts sa 
                        ON a.AccountId = sa.AccountId 

                    LEFT JOIN CurrentAccounts ca 
                        ON a.AccountId = ca.AccountId 

                    LEFT JOIN FixedDepositAccounts fda 
                        ON a.AccountId = fda.AccountId 

                    LEFT JOIN SalaryAccounts sya 
                        ON a.AccountId = sya.AccountId 

                    WHERE a.AccountNumber = @AccountNumber";

                using (DbCommand command = connection.CreateCommand())
                {
                    command.CommandText = query;

                    DbParameter accountNumberParameter =
                        command.CreateParameter();

                    accountNumberParameter.ParameterName =
                        "@AccountNumber";

                    accountNumberParameter.Value =
                        accountNumber;

                    command.Parameters.Add(accountNumberParameter);

                    using (DbDataReader reader =
                           command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return CreateAccount(reader);
                        }
                    }
                }
            }

            return null;
        }


        // ============================================================
        // 2. SAVE ONE ACCOUNT
        // ============================================================

        public void SaveAccount(IAccount account, string pin)
        {
            using (DbConnection connection =
                   DataBaseConnectionManager.GetConnection())
            {
                connection.Open();

                DbTransaction transaction =
                    connection.BeginTransaction();

                try
                {
                    string insertAccountQuery = @"
                        INSERT INTO Accounts 
                        (
                            AccountNumber, 
                            Name, 
                            Age, 
                            AccountTypeId, 
                            Balance, 
                            AccountStatusId, 
                            AccountPrivilegeId, 
                            Pin
                        ) 
                        VALUES 
                        (
                            @AccountNumber, 
                            @Name, 
                            @Age, 
                            (
                                SELECT AccountTypeId 
                                FROM AccountTypes 
                                WHERE Code = @AccountType
                            ), 
                            @Balance, 
                            (
                                SELECT AccountStatusId 
                                FROM AccountStatuses 
                                WHERE Code = @AccountStatus
                            ), 
                            (
                                SELECT AccountPrivilegeId 
                                FROM AccountPrivileges 
                                WHERE Code = @AccountPrivilege
                            ), 
                            @Pin
                        );

                        SELECT CAST(SCOPE_IDENTITY() AS BIGINT);";

                    long accountId;

                    using (DbCommand command =
                           connection.CreateCommand())
                    {
                        command.Transaction = transaction;
                        command.CommandText = insertAccountQuery;

                        DbParameter accountNumberParameter =
                            command.CreateParameter();

                        accountNumberParameter.ParameterName =
                            "@AccountNumber";

                        accountNumberParameter.Value =
                            account.AccountNumber;

                        command.Parameters.Add(accountNumberParameter);


                        DbParameter nameParameter =
                            command.CreateParameter();

                        nameParameter.ParameterName =
                            "@Name";

                        nameParameter.Value =
                            account.Name;

                        command.Parameters.Add(nameParameter);


                        DbParameter ageParameter =
                            command.CreateParameter();

                        ageParameter.ParameterName =
                            "@Age";

                        ageParameter.Value =
                            account.Age;

                        command.Parameters.Add(ageParameter);


                        DbParameter accountTypeParameter =
                            command.CreateParameter();

                        accountTypeParameter.ParameterName =
                            "@AccountType";

                        accountTypeParameter.Value =
                            GetAccountTypeCode(account.AccountType);

                        command.Parameters.Add(accountTypeParameter);


                        DbParameter balanceParameter =
                            command.CreateParameter();

                        balanceParameter.ParameterName =
                            "@Balance";

                        balanceParameter.Value =
                            account.Balance;

                        command.Parameters.Add(balanceParameter);


                        DbParameter accountStatusParameter =
                            command.CreateParameter();

                        accountStatusParameter.ParameterName =
                            "@AccountStatus";

                        accountStatusParameter.Value =
                            GetAccountStatusCode(account.Status);

                        command.Parameters.Add(accountStatusParameter);


                        DbParameter accountPrivilegeParameter =
                            command.CreateParameter();

                        accountPrivilegeParameter.ParameterName =
                            "@AccountPrivilege";

                        accountPrivilegeParameter.Value =
                            GetAccountPrivilegeCode(account.Privilege);

                        command.Parameters.Add(accountPrivilegeParameter);


                        DbParameter pinParameter =
                            command.CreateParameter();

                        pinParameter.ParameterName =
                            "@Pin";

                        pinParameter.Value =
                            pin;

                        command.Parameters.Add(pinParameter);


                        accountId =
                            Convert.ToInt64(
                                command.ExecuteScalar());
                    }


                    // ------------------------------------------------
                    // Insert subtype record
                    // ------------------------------------------------

                    if (account is SavingsAccount savings)
                    {
                        string query = @"
                            INSERT INTO SavingsAccounts 
                            (
                                AccountId, 
                                InterestRate, 
                                MinimumBalance
                            ) 
                            VALUES 
                            (
                                @AccountId, 
                                @InterestRate, 
                                @MinimumBalance
                            )";

                        using (DbCommand command =
                               connection.CreateCommand())
                        {
                            command.Transaction = transaction;
                            command.CommandText = query;


                            DbParameter accountIdParameter =
                                command.CreateParameter();

                            accountIdParameter.ParameterName =
                                "@AccountId";

                            accountIdParameter.Value =
                                accountId;

                            command.Parameters.Add(accountIdParameter);


                            DbParameter interestRateParameter =
                                command.CreateParameter();

                            interestRateParameter.ParameterName =
                                "@InterestRate";

                            interestRateParameter.Value =
                                savings.InterestRate / 100.0;

                            command.Parameters.Add(interestRateParameter);


                            DbParameter minimumBalanceParameter =
                                command.CreateParameter();

                            minimumBalanceParameter.ParameterName =
                                "@MinimumBalance";

                            minimumBalanceParameter.Value =
                                savings.MinBalance;

                            command.Parameters.Add(minimumBalanceParameter);

                            command.ExecuteNonQuery();
                        }
                    }


                    else if (account is CurrentAccount current)
                    {
                        string query = @"
                            INSERT INTO CurrentAccounts 
                            (
                                AccountId, 
                                OverdraftLimit
                            ) 
                            VALUES 
                            (
                                @AccountId, 
                                @OverdraftLimit
                            )";

                        using (DbCommand command =
                               connection.CreateCommand())
                        {
                            command.Transaction = transaction;
                            command.CommandText = query;


                            DbParameter accountIdParameter =
                                command.CreateParameter();

                            accountIdParameter.ParameterName =
                                "@AccountId";

                            accountIdParameter.Value =
                                accountId;

                            command.Parameters.Add(accountIdParameter);


                            DbParameter overdraftParameter =
                                command.CreateParameter();

                            overdraftParameter.ParameterName =
                                "@OverdraftLimit";

                            overdraftParameter.Value =
                                current.OverdraftLimit;

                            command.Parameters.Add(overdraftParameter);

                            command.ExecuteNonQuery();
                        }
                    }


                    else if (account is FixedDepositAccount fixedDeposit)
                    {
                        string query = @"
                            INSERT INTO FixedDepositAccounts 
                            (
                                AccountId, 
                                InterestRate, 
                                TenureMonths, 
                                PrincipalAmount, 
                                MaturityDate, 
                                MaturityAmount
                            ) 
                            VALUES 
                            (
                                @AccountId, 
                                @InterestRate, 
                                @TenureMonths, 
                                @PrincipalAmount, 
                                DATEADD(
                                    MONTH, 
                                    @TenureMonths, 
                                    SYSUTCDATETIME()
                                ), 
                                @PrincipalAmount
                            )";

                        using (DbCommand command =
                               connection.CreateCommand())
                        {
                            command.Transaction = transaction;
                            command.CommandText = query;


                            DbParameter accountIdParameter =
                                command.CreateParameter();

                            accountIdParameter.ParameterName =
                                "@AccountId";

                            accountIdParameter.Value =
                                accountId;

                            command.Parameters.Add(accountIdParameter);


                            DbParameter interestRateParameter =
                                command.CreateParameter();

                            interestRateParameter.ParameterName =
                                "@InterestRate";

                            interestRateParameter.Value =
                                fixedDeposit.InterestRate / 100.0;

                            command.Parameters.Add(interestRateParameter);


                            DbParameter tenureParameter =
                                command.CreateParameter();

                            tenureParameter.ParameterName =
                                "@TenureMonths";

                            tenureParameter.Value =
                                fixedDeposit.TenureMonths;

                            command.Parameters.Add(tenureParameter);


                            DbParameter principalParameter =
                                command.CreateParameter();

                            principalParameter.ParameterName =
                                "@PrincipalAmount";

                            principalParameter.Value =
                                fixedDeposit.Balance;

                            command.Parameters.Add(principalParameter);

                            command.ExecuteNonQuery();
                        }
                    }


                    else if (account is SalaryAccount salary)
                    {
                        string query = @"
                            INSERT INTO SalaryAccounts 
                            (
                                AccountId, 
                                EmployerName, 
                                InactiveMonths, 
                                SalaryAmount
                            ) 
                            VALUES 
                            (
                                @AccountId, 
                                @EmployerName, 
                                @InactiveMonths, 
                                @SalaryAmount
                            )";

                        using (DbCommand command =
                               connection.CreateCommand())
                        {
                            command.Transaction = transaction;
                            command.CommandText = query;


                            DbParameter accountIdParameter =
                                command.CreateParameter();

                            accountIdParameter.ParameterName =
                                "@AccountId";

                            accountIdParameter.Value =
                                accountId;

                            command.Parameters.Add(accountIdParameter);


                            DbParameter employerParameter =
                                command.CreateParameter();

                            employerParameter.ParameterName =
                                "@EmployerName";

                            employerParameter.Value =
                                salary.EmployerName;

                            command.Parameters.Add(employerParameter);


                            DbParameter inactiveMonthsParameter =
                                command.CreateParameter();

                            inactiveMonthsParameter.ParameterName =
                                "@InactiveMonths";

                            inactiveMonthsParameter.Value =
                                salary.InactiveMonths;

                            command.Parameters.Add(inactiveMonthsParameter);


                            DbParameter salaryAmountParameter =
                                command.CreateParameter();

                            salaryAmountParameter.ParameterName =
                                "@SalaryAmount";

                            salaryAmountParameter.Value =
                                salary.Balance;

                            command.Parameters.Add(salaryAmountParameter);

                            command.ExecuteNonQuery();
                        }
                    }

                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }


        public void UpdateBalance(string accountNumber, decimal balance)
        {
            using DbConnection connection =
                DataBaseConnectionManager.GetConnection();

            connection.Open();

            string query = @"
                UPDATE Accounts 
                SET Balance = @Balance 
                WHERE AccountNumber = @AccountNumber";

            using DbCommand command =
                connection.CreateCommand();

            command.CommandText = query;


            DbParameter balanceParameter =
                command.CreateParameter();

            balanceParameter.ParameterName =
                "@Balance";

            balanceParameter.Value =
                balance;

            command.Parameters.Add(balanceParameter);


            DbParameter accountNumberParameter =
                command.CreateParameter();

            accountNumberParameter.ParameterName =
                "@AccountNumber";

            accountNumberParameter.Value =
                accountNumber;

            command.Parameters.Add(accountNumberParameter);

            command.ExecuteNonQuery();
        }


        public void CloseAccount(string accountNumber)
        {
            using (DbConnection connection =
                   DataBaseConnectionManager.GetConnection())
            {
                connection.Open();

                string query = @"
                    UPDATE Accounts 
                    SET 
                        AccountStatusId = 
                        (
                            SELECT AccountStatusId 
                            FROM AccountStatuses 
                            WHERE Code = 'CLOSED'
                        ), 
                        ClosedAt = SYSUTCDATETIME() 
                    WHERE AccountNumber = @AccountNumber";

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

                    command.Parameters.Add(accountNumberParameter);


                    int rowsAffected =
                        command.ExecuteNonQuery();

                    if (rowsAffected == 0)
                        throw new Exception("Account not found.");
                }
            }
        }


        // ============================================================
        // 3. GET ALL ACCOUNTS
        // ============================================================

        public List<IAccount> GetAllAccounts()
        {
            List<IAccount> accounts =
                new List<IAccount>();

            using (DbConnection connection =
                   DataBaseConnectionManager.GetConnection())
            {
                connection.Open();

                string query = @"
                    SELECT 
                        a.AccountId, 
                        a.AccountNumber, 
                        a.Name, 
                        a.Age, 
                        a.Balance, 
                        a.Pin, 

                        at.Code AS AccountType, 
                        ast.Code AS AccountStatus, 
                        ap.Code AS AccountPrivilege, 

                        sa.InterestRate AS SavingsInterestRate, 
                        sa.MinimumBalance AS SavingsMinimumBalance, 

                        ca.OverdraftLimit, 

                        fda.InterestRate AS FixedDepositInterestRate, 
                        fda.TenureMonths, 

                        sya.EmployerName, 
                        sya.InactiveMonths 

                    FROM Accounts a 

                    INNER JOIN AccountTypes at 
                        ON a.AccountTypeId = at.AccountTypeId 

                    INNER JOIN AccountStatuses ast 
                        ON a.AccountStatusId = ast.AccountStatusId 

                    INNER JOIN AccountPrivileges ap 
                        ON a.AccountPrivilegeId = ap.AccountPrivilegeId 

                    LEFT JOIN SavingsAccounts sa 
                        ON a.AccountId = sa.AccountId 

                    LEFT JOIN CurrentAccounts ca 
                        ON a.AccountId = ca.AccountId 

                    LEFT JOIN FixedDepositAccounts fda 
                        ON a.AccountId = fda.AccountId 

                    LEFT JOIN SalaryAccounts sya 
                        ON a.AccountId = sya.AccountId 

                    ORDER BY a.AccountId";

                using (DbCommand command =
                       connection.CreateCommand())
                {
                    command.CommandText = query;

                    using (DbDataReader reader =
                           command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            IAccount account =
                                CreateAccount(reader);

                            accounts.Add(account);
                        }
                    }
                }
            }

            return accounts;
        }


        // ============================================================
        // 4. SAVE TWO ACCOUNTS
        // Used for TRANSFER
        // ============================================================

        public void SaveAccounts(
            IAccount fromAccount,
            IAccount toAccount)
        {
            using (DbConnection connection =
                   DataBaseConnectionManager.GetConnection())
            {
                connection.Open();

                DbTransaction transaction =
                    connection.BeginTransaction();

                try
                {
                    string query = @"
                        UPDATE Accounts 
                        SET Balance = @Balance 
                        WHERE AccountNumber = @AccountNumber";

                    using (DbCommand command =
                           connection.CreateCommand())
                    {
                        command.Transaction = transaction;
                        command.CommandText = query;


                        DbParameter balanceParameter =
                            command.CreateParameter();

                        balanceParameter.ParameterName =
                            "@Balance";

                        balanceParameter.Value =
                            fromAccount.Balance;

                        command.Parameters.Add(balanceParameter);


                        DbParameter accountNumberParameter =
                            command.CreateParameter();

                        accountNumberParameter.ParameterName =
                            "@AccountNumber";

                        accountNumberParameter.Value =
                            fromAccount.AccountNumber;

                        command.Parameters.Add(accountNumberParameter);


                        command.ExecuteNonQuery();


                        balanceParameter.Value =
                            toAccount.Balance;

                        accountNumberParameter.Value =
                            toAccount.AccountNumber;

                        command.ExecuteNonQuery();
                    }

                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }


        // ============================================================
        // CREATE THE CORRECT ACCOUNT OBJECT
        // ============================================================

        private IAccount CreateAccount(
            DbDataReader reader)
        {
            string accountNumber =
                reader["AccountNumber"].ToString();

            string name =
                reader["Name"].ToString();

            int age =
                Convert.ToInt32(reader["Age"]);

            decimal balance =
                Convert.ToDecimal(reader["Balance"]);

            AccountType accountType =
                ConvertAccountType(
                    reader["AccountType"].ToString());

            AccountStatus status =
                ConvertAccountStatus(
                    reader["AccountStatus"].ToString());

            AccountPrivilege privilege =
                ConvertAccountPrivilege(
                    reader["AccountPrivilege"].ToString());

            string pin =
                reader["Pin"].ToString();


            // --------------------------------------------------------
            // SAVINGS
            // --------------------------------------------------------

            if (accountType == AccountType.Savings)
            {
                decimal minBalance =
                    reader["SavingsMinimumBalance"] == DBNull.Value
                    ? 1000.0m
                    : Convert.ToDecimal(
                        reader["SavingsMinimumBalance"]);

                double interestRate =
                    reader["SavingsInterestRate"] == DBNull.Value
                    ? 4.0
                    : Convert.ToDouble(
                        reader["SavingsInterestRate"]) * 100;

                return new SavingsAccount(
                    accountNumber,
                    name,
                    age,
                    balance,
                    accountType,
                    status,
                    pin,
                    privilege,
                    minBalance,
                    interestRate);
            }


            // --------------------------------------------------------
            // CURRENT
            // --------------------------------------------------------

            if (accountType == AccountType.Current)
            {
                decimal overdraftLimit =
                    reader["OverdraftLimit"] == DBNull.Value
                    ? 25000.0m
                    : Convert.ToDecimal(
                        reader["OverdraftLimit"]);

                return new CurrentAccount(
                    accountNumber,
                    name,
                    age,
                    balance,
                    accountType,
                    status,
                    pin,
                    privilege,
                    overdraftLimit);
            }


            // --------------------------------------------------------
            // FIXED DEPOSIT
            // --------------------------------------------------------

            if (accountType == AccountType.FixedDeposit)
            {
                int tenureMonths =
                    reader["TenureMonths"] == DBNull.Value
                    ? 12
                    : Convert.ToInt32(
                        reader["TenureMonths"]);

                double interestRate =
                    reader["FixedDepositInterestRate"] == DBNull.Value
                    ? 6.5
                    : Convert.ToDouble(
                        reader["FixedDepositInterestRate"]) * 100;

                return new FixedDepositAccount(
                    accountNumber,
                    name,
                    age,
                    balance,
                    accountType,
                    status,
                    pin,
                    privilege,
                    tenureMonths,
                    interestRate);
            }


            // --------------------------------------------------------
            // SALARY
            // --------------------------------------------------------

            if (accountType == AccountType.Salary)
            {
                string employerName =
                    reader["EmployerName"] == DBNull.Value
                    ? "TechCorp"
                    : reader["EmployerName"].ToString();

                return new SalaryAccount(
                    accountNumber,
                    name,
                    age,
                    balance,
                    accountType,
                    status,
                    pin,
                    privilege,
                    employerName);
            }


            throw new Exception(
                "Unknown account type.");
        }


        // ============================================================
        // ENUM CONVERSIONS
        // ============================================================

        private AccountType ConvertAccountType(
            string value)
        {
            switch (value)
            {
                case "SAVINGS":
                    return AccountType.Savings;

                case "CURRENT":
                    return AccountType.Current;

                case "FIXED_DEPOSIT":
                    return AccountType.FixedDeposit;

                case "SALARY":
                    return AccountType.Salary;

                default:
                    throw new Exception(
                        "Invalid account type: " + value);
            }
        }


        private AccountStatus ConvertAccountStatus(
            string value)
        {
            return (AccountStatus)Enum.Parse(
                typeof(AccountStatus),
                value,
                true);
        }


        private AccountPrivilege ConvertAccountPrivilege(
            string value)
        {
            return (AccountPrivilege)Enum.Parse(
                typeof(AccountPrivilege),
                value,
                true);
        }


        private string GetAccountTypeCode(
            AccountType accountType)
        {
            switch (accountType)
            {
                case AccountType.Savings:
                    return "SAVINGS";

                case AccountType.Current:
                    return "CURRENT";

                case AccountType.FixedDeposit:
                    return "FIXED_DEPOSIT";

                case AccountType.Salary:
                    return "SALARY";

                default:
                    throw new Exception(
                        "Invalid account type.");
            }
        }


        private string GetAccountStatusCode(
            AccountStatus status)
        {
            return status.ToString().ToUpper();
        }


        private string GetAccountPrivilegeCode(
            AccountPrivilege privilege)
        {
            return privilege.ToString().ToUpper();
        }
    }
}