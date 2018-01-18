using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DxLibDLL;

namespace GeisterCompletely
{
	class Program
	{
		static void Main(string[] args)
		{
			const int windowSizeX = 800;
			const int windowSizeY = 600;
			string logFileName = "";

			if (args.Count() >= 1)
			{
				logFileName = args[0];
				Console.WriteLine("logFileName = " + args[0]);
			}

			DX.ChangeWindowMode(DX.TRUE);
			DX.SetBackgroundColor(255, 255, 255);
			DX.SetGraphMode(windowSizeX, windowSizeY, 32);
			DX.DxLib_Init();
			DX.SetDrawScreen(DX.DX_SCREEN_BACK);

			GameController controller = new GameController();
			controller.Start(windowSizeX, windowSizeY, logFileName);

			DX.DxLib_End();
		}
	}
}
