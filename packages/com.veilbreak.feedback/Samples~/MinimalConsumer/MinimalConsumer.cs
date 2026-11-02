using UnityEngine;
using Veilbreak.Feedback;
public sealed class FeedbackSample:MonoBehaviour {public Material additiveMaterial;FeedbackPool pool;void Start(){pool=gameObject.AddComponent<FeedbackPool>();pool.Initialize(additiveMaterial);pool.Emit(new FeedbackRequest{to=Vector3.up,color=Color.cyan,size=.1f,duration=.4f,priority=2});}void Update(){pool.Advance(Time.deltaTime);}}
