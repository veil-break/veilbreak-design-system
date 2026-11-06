using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UIElements;
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
 // ds#22: the border rule. Gui.uss lists the nested surfaces between nest:begin and nest:end; Layout.uss gives each its edge back under .vb-contrast.
 static (string[] selectors,string body) Block(string file,string begin,string end){var css=File.ReadAllText(Package(file));int a=css.IndexOf(begin,System.StringComparison.Ordinal)+begin.Length,b=css.IndexOf(end,System.StringComparison.Ordinal);Assert.Greater(a,begin.Length-1,begin);Assert.Greater(b,a,end);
  var text=css.Substring(a,b-a);int brace=text.IndexOf('{');return (text.Substring(0,brace).Split(',').Select(x=>x.Trim()).Where(x=>x.Length>0).ToArray(),text.Substring(brace));}
 [Test] public void NestedSurfacesDropTheirEdge(){var (nested,body)=Block("Runtime/Styles/Gui.uss","/* nest:begin */","/* nest:end */");
  foreach(var rule in new[]{"--vb-bw: 0","--vb-dash: 0","--vb-glow: rgba(0, 0, 0, 0)","--vb-bracket: rgba(0, 0, 0, 0)"})StringAssert.Contains(rule,body,"A nested surface has no edge width, dashes, glow or brackets");
  foreach(var selector in new[]{".vb-nest",".vb-tile",".vb-slot",".vb-operative .vb-skill",".vb-queue-chip",".vb-queue__row",".vb-hud__resources",".vb-chip-row__plate",".vb-perk-detail__thumb",".vb-perk-row__thumb",".vb-contribution__portrait",".vb-details__skill-icon",".vb-result",".vb-dialog .vb-button--secondary",".vb-drawer__sheet .vb-button--secondary"})CollectionAssert.Contains(nested,selector,"The report's border budget lists "+selector);
  var (contrast,contrastBody)=Block("Runtime/Styles/Layout.uss","/* nest:contrast:begin */","/* nest:contrast:end */");CollectionAssert.AreEqual(nested.Select(x=>".vb-contrast "+x).ToArray(),contrast,"Every nested surface has its high-contrast edge");StringAssert.Contains("--vb-bw: 1",contrastBody);StringAssert.Contains("--vb-edge: var(--vb-color-line-strong)",contrastBody);
  var gui=File.ReadAllText(Package("Runtime/Styles/Gui.uss"));
  // Selections, the one primary, danger and an empty drop target keep an edge: their rules come after the nest block and set one.
  int nestAt=gui.IndexOf("/* nest:end */");foreach(var keeper in new[]{".vb-tile--focused, .vb-tile--focused:hover {",".vb-slot--empty { --vb-dash: 1; --vb-bw: 1;",".vb-choice--selected {",".vb-button--primary {",".vb-button--danger {",".vb-operative--aiming {"}){int at=gui.IndexOf(keeper,System.StringComparison.Ordinal);Assert.Greater(at,nestAt,keeper+" follows the nest rule so it keeps its edge");}
  // Hairline separators inside a bordered surface are spacing now: none of the nested parts carries a CSS border.
  foreach(var part in new[]{".vb-skill__charge",".vb-meter__preview",".vb-skill__sweep",".vb-details__skill ",".vb-contribution__grid",".vb-perk-detail__rows",".vb-results__stats"}){var m=Regex.Match(gui+File.ReadAllText(Package("Runtime/Styles/Views.uss")),"(?m)^"+Regex.Escape(part)+"\\s*\\{[^}]*\\}");Assert.IsTrue(m.Success,part);StringAssert.DoesNotContain("border",m.Value,part+" is a surface inside a surface");}
  // A blocked action is dimmed, not dashed.
  StringAssert.DoesNotContain("--vb-dash: 1",Regex.Match(gui,"(?m)^\\.vb-button--blocked \\{[^}]*\\}").Value);}
 // A hover is a tone step, not an edge, on a surface inside a dialog panel.
 [Test] public void NestedHoversStepTheFillInsteadOfDrawingAnEdge(){var views=File.ReadAllText(Package("Runtime/Styles/Views.uss"));StringAssert.DoesNotContain(".vb-pause__perks:hover { --vb-bw: 1",views);}
 // ARC-51: FontPrimer covers every font token, and each face has its primer class.
 [Test] public void FontPrimerCoversEveryFace(){
  var tokens=Regex.Matches(File.ReadAllText(Package("Runtime/Styles/Tokens.uss")),"--vb-font-([a-z-]+):\\s*url\\(").Cast<Match>().Select(m=>m.Groups[1].Value).Distinct().OrderBy(x=>x).ToArray();
  Assert.IsNotEmpty(tokens);CollectionAssert.AreEquivalent(tokens,FontPrimer.Faces);var gui=File.ReadAllText(Package("Runtime/Styles/Gui.uss"));
  foreach(var face in FontPrimer.Faces)StringAssert.Contains($".vb-font-primer__{face} {{ -unity-font-definition: var(--vb-font-{face});",gui);
  foreach(var c in "SQUAD 0123456789·×…−›‹")StringAssert.Contains(c.ToString(),FontPrimer.Glyphs);
 }
 // ds#26: a popover or card title is primed in its own face and size too, mixed case included (the glyph run is every printable ASCII letter), so its tracking does not depend on what was drawn first.
 [Test] public void FontPrimerPrimesTheTitleStylesAtTheirOwnSize(){
  var gui=File.ReadAllText(Package("Runtime/Styles/Gui.uss"))+File.ReadAllText(Package("Runtime/Styles/Views.uss"));
  foreach(var style in FontPrimer.Styles)StringAssert.Contains("."+style+" {",gui,style+" is a real text style");
  CollectionAssert.Contains(FontPrimer.Styles,EffectDetails.TitleClass,"The effect popover's title is primed");CollectionAssert.Contains(FontPrimer.Styles,"vb-popover-card__title");CollectionAssert.Contains(FontPrimer.Styles,"vb-choice__title");
  foreach(var c in "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ")StringAssert.Contains(c.ToString(),FontPrimer.Glyphs);StringAssert.Contains(".vb-font-primer__style",gui);
  var host=new VisualElement();FontPrimer.Prime(host);var box=host.Q(className:FontPrimer.ClassName);Assert.IsNotNull(box);Assert.AreEqual(FontPrimer.Faces.Length+FontPrimer.Styles.Length,box.childCount,"One label per face and per style");
  foreach(var style in FontPrimer.Styles)Assert.IsNotNull(box.Q(className:style),style);}
}
}
