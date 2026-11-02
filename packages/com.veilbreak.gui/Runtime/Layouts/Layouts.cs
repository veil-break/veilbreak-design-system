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
 /// <summary>A <see cref="ListLayout"/> on <paramref name="parent"/>: rows go in it, the empty line shows while it has none.</summary>
 public static ListLayout List(VisualElement parent,string empty=null,params string[] classes)=>new ListLayout(empty).Classes(classes).Into(parent);
}
}
