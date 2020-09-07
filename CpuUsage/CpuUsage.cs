using System;
using System.Runtime.InteropServices.ComTypes;
using System.Threading;

namespace CpuUsage
{
	/// <summary>
	/// Использование CPU
	/// </summary>
	public class CpuUsage
	{
		/// <summary>
		/// Получает информацию о системном времени. В многопроцессорной системе 
		/// возвращаемые значения являются суммой назначенного времени для всех процессоров.
		/// </summary>
		/// <param name="lpIdleTime">Количество времени, в течение которого система простаивала.</param>
		/// <param name="lpKernelTime">Количество времени, которое система потратила на выполнение в 
		/// режиме ядра (включая все потоки во всех процессах, на всех процессорах).</param>
		/// <param name="lpUserTime">Количество времени, которое система потратила на выполнение в 
		/// пользовательском режиме (включая все потоки во всех процессах, на всех процессорах)</param>
		/// <returns>Если функция завершается успешно, возвращаемое значение отличное от нуля.</returns>
		[System.Runtime.InteropServices.DllImport("kernel32.dll", SetLastError = true)]
		static extern bool GetSystemTimes(out FILETIME lpIdleTime, out FILETIME lpKernelTime, out FILETIME lpUserTime);

		/// <summary>
		/// Количество времени, в течение которого система простаивала.
		/// </summary>
		private TimeSpan SysIdleOldTs;

		/// <summary>
		/// Количество времени, которое система потратила на выполнение в 
		/// режиме ядра (включая все потоки во всех процессах, на всех процессорах).
		/// </summary>
		private TimeSpan SysKernelOldTs;

		/// <summary>
		/// Количество времени, которое система потратила на выполнение в 
		/// пользовательском режиме (включая все потоки во всех процессах, на всех процессорах).
		/// </summary>
		private TimeSpan SysUserOldTs;

		/// <summary>
		/// Использование CPU
		/// </summary>
		public float ValueCpuUsage { private set; get; }

		/// <summary>
		/// Использование CPU
		/// </summary>
		public CpuUsage()
		{
			var th = new Thread(() =>
			{
				while (true)
				{
					Thread.Sleep(1500);
					Update();
				}
			});
			th.IsBackground = true;
			th.Start();
		}

		/// <summary>
		/// Обновление значений
		/// </summary>
		private bool Update()
		{
			FILETIME sysIdle;
			FILETIME sysKernel;
			FILETIME sysUser;

			if (GetSystemTimes(out sysIdle, out sysKernel, out sysUser))
			{
				TimeSpan sysIdleTs = GetTimeSpanFromFileTime(sysIdle);
				TimeSpan sysKernelTs = GetTimeSpanFromFileTime(sysKernel);
				TimeSpan sysUserTs = GetTimeSpanFromFileTime(sysUser);

				TimeSpan sysIdleDiffenceTs = sysIdleTs.Subtract(SysIdleOldTs);
				TimeSpan sysKernelDiffenceTs = sysKernelTs.Subtract(SysKernelOldTs);
				TimeSpan sysUserDiffenceTs = sysUserTs.Subtract(SysUserOldTs);

				SysIdleOldTs = sysIdleTs;
				SysKernelOldTs = sysKernelTs;
				SysUserOldTs = sysUserTs;

				TimeSpan system = sysKernelDiffenceTs.Add(sysUserDiffenceTs);

				ValueCpuUsage = (float)(((system.Subtract(sysIdleDiffenceTs).TotalMilliseconds) * 100) / system.TotalMilliseconds);

				return true;
			}
			else
			{
				return false;
			}
		}

		/// <summary>
		/// Промежуток времени
		/// </summary>
		/// <param name="time"></param>
		/// <returns></returns>
		private static TimeSpan GetTimeSpanFromFileTime(FILETIME time)
		{
			return TimeSpan.FromMilliseconds((((ulong)time.dwHighDateTime << 32) + (uint)time.dwLowDateTime) * 0.000001);
		}

		/// <summary>
		/// Строковое представление
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return string.Format("CPU: {0:0.00} %", ValueCpuUsage);
		}
	}
}

