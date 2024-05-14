﻿
using MoneyManager.Core;

namespace MoneyManager.REPL.Commands
{
    internal class ExitCommand(string pathToThisCommand) : Command(pathToThisCommand)
    {
        public override string Str => "exit";

        public override string About => "Exits the program, or the currently open Account or Category.";

        public override Command[] SubCommands => [
            new ExitProgramCommand(CommandPath)
            ];

        public override Argument[] Arguments => [];

        public override string[] RequiredArgIDs => [];

        public override string[] OptionalArgIDs => [];

        public override Action<ArgumentValueCollection>? Action =>
            (ArgumentValueCollection args) =>
            {
                // Case: current context is an account
                if (REPL.Instance.CurrentContext is Account account)
                {
                    // Exit account
                    REPL.Instance.CurrentContext = REPL.Instance.CurrentAccountBook;
                    Terminal.MessageSingle($"Exited account: {account.Name}");
                    return;
                }

                // Case: current context is a category
                if (REPL.Instance.CurrentContext is Category category)
                {
                    // Exit category
                    REPL.Instance.CurrentContext = REPL.Instance.CurrentAccountBook;
                    Terminal.MessageSingle($"Exited category: {category.Name}");
                    return;
                }

                // Otherwise -> call ExitProgramCommand
                new ExitProgramCommand("").Action!(new ArgumentValueCollection());
            };
    }
}
