using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DxLibDLL;

namespace GeisterCompletely
{
	class GameController
	{
		public void Start(int windowSizeX, int windowSizeY, string logFileName)
		{
			List<List<int>> board = new List<List<int>>();	//board[行][列]. 4,5行目側が先手プレイヤー. 0…空, 1,2…先手(赤・青), 3,4…後手(赤・青)
			int y, x;

			for (y = 0; y < 6; y++) board.Add(new List<int>());
			for (y = 0; y < 6; y++) for (x = 0; x < 6; x++) board[y].Add(0);

			View view = new View(windowSizeX, windowSizeY);
			LogfilePlayer com0 = new LogfilePlayer();
			LogfilePlayer com1 = new LogfilePlayer();
			int turnCount = 0;
			int losePlayer = -1;
			byte[] key = new byte[256];
			byte[] bkey = new byte[256];

			if (!com0.Init(logFileName)) return;
			if (!com1.Init(logFileName)) return;

			DX.GetHitKeyStateAll(bkey);
			if (!PutRedKoma(0, board, com0.PutRedKoma(0))) { while (DX.ProcessMessage() == 0 && DX.CheckHitKey(DX.KEY_INPUT_ESCAPE) == 0) view.ShowLoser(0); return; }
			if (!PutRedKoma(1, board, com1.PutRedKoma(1))) { while (DX.ProcessMessage() == 0 && DX.CheckHitKey(DX.KEY_INPUT_ESCAPE) == 0) view.ShowLoser(1); return; }

			while (DX.ScreenFlip() == 0 && DX.ProcessMessage() == 0 && DX.ClearDrawScreen() == 0 && DX.CheckHitKey(DX.KEY_INPUT_ESCAPE) == 0)
			{
				//表示
				for (int i = 0; i < 256; i++) { bkey[i] = key[i]; }
				DX.GetHitKeyStateAll(key);
				view.UpdateViewInfo();
				view.ShowBoard(board);
				if (losePlayer >= 0) { view.ShowLoser(losePlayer); continue; }

				//思考するか？
				if (!(bkey[DX.KEY_INPUT_RETURN] == 0 && key[DX.KEY_INPUT_RETURN] == 1)) { continue; }

				//時間切れ負け判定
				if (turnCount >= com0.MaxTurnNum()) { losePlayer = turnCount % 2; continue; }

				//思考 (手を決める)
				List <List<int>> cBoard = ToClientBoard(board, turnCount);
				string move;
				if (turnCount % 2 == 0) { move = com0.Think(turnCount); }
				else { move = com1.Think(turnCount); }

				//整合手, 勝敗判定など
				if (!IsRegalMove(cBoard, move)) { losePlayer = turnCount % 2; continue; }   //turnCount % 2 == 0 ⇔ 先手負け.

				//駒を動かす
				UpdateBoard(cBoard, move);

				//ボードの変換
				board = ToBoard(cBoard, turnCount);

				//勝敗判定
				if (IsEscape(move)) { losePlayer = (turnCount + 1) % 2; continue; } //turnCount % 2 == 0 ⇔ 先手勝ち.
				losePlayer = GetLosePlayer(cBoard, turnCount);

				//手番更新
				turnCount++;
			}
		}

		//赤駒を盤面に置く
		private bool PutRedKoma(int player, List<List<int>> board, string redPos)
		{
			bool[] isPut = new bool[8];
			int i;

			for (i = 0; i < 8; i++) { isPut[i] = false; }
			if (redPos.Length != 4) { return false; }
			for (i = 0; i < 4; i++) { isPut[(int)(redPos[i] - 'A')] = true; }

			//先手(player == 0)が手前
			if (player == 0)
			{
				for (i = 0; i < 8; i++)
				{
					if (isPut[i])
					{
						board[4 + i / 4][1 + i % 4] = 1;	//赤
					}
					else
					{
						board[4 + i / 4][1 + i % 4] = 2;	//青
					}
				}
			}
			else
			{
				for (i = 0; i < 8; i++)
				{
					if (isPut[i])
					{
						board[1 - i / 4][4 - i % 4] = 3;	//後手赤
					}
					else
					{
						board[1 - i / 4][4 - i % 4] = 4;	//後手青
					}
				}
			}
			return true;
		}


		//サーバで持っているboardを変換する。
		//手番プレイヤーが5行目側に来るようにboardを回転。 --> 手番プレイヤーの赤,青駒が「1,2」, 手番ではないプレイヤーの赤,青駒が「3,4」になるようにする。
		private List<List<int>> ToClientBoard(List<List<int>> board, int turnCount)
		{
			List<List<int>> clientBoard = new List<List<int>>();
			int y, x;
			for (y = 0; y < 6; y++) clientBoard.Add(new List<int>());
			for (y = 0; y < 6; y++) for (x = 0; x < 6; x++) clientBoard[y].Add(board[y][x]);

			if (turnCount % 2 == 0) { return clientBoard; }

			//180度回転
			for (y = 0; y < 3; y++)
			{
				for (x = 0; x < 6; x++)
				{
					int t = clientBoard[y][x];
					clientBoard[y][x] = clientBoard[5 - y][5 - x];
					clientBoard[5 - y][5 - x] = t;
				}
			}

			int[] convert = { 0, 3, 4, 1, 2 };
			for (y = 0; y < 6; y++)
			{
				for (x = 0; x < 6; x++)
				{
					clientBoard[y][x] = convert[clientBoard[y][x]];
				}
			}
			return clientBoard;
		}

		//ボード変換
		//先手が5行目側に来るようにする。先手プレイヤーの赤, 青が「1, 2」になるようにする。
		private List<List<int>> ToBoard(List<List<int>> cBoard, int turnCount)
		{
			return ToClientBoard(cBoard, turnCount);	//逆変換＝変換, を利用
		}

		//合法手かを判定する。
		private bool IsRegalMove(List<List<int>> cBoard, string move)
		{
			if (move.Length != 13) { return false; }

			int y, x;

			try
			{
				y = int.Parse(move[5].ToString());
				x = int.Parse(move[8].ToString());
			}
			catch
			{
				return false;
			}

			char dir = move[12];
			string Direction = "URDL";

			if (!(0 <= y && y < 6 && 0 <= x && x < 6)) { return false; }
			if (cBoard[y][x] != 1 && cBoard[y][x] != 2) { return false; }

			int i;
			for (i = 0; i < 4; i++)
			{
				if (dir == Direction[i]) { break; }
			}
			if (i == 4) { return false; }

			//動けるか判定
			int[] dy = { -1, 0, 1, 0 };
			int[] dx = { 0, 1, 0, -1 };
			int ny = y + dy[i];
			int nx = x + dx[i];

			//(y, x) --> (ny, nx)の判定
			if ((ny == 0 && nx == -1) || (ny == 0 && nx == 6)) { return cBoard[y][x] == 2; }
			if (0 <= ny && ny < 6 && 0 <= nx && nx < 6 && cBoard[ny][nx] != 1 && cBoard[ny][nx] != 2) { return true; }
			return false;
		}

		//脱出したか？（RegalMoveを仮定)
		private bool IsEscape(string move)
		{
			int y = int.Parse(move[5].ToString());
			int x = int.Parse(move[8].ToString());
			char dir = move[12];
			string Direction = "URDL";

			int i;
			for (i = 0; i < 4; i++) { if (dir == Direction[i]) { break; } }

			int[] dy = { -1, 0, 1, 0 };
			int[] dx = { 0, 1, 0, -1 };
			int ny = y + dy[i];
			int nx = x + dx[i];

			if ((ny == 0 && nx == -1) || (ny == 0 && nx == 6)) { return true; }
			return false;
		}

		//駒数による敗者を決める。まだ決まっていない場合は-1。先手が負けた場合0, 後手が負けた場合1を返す。
		//手番プレイヤーが駒を動かした直後の盤面で考える。
		private int GetLosePlayer(List<List<int>> cBoard, int turnCount)
		{
			List<List<int>> board = ToBoard(cBoard, turnCount);
			int i, y, x;
			int[] cnt = new int[5];

			for (i = 0; i < 5; i++) { cnt[i] = 0; }
			for (y = 0; y < 6; y++)
			{
				for (x = 0; x < 6; x++)
				{
					cnt[board[y][x]]++;
				}
			}

			if (cnt[1] == 0 || cnt[4] == 0) { return 1; }
			if (cnt[2] == 0 || cnt[3] == 0) { return 0; }
			return -1;
		}

		//駒を動かす
		void UpdateBoard(List<List<int>> cBoard, string move)
		{
			int y = int.Parse(move[5].ToString());
			int x = int.Parse(move[8].ToString());
			char dir = move[12];
			string Direction = "URDL";

			int i;
			for (i = 0; i < 4; i++) { if (dir == Direction[i]) { break; } }

			int[] dy = { -1, 0, 1, 0 };
			int[] dx = { 0, 1, 0, -1 };
			int ny = y + dy[i];
			int nx = x + dx[i];

			if (0 <= ny && ny < 6 && 0 <= nx && nx < 6) { cBoard[ny][nx] = cBoard[y][x]; }
			cBoard[y][x] = 0;
		}
	}
}
