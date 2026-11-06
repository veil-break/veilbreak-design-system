using UnityEngine;
namespace Veilbreak.Gui {
/// <summary>Portable motion data. Milliseconds and logical UI units, independent of the simulation clock.</summary>
public readonly struct MotionRecipe {
 public readonly int milliseconds;public readonly float x,y,rotation,scale;
 public MotionRecipe(int milliseconds,float x=0,float y=0,float rotation=0,float scale=1){this.milliseconds=milliseconds;this.x=x;this.y=y;this.rotation=rotation;this.scale=scale;}
}
public static class MotionRecipes {
 public static readonly MotionRecipe ViewIn=new MotionRecipe(320,0,12,0,1);
 public static readonly MotionRecipe ViewOut=new MotionRecipe(180,0,-8,0,1);
 public static readonly MotionRecipe Deal=new MotionRecipe(420,0,24,-6,0.96f);
 public static readonly MotionRecipe DrawerIn=new MotionRecipe(320,0,80,0,1);
 public static readonly MotionRecipe DrawerOut=new MotionRecipe(220,0,80,0,1);
 public static readonly MotionRecipe HudTop=new MotionRecipe(320,0,-24,0,1);
 public static readonly MotionRecipe HudCard=new MotionRecipe(360,0,32,-2,0.98f);
 public static readonly MotionRecipe OverlayIn=new MotionRecipe(220,0,12,0,0.97f);
 public static readonly MotionRecipe OverlayOut=new MotionRecipe(150,0,6,0,0.98f);
 public static readonly MotionRecipe PopoverIn=new MotionRecipe(140,0,4,0,0.97f);
 public static readonly MotionRecipe PopoverOut=new MotionRecipe(90,0,0,0,0.98f);
 public static readonly MotionRecipe DrawerFit=new MotionRecipe(180,0,0,0,1);
 public static readonly MotionRecipe LabelMerge=new MotionRecipe(180,0,0,0,1.12f);
 public static readonly MotionRecipe LabelSplit=new MotionRecipe(180,0,0,0,0.9f);
 public static readonly MotionRecipe DragLift=new MotionRecipe(120,0,-4,0,1.05f);
 public static readonly MotionRecipe DropSettle=new MotionRecipe(220,0,0,0,1);
 public static readonly MotionRecipe PressSink=new MotionRecipe(100,0,1,0,0.92f);
 public static readonly MotionRecipe Fallen=new MotionRecipe(420,0,6,0,0.97f);
 public const int Stagger=80,MaxDealDelay=400;
 public const int Rebuild=180;
 public const int Reveal=2000;
 public static int DealDelay(int index)=>Mathf.Min(Mathf.Max(0,index)*Stagger,MaxDealDelay);
 public static MotionRecipe For(MotionKind kind,bool enter=true)=>kind switch {
  MotionKind.Deal=>Deal,MotionKind.Rise=>new MotionRecipe(enter?320:180,0,12),MotionKind.Drop=>new MotionRecipe(enter?320:180,0,-12),
  MotionKind.Left=>new MotionRecipe(enter?320:180,-12),MotionKind.Right=>new MotionRecipe(enter?320:180,12),MotionKind.Zoom=>new MotionRecipe(enter?320:180,0,0,0,.94f),
  _=>new MotionRecipe(enter?320:180)
 };
}
}
