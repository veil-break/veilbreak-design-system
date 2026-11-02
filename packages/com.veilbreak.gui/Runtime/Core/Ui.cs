using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UIElements;
namespace Veilbreak.Gui {
/// <summary>Small fluent helpers every component builds with, so markup stays declarative and class-driven.</summary>
public static partial class Ui {
 public static T Classes<T>(this T e,params string[] classes) where T:VisualElement {foreach(var c in classes)if(!string.IsNullOrEmpty(c))e.AddToClassList(c);return e;}
 public static T Into<T>(this T e,VisualElement parent) where T:VisualElement {parent?.Add(e);return e;}
 public static T Flag<T>(this T e,string cls,bool on) where T:VisualElement {e.EnableInClassList(cls,on);return e;}
 public static VisualElement Box(VisualElement parent,params string[] classes)=>new VisualElement{pickingMode=PickingMode.Ignore}.Classes(classes).Into(parent);
 public static Label Text(VisualElement parent,string text,params string[] classes){var l=new Label(text){pickingMode=PickingMode.Ignore}.Classes(classes).Into(parent);return l;}
 public static void Show(this VisualElement e,bool visible)=>e.style.display=visible?DisplayStyle.Flex:DisplayStyle.None;
 /// <summary>True when <paramref name="e"/> or an ancestor carries <paramref name="cls"/> (a root flag such as <c>vb-reduced-motion</c>).</summary>
 public static bool Under(VisualElement e,string cls){for(var p=e;p!=null;p=p.hierarchy.parent)if(p.ClassListContains(cls))return true;return false;}
 /// <summary>A shaped panel (<c>vb-panel</c> plus modifiers such as <c>vb-panel--solid</c>) drawn by <see cref="VbShape"/>.</summary>
 public static VisualElement Panel(VisualElement parent,params string[] classes)=>VbShape.Attach(Box(parent,"vb-panel").Classes(classes));
 /// <summary>A shaped panel of a <see cref="FrameKind"/>: <c>vb-panel vb-frame--slim</c> or <c>vb-panel vb-frame--chipped</c>, plus modifiers
 /// (<c>vb-frame--raised</c>, <c>--solid</c>, <c>--ally</c>, <c>--enemy</c>, <c>--power</c>).</summary>
 public static VisualElement Frame(VisualElement parent,FrameKind kind,params string[] classes)=>Panel(parent,kind==FrameKind.Slim?"vb-frame--slim":kind==FrameKind.Chipped?"vb-frame--chipped":null).Classes(classes);
 /// <summary>Takes the free space in a column or row, pushing later siblings to the far end.</summary>
 public static VisualElement Spacer(VisualElement parent)=>Box(parent,"vb-grow");
 /// <summary>A vertical scroll area with hidden scrollbars; touch and wheel scroll it, clipping shows there is more.</summary>
 public static ScrollView Scroll(VisualElement parent,params string[] classes)=>new ScrollView(ScrollViewMode.Vertical){horizontalScrollerVisibility=ScrollerVisibility.Hidden,verticalScrollerVisibility=ScrollerVisibility.Hidden}.Classes(classes).Into(parent);
 /// <summary>A caps title and an optional right-aligned meta line at the top of a panel (<c>vb-panel__head</c>).</summary>
 public static VisualElement Head(VisualElement panel,string title,string meta=null){var head=Box(panel,"vb-panel__head");head.SendToBack();Text(head,title,"vb-panel__title");if(!string.IsNullOrEmpty(meta))Text(head,meta,"vb-panel__meta");return head;}
 /// <summary>A titled panel grouping related settings or facts (<c>vb-section</c>).</summary>
 public static VisualElement Section(VisualElement parent,string title,params string[] classes){var p=Panel(parent,"vb-section").Classes(classes);new LabelComponent(title).Classes("vb-section__title").Into(p);return p;}
 /// <summary>Runs <paramref name="set"/> with each new value the user picks.</summary>
 public static VbSlider Changed(this VbSlider field,Action<float> set){field.RegisterValueChangedCallback(e=>set(e.newValue));return field;}
 public static VbSwitch Changed(this VbSwitch field,Action<bool> set){field.RegisterValueChangedCallback(e=>set(e.newValue));return field;}
 public static VbSegmented Changed(this VbSegmented field,Action<int> set){field.RegisterValueChangedCallback(e=>set(e.newValue));return field;}
 /// <summary>Makes any element act as a button: pointer click, Enter/Space and focus.</summary>
 public static T Clickable<T>(this T e,Action click) where T:VisualElement {
  e.focusable=true;e.pickingMode=PickingMode.Position;e.AddManipulator(new Clickable(()=>click?.Invoke()));
  e.RegisterCallback<KeyDownEvent>(k=>{if(k.keyCode==KeyCode.Return||k.keyCode==KeyCode.KeypadEnter||k.keyCode==KeyCode.Space){click?.Invoke();k.StopPropagation();}});
  return e;
 }
 /// <summary>Replaces a state modifier (block--a, block--b …) with the one for <paramref name="state"/>. Class names are built once
 /// per enum and block, so per-frame binding (world bars, HUD) makes no garbage.</summary>
 public static void State<TEnum>(VisualElement e,string block,TEnum state) where TEnum:struct,Enum {var values=States<TEnum>.Values;var names=States<TEnum>.Names(block);var same=EqualityComparer<TEnum>.Default;for(int i=0;i<values.Length;i++)e.EnableInClassList(names[i],same.Equals(values[i],state));}
 static class States<TEnum> where TEnum:struct,Enum {
  public static readonly TEnum[] Values=(TEnum[])Enum.GetValues(typeof(TEnum));static readonly Dictionary<string,string[]> names=new Dictionary<string,string[]>();
  public static string[] Names(string block){if(!names.TryGetValue(block,out var n)){n=new string[Values.Length];for(int i=0;i<n.Length;i++)n[i]=block+"--"+Kebab(Values[i].ToString());names[block]=n;}return n;}
 }
 /// <summary>A child of <paramref name="frame"/> that holds an illustration at a USS-defined size and offset (a face or bust crop).</summary>
 public static VisualElement Crop(VisualElement frame){var c=frame.Q(className:"vb-crop")??Box(frame,"vb-crop");c.SendToBack();return c;}
 public static void Image(VisualElement e,Texture2D texture)=>e.style.backgroundImage=texture!=null?new StyleBackground(texture):new StyleBackground(StyleKeyword.None);
 public static void Fill(VisualElement e,float fraction,bool vertical=false){var p=Length.Percent(Mathf.Clamp01(fraction)*100);if(vertical)e.style.height=p;else e.style.width=p;}
 public static string Compact(long value)=>value>=1000000000?(value/1000000000d).ToString("0.#",CultureInfo.InvariantCulture)+"B":value>=1000000?(value/1000000d).ToString("0.#",CultureInfo.InvariantCulture)+"M":value>=10000?(value/1000d).ToString("0.#",CultureInfo.InvariantCulture)+"K":Number(value);
 public static string Number(long value)=>value.ToString("N0",CultureInfo.InvariantCulture);
 /// <summary>Seconds as "4.2s" below a minute, "m:ss" above. Strings below a minute are cached per tenth, so countdowns bound
 /// every frame make no garbage.</summary>
 public static string Seconds(float seconds){if(seconds>=0&&seconds<59.95f){int k=Mathf.RoundToInt(seconds*10);return tenths[k]??=(k/10f).ToString(k<100?"0.0":"0.#",CultureInfo.InvariantCulture)+"s";}
  return seconds<60?seconds.ToString("0.#",CultureInfo.InvariantCulture)+"s":TimeSpan.FromSeconds(seconds).ToString(@"m\:ss",CultureInfo.InvariantCulture);}
 /// <summary>An integer as invariant text, cached from 0 to 1023 so per-frame binding makes no garbage.</summary>
 public static string Count(long n)=>n>=0&&n<counts.Length?counts[n]??=n.ToString(CultureInfo.InvariantCulture):n.ToString(CultureInfo.InvariantCulture);
 static readonly string[] tenths=new string[600],counts=new string[1024];
 public static string Kebab(string name){var b=new System.Text.StringBuilder();for(int i=0;i<name.Length;i++){char c=name[i];if(char.IsUpper(c)&&i>0&&name[i-1]!='-')b.Append('-');b.Append(char.ToLowerInvariant(c==' '||c=='_'?'-':c));}return b.ToString();}
}
}
