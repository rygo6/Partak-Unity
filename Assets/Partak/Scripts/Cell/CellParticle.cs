using UnityEngine;

namespace Partak {
/// <summary>
/// Cell particle.
/// Particle which is contained in a cell.
/// </summary>
public class CellParticle {

	CellParticleStore _cellParticleStore;
	ParticleCell _particleCell;
	int _life;
	public int PlayerIndex { get; set; }

	public ParticleCell ParticleCell { 
		get { return _particleCell; }
		set {
			if (_particleCell != null) {
				_particleCell.CellParticle = null;
			}
			_particleCell = value;
			_particleCell.CellParticle = this;
		}
	}

	public int Life {
		get	{ return _life; }
		set { 
			_life = Mathf.Clamp(value, 0, 255); 
		}
	}

	public CellParticle(int playerIndex, ParticleCell particleCell, CellParticleStore cellParticleStore) {
		_cellParticleStore = cellParticleStore;
		PlayerIndex = playerIndex;
		Life = 255;
		ParticleCell = particleCell;
		_cellParticleStore.IncrementPlayerParticleCount(PlayerIndex);
	}

	public void ChangePlayer(int newPlayerIndex) {
		_cellParticleStore.DecrementPlayerParticleCount(PlayerIndex);
		ParticleCell.BottomCellGroup.RemovePlayerParticle(PlayerIndex);
		PlayerIndex = newPlayerIndex;
		Life = 255;
		ParticleCell.InhabitedBy = PlayerIndex;
		ParticleCell.BottomCellGroup.AddPlayerParticle(PlayerIndex);
		_cellParticleStore.IncrementPlayerParticleCount(PlayerIndex);
	}
}
}