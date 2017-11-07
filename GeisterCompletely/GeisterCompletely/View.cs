using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DxLibDLL;

namespace GeisterCompletely
{
	class View
	{
		private double windowSizeX;
		private double windowSizeY;
		private double Tx, Ty;		//画面の平行移動量。拡大前に行う。
		private double cellSize;	//拡大する前の1マスの大きさ
		private double Scale;       //拡大率
		private int font100;
		private int redKoma;
		private int blueKoma;

		public View(double windowSizeX, double windowSizeY)
		{
			this.windowSizeX = windowSizeX;
			this.windowSizeY = windowSizeY;
			Tx = 0;
			Ty = 0;
			cellSize = 50;
			Scale = 1.0;
			font100 = DX.CreateFontToHandle("メイリオ", 100, 10);
			redKoma = DX.LoadGraph("red.png");
			blueKoma = DX.LoadGraph("blue.png");
		}

		public void UpdateViewInfo()
		{
			if (DX.CheckHitKey(DX.KEY_INPUT_UP) == 1) { Ty += 3 / Scale; }
			if (DX.CheckHitKey(DX.KEY_INPUT_RIGHT) == 1) { Tx -= 3 / Scale; }
			if (DX.CheckHitKey(DX.KEY_INPUT_DOWN) == 1) { Ty -= 3 / Scale; }
			if (DX.CheckHitKey(DX.KEY_INPUT_LEFT) == 1) { Tx += 3 / Scale; }
			if (DX.CheckHitKey(DX.KEY_INPUT_Z) == 1) { Scale /= 1.02; }
			if (DX.CheckHitKey(DX.KEY_INPUT_X) == 1) { Scale *= 1.02; }
		}

		public void ShowBoard(List<List<int>> board)
		{
			int r, c;
			uint red = DX.GetColor(255, 0, 0);
			uint blue = DX.GetColor(0, 0, 255);
			uint white = DX.GetColor(255, 255, 255);
			uint[] Color = { white, red, blue, red, blue };

			for (r = 0; r < 6; r++)
			{
				for (c = 0; c < 6; c++)
				{
					int ly = (int)ToDrawLy(r);
					int ry = (int)ToDrawLy(r + 1);
					int lx = (int)ToDrawLx(c);
					int rx = (int)ToDrawLx(c + 1);
					int cx = (lx + rx) / 2;
					int cy = (ly + ry) / 2;
					int type = board[r][c];

					if (type == 0) { continue; }
					if (type == 1) { DX.DrawRotaGraph(cx, cy, Scale, 0, redKoma, DX.FALSE); }
					if (type == 2) { DX.DrawRotaGraph(cx, cy, Scale, 0, blueKoma, DX.FALSE); }
					if (type == 3) { DX.DrawRotaGraph(cx, cy, Scale, 3.1415926, redKoma, DX.FALSE); }
					if (type == 4) { DX.DrawRotaGraph(cx, cy, Scale, 3.1415926, blueKoma, DX.FALSE); }
				}
			}

			for (r = 0; r < 6; r++)
			{
				for (c = 0; c < 6; c++)
				{
					int ly = (int)ToDrawLy(r);
					int ry = (int)ToDrawLy(r + 1);
					int lx = (int)ToDrawLx(c);
					int rx = (int)ToDrawLx(c + 1);
					DX.DrawBox(lx, ly, rx, ry, 0, DX.FALSE);
				}
			}
		}

		public void ShowLoser(int player)
		{
			if (player == 0)
			{
				DX.DrawStringToHandle(150, 450, "後手勝ち", DX.GetColor(255, 0, 0), font100);
			}
			else
			{
				DX.DrawStringToHandle(150, 450, "先手勝ち", DX.GetColor(0, 0, 255), font100);
			}
		}

		//row(≧0)行目のマスの上辺のy座標を返す
		private double ToDrawLy(int row)
		{
			double y = row * cellSize + Ty;
			return (y - windowSizeY / 2) * Scale + windowSizeY / 2;
		}

		//col(≧0)列目のマスの左辺のx座標を返す
		private double ToDrawLx(int col)
		{
			double x = col * cellSize + Tx;
			return (x - windowSizeX / 2) * Scale + windowSizeX / 2;
		}
	}
}
