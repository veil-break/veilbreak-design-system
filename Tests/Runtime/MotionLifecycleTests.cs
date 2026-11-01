using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
namespace Veilbreak.Gui.Tests {
/// <summary>Attached-panel timing and ownership. These require the pinned Unity editor; source parsing is insufficient.</summary>
public sealed class MotionLifecycleTests {
 GameObject host;PanelSettings settings;System.Func<double> previousClock;double time;
 [SetUp]public void Before(){previousClock=VbMotion.Clock;VbMotion.Clock=()=>time;}
 [TearDown]public void After(){VbMotion.Clock=previousClock;if(host!=null)Object.Destroy(host);if(settings!=null)Object.Destroy(settings);}
 IEnumerator Panel(System.Action<VisualElement> build){settings=ScriptableObject.CreateInstance<PanelSettings>();host=new GameObject("Motion lifecycle");var doc=host.AddComponent<UIDocument>();doc.panelSettings=settings;yield return null;build(doc.rootVisualElement);yield return null;}
 [UnityTest] public IEnumerator ModalHoldsCoverageUntilExitAndRestoresNestedFocus(){VisualElement root=null,page=null;OverlayHost overlays=null;yield return Panel(r=>{root=r;page=new VisualElement().Into(r);overlays=new OverlayHost(r);});var one=overlays.Push(page);time+=1;yield return new WaitForSecondsRealtime(.04f);var two=overlays.Push(page);time+=1;yield return new WaitForSecondsRealtime(.04f);Assert.AreEqual(2,overlays.Coverage);overlays.Pop();Assert.AreEqual(2,overlays.Coverage);Assert.IsFalse(one.enabledSelf);time+=1;yield return new WaitForSecondsRealtime(.04f);Assert.AreEqual(1,overlays.Coverage);Assert.IsTrue(one.enabledSelf);Assert.IsFalse(page.enabledSelf);overlays.Pop();time+=1;yield return new WaitForSecondsRealtime(.04f);Assert.AreEqual(0,overlays.Coverage);Assert.IsTrue(page.enabledSelf);}
 [UnityTest] public IEnumerator FeedbackDoesNotCancelExitAndDetachFinalizesCount(){VisualElement item=null;Label count=null;yield return Panel(root=>{item=new VisualElement().Into(root);count=new Label().Into(root);});VbMotion.In(item,MotionKind.Deal);time+=.12;yield return new WaitForSecondsRealtime(.04f);VbMotion.Pop(item);VbMotion.Out(item);time+=1;yield return new WaitForSecondsRealtime(.04f);Assert.IsFalse(VbMotion.Shown(item));Assert.IsFalse(VbMotion.Busy(item));VbMotion.Count(count,1,null);VbMotion.Count(count,100,null);count.RemoveFromHierarchy();Assert.AreEqual("100",count.text);Assert.IsFalse(VbMotion.Busy(count));}
 [UnityTest] public IEnumerator ReversalAndReducedMotionSettleWithoutOldCallbacks(){VisualElement root=null,item=null;int closed=0;yield return Panel(r=>{root=r;item=new VisualElement().Into(r);});VbMotion.Out(item,MotionKind.Rise,()=>closed++);time+=.05;yield return new WaitForSecondsRealtime(.04f);VbMotion.In(item,MotionKind.Deal);root.AddToClassList(SafeAreaRoot.ReducedMotion);time+=1;yield return new WaitForSecondsRealtime(.04f);Assert.IsTrue(VbMotion.Shown(item));Assert.AreEqual(0,closed);Assert.IsFalse(VbMotion.Busy(item));}
}
}
