using UnityEngine;
using System.Collections;

public class GameTimer : MonoBehaviour 
{
	[SerializeField]
	private Material _surroundMaterial;

	private float _timeLimit = 60f;

	[SerializeField]
	private float _time;

	private void Update()
	{
		_time += Time.deltaTime;
		if (_time > _timeLimit)
		{

		}
	}
}
