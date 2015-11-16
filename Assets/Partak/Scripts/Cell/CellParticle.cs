using UnityEngine;
using System.Collections;

namespace Partak
{
	/// <summary>
	/// Cell particle.
	/// Particle which is contained in a cell.
	/// </summary>
	public class CellParticle
	{
		public ParticleCell ParticleCell
		{ 
			get { return _particleCell; }
			set
			{
				if (_particleCell != null)
				{
					_particleCell.CellParticle = null;
				}
				_particleCell = value;
				_particleCell.CellParticle = this;
			}
		}
		private ParticleCell _particleCell;

		public int PlayerIndex { get; set; }

		public int Life
		{
			get	{ return _life; }
			set
			{ 
				_life = Mathf.Clamp(value, 0, 255); 
			}
		}
		private int _life;

		public Color PlayerColor { get; set; }

		public CellParticle(int playerIndex, ParticleCell particleCell)
		{
			PlayerIndex = playerIndex;
			Life = 255;
			PlayerColor = Persistent.Get<PlayerSettings>().PlayerColors[PlayerIndex];
			ParticleCell = particleCell;
		}

		public void ChangePlayer(int newPlayerIndex)
		{
			ParticleCell.BottomCellGroup.RemovePlayerParticle(PlayerIndex);
			PlayerIndex = newPlayerIndex;
			Life = 255;
			PlayerColor = Persistent.Get<PlayerSettings>().PlayerColors[PlayerIndex];
			ParticleCell.InhabitedBy = PlayerIndex;
			ParticleCell.BottomCellGroup.AddPlayerParticle(PlayerIndex);
		}
	}
}