using UnityEngine;
using System.Collections;

static public class ColorExtension {
	static public Color SetRGB(this Color color, Color input) {
		color.r = input.r;
		color.g = input.g;
		color.b = input.b;
		return color;
	}

	static public bool RGBEquals(this Color color, Color input) {
		return color.r == input.r && color.g == input.g && color.b == input.b;
	}
}
