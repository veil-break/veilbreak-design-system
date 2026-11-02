using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UIElements;
using Veilbreak.Feedback;
public sealed class FeedbackConsumerTests {
 [Test] public void MajorCueReplacesRoutineCueWithoutGrowingPool(){var host=new GameObject("Feedback test");var material=new Material(Shader.Find("Veilbreak/Feedback/Additive"));try{var pool=host.AddComponent<FeedbackPool>();pool.Initialize(material);for(int i=0;i<100;i++)pool.Emit(new FeedbackRequest{color=Color.blue,duration=1});Assert.AreEqual(48,host.transform.childCount);pool.Emit(new FeedbackRequest{color=Color.red,duration=1,priority=2});Assert.AreEqual(48,host.transform.childCount);Assert.AreEqual(1,host.GetComponentsInChildren<LineRenderer>(true).Count(line=>line.startColor.r>.9f));pool.Advance(2);Assert.IsTrue(host.GetComponentsInChildren<Transform>(true).Where(t=>t!=host.transform).All(t=>!t.gameObject.activeSelf));}finally{Object.DestroyImmediate(host);Object.DestroyImmediate(material);}}
 [Test] public void RapidReadoutsCoalesceAndRemainBounded(){var ui=new ProjectedReadouts();for(int i=0;i<100;i++)ui.Number(1,Vector3.zero,1,false);Assert.AreEqual(1,ui.Query<Label>().ToList().Count);for(int i=2;i<200;i++)ui.Number(i,Vector3.zero,1,false);Assert.AreEqual(40,ui.Query<Label>().ToList().Count);ui.EndFrame(2,p=>Vector2.zero);Assert.IsTrue(ui.Query<Label>().ToList().All(label=>label.style.display.value==DisplayStyle.None));}
}
