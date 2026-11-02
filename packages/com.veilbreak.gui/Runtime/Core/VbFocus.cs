using System;
using UnityEngine.UIElements;

namespace Veilbreak.Gui
{
    /// <summary>
    /// Keyboard focus across in-place redraws, for screens that replace their buttons when data changes. A redraw drops the focused
    /// element with the old tree; <see cref="Keep"/> remembers its name and focuses the new element of that name, and
    /// <see cref="Soon"/> does the same once a fresh layout exists and scrolls it into view of its list. Pointer use has no focus to
    /// keep, so nothing happens then. <see cref="Read"/> makes plain text a focus stop, which is what lets the keyboard scroll a list
    /// of read-only rows.
    /// </summary>
    public static class VbFocus
    {
        /// <summary>Class <see cref="Read"/> adds; a host styles the focus frame of reading stops with it.</summary>
        public const string ReadClass = "read";

        /// <summary>Plain text the keyboard can still stop on: focus is what scrolls a list, and a list of read-only rows has nothing focusable in it. Names the element when <paramref name="name"/> is given.</summary>
        public static T Read<T>(T element, string name = null) where T : VisualElement
        {
            element.focusable = true; element.AddToClassList(ReadClass); if (name != null) element.name = name; return element;
        }

        /// <summary>Runs <paramref name="redraw"/>, then focuses the element that has the focused one's name, or the one <paramref name="fallback"/> names (asked after the redraw) when that element is gone.</summary>
        public static void Keep(VisualElement root, Action redraw, Func<string> fallback = null)
        {
            var held = root.focusController?.focusedElement as VisualElement;
            var name = held != null && held.panel != null ? held.name : null;
            redraw();
            if (!string.IsNullOrEmpty(name)) Soon(root, name, fallback);
        }

        /// <summary>Focuses a named element now, or a moment later once a fresh layout exists, and scrolls it into view of its list; <paramref name="fallback"/> names another when the first is missing, hidden or disabled.</summary>
        public static void Soon(VisualElement root, string name, Func<string> fallback = null)
        {
            VisualElement Pick() => Stop(root, name) ?? Stop(root, fallback?.Invoke());
            void Place(VisualElement e)
            {
                e.Focus(); var list = e.GetFirstAncestorOfType<ScrollView>();
                if (list != null) root.schedule.Execute(() => { if (e.panel != null && !float.IsNaN(e.worldBound.y)) list.ScrollTo(e); }).ExecuteLater(50);
            }
            var now = Pick(); if (now != null) { Place(now); return; }
            root.schedule.Execute(() => { var late = Pick(); if (late != null) Place(late); }).ExecuteLater(40);
        }

        static VisualElement Stop(VisualElement root, string name)
        {
            var e = string.IsNullOrEmpty(name) ? null : root.Q(name);
            return e != null && e.panel != null && e.focusable && e.enabledInHierarchy && e.style.display != DisplayStyle.None ? e : null;
        }
    }
}
