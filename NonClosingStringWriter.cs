namespace FinanceApp.Util
{
    using System.IO;

    public class NonClosingStringWriter : StringWriter
    {
        protected override void Dispose(bool disposing)
        {
            // Не закрываем внутренний writer
        }
    }
}
