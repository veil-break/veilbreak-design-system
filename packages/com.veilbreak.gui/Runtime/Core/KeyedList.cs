using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine.UIElements;
namespace Veilbreak.Gui {
/// <summary>Stable keyed children. A bind updates data without restarting presence or losing focus/scroll.</summary>
public sealed class KeyedList<T,TElement> where TElement:VisualElement {
 readonly VisualElement parent;readonly Func<T,string> key;readonly Func<T,TElement> create;Action<TElement,T> bind;
 readonly Dictionary<string,TElement> elements=new Dictionary<string,TElement>();
 public KeyedList(VisualElement parent,Func<T,string> key,Func<T,TElement> create,Action<TElement,T> bind){this.parent=parent;this.key=key;this.create=create;this.bind=bind;}
 public void Configure(Action<TElement,T> bind)=>this.bind=bind;
 public IEnumerable<TElement> Elements=>elements.Values;
 public void Bind(IEnumerable<T> values,bool animate=false){var snapshot=new List<T>(values);var seen=new HashSet<string>();foreach(var value in snapshot){string id=key(value);if(string.IsNullOrEmpty(id)||!seen.Add(id))throw new ArgumentException("Missing or duplicate item identity: "+id);}int index=0,newIndex=0;var focused=parent.panel?.focusController?.focusedElement as VisualElement;
  foreach(var value in snapshot){string id=key(value);
   bool added=!elements.TryGetValue(id,out var element);if(added){element=create(value);element.name=id;elements.Add(id,element);}
   bind(element,value);if(element.parent!=parent||parent.IndexOf(element)!=index)parent.Insert(index,element);index++;
   if(added&&animate)VbMotion.In(element,MotionRecipes.Deal,MotionRecipes.DealDelay(newIndex++));
  }
  if(focused?.panel!=null&&parent.Contains(focused)&&focused.enabledInHierarchy)focused.Focus();
  foreach(var id in new List<string>(elements.Keys))if(!seen.Contains(id)){var element=elements[id];elements.Remove(id);if(animate){element.SetEnabled(false);element.AddToClassList("vb-list-item-leaving");element.style.position=UnityEngine.UIElements.Position.Absolute;VbMotion.Out(element,MotionKind.Rise,element.RemoveFromHierarchy);}else{VbMotion.Finish(element);element.RemoveFromHierarchy();}}
 }
}
public static class Keyed {
 static class Cache<T,TElement> where TElement:VisualElement {internal static readonly ConditionalWeakTable<VisualElement,KeyedList<T,TElement>> lists=new ConditionalWeakTable<VisualElement,KeyedList<T,TElement>>();}
 public static void Bind<T,TElement>(VisualElement parent,IEnumerable<T> values,Func<T,string> key,Func<T,TElement> create,Action<TElement,T> bind,bool animate=false) where TElement:VisualElement {
  var list=Cache<T,TElement>.lists.GetValue(parent,_=>new KeyedList<T,TElement>(parent,key,create,bind));list.Configure(bind);list.Bind(values,animate);
 }
}

}
