﻿namespace MoneyManager.REPL.Commands
{
    internal class EnterCategoryCommand(Command[] pathToThisCommand) : Command(pathToThisCommand)
    {
        public override string Str => "category";

        public override string About => "Enters into the given Category to access and edit its details.";

        public override Argument[] Arguments => [
            new StringArgument("categoryName", true)
            ];

        protected override Action<ArgumentValueCollection>? Action =>
            (ArgumentValueCollection args) =>
            {
                // Get category name
                string categoryName = (string)args["categoryName"].Value;

                // Validity check: a category with the given name must exist
                if (!REPL.Instance.CurrentAccountBook.Categories.Select(x => x.Name).Contains(categoryName))
                    throw new REPLCommandActionException($"Couldn't find Account: \"{categoryName}\"");

                // Enter into the context of the category
                REPL.Instance.CurrentContext = REPL.Instance.CurrentAccountBook.Categories.Where(x => x.Name == categoryName).First();
                Terminal.MessageSingle($"Successfully entered into category: {categoryName}");
            };
    }
}
