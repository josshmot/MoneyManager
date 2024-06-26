﻿namespace MoneyManager.Core
{
    /// <summary>
    /// Represents a financial sheet containing Accounts and Categories.
    /// </summary>
    public class AccountBook : Balanceable
    {
        public override Transaction[] Transactions => Accounts.SelectMany(x => x.Transactions).OrderBy(x => x.Date).ToArray();
        
        public Account[] Accounts => accounts.ToArray();
        private readonly List<Account> accounts = [];

        public Category[] Categories => categories.ToArray();
        private readonly List<Category> categories = [];

        public AccountBook() { }

        /// <summary>
        /// Adds the given <see cref="Account"/> to this Sheet.
        /// Throws a <see cref="AccountBookException"/> if it already exists within this Sheet.
        /// </summary>
        /// <param name="account"></param>
        /// <exception cref="AccountBookException"></exception>
        public void AddAccount(Account account)
        {
            // Validity check: Accounts must not already contain account
            if (Accounts.Contains(account)) throw new AccountBookException($"Account \"{account.Name}\" already exists in sheet.");

            accounts.Add(account);
        }

        /// <summary>
        /// Adds the given Accounts to this Sheet.
        /// Throws a <see cref="AccountBookException"/> if any of the them already exist in this Sheet.
        /// </summary>
        /// <param name="accounts"></param>
        /// <exception cref="AccountBookException"></exception>
        public void AddAccounts(params Account[] accounts)
        {
            // Validity check: Accounts must not already contain any of accounts
            foreach (Account account in accounts) // Use a foreach here rather than linq so we can access the account name
                if (Accounts.Contains(account)) throw new AccountBookException($"Account \"{account.Name}\" already exists in sheet.");

            this.accounts.AddRange(accounts);
        }

        /// <summary>
        /// Removes the given <see cref="Account"/> from this Sheet.
        /// </summary>
        /// <param name="account"></param>
        /// <exception cref="IndexOutOfRangeException"></exception>
        public void RemoveAccount(Account account)
        {
            // Validity check: account must be within Accounts
            if (!Accounts.Contains(account)) throw new IndexOutOfRangeException();

            accounts.Remove(account);
        }

        /// <summary>
        /// Removes the <see cref="Account"/> in this Sheet at the given index.
        /// </summary>
        /// <param name="index"></param>
        /// <exception cref="IndexOutOfRangeException"></exception>
        public void RemoveAccountAt(int index)
        {
            // Validity check: index must be within the range of accounts
            if (index < 0 || index >= accounts.Count) throw new IndexOutOfRangeException();

            accounts.RemoveAt(index);
        }

        /// <summary>
        /// Adds the given <see cref="Category"/> to this Sheet.
        /// Throws a <see cref="AccountBookException"/> if the given Category already exists within this Sheet.
        /// </summary>
        /// <param name="category"></param>
        /// <exception cref="AccountBookException"></exception>
        public void AddCategory(Category category)
        {
            // Validity check: category must not already be in Categories
            if (Categories.Contains(category)) throw new AccountBookException($"Category \"{category.Name}\" already exists in sheet.");

            categories.Add(category);
        }

        /// <summary>
        /// Adds the given Categories to this Sheet.
        /// Throws a <see cref="AccountBookException"/> if any of them already exist within this Sheet.
        /// </summary>
        /// <param name="categories"></param>
        /// <exception cref="AccountBookException"></exception>
        public void AddCategories(params Category[] categories)
        {
            // Validity check: Categories must not already contain any of categories
            foreach (Category category in categories) // Use a foreach here rather than linq so we can access the category name
                if (Categories.Contains(category)) throw new AccountBookException($"Category \"{category.Name}\" already exists in sheet.");

            this.categories.AddRange(categories);
        }

        /// <summary>
        /// Removes the given <see cref="Category"/> from this Sheet, and detaches all <see cref="Transaction"/>s contained in that Category.
        /// </summary>
        /// <param name="category"></param>
        /// <exception cref="IndexOutOfRangeException"></exception>
        public void RemoveCategory(Category category)
        {
            // Validity check: category must be contained within Categories
            if (!Categories.Contains(category)) throw new IndexOutOfRangeException();

            categories.Remove(category);

            // Remove all references to this category from its transactions
            // PURGE IT FROM ALL SPACETIME!!
            foreach (Transaction transaction in category.Transactions) transaction.Category = null;
        }

        /// <summary>
        /// Removes the <see cref="Category"/> at the given index from this Sheet, and detaches all <see cref="Transaction"/>s contained in that Category.
        /// </summary>
        /// <param name="index"></param>
        /// <exception cref="IndexOutOfRangeException"></exception>
        public void RemoveCategoryAt(int index)
        {
            // Validity check: index must be within the bounds of categories
            if (index < 0 ||index >= categories.Count) throw new IndexOutOfRangeException();

            RemoveCategory(categories[index]); // Avoid duplicating purging code
        }

        /// <summary>
        /// Generates a <see cref="ReportChunk"/> for the given <see cref="Period"/> starting at the given date.
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="period"></param>
        /// <returns></returns>
        public ReportChunk GenerateReportChunk(DateOnly startDate, Period period)
        {
            // Generate ReportChunkCategories
            List<ReportChunkCategory> reportChunkCategories = [];

            foreach (Category category in Categories)
            {
                var reportChunkCategory = new ReportChunkCategory(category, startDate, period, category.BalanceInfoForPeriod(startDate, period))
                {
                    BudgetedIncome = category.IncomeBudget?.Get(period),
                    BudgetedExpenses = category.ExpensesBudget?.Get(period),
                    IncomeDifference = category.GetIncomeDifference(startDate, period),
                    ExpensesDifference = category.GetExpensesDifference(startDate, period)
                };
                reportChunkCategories.Add(reportChunkCategory);
            }

            // Add uncategorised transactions
            var uncategorisedTransactions = Transactions.Where(x => x.Category == null).OrderBy(x => x.Date).ToArray();
            var tempAccount = new Account("", uncategorisedTransactions);
            var miscReportChunkCategory = new ReportChunkCategory(null, startDate, period, tempAccount.BalanceInfoForPeriod(startDate, period));
            reportChunkCategories.Add(miscReportChunkCategory);

            // Generate ReportChunk and Return
            return new ReportChunk(
                startDate,
                period,
                BalanceInfoForPeriod(startDate, period),
                BalanceInfoAtDate(period.GetEndDateInclusive(startDate)),
                reportChunkCategories.ToArray()
                );
        }

        /// <summary>
        /// Generates a <see cref="ReportStepped"/> for the given <see cref="Period"/>, starting at the given date and with the given step period.
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="period"></param>
        /// <param name="stepPeriod"></param>
        /// <returns></returns>
        /// <exception cref="AccountBookException"></exception>
        public ReportStepped GenerateReportStepped(DateOnly startDate, Period period, Period stepPeriod)
        {
            // Generate ReportChunks
            int numReportChunks = period.DivideInto(stepPeriod);
            if (numReportChunks == 0) throw new AccountBookException("Step period cannot be larger than total period.");
            ReportChunk[] reportChunks = new ReportChunk[numReportChunks];
            DateOnly currentDate = startDate;

            for (int i = 0; i < numReportChunks; i++)
            {
                reportChunks[i] = GenerateReportChunk(currentDate, stepPeriod);
                currentDate = stepPeriod.GetEndDateExclusive(currentDate);
            }

            // Generate ReportStepped and Return
            return new ReportStepped(startDate, period, stepPeriod, reportChunks, GenerateReportChunk(startDate, period));
        }
    }
}
