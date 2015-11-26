using UnityEngine;
using System.Collections;
using Prime31;



namespace Prime31
{
	public class EtceteraGUIManagerThree : MonoBehaviourGUI
	{
#if UNITY_IOS
		void OnGUI()
		{
			beginColumn();
	
			GUILayout.Label( "Inline Webview Methods" );
	
			if( GUILayout.Button( "Show Inline WebView" ) )
			{
				// remember, iOS uses points not pixels for positioning and layout!
				EtceteraBinding.inlineWebViewShow( 50, 10, 260, 300 );
				EtceteraBinding.inlineWebViewSetUrl( "http://google.com" );
			}
	
	
			if( GUILayout.Button( "Close Inline WebView" ) )
			{
				EtceteraBinding.inlineWebViewClose();
			}
	
	
			if( GUILayout.Button( "Set Url of Inline WebView" ) )
			{
				EtceteraBinding.inlineWebViewSetUrl( "http://prime31.com" );
			}
	
	
			if( GUILayout.Button( "Set Frame of Inline WebView" ) )
			{
				// remember, iOS uses points not pixels for positioning and layout!
				EtceteraBinding.inlineWebViewSetFrame( 10, 200, 250, 250 );
			}
	
	
			// Second row
			endColumn( true );
	
	
			if( GUILayout.Button( "Get Badge Count" ) )
			{
				Debug.Log( "badge count is: " + EtceteraBinding.getBadgeCount() );
			}
	
	
			if( GUILayout.Button( "Set Badge Count" ) )
			{
				EtceteraBinding.setBadgeCount( 46 );
			}
	
	
			if( GUILayout.Button( "Get First 25 Contacts" ) )
			{
				// note that accessing contacts requires special permission. Please see note in the documentation available here: https://prime31.com/docs#iosEtc
				var contacts = EtceteraBinding.getContacts( 0, 25 );
				Utils.logObject( contacts );
			}
	
	
			if( GUILayout.Button( "Get Pasteboard String" ) )
			{
				Debug.Log( "Pasteboard string: " + EtceteraBinding.getPasteboardString() );
			}
	
	
			if( GUILayout.Button( "Set Pasteboard String" ) )
			{
				EtceteraBinding.setPasteboardString( "setting the pasteboard string from Unity" );
			}
	
	
			if( GUILayout.Button( "Set Pasteboard Image" ) )
			{
				StartCoroutine( EtceteraBinding.getScreenShotTexture( tex =>
				{
					var bytes = tex.EncodeToPNG();
					EtceteraBinding.setPasteboardImage( bytes );
				}) );
			}
	
	
	
			endColumn();
	
	
			// Next scene button
			if( bottomRightButton( "Back" ) )
			{
				Application.LoadLevel( "EtceteraTestScene" );
			}
		}
#endif
	}

}
