using UnityEngine;

namespace Partak
{
    /// <summary>
    /// Cell particle.
    /// Particle which is contained in a cell.
    /// </summary>
    public class CellParticle
	{
		private CellParticleStore _cellParticleStore;

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

		public int Life
		{
			get	{ return _life; }
			set
			{ 
				_life = Mathf.Clamp(value, 0, 255); 
			}
		}
		private int _life;

		public int PlayerIndex { get; set; }

		public Color PlayerColor { get; set; }

		private PlayerSettings _playerSettings;

		public CellParticle(int playerIndex, ParticleCell particleCell, CellParticleStore cellParticleStore)
		{
			_playerSettings = Persistent.Get<PlayerSettings>();
			_cellParticleStore = cellParticleStore;
			PlayerIndex = playerIndex;
			Life = 255;
			PlayerColor = _playerSettings.PlayerColors[PlayerIndex];
			ParticleCell = particleCell;
			_cellParticleStore.IncrementPlayerParticleCount(PlayerIndex);
		}

		public void ChangePlayer(int newPlayerIndex)
		{
			_cellParticleStore.DecrementPlayerParticleCount(PlayerIndex);
			ParticleCell.BottomCellGroup.RemovePlayerParticle(PlayerIndex);
			PlayerIndex = newPlayerIndex;
			Life = 255;
			PlayerColor = _playerSettings.PlayerColors[PlayerIndex];
			ParticleCell.InhabitedBy = PlayerIndex;
			ParticleCell.BottomCellGroup.AddPlayerParticle(PlayerIndex);
			_cellParticleStore.IncrementPlayerParticleCount(PlayerIndex);
		}
	}
}