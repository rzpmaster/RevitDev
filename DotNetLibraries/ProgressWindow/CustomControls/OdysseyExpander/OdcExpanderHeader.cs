using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace ProgressWindow.CustomControls.OdysseyExpander
{
    /// <summary>
    /// A helper class to specify the header of an OdcExpander.
    /// </summary>
    internal class OdcExpanderHeader : ToggleButton
    {
        static OdcExpanderHeader()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(OdcExpanderHeader), new FrameworkPropertyMetadata(typeof(OdcExpanderHeader)));
        }


        /// <summary>
        /// Gets whether the expand geometry is not null.
        /// </summary>
        public bool HasExpandGeometry
        {
            get { return (bool)GetValue(HasExpandGeometryProperty); }
            set { SetValue(HasExpandGeometryProperty, value); }
        }

        public static readonly DependencyProperty HasExpandGeometryProperty =
            DependencyProperty.Register("HasExpandGeometry", typeof(bool), typeof(OdcExpanderHeader), new UIPropertyMetadata(false));



        /// <summary>
        /// Gets or sets the geometry for the collapse symbol.
        /// </summary>
        public Geometry CollapseGeometry
        {
            get { return (Geometry)GetValue(CollapseGeometryProperty); }
            set { SetValue(CollapseGeometryProperty, value); }
        }

        public static readonly DependencyProperty CollapseGeometryProperty =
            DependencyProperty.Register("CollapseGeometry", typeof(Geometry), typeof(OdcExpanderHeader),
            new UIPropertyMetadata(null));


        public static void CollapseGeometryChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            OdcExpanderHeader eh = d as OdcExpanderHeader;
            eh.HasExpandGeometry = e.NewValue != null;
        }


        /// <summary>
        /// Gets or sets the geometry for the expand symbol.
        /// </summary>
        public Geometry ExpandGeometry
        {
            get { return (Geometry)GetValue(ExpandGeometryProperty); }
            set { SetValue(ExpandGeometryProperty, value); }
        }

        public static readonly DependencyProperty ExpandGeometryProperty =
            DependencyProperty.Register("ExpandGeometry", typeof(Geometry), typeof(OdcExpanderHeader), new UIPropertyMetadata(null, CollapseGeometryChangedCallback));



        /// <summary>
        /// Gets or sets the corner radius for the header.
        /// </summary>
        public CornerRadius CornerRadius
        {
            get { return (CornerRadius)GetValue(CornerRadiusProperty); }
            set { SetValue(CornerRadiusProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CornerRadius.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CornerRadiusProperty =
            DependencyProperty.Register("CornerRadius", typeof(CornerRadius), typeof(OdcExpanderHeader), new UIPropertyMetadata(null));



        /// <summary>
        /// Gets or sets whether to display the ellipse arround the collapse/expand symbol.
        /// </summary>
        public bool ShowEllipse
        {
            get { return (bool)GetValue(ShowEllipseProperty); }
            set { SetValue(ShowEllipseProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ShowEllipse.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ShowEllipseProperty =
            DependencyProperty.Register("ShowEllipse", typeof(bool), typeof(OdcExpanderHeader), new UIPropertyMetadata(true));



        /// <summary>
        /// Gets or sets the Image to display on the header.
        /// </summary>
        public ImageSource Image
        {
            get { return (ImageSource)GetValue(ImageProperty); }
            set { SetValue(ImageProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Image.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ImageProperty =
            DependencyProperty.Register("Image", typeof(ImageSource), typeof(OdcExpanderHeader), new UIPropertyMetadata(null));

    }
}
