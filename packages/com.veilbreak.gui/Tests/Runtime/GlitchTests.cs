using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
namespace Veilbreak.Gui.Tests {
public sealed class GlitchTests {
 GameObject host;PanelSettings settings;VisualElement root;
 [UnitySetUp] public IEnumerator Open(){settings=ScriptableObject.CreateInstance<PanelSettings>();host=new GameObject("Projection test");var document=host.AddComponent<UIDocument>();document.panelSettings=settings;yield return null;root=document.rootVisualElement;root.AddToClassList("vb-root");
#if UNITY_EDITOR
 root.styleSheets.Add(UnityEditor.AssetDatabase.LoadAssetAtPath<StyleSheet>("Packages/com.veilbreak.gui/Runtime/Styles/Gui.uss"));root.styleSheets.Add(UnityEditor.AssetDatabase.LoadAssetAtPath<StyleSheet>("Packages/com.veilbreak.gui/Runtime/Styles/Tokens.uss"));
#endif
 }
 [UnityTearDown] public IEnumerator Close(){Object.Destroy(host);Object.Destroy(settings);yield return null;}
 [UnityTest] public IEnumerator ChannelsAndEightBandsSettleAndReducedModesOnlyFade(){var text=new GlitchText("WAVE 08").Into(root);text.style.height=40;yield return null;Glitch.In(text);yield return new WaitForSecondsRealtime(.06f);Assert.AreEqual(8,text.Query(className:"vb-glitch__clip").ToList().Count);Assert.AreEqual(DisplayStyle.Flex,text.Q(className:"vb-glitch__red").style.display.value);
  yield return new WaitForSecondsRealtime(.3f);Assert.AreEqual(DisplayStyle.None,text.Q(className:"vb-glitch__red").style.display.value);Assert.AreEqual(1,text.style.opacity.value,.001);
  foreach(var flag in new[]{SafeAreaRoot.ReducedMotion,SafeAreaRoot.ReducedFlashes}){root.AddToClassList(flag);Glitch.Slam(text);yield return new WaitForSecondsRealtime(.04f);Assert.AreEqual(DisplayStyle.None,text.Q(className:"vb-glitch__red").style.display.value);Assert.IsTrue(text.style.scale.keyword==StyleKeyword.Null||text.style.scale.value.value==Vector3.one);yield return new WaitForSecondsRealtime(.2f);root.RemoveFromClassList(flag);}
  LogAssert.NoUnexpectedReceived();}
 [UnityTest] public IEnumerator ReplacementAndDetachCancelOldCallbacks(){var text=new GlitchText("OLD").Into(root);yield return null;int stale=0,fresh=0;Glitch.Out(text,()=>stale++);Glitch.In(text,()=>fresh++);yield return new WaitForSecondsRealtime(.4f);Assert.AreEqual(0,stale);Assert.AreEqual(1,fresh);Glitch.Out(text,()=>stale++);text.RemoveFromHierarchy();yield return new WaitForSecondsRealtime(.3f);root.Add(text);yield return new WaitForSecondsRealtime(.2f);Assert.AreEqual(0,stale);LogAssert.NoUnexpectedReceived();}
 [UnityTest] public IEnumerator AnnouncerPreemptsAndEveryNoticeLeaves(){var text=new GlitchText().Into(root);var lane=new Announcer(text);lane.Say("INCOMING",milliseconds:80);yield return new WaitForSecondsRealtime(.04f);lane.Say("BREACH",true,80);yield return new WaitForSecondsRealtime(.4f);Assert.AreEqual("BREACH",text.text);yield return new WaitForSecondsRealtime(.4f);Assert.AreEqual(DisplayStyle.None,text.style.display.value);lane.Say("A",milliseconds:40);lane.Say("B",milliseconds:40);yield return new WaitForSecondsRealtime(.85f);Assert.AreEqual("B",text.text);lane.Clear();lane.Say("AFTER",interrupt:true);Assert.AreEqual("AFTER",text.text,"A fresh notice must not resurrect retired text");lane.Dispose();LogAssert.NoUnexpectedReceived();}
 [UnityTest] public IEnumerator WaveChangesSlamOnceAndDoNotChangeSimulationNumbers(){var hud=new HudTop().Into(root);var view=new HudView{wave=1,credits=90};int slammed=0;hud.WaveSlammed+=()=>slammed++;hud.Bind(view);yield return null;view.wave=8;for(int i=0;i<10;i++){hud.Bind(view);yield return null;}yield return new WaitForSecondsRealtime(.4f);Assert.AreEqual(1,slammed);Assert.AreEqual("08",hud.Q<GlitchText>("wave-number").text);root.AddToClassList(SafeAreaRoot.ReducedMotion);view.wave=9;hud.Bind(view);yield return new WaitForSecondsRealtime(.5f);Assert.AreEqual(1,slammed);Assert.AreEqual("09",hud.Q<GlitchText>("wave-number").text);LogAssert.NoUnexpectedReceived();}
}
}
