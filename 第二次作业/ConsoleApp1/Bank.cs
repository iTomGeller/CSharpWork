using System;

namespace BankSimulation
{
    // 自定义异常类
    public class BadCashException : Exception
    {
        public BadCashException(string message) : base(message) { }
    }

    // 事件参数类
    public class BigMoneyArgs : EventArgs
    {
        public Account Account { get; }
        public decimal Amount { get; }

        public BigMoneyArgs(Account account, decimal amount)
        {
            Account = account;
            Amount = amount;
        }
    }

    // 抽象账号类
    public abstract class Account
    {
        public string AccountNumber { get; }
        public decimal Balance { get; protected set; }

        protected Account(string accountNumber, decimal initialBalance)
        {
            AccountNumber = accountNumber;
            Balance = initialBalance;
        }

        public virtual void Deposit(decimal amount)
        {
            Balance += amount;
        }

        public abstract void Withdraw(decimal amount);
    }

    // 信用账号类
    public class CreditAccount : Account
    {
        public decimal CreditLimit { get; }

        public CreditAccount(string accountNumber, decimal initialBalance, decimal creditLimit) 
            : base(accountNumber, initialBalance)
        {
            CreditLimit = creditLimit;
        }

        public override void Withdraw(decimal amount)
        {
            if (Balance + CreditLimit < amount)
                throw new InvalidOperationException("超出信用额度");
            Balance -= amount;
        }
    }

    // ATM 类
    public class ATM
    {
        public event EventHandler<BigMoneyArgs> BigMoneyFetched;

        public void Withdraw(Account account, decimal amount)
        {
            Random rand = new Random();
            if (rand.NextDouble() < 0.3) // 30% 概率抛出 BadCashException
            {
                throw new BadCashException("取款时发现坏钞！");
            }

            account.Withdraw(amount);
            Console.WriteLine($"取款成功: {amount} 元，当前余额: {account.Balance} 元");

            if (amount > 10000)
            {
                OnBigMoneyFetched(new BigMoneyArgs(account, amount));
            }
        }

        protected virtual void OnBigMoneyFetched(BigMoneyArgs e)
        {
            BigMoneyFetched?.Invoke(this, e);
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Account myAccount = new CreditAccount("123456789", 5000, 2000);
            ATM atm = new ATM();

            // 订阅事件
            atm.BigMoneyFetched += (sender, e) =>
            {
                Console.WriteLine($"警告: 账号 {e.Account.AccountNumber} 取款大于 10000 元，金额: {e.Amount}");
            };

            try
            {
                atm.Withdraw(myAccount, 15000);
            }
            catch (BadCashException ex)
            {
                Console.WriteLine($"异常: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"发生错误: {ex.Message}");
            }
        }
    }
}