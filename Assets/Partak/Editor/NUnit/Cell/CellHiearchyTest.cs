using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;

namespace GeoTetra.Partak
{
    [TestFixture]
    public class CellHiearchyTest
    {
        private const int PlayerCount = 4;

        [Test]
        public void BuildParticleCellGridSizeTest()
        {
            ParticleCellGrid particleCellGrid = new ParticleCellGrid(192, 192);

            if (particleCellGrid.Grid.Length == 192 * 192)
            {
                DrawRayFromParticleCellCorner(particleCellGrid.Grid);
                Assert.Pass("particleCellList.Count == " + particleCellGrid.Grid.Length);
            }
            else
            {
                Assert.Fail("particleCellList.Count == " + particleCellGrid.Grid.Length);
            }
        }

        [TestCase(0)]
        [TestCase(128)]
        [TestCase(640)]
        public void BuildParticleCellGridUpTest(int startIndex)
        {
            ParticleCellGrid particleCellGrid = new ParticleCellGrid(192, 192);
            ParticleCell topCell = particleCellGrid.Grid[0].DirectionalParticleCellArray[Direction12.NNE];

            if (topCell.DirectionalParticleCellArray[Direction12.SSE] == particleCellGrid.Grid[0])
            {
                Assert.Pass();
            }
            else
            {
                Assert.Fail();
            }
        }

        [TestCase(0)]
        [TestCase(128)]
        [TestCase(640)]
        public void BuildParticleCellGridRightTest(int startIndex)
        {
            ParticleCellGrid particleCellGrid = new ParticleCellGrid(192, 192);
            ParticleCell rightCell = particleCellGrid.Grid[0].DirectionalParticleCellArray[Direction12.NEE];

            if (rightCell.DirectionalParticleCellArray[Direction12.NWW] == particleCellGrid.Grid[0])
            {
                Assert.Pass();
            }
            else
            {
                Assert.Fail();
            }
        }

        [Test]
        public void BuildParticleCellGridUpperRightNullTest()
        {
            ParticleCellGrid particleCellGrid = new ParticleCellGrid(192, 192);
            int lastIndex = particleCellGrid.Grid.Length - 1;

            if (particleCellGrid.Grid[lastIndex].DirectionalParticleCellArray[Direction12.NE] == null)
            {
                Assert.Pass("Upper Right Position: " + particleCellGrid.Grid[lastIndex].WorldPosition);
            }
            else
            {
                Assert.Fail();
            }
        }

        [Test]
        public void BuildCellGroupLayerFromParticleCellGridTest()
        {
            ParticleCellGrid particleCellGrid = new ParticleCellGrid(192, 192);
            CellGroupGrid cellGroupGrid = new CellGroupGrid(particleCellGrid, PlayerCount);

            if (cellGroupGrid.Grid.Length == 192 * 192)
            {
                DrawRayFromCellGroupCorner(cellGroupGrid.Grid);
                Assert.Pass();
            }
            else
            {
                Assert.Fail();
            }
        }

        [Test]
        public void BuildCellGroupLayerFromParticleCellGridRightNullTest()
        {
            ParticleCellGrid particleCellGrid = new ParticleCellGrid(192, 192);
            CellGroupGrid cellGroupGrid = new CellGroupGrid(particleCellGrid, PlayerCount);

            CellGroup rightCell = cellGroupGrid.Grid[0].DirectionalCellGroupArray[Direction12.NEE];

            if (rightCell.DirectionalCellGroupArray[Direction12.NWW] == cellGroupGrid.Grid[0])
            {
                Assert.Pass();
            }
            else
            {
                Assert.Fail();
            }
        }

        [Test]
        public void BuildParentCellGroupLayer1SizeTest()
        {
            ParticleCellGrid particleCellGrid = new ParticleCellGrid(192, 192);
            CellGroupGrid cellGroupGrid0 = new CellGroupGrid(particleCellGrid, PlayerCount);
            CellGroupGrid cellGroupGrid1 = new CellGroupGrid(cellGroupGrid0, PlayerCount);

            if (cellGroupGrid1.Grid.Length == 96 * 96)
            {
                DrawRayFromCellGroupCorner(cellGroupGrid1.Grid);
                Assert.Pass("Elements: " + ElementsInArray(cellGroupGrid1.Grid) + " Null Elements: " +
                            NullElementsInArray(cellGroupGrid1.Grid));
            }
            else
            {
                Assert.Fail();
            }
        }

        [Test]
        public void BuildParentCellGroupLayer1RightNullTest()
        {
            ParticleCellGrid particleCellGrid = new ParticleCellGrid(192, 192);
            CellGroupGrid cellGroupGrid0 = new CellGroupGrid(particleCellGrid, PlayerCount);
            CellGroupGrid cellGroupGrid1 = new CellGroupGrid(cellGroupGrid0, PlayerCount);

            CellGroup cellGroup = cellGroupGrid1.Grid[0].DirectionalCellGroupArray[Direction12.NEE];

            if (cellGroup.DirectionalCellGroupArray[Direction12.NWW] == cellGroupGrid1.Grid[0] &&
                cellGroup.DirectionalCellGroupArray[Direction12.NWW].CellGroupGrid.ParentLevel ==
                cellGroupGrid1.ParentLevel)
            {
                Assert.Pass();
            }
            else
            {
                Assert.Fail();
            }
        }

        [Test]
        public void BuildParentCellGroupLayer2SizeTest()
        {
            ParticleCellGrid particleCellGrid = new ParticleCellGrid(192, 192);
            CellGroupGrid cellGroupGrid0 = new CellGroupGrid(particleCellGrid, PlayerCount);
            CellGroupGrid cellGroupGrid1 = new CellGroupGrid(cellGroupGrid0, PlayerCount);
            CellGroupGrid cellGroupGrid2 = new CellGroupGrid(cellGroupGrid1, PlayerCount);

            if (cellGroupGrid2.Grid.Length == 48 * 48)
            {
                DrawRayCellGroupArray(cellGroupGrid2.Grid);
                Assert.Pass("Elements: " + ElementsInArray(cellGroupGrid2.Grid) + " Null Elements: " +
                            NullElementsInArray(cellGroupGrid2.Grid));
            }
            else
            {
                Assert.Fail("Elements: " + ElementsInArray(cellGroupGrid2.Grid) + " Null Elements: " +
                            NullElementsInArray(cellGroupGrid2.Grid));
            }
        }

        [Test]
        public void BuildParentCellGroupLayer3SizeTest()
        {
            ParticleCellGrid particleCellGrid = new ParticleCellGrid(192, 192);
            CellGroupGrid cellGroupGrid0 = new CellGroupGrid(particleCellGrid, PlayerCount);
            CellGroupGrid cellGroupGrid1 = new CellGroupGrid(cellGroupGrid0, PlayerCount);
            CellGroupGrid cellGroupGrid2 = new CellGroupGrid(cellGroupGrid1, PlayerCount);
            CellGroupGrid cellGroupGrid3 = new CellGroupGrid(cellGroupGrid2, PlayerCount);

            if (cellGroupGrid3.Grid.Length == 24 * 24)
            {
                DrawRayCellGroupArray(cellGroupGrid3.Grid);
                Assert.Pass("Elements: " + ElementsInArray(cellGroupGrid3.Grid) + " Null Elements: " +
                            NullElementsInArray(cellGroupGrid3.Grid));
            }
            else
            {
                Assert.Fail();
            }
        }

        [Test]
        public void BuildParentCellGroupLayer4SizeTest()
        {
            ParticleCellGrid particleCellGrid = new ParticleCellGrid(192, 192);
            CellGroupGrid cellGroupGrid0 = new CellGroupGrid(particleCellGrid, PlayerCount);
            CellGroupGrid cellGroupGrid1 = new CellGroupGrid(cellGroupGrid0, PlayerCount);
            CellGroupGrid cellGroupGrid2 = new CellGroupGrid(cellGroupGrid1, PlayerCount);
            CellGroupGrid cellGroupGrid3 = new CellGroupGrid(cellGroupGrid2, PlayerCount);
            CellGroupGrid cellGroupGrid4 = new CellGroupGrid(cellGroupGrid3, PlayerCount);

            if (cellGroupGrid4.Grid.Length == 12 * 12)
            {
                DrawRayFromCellGroupCorner(cellGroupGrid4.Grid);
                Assert.Pass("Elements: " + ElementsInArray(cellGroupGrid4.Grid) + " Null Elements: " +
                            NullElementsInArray(cellGroupGrid4.Grid));
            }
            else
            {
                Assert.Fail();
            }
        }

        [Test]
        public void BuildParentCellGroupLayer0SizeTo3SizeTest()
        {
            ParticleCellGrid particleCellGrid = new ParticleCellGrid(192, 192);
            CellGroupGrid cellGroupGrid0 = new CellGroupGrid(particleCellGrid, PlayerCount);
            CellGroupGrid cellGroupGrid1 = new CellGroupGrid(cellGroupGrid0, PlayerCount);
            CellGroupGrid cellGroupGrid2 = new CellGroupGrid(cellGroupGrid1, PlayerCount);
            CellGroupGrid cellGroupGrid3 = new CellGroupGrid(cellGroupGrid2, PlayerCount);

            if (cellGroupGrid3.Grid.Length == 24 * 24)
            {
                DrawRayFromCellGroupCorner(cellGroupGrid0.Grid);
                Assert.Pass("Elements: " + ElementsInArray(cellGroupGrid3.Grid) + " Null Elements: " +
                            NullElementsInArray(cellGroupGrid3.Grid));
            }
            else
            {
                Assert.Fail();
            }
        }

        private int NullElementsInArray(object[] objectArray)
        {
            int count = 0;
            for (int i = 0; i < objectArray.Length; ++i)
            {
                if (objectArray[i] == null)
                {
                    count++;
                }
            }

            return count;
        }

        private int ElementsInArray(object[] objectArray)
        {
            int count = 0;
            for (int i = 0; i < objectArray.Length; ++i)
            {
                if (objectArray[i] != null)
                {
                    count++;
                }
            }

            return count;
        }

        private void DrawRayCellGroupArray(CellGroup[] cellGroupArray)
        {
            for (int i = 0; i < cellGroupArray.Length; ++i)
            {
                if (cellGroupArray[i] != null)
                {
                    Debug.DrawRay(cellGroupArray[i].WorldPosition, Vector3.up * 2f);
                }
            }
        }

        private void DrawRayTraverseCellGroupArray(CellGroup[] cellGroupArray, int startIndex, int direction,
            Color color)
        {
            int runTimeout = 0;
            CellGroup cellGroup = cellGroupArray[startIndex];
            while (runTimeout < 512)
            {
                Debug.DrawRay(cellGroup.WorldPosition, Vector3.up * 2f, color);
                cellGroup = cellGroup.DirectionalCellGroupArray[direction];
                runTimeout++;
                if (cellGroup == null)
                {
                    runTimeout = 512;
                }
            }
        }

        private void DrawRayFromCellGroupCorner(CellGroup[] cellGroupArray)
        {
            for (int i = 0; i < cellGroupArray.Length; i += cellGroupArray.Length / 13)
            {
                if (cellGroupArray[i] != null)
                {
                    Color color = RandomColor();
                    DrawRayTraverseCellGroupArray(cellGroupArray, i, Direction12.NEE, color);
                    DrawRayTraverseCellGroupArray(cellGroupArray, i, Direction12.NE, color);
                    DrawRayTraverseCellGroupArray(cellGroupArray, i, Direction12.NNE, color);
                    DrawRayTraverseCellGroupArray(cellGroupArray, i, Direction12.NW, color);
                    DrawRayTraverseCellGroupArray(cellGroupArray, i, Direction12.NWW, color);
                    DrawRayTraverseCellGroupArray(cellGroupArray, i, Direction12.SW, color);
                    DrawRayTraverseCellGroupArray(cellGroupArray, i, Direction12.SSW, color);
                    DrawRayTraverseCellGroupArray(cellGroupArray, i, Direction12.SE, color);
                }
            }
        }

        private void DrawRayTraverseParticleCellArray(ParticleCell[] particleCellArray, int startIndex, int direction,
            Color color)
        {
            int runTimeout = 0;
            ParticleCell particleCell = particleCellArray[startIndex];
            while (runTimeout < 512)
            {
                Debug.DrawRay(particleCell.WorldPosition, Vector3.up * 2f, color);
                particleCell = particleCell.DirectionalParticleCellArray[direction];
                runTimeout++;
                if (particleCell == null)
                {
                    runTimeout = 512;
                }
            }
        }

        private void DrawRayFromParticleCellCorner(ParticleCell[] particleCellArray)
        {
            for (int i = 0; i < particleCellArray.Length; i += particleCellArray.Length / 13)
            {
                if (particleCellArray[i] != null)
                {
                    Color color = RandomColor();
                    DrawRayTraverseParticleCellArray(particleCellArray, i, Direction12.NEE, color);
                    DrawRayTraverseParticleCellArray(particleCellArray, i, Direction12.NE, color);
                    DrawRayTraverseParticleCellArray(particleCellArray, i, Direction12.NNE, color);
                    DrawRayTraverseParticleCellArray(particleCellArray, i, Direction12.NW, color);
                    DrawRayTraverseParticleCellArray(particleCellArray, i, Direction12.NWW, color);
                    DrawRayTraverseParticleCellArray(particleCellArray, i, Direction12.SW, color);
                    DrawRayTraverseParticleCellArray(particleCellArray, i, Direction12.SSW, color);
                    DrawRayTraverseParticleCellArray(particleCellArray, i, Direction12.SE, color);
                }
            }
        }

        private Color RandomColor()
        {
            int random = UnityEngine.Random.Range(0, 9);
            switch (random)
            {
                case 0:
                    return Color.black;
                case 1:
                    return Color.blue;
                case 2:
                    return Color.cyan;
                case 3:
                    return Color.gray;
                case 4:
                    return Color.green;
                case 5:
                    return Color.magenta;
                case 6:
                    return Color.red;
                case 7:
                    return Color.white;
                case 8:
                    return Color.yellow;
                default:
                    return Color.grey;
            }
        }
    }
}