using System;
using UnityEngine;
using UnityEngine.UIElements;
namespace Veilbreak.Gui {
/// <summary>Tile states. In (picked, numbered) and Focused (its details are open) combine; Locked and Empty stand alone.</summary>
[Flags] public enum TileState { Idle=0, In=1, Focused=2, Locked=4, Empty=8 }

/// <summary>A square-ish, pickable cell: a picture or a glyph, a short name, a caption with its own glyph, a state.
/// Rosters, squad slots and inventories extend it (<see cref="OperativeTile"/>, <see cref="SquadSlot"/>).
/// Tiles are chipped (something you pick); focused = brackets + glow; locked tiles stay visible and say how to unlock.</summary>
[UxmlElement] public partial class TileComponent:SurfaceView {
 public const string ClassName="vb-tile";
 protected readonly string Block;
 readonly VisualElement band,portrait,kit,body;readonly VbIcon glyph,captionIcon;readonly Label order,title,caption;VisualElement empty;Label emptyLabel;TileState state;bool hasPicture;
 public event Action Picked;
 public TileComponent():this(ClassName){}
 protected TileComponent(string block){
  Block=block;AddToClassList(block);this.Clickable(()=>Picked?.Invoke());
  band=Ui.Box(this,block+"__band");portrait=Ui.Box(band,block+"__portrait");Ui.Box(band,block+"__shade");glyph=new VbIcon("",32).Classes(block+"__glyph").Into(band);glyph.Show(false);order=Ui.Text(band,"",block+"__order");order.Show(false);
  kit=Ui.Box(this,block+"__kit");body=Ui.Box(this,block+"__body");title=Ui.Text(body,"",block+"__name");
  var r=Ui.Box(body,block+"__role");captionIcon=new VbIcon("",12).Into(r);captionIcon.Show(false);caption=Ui.Text(r,"",block+"__role-text");
 }
 [UxmlAttribute] public string Title {get=>title.text;set=>title.text=(value??"").ToUpperInvariant();}
 [UxmlAttribute] public string Caption {get=>caption.text;set=>caption.text=(value??"").ToUpperInvariant();}
 [UxmlAttribute] public string CaptionIcon {get=>captionIcon.Icon;set{captionIcon.Icon=value;captionIcon.Show(!string.IsNullOrEmpty(value));}}
 /// <summary>A glyph drawn in the band when there is no picture (inventory cells, gear).</summary>
 [UxmlAttribute] public string Glyph {get=>glyph.Icon;set{glyph.Icon=value;glyph.Show(!string.IsNullOrEmpty(value)&&!hasPicture);}}
 /// <summary>The squad position shown in the corner; 0 hides it.</summary>
 [UxmlAttribute] public int Order {get=>int.TryParse(order.text,out var n)?n:0;set{order.text=value>0?Ui.Count(value):"";order.Show(value>0&&!state.HasFlag(TileState.Locked));}}
 [UxmlAttribute] public TileState State {get=>state;set{state=value;
  EnableInClassList(Block+"--in",value.HasFlag(TileState.In)&&!value.HasFlag(TileState.Locked));EnableInClassList(Block+"--focused",value.HasFlag(TileState.Focused));
  EnableInClassList(Block+"--locked",value.HasFlag(TileState.Locked));bool isEmpty=value.HasFlag(TileState.Empty);EnableInClassList(Block+"--empty",isEmpty);
  band.Show(!isEmpty);kit.Show(!isEmpty);body.Show(!isEmpty);if(isEmpty)Empty();empty?.Show(isEmpty);order.Show(order.text.Length>0&&!value.HasFlag(TileState.Locked));}}
 public RarityTier Tier {set=>Ui.State(this,Block,value);}
 public Texture2D Picture {set{hasPicture=value!=null;Ui.Image(portrait,value);glyph.Show(!hasPicture&&!string.IsNullOrEmpty(glyph.Icon));}}
 public Color Kit {set=>kit.style.backgroundColor=value;}
 /// <summary>The caption shown on an empty tile ("SLOT 4").</summary>
 public string EmptyLabel {set{Empty();emptyLabel.text=(value??"").ToUpperInvariant();}}
 void Empty(){if(empty!=null)return;empty=Ui.Box(this,Block+"__empty");new VbIcon("add",20).Into(empty);emptyLabel=Ui.Text(empty,"",Block+"__empty-label");empty.Show(false);}
 /// <summary>Sets everything at once and returns the tile.</summary>
 public TileComponent Set(string title,string caption=null,Texture2D picture=null,string glyph=null,TileState state=TileState.Idle,int order=0,string captionIcon=null){
  Title=title;Caption=caption;Picture=picture;Glyph=glyph;CaptionIcon=captionIcon;State=state;Order=order;return this;}
}
}
