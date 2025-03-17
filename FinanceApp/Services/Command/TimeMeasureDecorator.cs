using System;
using System.Diagnostics;

namespace FinanceApp.Services.Command
{
    public class TimeMeasureDecorator : ICommand
    {
        private readonly ICommand _command;
        private readonly string _commandName;

        public TimeMeasureDecorator(ICommand command, string commandName = null)
        {
            _command = command;
            _commandName = commandName ?? command.GetType().Name;
        }

        public void Execute()
        {
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                _command.Execute();
            }
            finally
            {
                stopwatch.Stop();
                Console.WriteLine($"Время выполнения команды {_commandName}: {stopwatch.ElapsedMilliseconds} мс");
            }
        }
    }
}
