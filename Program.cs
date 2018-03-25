using System;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace Find_Peda_Router {
	class Program {
		static AutoResetEvent commandDone = new AutoResetEvent(false);
		static StringBuilder stringBuilder = new StringBuilder();
		delegate void Bang();
		static string target = "172.20.0.1";
		static void Main(string[] args) {
			if (args.Length != 0) {
				target = args[0];
			}
			Console.WriteLine("target: {0}", target);
			runCommand();
			commandDone.WaitOne();

		}

		private static void runCommand() {
			stringBuilder = new StringBuilder();
			ProcessStartInfo cmdStartInfo = new ProcessStartInfo();
			cmdStartInfo.FileName = @"C:\Windows\System32\cmd.exe";
			cmdStartInfo.RedirectStandardOutput = true;
			cmdStartInfo.RedirectStandardError = true;
			cmdStartInfo.RedirectStandardInput = true;
			cmdStartInfo.UseShellExecute = false;
			cmdStartInfo.CreateNoWindow = true;

			Process cmdProcess = new Process();
			cmdProcess.StartInfo = cmdStartInfo;
			cmdProcess.OutputDataReceived += cmd_DataReceived;
			cmdProcess.EnableRaisingEvents = true;
			cmdProcess.Start();
			cmdProcess.BeginOutputReadLine();
			cmdProcess.BeginErrorReadLine();
			Console.WriteLine("Releasing");
			cmdProcess.StandardInput.WriteLine("ipconfig /release");
			Console.WriteLine("Renewing");
			cmdProcess.StandardInput.WriteLine("ipconfig /renew");
			cmdProcess.StandardInput.WriteLine("exit");
			cmdProcess.WaitForExit();
			Evaluate();
		}

		static void Evaluate() {
			string s = stringBuilder.ToString();
			foreach (string line in s.Split('\n')) {
				if (line.Contains("Default Gateway")) {
					Console.WriteLine(line);
				}
			}
			if (s.Contains(target)) {
				Console.WriteLine("Connected");
				commandDone.Set();
			} else {
				Console.WriteLine("Retry");
				runCommand();
			}
		}

		static void cmd_DataReceived(object sender, DataReceivedEventArgs e) {
			//Console.WriteLine("Output from other process");
			stringBuilder.AppendLine(e.Data);
		}
	}
}
