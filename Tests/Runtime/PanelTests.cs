using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
namespace Veilbreak.Gui.Tests {
/// <summary>Package checks that need a live runtime panel: style resolution, layout and pointer targets.</summary>
public sealed class PanelTests {
 GameObject host;PanelSettings settings;
 [TearDown] public void Close(){if(host!=null)Object.Destroy(host);if(settings!=null)Object.Destroy(settings);}
 IEnumerator Panel(string sheet,System.Action<VisualElement> build){
  settings=ScriptableObject.CreateInstance<PanelSettings>();settings.referenceResolution=new Vector2Int(390,844);settings.scaleMode=PanelScaleMode.ScaleWithScreenSize;settings.match=0;
  host=new GameObject("Gui panel test");var document=host.AddComponent<UIDocument>();document.panelSettings=settings;yield return null;
#if UNITY_EDITOR
  document.rootVisualElement.styleSheets.Add(UnityEditor.AssetDatabase.LoadAssetAtPath<StyleSheet>("Packages/com.veilbreak.gui/Tests/Runtime/"+sheet));
#endif
  build(document.rootVisualElement);for(int i=0;i<3;i++)yield return null;}

 // Spike for token ownership (plan T3.2): shape colours and numbers may come from token variables, so Gui.uss can reference tokens instead of literals.
 [UnityTest] public IEnumerator ShapePropertiesResolveTokenVariables(){VisualElement token=null,literal=null;
  yield return Panel("TokenVariables.uss",root=>{var h=new VisualElement();h.AddToClassList("probe-host");root.Add(h);token=new VisualElement();token.AddToClassList("probe-token");h.Add(token);literal=new VisualElement();literal.AddToClassList("probe-literal");h.Add(literal);});
  Assert.IsTrue(literal.customStyle.TryGetValue(new CustomStyleProperty<Color>("--vb-fill"),out var plain),"Literal custom colours resolve");Assert.AreEqual(Color.green,plain);
  Assert.IsTrue(token.customStyle.TryGetValue(new CustomStyleProperty<Color>("--vb-fill"),out var colour),"var() inside a custom colour property resolves");Assert.AreEqual(Color.red,colour);
  Assert.IsTrue(token.customStyle.TryGetValue(new CustomStyleProperty<float>("--vb-cut"),out var cut),"var() inside a custom number property resolves");Assert.AreEqual(9,cut);}
 // Spike for font variables (plan T3.4): a font asset carried by a custom property, so consumer sheets need no package paths.
 [UnityTest] public IEnumerator FontDefinitionsResolveFromVariables(){Label text=null;
  yield return Panel("TokenVariables.uss",root=>{var h=new VisualElement();h.AddToClassList("probe-host");root.Add(h);text=new Label("FONT");text.AddToClassList("probe-font");h.Add(text);});
  var font=text.resolvedStyle.unityFontDefinition;Assert.IsTrue(font.font!=null||font.fontAsset!=null,"var() inside -unity-font-definition resolves to the font");}
 // Change events dispatch only inside a panel: forms and settings views report edits once hosted.
 [UnityTest] public IEnumerator FormsReportChangesAndSaveState(){ScreenHeader header=null;VbSwitch s=null;int changes=0;
  yield return Panel("TokenVariables.uss",root=>{header=new ScreenHeader("SETTINGS","").Into(root);var form=new FormComposite(header){SavedMeta="SAVED"}.Into(root);form.Changed+=()=>changes++;s=new VbSwitch("HAPTICS",null,false).Into(form.Fieldset("FEEL"));});
  s.value=true;Assert.AreEqual(1,changes);Assert.AreEqual("SAVED",header.Meta);}
 [UnityTest] public IEnumerator SettingsWriteTheModelThenRaiseChanged(){var model=new SettingsViewModel{haptics=false};SettingsViewModel got=null;SettingsView view=null;
  yield return Panel("TokenVariables.uss",root=>{view=new SettingsView().Into(root);view.Bind(model);view.Changed+=m=>got=m;});
  view.Q<VbSwitch>().value=true;Assert.AreSame(model,got);Assert.IsTrue(model.haptics);}
}
}
