using System;
using System.Threading;
using System.Threading.Tasks;

namespace TaskDisposeExample
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            MyClass myClass = new MyClass();

            Thread.Sleep(5000);

            myClass.Dispose();

            Console.ReadLine();
        }
    }

    public class MyClass : IDisposable, IAsyncDisposable
    {
        private readonly Task pingTask;
        private readonly CancellationTokenSource pingCancellationTokenSource = new CancellationTokenSource();

        public MyClass()
        {
            pingTask = Task.Run(async () =>
            {
                while (!pingCancellationTokenSource.IsCancellationRequested)
                {
                    await Task.Delay(1000);
                    await Task.Run(() => { Console.WriteLine($"Time {DateTime.Now.Second}"); });
                }
            });
        }

        public void Dispose()
        {
            Console.WriteLine("Dispose()");

            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            Console.WriteLine("virtual Dispose()");

            if (disposing)
            {
                pingCancellationTokenSource.Cancel();
                pingTask.Wait();

                pingCancellationTokenSource.Dispose();
                pingTask.Dispose();
            }
        }

        public async ValueTask DisposeAsync()
        {
            Console.WriteLine("DisposeAsync");

            await DisposeAsyncCore().ConfigureAwait(false);
            Dispose(false);
            GC.SuppressFinalize(this);
        }

        protected virtual async ValueTask DisposeAsyncCore()
        {
            Console.WriteLine("DisposeAsyncCore");

            pingCancellationTokenSource.Cancel();
            await pingTask.ConfigureAwait(false);

            pingCancellationTokenSource.Dispose();
            pingTask.Dispose();
        }
    }
}
