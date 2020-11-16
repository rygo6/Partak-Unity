using UnityEngine;
using System.Collections;

/// <summary>
/// Disables specified RectTransforms if they are inside this RectTransform.
/// </summary>
public class DisableIn : MonoBehaviour 
{
	[SerializeField]
	private RectTransform[] _rectTransforms;

	private RectTransform _centerBounds;

	public void Initialize()
	{
		Vector3[] corners = new Vector3[4];
		GetComponent<RectTransform>().GetWorldCorners(corners);

		Bounds bounds = new Bounds((corners[2] + corners[0]) / 2f, corners[2] - corners[0]);
		bounds.size = new Vector3(bounds.size.x, bounds.size.y, 1f);

		for (int i = 0; i < _rectTransforms.Length; ++i)
		{
			if (bounds.Contains(_rectTransforms[i].position))
			{
				_rectTransforms[i].gameObject.SetActive(false);
			}
			else
			{
				_rectTransforms[i].gameObject.SetActive(true);
			}
		}
	}
}
