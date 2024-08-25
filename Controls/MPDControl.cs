using System.Windows;
using System.Windows.Controls;

namespace MTGProxyDesk.Controls
{
    public abstract partial class MPDControl : UserControl
    {
        public static readonly DependencyProperty TextContentProperty = DependencyProperty.RegisterAttached(
            name: "TextContent",
            propertyType: typeof(string),
            ownerType: typeof(MPDControl),
            defaultMetadata: new FrameworkPropertyMetadata(
                defaultValue: "",
                flags: FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.BindsTwoWayByDefault
            )
        );

        public static void SetTextContent(MPDControl ctrl, string value)
        {
            ctrl.SetValue(TextContentProperty, value);
        }

        private string _content = "";
        public virtual string TextContent
        {
            get => (string)GetValue(TextContentProperty);
            set => SetTextContent(this, value);
        }

        public abstract Visibility CtrlVisibility { get; set; }

        public MPDControl()
        {
            DataContext = this;
        }
    }
}