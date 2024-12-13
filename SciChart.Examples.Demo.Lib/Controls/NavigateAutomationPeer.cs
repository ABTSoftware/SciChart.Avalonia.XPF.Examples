using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;

#if XPF
using SciChart.Charting;
using SciChart.Drawing.VisualXcceleratorRasterizer;
#endif

namespace SciChart.Examples.Demo.Lib.Controls
{
    public class NavigateAutomationPeer : FrameworkElementAutomationPeer, IValueProvider
    {
        private readonly AutomationNavigateButton _owner;

        public bool IsReadOnly => false;

        public string Value => GetValue();

        public NavigateAutomationPeer(AutomationNavigateButton owner) : base(owner)
        {
            _owner = owner;
        }

        protected override AutomationControlType GetAutomationControlTypeCore()
        {
            return AutomationControlType.Button;
        }

        protected override string GetClassNameCore()
        {
            return Value.GetType().Name;
        }

        public override object GetPattern(PatternInterface patternInterface)
        {
            if (patternInterface == PatternInterface.Value)
            {
                return this;
            }
            return base.GetPattern(patternInterface);
        }

        public void SetValue(string value)
        {
            _owner.ExampleUri = value;
        }

        private string GetValue()
        {
#if XPF
            return VisualXcceleratorEngine.ActiveGraphicsAPI == VxGraphicsAPI.DirectX
                ? $"Xpf.Vx.DirectX;{_owner.ExampleUri}"
                : $"Xpf.Vx.OpenGL;{_owner.ExampleUri}";
#else
            throw new System.NotSupportedException();
#endif
        }
    }
}
