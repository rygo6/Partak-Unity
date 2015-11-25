using UnityEngine;
using System.Collections;

namespace Partak
{
	/// <summary>
	/// Cell group.
	/// Group which contains multiple cells.
	/// </summary>
	public class CellGroup
	{
		public Vector3 WorldPosition { get; set; }

		public readonly CellGroupGrid CellGroupGrid;

		/// <summary>
		/// How many particles of each player this group contains.
		/// </summary>
		public readonly int[] PlayerParticleCount =  new int[PlayerSettings.MaxPlayers];
		
		public CellGroup ParentCellGroup { get; set; }

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
		/// Used to keep tabs on which cells have been added to the step array for calculation
		/// </summary>
		public bool InStepArray { get; set; }

		public CellGroup(CellGroupGrid cellGroupGrid, CellGroup[] childCellGroupArray, ParticleCell[] childParticleCellArray)
		{
			DirectionalCellGroupArray = new CellGroup[Direction12.Count];
			CellGroupGrid = cellGroupGrid;
			ChildParticleCellArray = childParticleCellArray;
			ChildCellGroupArray = childCellGroupArray;
			WorldPosition = ChildParticleAverageWorldPosition(childParticleCellArray);
			SetChildCellGroupParentCellGroup(childCellGroupArray, this);
			SetChildParticleArrayTopCellGroup(childParticleCellArray, this);
		}

		public void AddPlayerParticle(int playerIndex)
		{
			PlayerParticleCount[playerIndex]++;
			if (ParentCellGroup != null)
			{
				ParentCellGroup.AddPlayerParticle(playerIndex);
			}
		}

		public void RemovePlayerParticle(int playerIndex)
		{
			PlayerParticleCount[playerIndex]--;
			if (ParentCellGroup != null)
			{
				ParentCellGroup.RemovePlayerParticle(playerIndex);
			}
		}

		public void SetPrimaryDirectionChldParticleCell(int direction, int playerIndex)
		{
			for (int i = 0; i < ChildParticleCellArray.Length; ++i)
			{
				ChildParticleCellArray[i].PrimaryDirectionArray[playerIndex] = direction;
			}
		}

		private void SetChildParticleArrayTopCellGroup(ParticleCell[] childParticleCellArray, CellGroup topCellGroup)
		{
			for (int i = 0; i < childParticleCellArray.Length; ++i)
			{
				childParticleCellArray[i].TopCellGroup = topCellGroup;
			}
		}

		private void SetChildCellGroupParentCellGroup(CellGroup[] childCellGroup, CellGroup parentCellgroup)
		{
			if (childCellGroup != null)
			{
				for (int i = 0; i < childCellGroup.Length; ++i)
				{
					childCellGroup[i].ParentCellGroup = parentCellgroup;
				}
			}
		}

		private Vector3 ChildParticleAverageWorldPosition(ParticleCell[] childParticleCellArray)
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