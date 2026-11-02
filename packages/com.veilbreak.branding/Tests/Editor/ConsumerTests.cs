using NUnit.Framework;
using UnityEngine.UIElements;
using Veilbreak.Branding;
public sealed class BrandingConsumerTests {
 [Test] public void ReplacingAndStoppingSplashLeavesNoOrphanOverlay(){var host=new VisualElement();try{IntroSplash.Play(host,true);Assert.AreEqual(1,host.childCount);IntroSplash.Play(host,false);Assert.AreEqual(1,host.childCount);IntroSplash.Stop();Assert.AreEqual(0,host.childCount);IntroSplash.Stop();Assert.AreEqual(0,host.childCount);}finally{IntroSplash.Stop();}}
 // The cut's sound ships with the package and starts early enough for its whoosh to lead into the impact.
 [Test] public void SliceSoundShipsAndLandsOnTheCut(){var clip=IntroSplash.SliceSound;Assert.IsNotNull(clip,"Resources/VeilbreakSplashSlice");Assert.Greater(clip.length,IntroSplash.SliceLead+.5f);Assert.Greater(IntroSplash.SliceStart,0);
  var custom=UnityEngine.AudioClip.Create("custom",32,1,32000,false);try{IntroSplash.SliceSound=custom;Assert.AreSame(custom,IntroSplash.SliceSound);}finally{IntroSplash.SliceSound=clip;UnityEngine.Object.DestroyImmediate(custom);}}
}
