using System;
using System.Threading;
using System.Threading.Tasks;

namespace Avanade.XRM.Deployer.Common
{
	public class TimedRunner
	{
		public static void RunAndTime(Action a, int wait)
		{
			var start = DateTime.Now;
			Console.WriteLine($"[LOG] => Inizio esecuzione.");

			Task.Run(() => a.Invoke()).Wait();

			var end = DateTime.Now;
			var elapsed = (end - start);

			Console.WriteLine($"[LOG] => Esecuzione terminata in {elapsed.Hours}:{elapsed.Minutes}:{elapsed.Seconds}");
			Thread.Sleep(wait);
		}
	}
}
