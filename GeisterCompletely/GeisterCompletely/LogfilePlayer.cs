using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace GeisterCompletely
{
	class LogfilePlayer
	{
		string RedPutFirst;
		string RedPutSecond;

		string MoveKomaName;
		string MoveKomaDir;

		//シミュレーション用変数 (クライアント盤面での座標)
		int[] dy = { -1, 0, 1, 0 };
		int[] dx = { 0, 1, 0, -1 };
		string MoveDir = "URDL";
		List<List<int>> posY;
		List<List<int>> posX;

		public void Init()
		{
			//初期配置
			posY = new List<List<int>>();
			posX = new List<List<int>>();

			for (int i = 0; i < 2; i++) { posX.Add(new List<int>()); posY.Add(new List<int>()); }
			for (int i = 0; i < 2; i++)
			{
				for (int j = 0; j < 8; j++)
				{
					posY[i].Add(4 + j / 4);
					posX[i].Add(1 + j % 4);
				}
			}

			//ファイル名列挙
			DirectoryInfo directory = new DirectoryInfo(@"C:\Users\hashimotolab\Documents\GitHub\geister_server.java\log");
			FileInfo[] files = directory.GetFiles("*.txt", SearchOption.TopDirectoryOnly);
			List<string> fnames = new List<string>();
			foreach (FileInfo file in files)
			{
				fnames.Add(file.FullName);
			}
			fnames.Sort((a, b) => -a.CompareTo(b));

			List<string> FileNames = new List<string>();
			for (int i = 0; i < fnames.Count; i++)
			{
				if (fnames[i].CompareTo(@"C:\Users\hashimotolab\Documents\GitHub\geister_server.java\log\log-2017-11-10-20-12-17-625.txt") < 0)
				{
					break;
				}
				FileNames.Add(fnames[i]);
			}
			FileNames.Reverse();

			//ファイル読み込み
			StreamReader reader = new StreamReader(FileNames[10 - 1]);
			string s;

			s = reader.ReadLine();
			if (s[7] == '0') { for (int i = 13; i < 17; i++) { RedPutFirst += s[i]; } }
			else { for (int i = 13; i < 17; i++) { RedPutSecond += s[i]; } }
			s = reader.ReadLine();
			if (s[7] == '0') { for (int i = 13; i < 17; i++) { RedPutFirst += s[i]; } }
			else { for (int i = 13; i < 17; i++) { RedPutSecond += s[i]; } }

			while (true)
			{
				s = reader.ReadLine();
				if (s == null || s.Length == 0 || s[0] == 'w' && s[1] == 'i' && s[2] == 'n')
				{
					break;
				}
				MoveKomaName += s[13];
				MoveKomaDir += ConvertDirChar(s[15]);
			}
			reader.Close();
		}

		private char ConvertDirChar(char dir)
		{
			if (dir == 'N') return 'U';
			if (dir == 'E') return 'R';
			if (dir == 'S') return 'D';
			return 'L';
		}

		public string PutRedKoma(int player)
		{
			if (player == 0) { return RedPutFirst; }
			return RedPutSecond;
		}

		public int MaxTurnNum()
		{
			return MoveKomaName.Length;
		}

		public string Think(int TurnCount)
		{
			int id = MoveKomaName[TurnCount] - 'A';
			int y = posY[TurnCount % 2][id];
			int x = posX[TurnCount % 2][id];
			string ret = "MOV:(" + y + ", " + x + "), " + MoveKomaDir[TurnCount];

			int dir;
			for (dir = 0; dir < 4; dir++) { if (MoveKomaDir[TurnCount] == MoveDir[dir]) { break; } }
			posY[TurnCount % 2][id] = y + dy[dir];
			posX[TurnCount % 2][id] = x + dx[dir];
			return ret;
		}
	}
}
