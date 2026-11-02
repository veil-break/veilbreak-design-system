using System;
using UnityEngine;
namespace Veilbreak.Cityscape {
/// <summary>The street's parts: baked model meshes with the palette slot of each submesh, one material per palette slot,
/// and the street, effect and sky materials. Editor/KitBuilder writes it from Models~, Runtime/Materials and
/// Runtime/Textures; consumers only read it.</summary>
[CreateAssetMenu(menuName="Veilbreak/Environment/City kit")]
public sealed class CityKit:ScriptableObject {
 /// <summary>The models' shared material palette, in the order the source GLBs declare it.</summary>
 public static readonly string[] Palette={"concrete","metal","glass","violet","foliage","wood","lamp","asphalt","taillight","hazard","ember","paint"};
 public const int Concrete=0,Metal=1,Glass=2,Violet=3,Foliage=4,Wood=5,Lamp=6,Asphalt=7,Taillight=8,Hazard=9,Ember=10,Paint=11,Pool=12;
 /// <summary>Slots whose submeshes give light: drawn again into the mirrored wet-street pass.</summary>
 public static bool Emissive(int slot)=>slot==Glass||slot==Violet||slot==Lamp||slot==Taillight||slot==Ember;
 [Serializable] public sealed class Model { public string name; public Mesh mesh; public int[] slots; }
 public Model[] models=Array.Empty<Model>();
 [Tooltip("One material per palette slot.")] public Material[] palette=new Material[12];
 public Material street,sidewalk,lightPool,fog,sky,skyline,ember,smoke,rain;
 public Texture2D noise,skyPlate;
 public Mesh quad,dome;
 [Tooltip("Ambience loops from Tools~/audio.py: rain and city hum beds, and the positional wreck-fire crackle.")] public AudioClip rainLoop,humLoop,fireLoop;
 public Model Find(string name){foreach(var m in models)if(m.name==name)return m;return null;}
 /// <summary>The material for a module key: a palette slot, or <see cref="Pool"/> for ground light pools.</summary>
 public Material Slot(int slot)=>slot==Pool?lightPool:slot>=0&&slot<palette.Length?palette[slot]:null;
}
}
