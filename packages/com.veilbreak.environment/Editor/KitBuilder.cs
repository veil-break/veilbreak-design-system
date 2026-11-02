using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
namespace Veilbreak.Cityscape.Editor {
/// <summary>Writes the package's generated content from its sources: Models~/*.glb into Runtime/Meshes, the palette and
/// effect materials into Runtime/Materials, a quad and a sky dome, texture import settings, and Runtime/CityKit.asset.
/// Existing assets are updated in place, so their GUIDs (and every reference to them) survive a rebuild. Batch:
/// `-executeMethod Veilbreak.Cityscape.Editor.KitBuilder.Build`.</summary>
public static class KitBuilder {
 const string Root="Packages/com.veilbreak.environment/",Meshes=Root+"Runtime/Meshes/",Materials=Root+"Runtime/Materials/",Textures=Root+"Runtime/Textures/",Shaders=Root+"Runtime/Shaders/",Audio=Root+"Runtime/Audio/";

 [MenuItem("Veilbreak/Environment/Rebuild city kit")]
 public static void Build(){
  AssetDatabase.Refresh();ImportTextures();ImportAudio();
  var kit=AssetDatabase.LoadAssetAtPath<CityKit>(Root+"Runtime/CityKit.asset");if(kit==null){kit=ScriptableObject.CreateInstance<CityKit>();AssetDatabase.CreateAsset(kit,Root+"Runtime/CityKit.asset");}
  var models=Path.GetFullPath(Root+"Models~");
  kit.models=Directory.GetFiles(models,"*.glb").OrderBy(x=>x,StringComparer.Ordinal).Select(file=>{var r=GlbImport.Read(file,CityKit.Palette);return new CityKit.Model{name=r.mesh.name,mesh=SaveMesh(r.mesh),slots=r.slots};}).ToArray();
  kit.quad=SaveMesh(Quad());kit.dome=SaveMesh(Dome(24,16));
  var glow=Load<Texture2D>(Textures+"T_Glow.png");var asphalt=Load<Texture2D>(Textures+"T_Asphalt_BC.png");var asphaltN=Load<Texture2D>(Textures+"T_Asphalt_N.png");
  kit.noise=Load<Texture2D>(Textures+"T_Noise.png");kit.skyPlate=Load<Texture2D>(Textures+"T_MenuSky_BC.png");
  var city=Load<Shader>(Shaders+"City.shader");
  // Palette values from the donor's UrbanKit.Build; emission strengths are the donor's HDR multipliers.
  Material P(string name,Color c,float smooth,float metal=0,float grime=.5f,Color emission=default,float flicker=0,float wet=1)=>Mat("M_City_"+name,city,m=>{m.SetColor("_BaseColor",c);m.SetFloat("_Smoothness",smooth);m.SetFloat("_Metallic",metal);m.SetFloat("_Grime",grime);m.SetColor("_EmissionColor",emission);m.SetFloat("_Flicker",flicker);m.SetFloat("_Wet",wet);});
  kit.palette=new[]{
   P("Concrete",new Color(.58f,.54f,.62f),.18f,0,.75f),
   P("Metal",new Color(.13f,.12f,.17f),.55f,.7f,.4f),
   P("Glass",new Color(.04f,.035f,.07f),.95f,.2f,.1f,wet:0),
   P("Violet",new Color(.3f,.12f,.5f),.9f,0,.15f,new Color(.71f,.42f,1)*4.5f),
   P("Foliage",new Color(.08f,.1f,.07f),.1f,0,.4f),
   P("Wood",new Color(.17f,.11f,.1f),.2f,0,.5f),
   P("Lamp",new Color(1,.7f,.45f),.9f,0,0,new Color(1,.62f,.35f)*6),
   P("Asphalt",new Color(.07f,.065f,.08f),.45f,0,.5f),
   P("Taillight",new Color(.6f,.05f,.08f),.8f,0,.1f,new Color(1,.1f,.15f)*3),
   P("Hazard",new Color(.9f,.72f,.35f),.5f,0,.6f),
   P("Ember",new Color(.3f,.08f,.02f),.3f,0,.3f,new Color(1,.35f,.08f)*5,1),
   P("Paint",new Color(.22f,.21f,.27f),.6f,.4f,.5f)};
  var street=Load<Shader>(Shaders+"Street.shader");var glowShader=Load<Shader>(Shaders+"Glow.shader");
  kit.street=Mat("M_Street",street,m=>{m.SetTexture("_BaseMap",asphalt);m.SetTexture("_BumpMap",asphaltN);m.SetColor("_Tint",new Color(.62f,.58f,.7f));m.SetFloat("_Tiling",.25f);m.SetFloat("_Sidewalk",0);m.DisableKeyword("_SIDEWALK");});
  kit.sidewalk=Mat("M_Sidewalk",street,m=>{m.SetTexture("_BaseMap",asphalt);m.SetTexture("_BumpMap",asphaltN);m.SetColor("_Tint",new Color(.78f,.74f,.84f));m.SetFloat("_Tiling",.5f);m.SetFloat("_Smoothness",.22f);m.SetFloat("_Puddles",-.04f);m.SetFloat("_Sidewalk",1);m.EnableKeyword("_SIDEWALK");});
  Material G(string name,Color c,float lamp,float near=0)=>Mat(name,glowShader,m=>{m.SetTexture("_BaseMap",glow);m.SetColor("_Color",c);m.SetFloat("_Lamp",lamp);m.SetFloat("_Near",near);});
  kit.lightPool=G("M_LightPool",new Color(1,.62f,.34f)*.42f,1);
  kit.ember=G("M_Ember",new Color(1,.45f,.12f)*3,0,3);
  kit.rain=G("M_Rain",new Color(.55f,.62f,.8f)*.5f,0,4);
  kit.smoke=Mat("M_Smoke",Shader.Find("Universal Render Pipeline/Particles/Unlit"),m=>{m.SetTexture("_BaseMap",glow);m.SetColor("_BaseColor",new Color(.16f,.13f,.2f,.5f));m.SetFloat("_Surface",1);m.SetFloat("_Blend",0);m.SetFloat("_SrcBlend",(float)BlendMode.SrcAlpha);m.SetFloat("_DstBlend",(float)BlendMode.OneMinusSrcAlpha);m.SetFloat("_ZWrite",0);m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");m.SetOverrideTag("RenderType","Transparent");m.renderQueue=(int)RenderQueue.Transparent;});
  kit.fog=Mat("M_FogSheet",Load<Shader>(Shaders+"Fog.shader"),m=>{});
  kit.sky=Mat("M_Sky",Load<Shader>(Shaders+"Sky.shader"),m=>{});
  kit.skyline=Mat("M_Skyline",Load<Shader>(Shaders+"Backdrop.shader"),m=>m.SetTexture("_BaseMap",Load<Texture2D>(Textures+"T_MenuSkyline_BC.png")));
  kit.rainLoop=Load<AudioClip>(Audio+"A_Rain_Loop.wav");kit.humLoop=Load<AudioClip>(Audio+"A_CityHum_Loop.wav");kit.fireLoop=Load<AudioClip>(Audio+"A_Fire_Loop.wav");
  EditorUtility.SetDirty(kit);AssetDatabase.SaveAssets();
  Debug.Log($"City kit rebuilt: {kit.models.Length} models, {kit.models.Sum(m=>m.mesh.triangles.Length/3)} triangles");
 }

 static T Load<T>(string path) where T:UnityEngine.Object{var a=AssetDatabase.LoadAssetAtPath<T>(path);if(a==null)throw new FileNotFoundException("Missing "+path);return a;}
 static Mesh SaveMesh(Mesh mesh){
  Directory.CreateDirectory(Meshes);var path=Meshes+mesh.name+".asset";var asset=AssetDatabase.LoadAssetAtPath<Mesh>(path);
  if(asset==null){AssetDatabase.CreateAsset(mesh,path);return mesh;}
  EditorUtility.CopySerialized(mesh,asset);UnityEngine.Object.DestroyImmediate(mesh);EditorUtility.SetDirty(asset);return asset;
 }
 static Material Mat(string name,Shader shader,Action<Material> set){
  if(shader==null)throw new InvalidOperationException("Shader for "+name+" not found");Directory.CreateDirectory(Materials);var path=Materials+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);
  if(m==null){m=new Material(shader){name=name};AssetDatabase.CreateAsset(m,path);}else m.shader=shader;
  m.enableInstancing=true;set(m);EditorUtility.SetDirty(m);return m;
 }
 static void ImportTextures(){
  void Set(string file,Action<TextureImporter> apply){var importer=(TextureImporter)AssetImporter.GetAtPath(Textures+file);if(importer==null)throw new FileNotFoundException("Missing "+Textures+file);apply(importer);
   importer.SetPlatformTextureSettings(new TextureImporterPlatformSettings{name="Android",overridden=true,maxTextureSize=importer.maxTextureSize,format=importer.textureCompression==TextureImporterCompression.Uncompressed?TextureImporterFormat.RGBA32:TextureImporterFormat.ASTC_6x6});
   importer.SetPlatformTextureSettings(new TextureImporterPlatformSettings{name="iPhone",overridden=true,maxTextureSize=importer.maxTextureSize,format=importer.textureCompression==TextureImporterCompression.Uncompressed?TextureImporterFormat.RGBA32:TextureImporterFormat.ASTC_6x6});
   importer.SaveAndReimport();}
  Set("T_Asphalt_BC.png",i=>{i.textureType=TextureImporterType.Default;i.sRGBTexture=true;i.wrapMode=TextureWrapMode.Repeat;i.maxTextureSize=1024;i.textureCompression=TextureImporterCompression.CompressedHQ;});
  Set("T_Asphalt_N.png",i=>{i.textureType=TextureImporterType.NormalMap;i.wrapMode=TextureWrapMode.Repeat;i.maxTextureSize=1024;i.textureCompression=TextureImporterCompression.CompressedHQ;});
  Set("T_Noise.png",i=>{i.textureType=TextureImporterType.Default;i.sRGBTexture=false;i.alphaSource=TextureImporterAlphaSource.FromInput;i.wrapMode=TextureWrapMode.Repeat;i.maxTextureSize=256;i.textureCompression=TextureImporterCompression.Uncompressed;});
  Set("T_Glow.png",i=>{i.textureType=TextureImporterType.Default;i.sRGBTexture=false;i.alphaSource=TextureImporterAlphaSource.FromInput;i.wrapMode=TextureWrapMode.Clamp;i.maxTextureSize=64;i.textureCompression=TextureImporterCompression.Uncompressed;});
  Set("T_MenuSky_BC.png",i=>{i.textureType=TextureImporterType.Default;i.sRGBTexture=true;i.wrapModeU=TextureWrapMode.Mirror;i.wrapModeV=TextureWrapMode.Clamp;i.mipmapEnabled=false;i.maxTextureSize=2048;i.textureCompression=TextureImporterCompression.CompressedHQ;});
  Set("T_MenuSkyline_BC.png",i=>{i.textureType=TextureImporterType.Default;i.sRGBTexture=true;i.alphaIsTransparency=true;i.wrapMode=TextureWrapMode.Clamp;i.maxTextureSize=2048;i.textureCompression=TextureImporterCompression.CompressedHQ;});
  Set("T_LensDirt_BC.png",i=>{i.textureType=TextureImporterType.Default;i.sRGBTexture=true;i.wrapMode=TextureWrapMode.Clamp;i.maxTextureSize=512;i.textureCompression=TextureImporterCompression.CompressedHQ;});
 }

 // Ambience loops stream from compressed memory (Vorbis); they are long, quiet and always playing.
 static void ImportAudio(){foreach(var file in new[]{"A_Rain_Loop.wav","A_CityHum_Loop.wav","A_Fire_Loop.wav"}){var importer=(AudioImporter)AssetImporter.GetAtPath(Audio+file);if(importer==null)throw new FileNotFoundException("Missing "+Audio+file);
   importer.loadInBackground=true;importer.defaultSampleSettings=new AudioImporterSampleSettings{loadType=AudioClipLoadType.CompressedInMemory,compressionFormat=AudioCompressionFormat.Vorbis,quality=.45f,sampleRateSetting=AudioSampleRateSetting.PreserveSampleRate,preloadAudioData=true};importer.SaveAndReimport();}}
 static Mesh Quad(){var m=new Mesh{name="Quad"};m.vertices=new[]{new Vector3(-.5f,0,-.5f),new Vector3(-.5f,0,.5f),new Vector3(.5f,0,.5f),new Vector3(.5f,0,-.5f)};m.normals=Enumerable.Repeat(Vector3.up,4).ToArray();
  m.uv=new[]{new Vector2(0,0),new Vector2(0,1),new Vector2(1,1),new Vector2(1,0)};m.triangles=new[]{0,1,2,0,2,3};m.RecalculateBounds();return m;}
 /// <summary>A unit UV sphere; the Sky shader draws its inside (Cull Front) at the far plane.</summary>
 static Mesh Dome(int around,int up){var v=new Vector3[(around+1)*(up+1)];var uv=new Vector2[v.Length];
  for(int j=0;j<=up;j++)for(int i=0;i<=around;i++){float a=i*Mathf.PI*2/around,b=Mathf.PI*(j/(float)up-.5f);v[j*(around+1)+i]=new Vector3(Mathf.Cos(b)*Mathf.Sin(a),Mathf.Sin(b),Mathf.Cos(b)*Mathf.Cos(a));uv[j*(around+1)+i]=new Vector2(i/(float)around,j/(float)up);}
  var t=new int[around*up*6];int k=0;for(int j=0;j<up;j++)for(int i=0;i<around;i++){int a=j*(around+1)+i,b=a+around+1;t[k++]=a;t[k++]=b;t[k++]=a+1;t[k++]=a+1;t[k++]=b;t[k++]=b+1;}
  var m=new Mesh{name="SkyDome",vertices=v,uv=uv,triangles=t};m.RecalculateNormals();m.RecalculateBounds();return m;}
}
}
