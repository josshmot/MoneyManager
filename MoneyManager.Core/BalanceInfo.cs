﻿namespace MoneyManager.Core
{
    /// <summary>
    /// Holds information about the Balance, Income and Expenses of a <see cref="Balanceable"/> inheritor.
    /// </summary>
    public class BalanceInfo : IEquatable<BalanceInfo>
    {
        public Money Balance { get; }
        public Money ClearedBalance { get; }

        public Money Income { get; }
        public Money ClearedIncome { get; }

        public Money Expenses { get; }
        public Money ClearedExpenses { get; }

        public BalanceInfo(Transaction[] transactions)
        {
            // Check for empty transactions[]
            if (transactions.Length == 0)
            {
                Balance = 0;
                ClearedBalance = 0;
                Income = 0;
                ClearedIncome = 0;
                Expenses = 0;
                ClearedExpenses = 0;
                return;
            }
            
            // Calculate balances
            Balance = transactions
                .Select(x => x.Value)
                .Aggregate((total, next) => total + next);
            ClearedBalance = transactions
                .Select(x => x.ClearedValue)
                .Aggregate((total, next) => total + next);

            // Calculate incomes
            var incomes = transactions
                .Where(x => x.TransactionType is TransactionType.Deposit);
            if (incomes.Any()) // Case: some income exists
            {
                Income = incomes
                    .Select(x => x.Value)
                    .Aggregate((total, next) => total + next);
                ClearedIncome = incomes
                    .Select(x => x.ClearedValue)
                    .Aggregate((total, next) => total + next);
            }
            else // Case: no income (is poor) (an exception is thrown if we don't check for this)
            {
                Income = 0;
                ClearedIncome = 0;
            }

            // Calculate expenses
            var expenses = transactions
                .Where(x => x.TransactionType is TransactionType.Withdrawal);
            if (expenses.Any()) // Case: expenses exist
            {
                Expenses = expenses
                    .Select(x => x.Value)
                    .Aggregate((total, next) => total + next);
                ClearedExpenses = expenses
                    .Select(x => x.ClearedValue)
                    .Aggregate((total, next) => total + next);
            }
            else // Case: no expenses
            {
                Expenses = 0;
                ClearedExpenses = 0;
            }
        }

        public bool Equals(BalanceInfo? other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (other is null) return false;
            
            return
                Balance == other.Balance &&
                ClearedBalance == other.ClearedBalance &&
                Expenses == other.Expenses &&
                ClearedExpenses == other.ClearedExpenses &&
                Income == other.Income &&
                ClearedIncome == other.ClearedIncome;
        }
    }
}
