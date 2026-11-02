using UnityEngine;
using UnityEngine.UIElements;
using Veilbreak.Branding;
[RequireComponent(typeof(UIDocument))]public sealed class BrandingSample:MonoBehaviour {void Start(){IntroSplash.Play(GetComponent<UIDocument>().rootVisualElement,false);}void OnDisable(){IntroSplash.Stop();}}
