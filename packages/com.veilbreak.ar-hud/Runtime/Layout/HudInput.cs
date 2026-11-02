using UnityEngine;
using UnityEngine.UIElements;

namespace Veilbreak.ARHud
{
    // Input System/cameras use bottom-left screen pixels; UI Toolkit uses top-left panel units.
    // Keep the conversion independent of the board's scale, safe area and handedness. A panel's own conversion may bend
    // pointer positions (HudHost.ScreenToPanel, a lens such as a curved visor); camera projections are in the unbent source
    // image, so SourceToPanel and PanelToScreen map linearly (a lens that fixes the four corners keeps the corner readings true).
    public static class HudInput
    {
        // Marks an element (an open sheet's background, say) that blocks the world under it like a control does.
        public const string BlockClass = "blocks-arena-input";

        public static Vector2 ScreenToPanel(IPanel panel, Vector2 screen) =>
            RuntimePanelUtils.ScreenToPanel(panel, new Vector2(screen.x, Screen.height - screen.y));

        public static Vector2 SourceToPanel(IPanel panel, Vector2 source)
        {
            var origin = RuntimePanelUtils.ScreenToPanel(panel, Vector2.zero);
            var extent = RuntimePanelUtils.ScreenToPanel(panel, new Vector2(Screen.width, Screen.height)) - origin;
            return new Vector2(origin.x + source.x / Screen.width * extent.x, origin.y + (Screen.height - source.y) / Screen.height * extent.y);
        }

        public static Vector2 PanelToScreen(IPanel panel, Vector2 point)
        {
            var origin = RuntimePanelUtils.ScreenToPanel(panel, Vector2.zero);
            var extent = RuntimePanelUtils.ScreenToPanel(panel, new Vector2(Screen.width, Screen.height)) - origin;
            return new Vector2((point.x - origin.x) / extent.x * Screen.width,
                Screen.height - (point.y - origin.y) / extent.y * Screen.height);
        }

        // True when the screen point lies on a control of `root` (a button, a scroll view, a field or a BlockClass element)
        // rather than on the world behind the UI.
        public static bool IsOverControls(VisualElement root, Vector2 screen)
        {
            if (root?.panel == null) return false;
            var picked = root.panel.Pick(ScreenToPanel(root.panel, screen));
            if (picked == null || !root.Contains(picked)) return false;
            for (var element = picked; element != null; element = element.parent)
            {
                if (element is Button || element is ScrollView ||
                    element.ClassListContains(BaseField<bool>.ussClassName) || element.ClassListContains(BlockClass)) return true;
                if (element == root) break;
            }
            return false;
        }

        // PickingMode.Ignore applies only to an element, not its descendants.
        public static void PassThrough(VisualElement decoration)
        {
            decoration.pickingMode = PickingMode.Ignore;
            foreach (var child in decoration.Children()) PassThrough(child);
        }
    }
}
