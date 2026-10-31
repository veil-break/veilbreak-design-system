using System;
using UnityEngine.UIElements;
namespace Veilbreak.Gui {
/// <summary>One page at a time on a screen layer, with modal layers and popovers above it.
/// Back closes the topmost popover, then the top modal, then runs the page's back action, then the host's fallback.</summary>
public sealed class Navigator {
 readonly VisualElement screen;
 public readonly ModalHost Modals;
 /// <summary>Raised before a page replaces the screen, with its name; hosts cancel drags and cover the HUD here.</summary>
 public event Action<string> Opening;
 /// <summary>Raised after <see cref="Close"/> hides the screen.</summary>
 public event Action Closed;
 /// <summary>While true, Back does nothing (an intro or transition owns input).</summary>
 public Func<bool> Blocked;
 /// <summary>Runs when Back finds no popover, modal or page back action (pause the game, confirm exit).</summary>
 public Action Fallback;
 /// <summary>What Back does on the current page; set it again when a page changes its own back target.</summary>
 public Action PageBack;
 public string Page {get;private set;}="";
 public Navigator(VisualElement screen){this.screen=screen;Modals=new ModalHost(screen);}
 /// <summary>Whether a page is showing.</summary>
 public bool Visible=>screen.style.display!=DisplayStyle.None;
 /// <summary>The current page, which modal layers cover; an empty page when none was opened (a layer must not sit inside what it disables).</summary>
 public VisualElement Content=>screen.childCount>0?screen[0]:Ui.Box(screen,"vb-page");
 /// <summary>Clears the screen and returns an empty page (<c>vb-page vb-page--{name}</c>) on it (<c>vb-screen vb-screen--{name}</c>).</summary>
 public VisualElement Open(string name,Action back=null){Opening?.Invoke(name);Popover.CloseAll();Modals.Clear();Page=name;PageBack=back;
  screen.Clear();screen.ClearClassList();screen.AddToClassList("vb-screen");screen.AddToClassList("vb-screen--"+name);screen.Show(true);return Ui.Box(screen,"vb-page","vb-page--"+name);}
 /// <summary>Shows the screen layer and pushes a modal layer over its <see cref="Content"/>; Back pops it first.</summary>
 public VisualElement Push(Action onClose=null){screen.Show(true);return Modals.Push(Content,onClose);}
 /// <summary>Hides the screen layer and drops its popovers and modals.</summary>
 public void Close(){Popover.CloseAll();Modals.Clear();Page="";PageBack=null;screen.Show(false);Closed?.Invoke();}
 /// <summary>Steps back once; false only when nothing handled it.</summary>
 public bool Back(){if(Blocked?.Invoke()==true)return true;if(Popover.CloseAll()||Modals.Pop())return true;if(PageBack!=null){PageBack();return true;}if(Fallback==null)return false;Fallback();return true;}
}
}
