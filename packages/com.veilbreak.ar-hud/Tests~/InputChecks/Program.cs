using UnityEngine;
using UnityEngine.UIElements;
using Veilbreak.ARHud;

// HudStage board geometry and HudInput pick blocking, on the production sources with headless stand-ins (Shims.cs).
int checks = 0;
void Check(bool valid, string label)
{
    if (!valid) throw new InvalidOperationException("FAILED: " + label);
    checks++; Console.WriteLine("ok " + label);
}
bool Close(float a, float b) => Math.Abs(a - b) < .02f;

// Multiple panel scales, origins and container offsets; conversion must not assume a full-screen artboard.
foreach (var size in new[] { (2400, 1080), (2532, 1170), (1280, 720), (1080, 2400) })
foreach (var scale in new[] { .5f, 1f, 1.7f })
foreach (var mirror in new[] { false, true })
{
    Screen.width = size.Item1; Screen.height = size.Item2;
    Screen.safeArea = new Rect(80, 24, Screen.width - 80, Screen.height - 24);
    var panel = new TestPanel { origin = new Vector2(11, 7), extent = new Vector2(Screen.width * scale, Screen.height * scale) };
    var stage = new HudStage { panel = panel, Mirror = mirror, layout = new Rect(31, 19, panel.extent.x - 50, panel.extent.y - 30) };
    stage.Fit();
    foreach (var v in new[] { new Vector3(.1f, .1f, 1), new Vector3(.5f, .5f, 1), new Vector3(.9f, .8f, 1) })
    {
        var board = stage.FromViewport(v);
        var actualPanel = stage.Board.LocalToWorld(board);
        var expectedPanel = RuntimePanelUtils.ScreenToPanel(panel, new Vector2(v.x * Screen.width, (1 - v.y) * Screen.height));
        Check(Close(actualPanel.x, expectedPanel.x) && Close(actualPanel.y, expectedPanel.y), "fighter label aligns with camera projection despite container offsets");
        var screen = stage.ToScreen(board);
        Check(Close(screen.x, v.x * Screen.width) && Close(screen.y, v.y * Screen.height), $"viewport/screen round trip {size} scale {scale} mirror {mirror}");
    }
    var control = stage.Place(new Button(), 24, 10, 32, 8);
    var expectedLeft = (mirror ? 32 : 24) + 80 * stage.layout.width / Screen.width / stage.Scale;
    Check(Close(control.style.left.value, expectedLeft) && Close(control.style.right.value, mirror ? 24 : 32), "notch stays on physical edge; right inset never becomes negative");
    Check(Close(control.style.top.value, 10) && Close(control.style.bottom.value, 8 + 24 * stage.layout.height / Screen.height / stage.Scale), "bottom safe area is independent of top inset");
}

Screen.width = 2400; Screen.height = 1080;
var hudRoot = new VisualElement();
var hudPanel = new TestPanel(); hudRoot.panel = hudPanel;
var emptyWrapper = new VisualElement(); hudRoot.Add(emptyWrapper); hudPanel.pick = _ => emptyWrapper;
Check(!HudInput.IsOverControls(hudRoot, new Vector2(500, 300)), "empty full-screen wrapper does not block arena");
var button = new Button(); var label = new VisualElement(); button.Add(label); hudRoot.Add(button); hudPanel.pick = _ => label;
Check(HudInput.IsOverControls(hudRoot, new Vector2(500, 300)), "button label blocks arena input");
Check(Close(hudPanel.lastPick.x, 250) && Close(hudPanel.lastPick.y, 390), "bottom-left input flips once and respects panel scale");
var field = new BaseField<bool>(); field.AddToClassList(BaseField<bool>.ussClassName); hudRoot.Add(field); hudPanel.pick = _ => field;
Check(HudInput.IsOverControls(hudRoot, Vector2.zero), "settings fields block arena input");
var sheet = new VisualElement(); sheet.AddToClassList(HudInput.BlockClass); var sheetLabel = new VisualElement(); sheet.Add(sheetLabel); hudRoot.Add(sheet); hudPanel.pick = _ => sheetLabel;
Check(HudInput.IsOverControls(hudRoot, Vector2.zero), "open sheet background blocks arena input");
HudInput.PassThrough(emptyWrapper); var decoration = new VisualElement(); var decorationChild = new VisualElement(); decoration.Add(decorationChild); HudInput.PassThrough(decoration);
Check(decoration.pickingMode == PickingMode.Ignore && decorationChild.pickingMode == PickingMode.Ignore, "world decorations ignore picking recursively");

Console.WriteLine($"{checks} ar-hud input checks passed");
