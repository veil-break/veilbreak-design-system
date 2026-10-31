using System;
using UnityEngine.UIElements;
namespace Veilbreak.Gui {
/// <summary>One page at a time on a screen layer, with modal layers and popovers above it.
/// Back closes the topmost popover, then the top modal, then runs the page's back action, then the host's fallback.
/// Every change is animated through <see cref="VbMotion"/>: the screen fades in and out, a new page rises in while the old one fades out
/// (disabled, so it takes no input while it leaves), and re-opening the page that is showing replaces it in place without a deal.</summary>
public sealed class Navigator {
 /// <summary>Class on a page that is fading out; it is disabled and removed when its exit finishes.</summary>
 public const string Leaving="vb-page--leaving";
 readonly VisualElement screen;VisualElement page;
 public readonly ModalHost Modals;
 /// <summary>Raised before a page replaces the screen, with its name; hosts cancel drags and cover the HUD here.</summary>
 public event Action<string> Opening;
 /// <summary>Raised after <see cref="Close"/> starts hiding the screen.</summary>
 public event Action Closed;
 /// <summary>While true, Back does nothing (an intro or transition owns input).</summary>
 public Func<bool> Blocked;
 /// <summary>Runs when Back finds no popover, modal or page back action (pause the game, confirm exit).</summary>
 public Action Fallback;
 /// <summary>What Back does on the current page; set it again when a page changes its own back target.</summary>
 public Action PageBack;
 public string Page {get;private set;}="";
 public Navigator(VisualElement screen){this.screen=screen;Modals=new ModalHost(screen);}
 /// <summary>Whether a page is showing (or opening); false as soon as <see cref="Close"/> starts its fade.</summary>
 public bool Visible=>VbMotion.Shown(screen);
 /// <summary>The current page, which modal layers cover; an empty page when none was opened (a layer must not sit inside what it disables).</summary>
 public VisualElement Content=>page!=null&&page.parent==screen?page:page=Ui.Box(screen,"vb-page");
 /// <summary>Replaces the screen's page with an empty one (<c>vb-page vb-page--{name}</c>) on it (<c>vb-screen vb-screen--{name}</c>) and returns it.
 /// The new page rises in (<paramref name="enter"/>); re-opening the page already showing swaps it at once.</summary>
 public VisualElement Open(string name,Action back=null,MotionKind enter=MotionKind.Rise){
  bool same=name==Page&&Visible;Opening?.Invoke(name);Popover.CloseAll();Modals.Clear();Page=name;PageBack=back;
  screen.ClearClassList();screen.AddToClassList("vb-screen");screen.AddToClassList("vb-screen--"+name);
  for(int i=screen.childCount-1;i>=0;i--){var old=screen[i];if(same||old.ClassListContains(Leaving)||!old.ClassListContains("vb-page")){VbMotion.Finish(old);old.RemoveFromHierarchy();continue;}Leave(old);}
  page=Ui.Box(screen,"vb-page","vb-page--"+name);bool appearing=!Visible;VbMotion.Show(screen,true);
  if(!same)VbMotion.In(page,appearing?MotionKind.Fade:enter);return page;}
 /// <summary>Shows the screen layer and pushes a modal layer over its <see cref="Content"/>; Back pops it first.</summary>
 public VisualElement Push(Action onClose=null){VbMotion.Show(screen,true);return Modals.Push(Content,onClose);}
 /// <summary>Fades the screen layer out and drops its popovers and modals; its pages are removed once it is hidden.</summary>
 public void Close(){Popover.CloseAll();Modals.Clear();Page="";PageBack=null;var closing=page;page=null;
  VbMotion.Show(screen,false,MotionKind.Fade,()=>{if(Page.Length>0)return;for(int i=screen.childCount-1;i>=0;i--)if(screen[i]!=page)screen[i].RemoveFromHierarchy();});
  closing?.SetEnabled(false);Closed?.Invoke();}
 /// <summary>Steps back once; false only when nothing handled it.</summary>
 public bool Back(){if(Blocked?.Invoke()==true)return true;if(Popover.CloseAll()||Modals.Pop())return true;if(PageBack!=null){PageBack();return true;}if(Fallback==null)return false;Fallback();return true;}
 static void Leave(VisualElement old){old.AddToClassList(Leaving);old.SetEnabled(false);old.pickingMode=PickingMode.Ignore;old.style.position=Position.Absolute;old.style.left=old.style.right=old.style.top=old.style.bottom=0;VbMotion.Out(old,MotionKind.Fade,old.RemoveFromHierarchy);}
}
}
