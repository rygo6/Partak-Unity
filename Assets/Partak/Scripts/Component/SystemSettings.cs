using UnityEngine;
using System.Collections;

public class SystemSettings : MonoBehaviour 
{
	public bool FullVersion { get; set; }

	private const string WelcomeFullMessage = "Partak is a complete rewrite and rebrand of the game priorly known as enrgy. This is the first step of a new effort to revamp and improve this game, expect more updates. As someone who purchased the Full Version of enrgy, know that you will always recieve a fully featured, unlocked and ad-free version of Partak. Thank you for your early support.";

	private const string WelcomeMessage = "Partak is a complete rewrite and rebrand of the game priorly known as enrgy. This is the first step of a new effort to revamp and improve this game, expect more updates.";

	private void Awake()
	{
		if (PlayerPrefs.HasKey("isFullVersion"))
		{
			FullVersion = true;	
			Prime31.EtceteraBinding.showAlertWithTitleMessageAndButtons("Welcome to Partak", WelcomeFullMessage, new string[1]{"Ok"});
		}
		else
		{
			Prime31.EtceteraBinding.showAlertWithTitleMessageAndButtons("Welcome to Partak", WelcomeMessage, new string[1]{"Ok"});
		}
	}
}
