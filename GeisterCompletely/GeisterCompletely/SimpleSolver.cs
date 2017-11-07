using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeisterCompletely
{
	//青い駒をゴールに近づけるだけの簡単なAI
	class SimpleSolver : SolverInterface
	{
		public string PutRedKoma()
		{
			return "EFGH";
		}

		//戻り値：MOV:(行, 列), 方向
		public string Think(List<List<int>> board)
		{
			int[] dy = { -1, 0, 1, 0 };
			int[] dx = { 0, 1, 0, -1 };
			string[] Direction = { "U", "R", "D", "L"};
			int y, x, dir;

			int minDist = 114514;
			string ret = "";

			for (y = 0; y < 6; y++)
			{
				for (x = 0; x < 6; x++)
				{
					if (board[y][x] != 2)
					{
						continue;
					}

					for (dir = 0; dir < 4; dir++)
					{
						int ny = y + dy[dir];
						int nx = x + dx[dir];

						if ((ny == 0 && nx == -1) || (ny == 0 && nx == 6)) { return "MOV:(" + y + ", " + x + "), " + Direction[dir]; }
						if (0 <= ny && ny < 6 && 0 <= nx && nx < 6 && board[ny][nx] != 1 && board[ny][nx] != 2)
						{
							//MOV:(y, x), Direction[dir]が可能
							int dist = ny + Math.Min(6 - nx, nx + 1);
							if (minDist > dist)
							{
								minDist = dist;
								ret = "MOV:(" + y + ", " + x + "), " + Direction[dir];
							}
						}
					}
				}
			}
			return ret;
		}
	}
}
