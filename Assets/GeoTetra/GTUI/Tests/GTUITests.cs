using System.Collections;
using GeoTetra.GTPooling;
using GeoTetra.GTUI;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

public class GTUITests
{
    [UnityTest]
    public IEnumerator LoadUIRendererPrefab()
    {
        Assert.NotNull(InstanceUIRendererPrefab());
        yield return null;
    }
    
    [UnityTest]
    public IEnumerator BuildUIRenderer()
    {
        // ComponentContainer componentContainer = InstanceComponentContiner();
        // GameObject gameObject = new GameObject("UIRenderer");
        // gameObject.SetActive(false);
        // UIRenderer uiRenderer = gameObject.AddComponent<UIRenderer>();
        // uiRenderer._componentContainer = new ServiceReference(componentContainer); 
        // gameObject.SetActive(true);
        yield return null;
        // Assert.NotNull(componentContainer.Get<UIRenderer>());
    }
    
    private ComponentContainer InstanceComponentContiner()
    {
        // return ScriptableObject.CreateInstance<ComponentContainer>();
        return null;
    }

    private UIRenderer InstanceUIRendererPrefab()
    {
        UIRenderer prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/GeoTetra/GTUI/Prefabs/UIRenderer.prefab").GetComponent<UIRenderer>();
        return Object.Instantiate(prefab);
    }
}
