using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

namespace Veilbreak.ARHud
{
    /// <summary>
    /// Base for observable view-models a screen binds to (runtime data binding, MVVM for UI Toolkit): a property set through
    /// <see cref="Set{T}"/> notifies only when its value changes, so an idle screen rebuilds no text. Give the class
    /// [GeneratePropertyBag] (or mark properties [CreateProperty]) and set it as an element's dataSource; supply values on the
    /// main thread.
    /// </summary>
    public abstract class HudViewModel : INotifyBindablePropertyChanged
    {
        public event EventHandler<BindablePropertyChangedEventArgs> propertyChanged;

        protected void Set<T>(ref T field, T value, [CallerMemberName] string property = "")
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return;
            field = value; Notify(property);
        }

        protected void Notify(string property) => propertyChanged?.Invoke(this, new BindablePropertyChangedEventArgs(property));
    }

    /// <summary>The gallery's and QuickStart sample's telemetry: an example view-model.</summary>
    [GeneratePropertyBag]
    public partial class HudModel : HudViewModel
    {
        private float integrity = .86f;
        private string objective = "REACH THE SIGNAL RELAY";
        private string ammunition = "24 / 120";
        [CreateProperty] public float Integrity { get => integrity; set => Set(ref integrity, float.IsNaN(value) ? 0 : Mathf.Clamp01(value)); }
        [CreateProperty] public string Objective { get => objective; set => Set(ref objective, value ?? ""); }
        [CreateProperty] public string Ammunition { get => ammunition; set => Set(ref ammunition, value ?? ""); }
    }

    public static class HudBinding
    {
        /// <summary>Binds <paramref name="targetProperty"/> of <paramref name="element"/> one way to <paramref name="sourceProperty"/> of its data source.</summary>
        public static void To(VisualElement element, string targetProperty, string sourceProperty)
        {
            element.SetBinding(targetProperty, new DataBinding
            {
                dataSourcePath = new PropertyPath(sourceProperty), bindingMode = BindingMode.ToTarget
            });
        }
    }
}
