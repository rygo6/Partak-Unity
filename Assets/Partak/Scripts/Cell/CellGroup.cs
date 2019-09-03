using UnityEngine;
using System.Collections.Generic;

namespace Partak
{
    /// <summary>
    /// Cell group.
    /// Group which contains multiple cells.
    /// </summary>
    public class CellGroup
    {
        public Vector3 WorldPosition { get; set; }

        public CellGroup ParentCellGroup { get; set; }

        public CellGroup[] ParentCellGroups { get; set; }

        public readonly CellGroupGrid CellGroupGrid;

        /// <summary>
        /// How many particles of each player this group contains.
        /// </summary>
        public readonly int[] PlayerParticleCount;

        /// <summary>
        /// Left to Right, Bottom to Top array of children cells. Or only 1 index if root level group.
        /// 2 - 3
        /// 0 - 1
        /// </summary>
        public readonly CellGroup[] ChildCellGroupArray;

        /// <summary>
        /// Left to Right, Bottom to Top array of children particle cells. Or only 1 index if root level group.
        /// 2 - 3
        /// 0 - 1
        /// </summary>
        public readonly ParticleCell[] ChildParticleCellArray;

        public readonly CellGroup[] DirectionalCellGroupArray;

        /// <summary>
        /// Used to keep tabs on which cells have been added to the step array for calculation, 
        /// 2 means not to mark as it is a child and it is only used to step up to top level grid
        /// </summary>
        public bool InStepArray { get; set; }

        public CellGroup(CellGroupGrid cellGroupGrid, 
            CellGroup[] childCellGroupArray,
            ParticleCell[] childParticleCellArray,
            int playerCount)
        {
            PlayerParticleCount = new int[playerCount];
            DirectionalCellGroupArray = new CellGroup[Direction12.Count];
            CellGroupGrid = cellGroupGrid;
            ChildParticleCellArray = childParticleCellArray;
            ChildCellGroupArray = childCellGroupArray;
            WorldPosition = ChildParticleAverageWorldPosition(childParticleCellArray);
            SetChildCellGroupParentCellGroup(childCellGroupArray, this);
            SetChildParticleArrayTopCellGroup(childParticleCellArray, this);
        }

        public void FillParentCellGroups()
        {
            List<CellGroup> parentCellGroupList = new List<CellGroup>();
            CellGroup parentCellGroup = ParentCellGroup;
            while (parentCellGroup != null)
            {
                parentCellGroupList.Add(parentCellGroup);
                parentCellGroup = parentCellGroup.ParentCellGroup;
            }

            ParentCellGroups = parentCellGroupList.ToArray();
        }

        public void AddPlayerParticle(int playerIndex)
        {
            PlayerParticleCount[playerIndex]++;
            for (int i = 0; i < ParentCellGroups.Length; ++i)
            {
                ParentCellGroups[i].PlayerParticleCount[playerIndex]++;
            }
        }

        public void RemovePlayerParticle(int playerIndex)
        {
            PlayerParticleCount[playerIndex]--;
            for (int i = 0; i < ParentCellGroups.Length; ++i)
            {
                ParentCellGroups[i].PlayerParticleCount[playerIndex]--;
            }
        }

        public void SetPrimaryDirectionChldParticleCell(int direction, int playerIndex)
        {
            int i = 0;
            int limit = ChildParticleCellArray.Length;
            for (i = 0; i < limit; ++i)
            {
                ChildParticleCellArray[i].PrimaryDirectionArray[playerIndex] = direction;
            }
        }

        void SetChildParticleArrayTopCellGroup(ParticleCell[] childParticleCellArray, CellGroup topCellGroup)
        {
            for (int i = 0; i < childParticleCellArray.Length; ++i)
            {
                childParticleCellArray[i].TopCellGroup = topCellGroup;
            }
        }

        void SetChildCellGroupParentCellGroup(CellGroup[] childCellGroup, CellGroup parentCellgroup)
        {
            if (childCellGroup != null)
            {
                for (int i = 0; i < childCellGroup.Length; ++i)
                {
                    childCellGroup[i].ParentCellGroup = parentCellgroup;
                }
            }
        }

        Vector3 ChildParticleAverageWorldPosition(ParticleCell[] childParticleCellArray)
        {
            Vector3 worldPosition = new Vector3();
            for (int i = 0; i < childParticleCellArray.Length; ++i)
            {
                worldPosition += childParticleCellArray[i].WorldPosition;
            }

            return worldPosition / childParticleCellArray.Length;
        }
    }
}