using System;
using UnityEngine;
using UnityEngine.UIElements;
using Veilbreak.Gui;
namespace Veilbreak.Feedback {
/// <summary>The 1.0 readout API over <see cref="WorldLayer"/> (com.veilbreak.gui), for consumers that project actor points themselves.
/// New code uses WorldLayer directly: it projects world points, merges combat text and adds labels, markers and off-screen chips.</summary>
public sealed class ProjectedReadouts:VisualElement {
 public readonly WorldLayer Layer=new WorldLayer();Func<Vector3,Vector2> project;
 public ProjectedReadouts(){name="world-readouts";pickingMode=PickingMode.Ignore;style.position=Position.Absolute;style.left=style.right=style.top=style.bottom=0;Layer.Project=p=>project!=null?project(p):(Vector2?)null;Add(Layer);}
 public void BeginFrame()=>Layer.BeginFrame();
 public void Actor(int id,Vector2 point,float health,Color color,float delta)=>Layer.BarAt(id,point,health).Tint(color);
 public void Number(int actor,Vector3 position,int amount,bool heal)=>Layer.Text(actor,position,amount,heal?CombatTextKind.Heal:CombatTextKind.Taken);
 public void EndFrame(float delta,Func<Vector3,Vector2> project){this.project=project;Layer.EndFrame(delta);}
}
}
