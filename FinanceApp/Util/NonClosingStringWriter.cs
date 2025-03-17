using System.IO;

namespace FinanceApp.Util
{
    public class NonClosingStringWriter : StringWriter
    {
        protected override void Dispose(bool disposing)
        {
            // Не закрываем внутренний writer
        }
    }
}
