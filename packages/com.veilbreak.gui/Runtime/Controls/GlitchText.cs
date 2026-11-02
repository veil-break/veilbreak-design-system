using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UIElements;
namespace Veilbreak.Gui {
/// <summary>Readable text with two chromatic copies and eight independently clipped projection bands.
/// Decorative copies never receive input. The ordinary label remains the source of layout and meaning.</summary>
[UxmlElement] public partial class GlitchText:VisualElement {
 public const string ClassName="vb-glitch";
 const int Bands=8;
 readonly Label main,red,cyan;
 [UxmlAttribute("text")] public string text {get=>main.text;set{main.text=value??"";}}readonly VisualElement[] clips=new VisualElement[Bands];readonly Label[] slices=new Label[Bands];
 public GlitchText():this(""){}
 public GlitchText(string value){
  AddToClassList(ClassName);pickingMode=PickingMode.Ignore;main=new Label(value){pickingMode=PickingMode.Ignore}.Classes("vb-glitch__main").Into(this);
  red=Copy(this,"vb-glitch__red");cyan=Copy(this,"vb-glitch__cyan");
  for(int i=0;i<Bands;i++){clips[i]=Ui.Box(this,"vb-glitch__clip");slices[i]=Copy(clips[i],"vb-glitch__slice");}
  RegisterCallback<GeometryChangedEvent>(_=>LayoutCopies());Disrupt(0,0);
 }
 static Label Copy(VisualElement parent,string cls)=>new Label{pickingMode=PickingMode.Ignore,enableRichText=false}.Classes("vb-glitch__copy",cls).Into(parent);
 void LayoutCopies(){
  float h=resolvedStyle.height,w=resolvedStyle.width;if(!(h>0)||!(w>0))return;
  Style(red);Style(cyan);for(int i=0;i<Bands;i++){Style(slices[i]);float top=h*i/Bands;clips[i].style.top=top;clips[i].style.height=h/Bands;clips[i].style.width=w;slices[i].style.top=-top;slices[i].style.width=w;slices[i].style.height=h;}
 }
 void Style(Label copy){copy.text=text;copy.style.unityFontDefinition=resolvedStyle.unityFontDefinition;copy.style.unityFontStyleAndWeight=resolvedStyle.unityFontStyleAndWeight;copy.style.fontSize=resolvedStyle.fontSize;copy.style.letterSpacing=resolvedStyle.letterSpacing;copy.style.unityTextAlign=resolvedStyle.unityTextAlign;
  copy.style.paddingLeft=resolvedStyle.paddingLeft;copy.style.paddingRight=resolvedStyle.paddingRight;copy.style.paddingTop=resolvedStyle.paddingTop;copy.style.paddingBottom=resolvedStyle.paddingBottom;}
 internal void Disrupt(float strength,int frame){
  bool on=strength>0&&!Glitch.Calm(this)&&!Ui.Under(this,SafeAreaRoot.Contrast);
  red.Show(on);cyan.Show(on);for(int i=0;i<Bands;i++)clips[i].Show(on);if(!on)return;
  LayoutCopies();float chroma=VbTokens.ChromaOffset*(1+strength*5);
  red.style.translate=new Translate(-chroma,0);cyan.style.translate=new Translate(chroma,0);
  red.style.opacity=cyan.style.opacity=.55f*strength;
  for(int i=0;i<Bands;i++){int hash=unchecked((frame+13)*(i+29)*1103515245+12345);float shift=((hash>>16)&15)-7;
   clips[i].style.translate=new Translate(shift*strength*2,0);clips[i].style.opacity=((hash>>12)&3)==0?.05f:.65f*strength;}
 }
}

/// <summary>Short, cancellable real-time projection transitions. A new transition replaces the old one, including its callback.
/// Schedules pause on detach; completion restores transforms, so hidden pages cannot leave delayed input or motion behind.</summary>
public static class Glitch {
 sealed class Track {public IVisualElementScheduledItem timer;public int serial;public StyleTranslate translate;public StyleScale scale;}
 static readonly ConditionalWeakTable<VisualElement,Track> tracks=new ConditionalWeakTable<VisualElement,Track>();
 public static bool Calm(VisualElement e)=>Ui.Under(e,SafeAreaRoot.ReducedMotion)||Ui.Under(e,SafeAreaRoot.ReducedFlashes);
 public static void In(VisualElement e,Action done=null)=>Play(e,0,VbTokens.DurGlitchIn,done);
 public static void Out(VisualElement e,Action done=null)=>Play(e,1,VbTokens.DurGlitchOut,done);
 public static void Pulse(VisualElement e,Action done=null)=>Play(e,2,VbTokens.DurGlitchIn,done);
 public static void Slam(VisualElement e,Action done=null)=>Play(e,3,VbTokens.DurWaveSlam,done);
 public static void Jitter(VisualElement e)=>Play(e,4,180,null);
 public static void Cancel(VisualElement e){if(e==null||!tracks.TryGetValue(e,out var t))return;t.serial++;t.timer?.Pause();t.timer=null;e.style.translate=t.translate;e.style.scale=t.scale;(e as GlitchText)?.Disrupt(0,0);e.style.opacity=1;}
 static void Play(VisualElement e,int kind,int duration,Action done){
  if(e==null)return;Cancel(e);var t=tracks.GetValue(e,key=>{var value=new Track();key.RegisterCallback<DetachFromPanelEvent>(_=>Cancel(key));return value;});
  t.translate=e.style.translate;t.scale=e.style.scale;int serial=++t.serial;float began=Time.realtimeSinceStartup;int frame=0;e.Show(true);
  void Draw(){if(t.serial!=serial)return;float p=Mathf.Clamp01((Time.realtimeSinceStartup-began)*1000/Mathf.Max(1,duration));bool calm=Calm(e);float envelope=Mathf.Sin(p*Mathf.PI);
   // Brightness flicker remains local to the text/page; no full-screen flash.
   e.style.opacity=kind==1?1-p:kind==0?p:1;
   if(!calm){if(kind<3)e.style.opacity=e.style.opacity.value*(frame%5==1?.64f:1);
    float x=kind==4?Mathf.Sin(p*180*.038f*Mathf.PI*2)*2*(1-p):kind==3?0:Mathf.Sin(frame*2.71f)*envelope*3;
    e.style.translate=new Translate(x,kind==4?Mathf.Cos(frame*2.3f)*(1-p):0);
    if(kind==3){float scale=p<.72f?Mathf.Lerp(2.4f,.91f,1-Mathf.Pow(1-p/.72f,3)):Mathf.Lerp(.91f,1,(p-.72f)/.28f);e.style.scale=new Scale(new Vector3(scale,scale,1));}}
   else{e.style.translate=t.translate;e.style.scale=t.scale;if(kind==3)e.style.opacity=p;else if(kind==2)e.style.opacity=1-.3f*envelope;}
   (e as GlitchText)?.Disrupt(calm?0:(kind==3?1-p:envelope),frame++);
   if(p<1)return;t.timer?.Pause();t.timer=null;e.style.translate=t.translate;e.style.scale=t.scale;(e as GlitchText)?.Disrupt(0,0);e.style.opacity=kind==1?0:1;if(kind==1)e.Show(false);done?.Invoke();
  }
  Draw();t.timer=e.schedule.Execute(Draw).Every(16);
 }
}
}
