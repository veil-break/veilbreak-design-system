using UnityEngine;
using UnityEngine.UIElements;
namespace Veilbreak.Gui {
/// <summary>Children in a column with a fixed rhythm: <c>vb-stack vb-stack--{4|8|12|16}</c> puts the gap under every child but the
/// last. UI Toolkit has no <c>:last-child</c>, so the stack marks its last child <c>vb-stack__last</c> as children change.</summary>
[UxmlElement] public partial class StackLayout:VisualElement {
 public const string ClassName="vb-stack";
 int gap=8;VisualElement last;
 [UxmlAttribute] public int Gap {get=>gap;set{RemoveFromClassList(ClassName+"--"+gap);gap=Snap(value);AddToClassList(ClassName+"--"+gap);}}
 public StackLayout():this(8){}
 public StackLayout(int gap){AddToClassList(ClassName);pickingMode=PickingMode.Ignore;Gap=gap;RegisterCallback<GeometryChangedEvent>(_=>MarkLast());}
 /// <summary>The nearest supported rhythm: 4, 8, 12 or 16.</summary>
 public static int Snap(int gap)=>gap<=4?4:gap<=8?8:gap<=12?12:16;
 /// <summary>Re-marks the last child; called on layout, call it yourself after adding children if you measure in the same frame.</summary>
 public void MarkLast(){var now=childCount>0?this[childCount-1]:null;if(now==last)return;last?.RemoveFromClassList(ClassName+"__last");last=now;last?.AddToClassList(ClassName+"__last");}
}

/// <summary>Cells 2, 3 or 4 across (<c>vb-grid vb-grid--{n}</c>): slots, tiles and result cells take four, perk drafts three, chips and
/// stats two. Pick the count the items fill; never a half-empty row. <see cref="Wrap"/> adds the 10-unit row rhythm.</summary>
[UxmlElement] public partial class VbGrid:VisualElement {
 public const string ClassName="vb-grid";
 int columns=4;
 [UxmlAttribute] public int Columns {get=>columns;set{RemoveFromClassList(ClassName+"--"+columns);columns=value<=2?2:value>=4?4:3;AddToClassList(ClassName+"--"+columns);}}
 [UxmlAttribute] public bool Wrap {get=>ClassListContains(ClassName+"--wrap");set=>EnableInClassList(ClassName+"--wrap",value);}
 public VbGrid():this(4){}
 public VbGrid(int columns,bool wrap=false){AddToClassList(ClassName);pickingMode=PickingMode.Ignore;Columns=columns;Wrap=wrap;}
}

/// <summary>Buttons side by side that fill the row: every visible child takes an equal share (<c>flex-grow:1; flex-basis:0; min-width:0</c>), with 8 between, so
/// a pair never leaves half a row empty. Below <see cref="MinItemWidth"/> per child the row stacks into a column (<c>vb-action-row--stacked</c>) and each
/// child takes the full width, the first child on top. <see cref="Fixed"/> keeps the children at one fixed width instead (the trailing BUY and SELL of a list
/// row). The row re-arranges on every layout change; call <see cref="Arrange"/> yourself to measure in the same frame.</summary>
[UxmlElement] public partial class ActionRow:VisualElement {
 public const string ClassName="vb-action-row",StackedClass=ClassName+"--stacked",FixedClass=ClassName+"--fixed",SingleClass=ClassName+"--single",FirstClass=ClassName+"__first";
 /// <summary>The narrowest share a child may take before the row stacks, and the gap between children.</summary>
 public const float DefaultMinItemWidth=112,Gap=8;
 float minItem=DefaultMinItemWidth;VisualElement first;bool single;
 [UxmlAttribute] public float MinItemWidth {get=>minItem;set{minItem=Mathf.Max(0,value);Arrange();}}
 /// <summary>Children keep one fixed width (the row never stacks); for trailing actions that must line up down a list.</summary>
 [UxmlAttribute] public bool Fixed {get=>ClassListContains(FixedClass);set{EnableInClassList(FixedClass,value);Arrange();}}
 /// <summary>One row, always: the row never stacks, however narrow it is. Buttons share the width, their trailing glyph drops away and their label may wrap, and an icon-only button (<see cref="VbButton.IconOnly"/>, an <see cref="IconButtonComposite"/>) keeps its own 48-unit square at the end. For the secondary buttons of a menu or a pause sheet, which belong on one line.</summary>
 [UxmlAttribute] public bool Single {get=>ClassListContains(SingleClass);set{EnableInClassList(SingleClass,value);Arrange();}}
 public bool Stacked=>ClassListContains(StackedClass);
 public ActionRow(){AddToClassList(ClassName);pickingMode=PickingMode.Ignore;RegisterCallback<GeometryChangedEvent>(_=>Arrange());}
 public ActionRow(params VisualElement[] actions):this(){foreach(var a in actions)Add(a);}
 /// <summary>Whether a row of <paramref name="count"/> children in <paramref name="width"/> units has to stack.</summary>
 public static bool Stacks(float width,int count,float minItemWidth)=>count>1&&width<count*minItemWidth+(count-1)*Gap-.5f;
 /// <summary>Marks the first visible child and stacks the row when its children no longer fit side by side.</summary>
 public void Arrange(){int n=0;VisualElement now=null;foreach(var child in Children())if(child.style.display!=DisplayStyle.None){n++;now??=child;}
  if(now!=first){first?.RemoveFromClassList(FirstClass);first=now;first?.AddToClassList(FirstClass);}
  bool want=Single;if(want||single)foreach(var child in Children())if(child is VbButton b&&b.Compact!=want)b.Compact=want;single=want;
  float w=layout.width;if(float.IsNaN(w)||w<=0)return;EnableInClassList(StackedClass,!Fixed&&!Single&&Stacks(w,n,minItem));}
}

/// <summary>Rows of the same kind in a scroll (scores, perks, rules, queue entries) with 6 between rows, and one empty-state line.</summary>
[UxmlElement] public partial class ListLayout:ScrollView {
 public const string ClassName="vb-list";
 readonly Label empty;
 /// <summary>The line shown while the list has no rows.</summary>
 [UxmlAttribute] public string Empty {get=>empty.text;set{empty.text=value??"";Refresh();}}
 public ListLayout():this(null){}
 public ListLayout(string emptyText):base(ScrollViewMode.Vertical){
  horizontalScrollerVisibility=ScrollerVisibility.Hidden;verticalScrollerVisibility=ScrollerVisibility.Hidden;AddToClassList(ClassName+"-scroll");AddToClassList("vb-page__scroll");contentContainer.AddToClassList(ClassName);
  empty=new Label{pickingMode=PickingMode.Ignore};empty.AddToClassList("vb-t-caption");empty.AddToClassList(ClassName+"__empty");hierarchy.Insert(0,empty);Empty=emptyText;
  contentContainer.RegisterCallback<GeometryChangedEvent>(_=>Refresh());
 }
 void Refresh(){empty.Show(empty.text.Length>0&&contentContainer.childCount==0);}
}

/// <summary>The battle board: a HUD band on top, the world in the middle (at least half the board), Power and the operative cards at
/// the bottom. The top and bottom never shrink; the world takes the rest, so any battle screen keeps the same safe band.</summary>
[UxmlElement] public partial class BoardLayout:VisualElement {
 public const string ClassName="vb-board";
 public VisualElement Top {get;}
 /// <summary>Put the <see cref="WorldLayer"/> here; the camera's gesture surface usually is this element.</summary>
 public VisualElement World {get;}
 public VisualElement Bottom {get;}
 public BoardLayout(){AddToClassList(ClassName);pickingMode=PickingMode.Ignore;Top=Ui.Box(this,ClassName+"__top");World=Ui.Box(this,ClassName+"__world");World.pickingMode=PickingMode.Position;Bottom=Ui.Box(this,ClassName+"__bottom");}
}

public static partial class Ui {
 /// <summary>A <see cref="StackLayout"/> on <paramref name="parent"/>.</summary>
 public static StackLayout Stack(VisualElement parent,int gap=8,params string[] classes)=>new StackLayout(gap).Classes(classes).Into(parent);
 /// <summary>A <see cref="VbGrid"/> on <paramref name="parent"/>.</summary>
 public static VbGrid Grid(VisualElement parent,int columns,bool wrap=false,params string[] classes)=>new VbGrid(columns,wrap).Classes(classes).Into(parent);
 /// <summary>An <see cref="ActionRow"/> on <paramref name="parent"/> holding <paramref name="actions"/>.</summary>
 public static ActionRow Actions(VisualElement parent,params VisualElement[] actions)=>new ActionRow(actions).Into(parent);
 /// <summary>A <see cref="ListLayout"/> on <paramref name="parent"/>: rows go in it, the empty line shows while it has none.</summary>
 public static ListLayout List(VisualElement parent,string empty=null,params string[] classes)=>new ListLayout(empty).Classes(classes).Into(parent);
}
}
