using System;
using System.Text;
namespace Veilbreak.Gui {
/// <summary>The one formatter for description text. The character data marks emphasis with asterisks (<c>one *Taxed* stack</c>); a label must not print
/// them. <see cref="Rich"/> turns each <c>*word*</c> (or <c>**word**</c>) into a rich-text colour run, so every description label shows the emphasis and no asterisk;
/// <see cref="Plain"/> drops the marks for tooltips and measuring. An unpaired or spaced asterisk ("5 * 3", "a *") is left as written, and text with no asterisk is
/// returned as it came, so binding the same text again makes no garbage.</summary>
public static class Markup {
 const char Mark='*';
 static string emphasis;
 static string Emphasis=>emphasis??=VbTokens.Hex(VbTokens.Text);
 /// <summary>The text with each emphasised run wrapped in a <c>&lt;color&gt;</c> tag; unbalanced marks stay.</summary>
 public static string Rich(string text)=>Convert(text,true);
 /// <summary>The text with its emphasis marks removed.</summary>
 public static string Plain(string text)=>Convert(text,false);
 static string Convert(string text,bool tag){
  if(string.IsNullOrEmpty(text))return text??"";if(text.IndexOf(Mark)<0)return text;
  var b=new StringBuilder(text.Length+32);int i=0;
  while(i<text.Length){
   if(text[i]!=Mark){b.Append(text[i++]);continue;}
   int j=i;while(j<text.Length&&text[j]==Mark)j++;int run=Math.Min(j-i,2);
   int close=j<text.Length&&!char.IsWhiteSpace(text[j])?text.IndexOf(new string(Mark,run),j,StringComparison.Ordinal):-1;
   if(close>j&&!char.IsWhiteSpace(text[close-1])){if(tag)b.Append("<color=").Append(Emphasis).Append('>');b.Append(text,j,close-j);if(tag)b.Append("</color>");i=close+run;continue;}
   b.Append(text,i,j-i);i=j;
  }
  return b.ToString();
 }
}
}
