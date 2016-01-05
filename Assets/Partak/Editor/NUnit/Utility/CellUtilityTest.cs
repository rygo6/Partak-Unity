using System;
using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;
using UnityEngine;


namespace Partak
{
	[TestFixture]
	public class CellUtilityTest
	{
		
		[TestCase(0, 0, 0)]
		[TestCase(99, 9, 9)]
		[TestCase(10, 0, 1)]
		[TestCase(25, 5, 2)]
		[TestCase(73, 3, 7)]
		[TestCase(-1, -1, -1)]
		[TestCase(100, -1, -1)]
		public void GridIndexToCoordinateTest(int index, int x, int y)
		{       
			Vector2Int vector2Int = CellUtility.GridIndexToCoordinate(index, 10, 10);
			if (vector2Int.X == x && vector2Int.Y == y)
			{
				Assert.Pass();
			}
			else
			{
				throw new Exception("Input " + index + " result " + vector2Int + " expecting " + x + ", "+ y + ".");
			}
        }
        
		[TestCase(0, 0, 0)]
		[TestCase(99, 9, 9)]
		[TestCase(10, 0, 1)]
		[TestCase(25, 5, 2)]
		[TestCase(73, 3, 7)]
		[TestCase(-1, -1, -1)]
		public void CoordinateToGridIndexTest(int index, int x, int y)
		{       
			int result = CellUtility.CoordinateToGridIndex(x, y, 10, 10);
			if (result == index)
			{
				Assert.Pass();
			}
			else
			{
				throw new Exception("Input "+ x + " " + y + " result " + result + " expecting " + index + ".");
            }
        }

		/*
		Directions:
		0 - NNE
		1 - NE
		2 - NEE
		3 - SEE
		4 - SE
		5 - SSE
		6 - SSW
		7 - SW
		8 - SWW
		9 - NWW
		10 - NW
		11 - NNW		
		*/
		[TestCase(-1, 0, 0)]
		[TestCase(0, 0, 1)]//0 - NNE
		[TestCase(1, 1, 1)]//1 - NE
		[TestCase(2, 1, 0)]//2 - NEE
		[TestCase(3, 1, 0)]//3 - SEE
		[TestCase(4, 1, -1)]//4 - SE
		[TestCase(5, 0, -1)]//5 - SSE
		[TestCase(6, 0, -1)]//6 - SSW
		[TestCase(7, -1, -1)]//7 - SW
		[TestCase(8, -1, 0)]//8 - SWW
		[TestCase(9, -1, 0)]//9 - NWW
		[TestCase(10, -1, 1)]//10 - NW
		[TestCase(11, 0, 1)]//11 - NNW
		public void DirectionToXYTest(int direction, int x, int y)
		{       
			Vector2Int vector2Int = CellUtility.DirectionToXY(direction);
			if (vector2Int.X == x && vector2Int.Y == y)
			{
				Assert.Pass();
			}
			else
			{
				throw new Exception("Input " + direction + " result " + vector2Int + " expecting " + x + ", "+ y + ".");
            }
        }

		[TestCase(0, -4, 12, 8)]
		[TestCase(0, 4, 12, 4)]
		[TestCase(0, -4, 8, 4)]
		[TestCase(0, 4, 8, 4)]
		[TestCase(2, 4, 8, 6)]
		[TestCase(2, -4, 8, 6)]
		public void RotateDirectionTest(int direction, int rotation, int directionCount, int result)
		{      
			if (CellUtility.RotateDirection(direction, rotation, directionCount) == result)
			{
				Assert.Pass();
			}
			else
			{
				Assert.Fail();
			}
		}        
	
	}

}