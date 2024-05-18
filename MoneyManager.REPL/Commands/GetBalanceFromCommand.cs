﻿using MoneyManager.Core;

namespace MoneyManager.REPL.Commands
{
    internal class GetBalanceFromCommand(Command[] pathToThisCommand) : Command(pathToThisCommand)
    {
        public override string Str => "from";

        public override string About => "Gets the total balance for the current context, for the given period from the given date.";

        public override Argument[] Arguments => [
            new DateArgument("fromDate", true)
            ];

        protected override string[] additionalRequiredArgIDs => ["getPeriod"];

        public override Type[] RequiredContextTypes => [typeof(Balanceable)];

        protected override Action<ArgumentValueCollection>? Action =>
            (ArgumentValueCollection args) =>
            {
                // Get period and date
                var getPeriod = (Period)args["getPeriod"].Value;
                var fromDate = (DateOnly)args["fromDate"].Value;

                // Get balabceable and print to terminal
                var balanceable = (Balanceable)REPL.Instance.CurrentContext;
                Terminal.MessageSingle(balanceable.BalanceInfoForPeriod(fromDate, getPeriod).Balance);
            };
    }
}
