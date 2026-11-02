using UnityEngine;
using Veilbreak.ARHud;

/// <summary>Optional demo only. Replace with your snapshot-to-view-model adapter.</summary>
public sealed class TelemetryDriver : MonoBehaviour
{
    [SerializeField] private HudHost host;
    [SerializeField, Range(0, 1)] private float integrity = .86f;
    [SerializeField] private string objective = "REACH THE SIGNAL RELAY";
    [SerializeField] private int ammunition = 24;
    private void Update()
    {
        if (!host) return;
        host.Model.Integrity = integrity;
        host.Model.Objective = objective;
        host.Model.Ammunition = $"{ammunition} / 120";
    }
}
