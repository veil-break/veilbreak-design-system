using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
namespace Veilbreak.Gui.Tests {
/// <summary>Change events only travel on a live panel, so the form and settings contracts are checked here.</summary>
public sealed class FormTests {
 GameObject host;PanelSettings settings;
 [TearDown] public void Close(){if(host!=null)Object.Destroy(host);if(settings!=null)Object.Destroy(settings);}
 IEnumerator Panel(System.Action<VisualElement> build){settings=ScriptableObject.CreateInstance<PanelSettings>();host=new GameObject("Form test");var document=host.AddComponent<UIDocument>();document.panelSettings=settings;yield return null;build(document.rootVisualElement);yield return null;}
 [UnityTest] public IEnumerator FormsReportChangesAndSaveState(){ScreenHeader header=null;FormComposite form=null;VbSwitch s=null;int changes=0;
  yield return Panel(root=>{header=new ScreenHeader("SETTINGS","").Into(root);form=new FormComposite(header){SavedMeta="SAVED"}.Into(root);form.Changed+=()=>changes++;s=new VbSwitch("HAPTICS",null,false).Into(form.Fieldset("FEEL"));});
  s.value=true;Assert.AreEqual(1,changes);Assert.AreEqual("SAVED",header.Meta);
  var scroll=form.Q<ScrollView>();scroll.verticalScroller.value=10;Assert.AreEqual(1,changes,"Scrolling the form is not a settings change");}
 [UnityTest] public IEnumerator SettingsWriteTheModelThenRaiseChanged(){SettingsView view=null;var model=new SettingsViewModel{haptics=false};SettingsViewModel got=null;
  yield return Panel(root=>{view=new SettingsView().Into(root);view.Bind(model);view.Changed+=m=>got=m;});
  view.Q<VbSwitch>().value=true;Assert.AreSame(model,got);Assert.IsTrue(model.haptics);}
}
}
