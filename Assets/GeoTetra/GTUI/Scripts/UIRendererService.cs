using System;
using System.Threading.Tasks;
using GeoTetra.GTPooling;
using UnityEngine;

namespace GeoTetra.GTUI
{
    [Serializable]
    public class UIRendererServiceRef : ServiceObjectReferenceT<UIRendererService>
    {
        public UIRendererServiceRef(string guid) : base(guid)
        { }
    }
    
    [CreateAssetMenu(menuName = "GeoTetra/Services/UIRendererService", fileName = "UIRendererService.asset")]
    public class UIRendererService : ServiceObject
    {
        [SerializeField] private UIRenderer _uiRendererPrefab;
        
        public UIRenderer OverlayUI { get; private set; }
        
        protected override async Task OnServiceStart()
        {
            OverlayUI = Instantiate(_uiRendererPrefab);
            //Await Two frames for Awake and Start
            await Task.Yield();
            await Task.Yield();
            await base.OnServiceStart();
        }
    }
}