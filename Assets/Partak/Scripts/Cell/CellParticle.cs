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

		public bool UpdateColor { get; set; }

		public int PlayerIndex { get; set; }

		public int Life
		{
			get	{ return _life; }
			set
			{ 
				_life = Mathf.Clamp(value, 0, 255); 
			}
		}
		private int _life = 255;

		public Color32 Color
		{
			get	{ return _color; }
			set
			{ 
				UpdateColor = true;
				_color = value; 
			}
		}
		public Color32 _color;
	}
}