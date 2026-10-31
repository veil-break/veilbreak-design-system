using System;
using UnityEngine.UIElements;
namespace Veilbreak.Gui {
/// <summary>Audio, feel, camera and quality as fieldsets in one <see cref="FormComposite"/>. Values apply as they change: every change
/// updates the bound model and raises <see cref="Changed"/>; the header meta reads SAVED. Clearing scores is a danger action in the
/// footer and confirms through <see cref="ConfirmDialogView"/>.</summary>
[UxmlElement] public partial class SettingsView:ScreenView {
 public const string Block="vb-settings";
 readonly ScreenHeader header;readonly FormComposite form;readonly VbSlider master,music,effects,sensitivity;readonly VbSwitch haptics,motion;readonly VbSegmented camera,quality;readonly Label note;
 SettingsViewModel view=new SettingsViewModel();
 public SettingsViewModel View=>view;
 public event Action<SettingsViewModel> Changed;
 public event Action ClearScores,Accessibility,Back;
 public SettingsView():base(Block,true){
  header=new ScreenHeader("SETTINGS","").Into(this);header.Back+=()=>Back?.Invoke();form=new FormComposite(header).Into(this);
  var audio=form.Fieldset("AUDIO");master=new VbSlider("Master",0).Into(audio).Changed(v=>view.master=v);music=new VbSlider("Music",0).Into(audio).Changed(v=>view.music=v);effects=new VbSlider("Effects",0).Into(audio).Changed(v=>view.effects=v);
  var feel=form.Fieldset("FEEL");haptics=new VbSwitch("HAPTICS","Light pulse when an order is accepted",false).Into(feel).Changed(v=>view.haptics=v);motion=new VbSwitch("REDUCED MOTION","No camera shake · fades replace flicker · flat panels",false).Into(feel).Changed(v=>view.reducedMotion=v);
  var cam=form.Fieldset("CAMERA");camera=new VbSegmented(new[]{("FULL","Parallax"),("REDUCED","No drift"),("OFF","Fixed")},0).Into(cam).Changed(v=>view.camera=v);sensitivity=new VbSlider("Sensitivity",0).Into(cam).Changed(v=>view.sensitivity=v);
  Ui.Text(cam,"Pinch to zoom, twist to turn, drag empty ground to pan, double-tap to reset.","vb-t-caption");
  var q=form.Fieldset("QUALITY");quality=new VbSegmented(new[]{("LOW","Stable 30 FPS"),("STANDARD","60 FPS target")},1).Into(q).Changed(v=>view.quality=v);Ui.Text(q,"Applies on the next wave. Telegraphs stay fully visible on both tiers.","vb-t-caption");
  note=form.Note("automation","").Q<Label>();
  form.Footer(new VbButton("CLEAR LOCAL SCORES",ButtonVariant.Danger,null,null,()=>ClearScores?.Invoke()){name="clear-scores"},new VbButton("ACCESSIBILITY",ButtonVariant.Ghost,null,null,()=>Accessibility?.Invoke()){name="accessibility"});
  // The field callbacks above write the model first; the form's change event (bubbling) then reports it.
  form.Changed+=()=>Changed?.Invoke(view);
  Bind(view);
 }
 /// <summary>Shows <paramref name="v"/> without raising change events; later edits write into the same instance.</summary>
 public SettingsView Bind(SettingsViewModel v){
  view=v??new SettingsViewModel();master.SetValueWithoutNotify(view.master);music.SetValueWithoutNotify(view.music);effects.SetValueWithoutNotify(view.effects);sensitivity.SetValueWithoutNotify(view.sensitivity);
  haptics.SetValueWithoutNotify(view.haptics);motion.SetValueWithoutNotify(view.reducedMotion);camera.SetValueWithoutNotify(view.camera);quality.SetValueWithoutNotify(view.quality);
  note.text=view.note??"";note.parent.Show(note.text.Length>0);form.SavedMeta=view.saved;return this;
 }
}
}
