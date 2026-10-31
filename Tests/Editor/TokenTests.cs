using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
namespace Veilbreak.Gui.Tests {
/// <summary>Tokens.uss and VbTokens are generated from Tokens~/tokens.json by Tools~/tokens.py; these fail when either drifts.</summary>
public sealed class TokenTests {
 static string Package(string path)=>PackageFiles.Path(path);
 // Veil-theme colour per token name, aliases resolved. A value is "#hex", "{alias}" or {"veil": …, "contrast": …}.
 static Dictionary<string,Color> Source(){var json=File.ReadAllText(Package("Tokens~/tokens.json"));int from=json.IndexOf("\"color\""),to=json.IndexOf("\"type\"",from);var raw=new Dictionary<string,string>();
  foreach(Match m in Regex.Matches(json.Substring(from,to-from),"\"name\":\\s*\"([a-z0-9-]+)\",\\s*\"value\":\\s*(\\{(?:[^{}]|\\{[^{}]*\\})*\\}|\"[^\"]*\")")){var v=m.Groups[2].Value;raw[m.Groups[1].Value]=v.StartsWith("{")&&v.Contains("\"veil\"")?Regex.Match(v,"\"veil\":\\s*\"([^\"]+)\"").Groups[1].Value:v.Trim('"');}
  string Resolve(string n)=>raw[n].StartsWith("{")?Resolve(raw[n].Substring(1,raw[n].Length-2)):raw[n];
  return raw.Keys.ToDictionary(n=>n,n=>{ColorUtility.TryParseHtmlString(Resolve(n),out var c);return c;});}
 static Dictionary<string,Color> Uss(string block){var uss=File.ReadAllText(Package("Runtime/Styles/Tokens.uss"));var body=Regex.Match(uss,Regex.Escape(block)+"\\s*\\{([^}]*)\\}").Groups[1].Value;var result=new Dictionary<string,Color>();
  foreach(Match m in Regex.Matches(body,"--vb-color-([a-z0-9-]+):\\s*([^;]+);")){var v=m.Groups[2].Value.Trim();var rgba=Regex.Match(v,"rgba\\((\\d+),\\s*(\\d+),\\s*(\\d+),\\s*([\\d.]+)\\)");
   result[m.Groups[1].Value]=rgba.Success?new Color(int.Parse(rgba.Groups[1].Value)/255f,int.Parse(rgba.Groups[2].Value)/255f,int.Parse(rgba.Groups[3].Value)/255f,float.Parse(rgba.Groups[4].Value,System.Globalization.CultureInfo.InvariantCulture)):ColorUtility.TryParseHtmlString(v,out var c)?c:Color.magenta;}
  return result;}
 static string Pascal(string name)=>string.Concat(name.Split('-').Select(p=>char.ToUpperInvariant(p[0])+p.Substring(1)));
 static void Same(Color expected,Color actual,string what){Assert.That(Vector4.Distance(expected,actual),Is.LessThan(.003f),what+": "+ColorUtility.ToHtmlStringRGBA(expected)+" vs "+ColorUtility.ToHtmlStringRGBA(actual));}

 [Test] public void UssColoursMatchTokens(){var source=Source();var uss=Uss(":root");Assert.Greater(source.Count,60);CollectionAssert.AreEquivalent(source.Keys,uss.Keys,"Tokens.uss defines exactly the colour tokens; run Tools~/tokens.py");foreach(var pair in source)Same(pair.Value,uss[pair.Key],pair.Key);
  Assert.Greater(Uss(".vb-contrast").Count,20,"The high-contrast theme overrides colours under .vb-contrast");}
 [Test] public void CodeColoursMatchTokens(){foreach(var pair in Source()){var field=typeof(VbTokens).GetField(Pascal(pair.Key),BindingFlags.Public|BindingFlags.Static);Assert.IsNotNull(field,"VbTokens."+Pascal(pair.Key)+"; run Tools~/tokens.py");Same(pair.Value,(Color)field.GetValue(null),pair.Key);}
  Assert.AreEqual(VbTokens.KitAria,VbTokens.Kit("aria"));Assert.AreEqual(VbTokens.Text,VbTokens.Kit("nobody"));Assert.AreEqual("#8ff0cf",VbTokens.Hex(VbTokens.AllyText));Assert.AreEqual(9,VbTokens.Hex(VbTokens.Text3).Length,"Translucent colours keep their alpha in rich text");}
 [Test] public void StylesReferenceOnlyDefinedTokens(){var defined=new HashSet<string>(Regex.Matches(File.ReadAllText(Package("Runtime/Styles/Tokens.uss")),"(--vb-[a-z0-9-]+):").Cast<Match>().Select(m=>m.Groups[1].Value));
  foreach(var sheet in new[]{"Runtime/Styles/Gui.uss","Runtime/Styles/Layout.uss","Runtime/Styles/TokensContrast.uss"})foreach(Match m in Regex.Matches(File.ReadAllText(Package(sheet)),"var\\((--vb-[a-z0-9-]+)\\)"))Assert.IsTrue(defined.Contains(m.Groups[1].Value),sheet+" references undefined "+m.Groups[1].Value);}
 // ARC-51: FontPrimer covers every font token, and each face has its primer class.
 [Test] public void FontPrimerCoversEveryFace(){
  var tokens=Regex.Matches(File.ReadAllText(Package("Runtime/Styles/Tokens.uss")),"--vb-font-([a-z-]+):\\s*url\\(").Cast<Match>().Select(m=>m.Groups[1].Value).Distinct().OrderBy(x=>x).ToArray();
  Assert.IsNotEmpty(tokens);CollectionAssert.AreEquivalent(tokens,FontPrimer.Faces);var gui=File.ReadAllText(Package("Runtime/Styles/Gui.uss"));
  foreach(var face in FontPrimer.Faces)StringAssert.Contains($".vb-font-primer__{face} {{ -unity-font-definition: var(--vb-font-{face});",gui);
  foreach(var c in "SQUAD 0123456789·×…")StringAssert.Contains(c.ToString(),FontPrimer.Glyphs);
 }
}
}
