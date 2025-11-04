using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows;

namespace YoYo.Toolkit.Controls.BusyIndicators
{
    public enum AnimationTypes
    {
        //
        // 摘要:
        //     Represents a BarChart type animation that allows the user to select and apply
        //     for the control.
        BarChart,
        //
        // 摘要:
        //     Represents a Clock type animation that allows the user to select and apply for
        //     the control.
        Clock,
        //
        // 摘要:
        //     Represents a Fluent type animation that allows the user to select and apply for
        //     the control.
        Fluent,
        //
        // 摘要:
        //     Represents a Message type animation that allows the user to select and apply
        //     for the control.
        Message,
        //
        // 摘要:
        //     Represents a DoubleRing type animation that allows the user to select and apply
        //     for the control.
        DoubleRing,
        //
        // 摘要:
        //     Represents a DualRing type animation that allows the user to select and apply
        //     for the control.
        DualRing,
        //
        // 摘要:
        //     Represents a Ripple type animation that allows the user to select and apply for
        //     the control.
        Ripple,
        //
        // 摘要:
        //     Represents a DotCircle type animation that allows the user to select and apply
        //     for the control.
        DotCircle,
        //
        // 摘要:
        //     Represents a Cupertino type animation that allows the user to select and apply
        //     for the control.
        Cupertino,
        //
        // 摘要:
        //     Represents a flower type animation that allows the user to select and apply for
        //     the control.
        Flower,
        //
        // 摘要:
        //     Represents a Gear type animation that allows the user to select and apply for
        //     the control.
        Gear,
        //
        // 摘要:
        //     Represents a Liquid type animation that allows the user to select and apply for
        //     the control.
        Liquid,
        //
        // 摘要:
        //     Represents a Box type animation that allows the user to select and apply for
        //     the control.
        Box,
        //
        // 摘要:
        //     Represents a HorizontalPulsingBox type animation that allows the user to select
        //     and apply for the control.
        HorizontalPulsingBox,
        //
        // 摘要:
        //     Represents a Rotation type animation that allows the user to select and apply
        //     for the BusyIndicator
        Rotation,
        //
        // 摘要:
        //     Represents a SliceBox type animation that allows the user to select and apply
        //     for the control.
        SliceBox,
        //
        // 摘要:
        //     Represents a DoubleCircle type animation that allows the user to select and apply
        //     for the control.
        DoubleCircle,
        //
        // 摘要:
        //     Represents a Drop type animation that allows the user to select and apply for
        //     the control.
        Drop,
        //
        // 摘要:
        //     Represents a Ball type animation that allows the user to select and apply for
        //     the control.
        Ball,
        //
        // 摘要:
        //     Represents a Delete type animation that allows the user to select and apply for
        //     the control.
        Delete,
        //
        // 摘要:
        //     Represents a Sunny type animation that allows the user to select and apply for
        //     the control.
        Sunny,
        //
        // 摘要:
        //     Represents a ECG type animation that allows the user to select and apply for
        //     the control.
        ECG,
        //
        // 摘要:
        //     Represents a GPS type animation that allows the user to select and apply for
        //     the control.
        GPS,
        //
        // 摘要:
        //     Represents a Pen type animation that allows the user to select and apply for
        //     the control.
        Pen,
        //
        // 摘要:
        //     Represents a Globe type animation that allows the user to select and apply for
        //     the control.
        Globe,
        //
        // 摘要:
        //     Represents a Print type animation that allows the user to select and apply for
        //     the control.
        Print,
        //
        // 摘要:
        //     Represents a Rectangle type animation that allows the user to select and apply
        //     for the control.
        Rectangle,
        //
        // 摘要:
        //     Represents a ArrowTrack type animation that allows the user to select and apply
        //     for the control.
        ArrowTrack,
        //
        // 摘要:
        //     Represents a Temperature type animation that allows the user to select and apply
        //     for the control.
        Temperature,
        //
        // 摘要:
        //     Represents a Umbrella type animation that allows the user to select and apply
        //     for the control.
        Umbrella,
        //
        // 摘要:
        //     Represents a Battery type animation that allows the user to select and apply
        //     for the control.
        Battery,
        //
        // 摘要:
        //     Represents a Windmill type animation that allows the user to select and apply
        //     for the control.
        Windmill,
        //
        // 摘要:
        //     Represents a Rainy type animation that allows the user to select and apply for
        //     the control.
        Rainy,
        //
        // 摘要:
        //     Represents a Snow type animation that allows the user to select and apply for
        //     the control.
        Snow,
        //
        // 摘要:
        //     Represents a Flight type animation that allows the user to select and apply for
        //     the control.
        Flight,
        //
        // 摘要:
        //     Represents a SingleCircle type animation that allows the user to select and apply
        //     for the control.
        SingleCircle,
        //
        // 摘要:
        //     Represents a SlicedCircle type animation that allows the user to select and apply
        //     for the control.
        SlicedCircle
    }
    public class SharpBusyIndicator : ContentControl, IDisposable
    {
        //
        // 摘要:
        //     MinAnimationSpeed field.
        private const double MinAnimationSpeed = 10.0;

        //
        // 摘要:
        //     MaxAnimationSpeed field.
        private const double MaxAnimationSpeed = 500.0;

        //
        // 摘要:
        //     Gets or sets the animation types of busyindicator. It is used to change the various
        //     types of animation.
        //
        // 言论：
        //     This BindableProperty is read-only.
        public static readonly DependencyProperty AnimationTypeProperty = DependencyProperty.Register("AnimationType", typeof(AnimationTypes), typeof(SharpBusyIndicator), new PropertyMetadata(AnimationTypes.Flower, OnAnimationTypeChanged));

        //
        // 摘要:
        //     Gets or sets the animation speed of busyindicator. Using this property, you can
        //     change the speed of animation.
        //
        // 言论：
        //     This BindableProperty is read-only.
        public static readonly DependencyProperty AnimationSpeedProperty = DependencyProperty.Register("AnimationSpeed", typeof(double), typeof(SharpBusyIndicator), new PropertyMetadata(115.0, OnAnimationSpeedChanged));

        //
        // 摘要:
        //     Identifies ViewBoxHeight. It is used to change the height of the view box BindableProperty.
        //
        //
        // 言论：
        //     This BindableProperty is read-only.
        public static readonly DependencyProperty ViewboxHeightProperty = DependencyProperty.Register("ViewboxHeight", typeof(double), typeof(SharpBusyIndicator), new PropertyMetadata(0.0));

        //
        // 摘要:
        //     Identifies ViewBoxWidth. It is used to change the width of the view box BindableProperty.
        //
        //
        // 言论：
        //     This BindableProperty is read-only.
        public static readonly DependencyProperty ViewboxWidthProperty = DependencyProperty.Register("ViewboxWidth", typeof(double), typeof(SharpBusyIndicator), new PropertyMetadata(0.0));

        //
        // 摘要:
        //     Identifies the IsBusy property of busy indicator. It is used to determine whether
        //     the animation of indiacator to be shown or not BindableProperty.
        //
        // 言论：
        //     This BindableProperty is read-only.
        public static readonly DependencyProperty IsBusyProperty = DependencyProperty.Register("IsBusy", typeof(bool), typeof(SharpBusyIndicator), new PropertyMetadata(false, OnIsBusyChanged));

        //
        // 摘要:
        //     Identifies the Header property of busy indicator. It is used to give the header
        //     to the Syncfusion.Windows.Controls.Notification.SfBusyIndicator
        //
        // 言论：
        //     This BindableProperty is read-only.
        public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register("Header", typeof(object), typeof(SharpBusyIndicator), new PropertyMetadata(null));

        //
        // 摘要:
        //     Identifies HeaderTemplate. It is used to give the template to the header in Syncfusion.Windows.Controls.Notification.SfBusyIndicator.
        public static readonly DependencyProperty HeaderTemplateProperty = DependencyProperty.Register("HeaderTemplate", typeof(DataTemplate), typeof(SharpBusyIndicator), new PropertyMetadata(null));

        //
        // 摘要:
        //     Indicate whether the Animation Paused or not.
        private Grid layoutRoot;

        //
        // 摘要:
        //     storyBoard field.
        private Storyboard storyBoard;

        //
        // 摘要:
        //     FrameworkElement Content.
        private FrameworkElement content = null;

        //
        // 摘要:
        //     storyBoard field.
        private bool isPaused = false;

        //
        // 摘要:
        //     Gets or sets the AnimationType of the Syncfusion.UI.Xaml.Controls.Notification.SfBusyIndicator
        //
        //
        // 值:
        //     The default value is AnimationTypes.Flower.
        public AnimationTypes AnimationType
        {
            get
            {
                return (AnimationTypes)GetValue(AnimationTypeProperty);
            }
            set
            {
                SetValue(AnimationTypeProperty, value);
            }
        }

        //
        // 摘要:
        //     Gets or sets the Maximum and Minimum AnimationSpeed. Using this property, you
        //     can change the speed of animation.
        public double AnimationSpeed
        {
            get
            {
                return (double)GetValue(AnimationSpeedProperty);
            }
            set
            {
                if (value >= 10.0 && value <= 500.0)
                {
                    SetValue(AnimationSpeedProperty, value);
                }
                else if (value < 10.0)
                {
                    SetValue(AnimationSpeedProperty, 10.0);
                }
                else
                {
                    SetValue(AnimationSpeedProperty, 500.0);
                }
            }
        }

        //
        // 摘要:
        //     Gets or sets the ViewBox Height of the Syncfusion.UI.Xaml.Controls.Notification.SfBusyIndicator
        //
        //
        // 值:
        //     The default value is 0.0.
        public double ViewboxHeight
        {
            get
            {
                return (double)GetValue(ViewboxHeightProperty);
            }
            set
            {
                SetValue(ViewboxHeightProperty, value);
            }
        }

        //
        // 摘要:
        //     Gets or sets the ViewBox Width of the Syncfusion.UI.Xaml.Controls.Notification.SfBusyIndicator
        //
        //
        // 值:
        //     The default value is 0.0.
        public double ViewboxWidth
        {
            get
            {
                return (double)GetValue(ViewboxWidthProperty);
            }
            set
            {
                SetValue(ViewboxWidthProperty, value);
            }
        }

        //
        // 摘要:
        //     Gets or sets a value indicating whether Syncfusion.UI.Xaml.Controls.Notification.SfBusyIndicator
        //     is busy.
        //
        // 值:
        //     true if this instance is selected; otherwise, false.
        public bool IsBusy
        {
            get
            {
                return (bool)GetValue(IsBusyProperty);
            }
            set
            {
                SetValue(IsBusyProperty, value);
            }
        }

        //
        // 摘要:
        //     Gets or sets the Header of the Syncfusion.UI.Xaml.Controls.Notification.SfBusyIndicator
        //
        //
        // 值:
        //     The default value is null.
        public object Header
        {
            get
            {
                return GetValue(HeaderProperty);
            }
            set
            {
                SetValue(HeaderProperty, value);
            }
        }

        //
        // 摘要:
        //     Gets or sets the HeaderTemplate of the Syncfusion.UI.Xaml.Controls.Notification.SfBusyIndicator
        //
        //
        // 值:
        //     The default value is null.
        public DataTemplate HeaderTemplate
        {
            get
            {
                return (DataTemplate)GetValue(HeaderTemplateProperty);
            }
            set
            {
                SetValue(HeaderTemplateProperty, value);
            }
        }

        //
        // 摘要:
        //     Initializes a new instance of the Syncfusion.Windows.Controls.Notification.SfBusyIndicator
        //     class.
        public SharpBusyIndicator()
        {
            base.DefaultStyleKey = typeof(SharpBusyIndicator);
            base.Loaded += SfBusyIndicator_Loaded;
            base.Unloaded += SfBusyIndicator_Unloaded;
            base.IsEnabledChanged += SfBusyIndicator_IsEnabledChanged;
        }

        //
        // 摘要:
        //     Disposes the Syncfusion.UI.Xaml.Controls.Notification.SfBusyIndicator control
        //     while unloading.
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        //
        // 摘要:
        //     Sets the State of Animation type of theSyncfusion.UI.Xaml.Controls.Notification.SfBusyIndicator
        //     control while applying template.
        //
        // 值:
        //     true if this instance is changed; otherwise, false.
        public override void OnApplyTemplate()
        {
            layoutRoot = GetTemplateChild("LayoutRoot") as Grid;
            VisualStateManager.GoToState(this, AnimationType.ToString(), useTransitions: true);
            storyBoard = ((from s in ((layoutRoot == null) ? null : VisualStateManager.GetVisualStateGroups(layoutRoot).OfType<VisualStateGroup>().FirstOrDefault())?.States.OfType<VisualState>()
                           where s.Name == AnimationType.ToString()
                           select s).FirstOrDefault())?.Storyboard;
            if (storyBoard != null)
            {
                storyBoard.SpeedRatio = AnimationSpeed / 100.0;
            }

            if (AnimationType == AnimationTypes.Fluent)
            {
                AnimationSpeed = 70.0;
            }

            base.OnApplyTemplate();
        }

        //
        // 摘要:
        //     Sets the State of Animation type of the argsSyncfusion.UI.Xaml.Controls.Notification.SfBusyIndicator
        //     control.
        //
        // 参数:
        //   args:
        //     DependencyPropertyChangedEventArgs as args
        //
        // 值:
        //     true if this instance is changed; otherwise, false.
        protected virtual void OnAnimationTypeChanged(DependencyPropertyChangedEventArgs args)
        {
            if (args.NewValue != null && args.OldValue != null && args.OldValue.ToString() != args.NewValue.ToString())
            {
                AnimationTypes oldAnimation = (AnimationTypes)args.OldValue;
                storyBoard = ((from s in ((layoutRoot == null) ? null : VisualStateManager.GetVisualStateGroups(layoutRoot).OfType<VisualStateGroup>().FirstOrDefault())?.States.OfType<VisualState>()
                               where s.Name == oldAnimation.ToString()
                               select s).FirstOrDefault())?.Storyboard;
                if (storyBoard != null)
                {
                    storyBoard.Stop(layoutRoot);
                }
            }

            VisualStateManager.GoToState(this, AnimationType.ToString(), useTransitions: true);
            UpdateVisualState();
        }

        //
        // 摘要:
        //     Dispose Method to remove all the instance.
        //
        // 参数:
        //   disposing:
        //     boolean as disposing
        protected virtual void Dispose(bool disposing)
        {
            if (!disposing)
            {
                return;
            }

            base.Loaded -= SfBusyIndicator_Loaded;
            base.Unloaded -= SfBusyIndicator_Unloaded;
            base.IsEnabledChanged -= SfBusyIndicator_IsEnabledChanged;
            if (storyBoard != null)
            {
                storyBoard.Stop();
                storyBoard.Children.Clear();
                storyBoard = null;
            }

            if (HeaderTemplate != null)
            {
                HeaderTemplate = null;
            }

            if (content != null)
            {
                content = null;
            }

            if (Header != null)
            {
                Header = null;
            }

            if (base.Template != null)
            {
                base.Template = null;
            }

            if (layoutRoot == null)
            {
                return;
            }

            VisualStateGroup visualStateGroup = VisualStateManager.GetVisualStateGroups(layoutRoot).OfType<VisualStateGroup>().FirstOrDefault();
            for (int i = 0; i < visualStateGroup.States.Count; i++)
            {
                if (visualStateGroup.States[i] is VisualState visualState)
                {
                    visualState.Storyboard.Stop();
                    visualState.Storyboard.Children.Clear();
                    visualState.Storyboard = null;
                }
            }

            layoutRoot.Children.Clear();
            layoutRoot = null;
        }

        //
        // 摘要:
        //     On property changed
        //
        // 参数:
        //   e:
        //     event args
        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.Property.Name == "Visibility")
            {
                if (e.NewValue.ToString() == "Collapsed")
                {
                    if (layoutRoot != null)
                    {
                        storyBoard.Stop(layoutRoot);
                    }
                }
                else if (IsBusy)
                {
                    if (layoutRoot != null && isPaused && layoutRoot.FindName(AnimationType.ToString()) != null)
                    {
                        storyBoard.Resume(layoutRoot);
                    }
                    else if (!isPaused & (layoutRoot != null))
                    {
                        storyBoard.Begin(layoutRoot, isControllable: true);
                    }
                }
            }

            base.OnPropertyChanged(e);
        }

        //
        // 摘要:
        //     Busy method
        private void BusyMethod()
        {
            if (storyBoard == null)
            {
                return;
            }

            if (IsBusy)
            {
                if (layoutRoot != null && isPaused && layoutRoot.FindName(AnimationType.ToString()) != null)
                {
                    storyBoard.Resume(layoutRoot);
                }
                else if (!isPaused)
                {
                    storyBoard.Begin(layoutRoot, isControllable: true);
                }
            }

            if (!IsBusy && layoutRoot != null)
            {
                storyBoard.Stop(layoutRoot);
            }
        }

        //
        // 摘要:
        //     OnIsBusyChanged Method.
        //
        // 参数:
        //   d:
        //     DependencyObject as d.
        //
        //   e:
        //     DependencyPropertyEvent Argument as e.
        private static void OnIsBusyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SharpBusyIndicator sfBusyIndicator = (SharpBusyIndicator)d;
            sfBusyIndicator.content = sfBusyIndicator.Content as FrameworkElement;
            sfBusyIndicator.BusyMethod();
            if (sfBusyIndicator.content != null)
            {
                if ((bool)e.NewValue)
                {
                    sfBusyIndicator.content.IsEnabled = false;
                }
                else
                {
                    sfBusyIndicator.content.IsEnabled = true;
                }
            }
        }

        //
        // 摘要:
        //     BusyIndicator Enabled Method.
        //
        // 参数:
        //   d:
        //     Dependency Object as d.
        //
        //   e:
        //     DependencyPropertyEvent Argument as e.
        private static void OnAnimationSpeedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SharpBusyIndicator sfBusyIndicator = d as SharpBusyIndicator;
            if (sfBusyIndicator.storyBoard != null)
            {
                sfBusyIndicator.storyBoard.SpeedRatio = sfBusyIndicator.AnimationSpeed / 100.0;
                sfBusyIndicator.UpdateVisualState();
            }
        }

        //
        // 摘要:
        //     OnAnimationTypeChanged Method.
        //
        // 参数:
        //   sender:
        //     DependencyObject as sender.
        //
        //   e:
        //     DependencyPropertyEvent Argument as e.
        private static void OnAnimationTypeChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is SharpBusyIndicator sfBusyIndicator)
            {
                sfBusyIndicator.OnAnimationTypeChanged(e);
            }
        }

        //
        // 摘要:
        //     BusyIndicator UnLoaded Method.
        //
        // 参数:
        //   sender:
        //     object as sender.
        //
        //   e:
        //     RoutedEvent Argument as e.
        private void SfBusyIndicator_Unloaded(object sender, RoutedEventArgs e)
        {
            base.Loaded -= SfBusyIndicator_Loaded;
            base.Unloaded -= SfBusyIndicator_Unloaded;
        }

        //
        // 摘要:
        //     BusyIndicator Loaded Method.
        //
        // 参数:
        //   sender:
        //     object as sender.
        //
        //   e:
        //     RoutedEvent Argument as e.
        private void SfBusyIndicator_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateVisualState();
            if (base.Content is FrameworkElement)
            {
                content = base.Content as FrameworkElement;
            }

            if (IsBusy && content != null)
            {
                content.IsEnabled = false;
            }
        }

        //
        // 摘要:
        //     BusyIndicator Enabled Method.
        //
        // 参数:
        //   sender:
        //     object as sender.
        //
        //   e:
        //     DependencyPropertyEvent Argument as e.
        private void SfBusyIndicator_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            UpdateVisualState();
        }

        //
        // 摘要:
        //     UpdateVisualState Method.
        private void UpdateVisualState()
        {
            storyBoard = ((from s in ((layoutRoot == null) ? null : VisualStateManager.GetVisualStateGroups(layoutRoot).OfType<VisualStateGroup>().FirstOrDefault())?.States.OfType<VisualState>()
                           where s.Name == AnimationType.ToString()
                           select s).FirstOrDefault())?.Storyboard;
            if (!base.IsEnabled && !IsBusy)
            {
                if (layoutRoot != null && storyBoard != null)
                {
                    VisualStateManager.GoToState(this, "Disabled", useTransitions: true);
                    if (layoutRoot.FindName(AnimationType.ToString()) != null && !isPaused)
                    {
                        storyBoard.Begin(layoutRoot, isControllable: true);
                        storyBoard.Pause(layoutRoot);
                        isPaused = true;
                    }
                }
            }
            else
            {
                if (storyBoard == null)
                {
                    return;
                }

                VisualStateManager.GoToState(this, "Normal", useTransitions: true);
                if (layoutRoot.FindName(AnimationType.ToString()) != null)
                {
                    if (isPaused)
                    {
                        storyBoard.Resume(layoutRoot);
                    }
                    else
                    {
                        storyBoard.SpeedRatio = AnimationSpeed / 100.0;
                        storyBoard.Stop(layoutRoot);
                        storyBoard.Begin(layoutRoot, isControllable: true);
                    }
                }

                if (!IsBusy && storyBoard != null)
                {
                    storyBoard.Stop(layoutRoot);
                }

                storyBoard.SpeedRatio = AnimationSpeed / 100.0;
            }
        }
    }
}
