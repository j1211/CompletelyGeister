using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeisterCompletely
{
	interface SolverInterface
	{
		/*自分の赤い駒の配置をA～Hのうち、4文字で指定する。前4つを赤くしたければABCD, を返す。*/
		string PutRedKoma();

		/*board[行][列] … client用のボード。0-indexed。0…空マス, 1と2…自分の赤・青駒。3と4…相手の赤・青駒。
		
		相手	
		00 01 02 03 04 05
		10 11 12 13 14 15
		…
		50 51 52 53 54 55
		自分(↑上方向, →右方向）

		戻り値：動かす駒の位置(行, 列)とその方向(↑U, →R, ↓D, ←L)。例えば、3行4列目の駒を↑方向に動かす場合は、
		MOV:(3, 4), U
		と指定する。この処理の後、3行4列目にあった駒は「2行4列目」に移動する。*/
		string Think(List<List<int>> board);
	}
}
