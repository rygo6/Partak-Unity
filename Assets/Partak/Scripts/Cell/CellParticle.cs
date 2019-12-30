using UnityEngine;

namespace GeoTetra.Partak
{
	/// <summary>
	///     Cell particle.
	///     Particle which is contained in a cell.
	/// </summary>
	public class CellParticle
    {
        private CellParticleStore _cellParticleStore;
        private int _life;
        private ParticleCell _particleCell;

        public CellParticle(int playerIndex, ParticleCell particleCell, CellParticleStore cellParticleStore)
        {
            _cellParticleStore = cellParticleStore;
            PlayerIndex = playerIndex;
            Life = 255;
            ParticleCell = particleCell;
            _cellParticleStore.IncrementPlayerParticleCount(PlayerIndex);
        }

        public int PlayerIndex { get; set; }

        public ParticleCell ParticleCell
        {
            get => _particleCell;
            set
            {
                if (_particleCell != null) _particleCell.CellParticle = null;
                _particleCell = value;
                _particleCell.CellParticle = this;
            }
        }

        public int Life
        {
            get => _life;
            set => _life = Mathf.Clamp(value, 0, 255);
        }

        public void ChangePlayer(int newPlayerIndex)
        {
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