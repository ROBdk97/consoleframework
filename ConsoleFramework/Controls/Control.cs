using ConsoleFramework.Core;
using ConsoleFramework.Events;
using ConsoleFramework.Native;
using ConsoleFramework.Rendering;
using ConsoleFramework.Xaml;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace ConsoleFramework.Controls;

/// <summary>
/// Base class for all controls.
/// </summary>
[DataContextProperty("DataContext")]
public partial class Control : INotifyPropertyChanged
{

    /// <summary>
    /// The part of RenderSlotRect of the control that is currently overlapped
    /// by one or more siblings positioned higher in Z-Order. It is maintained
    /// in an up-to-date state by the layout system in order to detect moments
    /// when a part that was previously hidden by another control becomes visible, and needs
    /// to be updated in the image on the screen. Since the control itself may not be
    /// added to the Invalidation Queue, this must be done automatically.
    /// Defined relative to the buffer of the control itself (not the Parent, like RenderSlotRect).
    /// </summary>
    internal Rect LastOverlappedRect;

    public object DataContext { get; set; }

    private Dictionary<string, object> resources;
    public Dictionary<string, object> Resources
    {
        get { return resources ??= []; }
    }

    internal static RoutedEvent PreviewMouseMoveEvent = EventManager.RegisterRoutedEvent("PreviewMouseMove", RoutingStrategy.Tunnel, typeof(MouseEventHandler), typeof(Control));
    internal static RoutedEvent MouseMoveEvent = EventManager.RegisterRoutedEvent("MouseMove", RoutingStrategy.Bubble, typeof(MouseEventHandler), typeof(Control));
    internal static RoutedEvent PreviewMouseDownEvent = EventManager.RegisterRoutedEvent("PreviewMouseDown", RoutingStrategy.Tunnel, typeof(MouseButtonEventHandler), typeof(Control));
    internal static RoutedEvent MouseDownEvent = EventManager.RegisterRoutedEvent("MouseDown", RoutingStrategy.Bubble, typeof(MouseButtonEventHandler), typeof(Control));
    internal static RoutedEvent PreviewMouseUpEvent = EventManager.RegisterRoutedEvent("PreviewMouseUp", RoutingStrategy.Tunnel, typeof(MouseButtonEventHandler), typeof(Control));
    internal static RoutedEvent MouseUpEvent = EventManager.RegisterRoutedEvent("MouseUp", RoutingStrategy.Bubble, typeof(MouseButtonEventHandler), typeof(Control));
    internal static RoutedEvent PreviewMouseWheelEvent = EventManager.RegisterRoutedEvent("PreviewMouseWheel", RoutingStrategy.Tunnel, typeof(MouseWheelEventHandler), typeof(Control));
    internal static RoutedEvent MouseWheelEvent = EventManager.RegisterRoutedEvent("MouseWheel", RoutingStrategy.Bubble, typeof(MouseWheelEventHandler), typeof(Control));
    internal static RoutedEvent MouseEnterEvent = EventManager.RegisterRoutedEvent("MouseEnter", RoutingStrategy.Direct, typeof(MouseEventHandler), typeof(Control));
    internal static RoutedEvent MouseLeaveEvent = EventManager.RegisterRoutedEvent("MouseLeave", RoutingStrategy.Direct, typeof(MouseEventHandler), typeof(Control));

    internal static RoutedEvent PreviewKeyDownEvent = EventManager.RegisterRoutedEvent("PreviewKeyDown", RoutingStrategy.Tunnel, typeof(KeyEventHandler), typeof(Control));
    internal static RoutedEvent KeyDownEvent = EventManager.RegisterRoutedEvent("KeyDown", RoutingStrategy.Bubble, typeof(KeyEventHandler), typeof(Control));
    internal static RoutedEvent PreviewKeyUpEvent = EventManager.RegisterRoutedEvent("PreviewKeyUp", RoutingStrategy.Tunnel, typeof(KeyEventHandler), typeof(Control));
    internal static RoutedEvent KeyUpEvent = EventManager.RegisterRoutedEvent("KeyUp", RoutingStrategy.Bubble, typeof(KeyEventHandler), typeof(Control));

    internal static RoutedEvent PreviewLostKeyboardFocusEvent = EventManager.RegisterRoutedEvent("PreviewLostKeyboardFocus", RoutingStrategy.Tunnel, typeof(KeyboardFocusChangedEventHandler), typeof(Control));
    internal static RoutedEvent LostKeyboardFocusEvent = EventManager.RegisterRoutedEvent("LostKeyboardFocus", RoutingStrategy.Bubble, typeof(KeyboardFocusChangedEventHandler), typeof(Control));
    internal static RoutedEvent PreviewGotKeyboardFocusEvent = EventManager.RegisterRoutedEvent("PreviewGotKeyboardFocus", RoutingStrategy.Tunnel, typeof(KeyboardFocusChangedEventHandler), typeof(Control));
    internal static RoutedEvent GotKeyboardFocusEvent = EventManager.RegisterRoutedEvent("GotKeyboardFocus", RoutingStrategy.Bubble, typeof(KeyboardFocusChangedEventHandler), typeof(Control));

    public event MouseEventHandler MouseMove
    {
        add => AddHandler(MouseMoveEvent, value);
        remove => RemoveHandler(MouseMoveEvent, value);
    }

    public event MouseButtonEventHandler MouseDown
    {
        add => AddHandler(MouseDownEvent, value);
        remove => RemoveHandler(MouseDownEvent, value);
    }

    public event MouseButtonEventHandler MouseUp
    {
        add => AddHandler(MouseUpEvent, value);
        remove => RemoveHandler(MouseUpEvent, value);
    }

    public event MouseEventHandler MouseEnter
    {
        add => AddHandler(MouseEnterEvent, value);
        remove => RemoveHandler(MouseEnterEvent, value);
    }

    public event MouseEventHandler MouseLeave
    {
        add => AddHandler(MouseLeaveEvent, value);
        remove => RemoveHandler(MouseLeaveEvent, value);
    }

    public event KeyEventHandler KeyDown
    {
        add => AddHandler(KeyDownEvent, value);
        remove => RemoveHandler(KeyDownEvent, value);
    }

    public event KeyEventHandler KeyUp
    {
        add => AddHandler(KeyUpEvent, value);
        remove => RemoveHandler(KeyUpEvent, value);
    }

    public event KeyboardFocusChangedEventHandler LostKeyboardFocus
    {
        add => AddHandler(LostKeyboardFocusEvent, value);
        remove => RemoveHandler(LostKeyboardFocusEvent, value);
    }

    public event KeyboardFocusChangedEventHandler GotKeyboardFocus
    {
        add => AddHandler(GotKeyboardFocusEvent, value);
        remove => RemoveHandler(GotKeyboardFocusEvent, value);
    }

    //        public void SetFocus() {
    //            ConsoleApplication.Instance.FocusManager.SetFocus(this);
    //        }

    /// <summary>
    /// Does the current control currently have focus (i.e., receives keyboard input)
    /// </summary>
    public bool HasFocus
    {
        get
        {
            return ConsoleApplication.Instance.FocusManager.FocusedElement == this;
        }
    }

    public void AddHandler(RoutedEvent routedEvent, Delegate @delegate)
    {
        EventManager.AddHandler(this, routedEvent, @delegate);
    }

    public void AddHandler(RoutedEvent routedEvent, Delegate @delegate, bool handledEventsToo)
    {
        EventManager.AddHandler(this, routedEvent, @delegate, handledEventsToo);
    }

    /// <summary>
    /// Addes specified routed event to event queue. This event will be processed in next pass.
    /// </summary>
    public static void RaiseEvent(RoutedEvent routedEvent, RoutedEventArgs args)
    {
        ArgumentNullException.ThrowIfNull(routedEvent);
        ArgumentNullException.ThrowIfNull(args);

        ConsoleApplication.Instance.EventManager.QueueEvent(routedEvent, args);
    }

    public void RemoveHandler(RoutedEvent routedEvent, Delegate @delegate)
    {
        EventManager.RemoveHandler(this, routedEvent, @delegate);
    }

    public T FindChildByName<T>(string name) where T : Control
    {
        return (T)VisualTreeHelper.FindChildByName(this, name);
    }

    public Control FindDirectChildByName(string name)
    {
        return Children.FirstOrDefault(control => control.Name == name);
    }

    public T FindDirectChildByName<T>(string name) where T : Control
    {
        return (T)FindDirectChildByName(name);
    }

    internal LayoutInfo layoutInfo = new();
    internal LayoutInfo lastLayoutInfo = new();

    private Visibility visibility;

    public Visibility Visibility
    {
        get { return visibility; }
        set
        {
            if (visibility != value)
            {
                visibility = value;
                Invalidate();
            }
        }
    }

    /// <summary>
    /// Just for debug.
    /// </summary>
    public Size? MeasureArgument
    {
        get
        {
            return layoutInfo.validity != LayoutValidity.Nothing ? (Size?)layoutInfo.measureArgument : null;
        }
    }

    /// <summary>
    /// Name of control. If set, it should be unique for siblings to avoid
    /// ambiguities when searching by name.
    /// </summary>
    public string Name
    {
        get;
        set;
    }

    /// <summary>
    /// Read-only collection of children controls ordered by Z-Order.
    /// (Last items will be on top.)
    /// </summary>
    protected internal IList<Control> Children;

    /// <summary>
    /// Collection of children controls.
    /// </summary>
    private readonly List<Control> children = [];

    /// <summary>
    /// Parent of current control in visual tree.
    /// </summary>
    public Control Parent
    {
        get;
        private set;
    }

    /// <summary>
    /// Called when control is added to some parent or removed from.
    /// Default implementation does nothing.
    /// </summary>
    protected virtual void OnParentChanged()
    {
    }

    private void attachedToRootElement()
    {
        attachedToVisualTree = true;
        foreach (Control child in Children)
        {
            child.attachedToRootElement();
        }
    }

    private void detachedFromRootElement()
    {
        attachedToVisualTree = false;
        foreach (Control child in Children)
        {
            child.detachedFromRootElement();
        }
    }

    /// <summary>
    /// Called by ConsoleApplication when Run() initializes the root element
    /// to init attached-detached system to be consistent.
    /// </summary>
    internal void ControlSetAsRootElement()
    {
        attachedToRootElement();
    }

    internal void ControlUnsetAsRootElement()
    {
        detachedFromRootElement();
    }

    private void parentChanged()
    {
        if (Parent == null)
        {
            detachedFromRootElement();
        }
        else
        {
            if (Parent.attachedToVisualTree) attachedToRootElement();
            else detachedFromRootElement();
        }

        OnParentChanged();
    }

    private bool attachedToVisualTree;

    protected void InsertChildAt(int index, Control child)
    {
        ArgumentNullException.ThrowIfNull(child);
        if (null != child.Parent)
            throw new ArgumentException("Specified child already has parent.");
        children.Insert(index, child);
        child.Parent = this;
        child.parentChanged();
        child.Invalidate();
        Invalidate();
    }

    protected void AddChild(Control child)
    {
        ArgumentNullException.ThrowIfNull(child);
        if (null != child.Parent)
            throw new ArgumentException("Specified child already has parent.");
        children.Add(child);
        child.Parent = this;
        child.parentChanged();
        child.Invalidate();
        Invalidate();
    }

    protected void RemoveChild(Control child)
    {
        ArgumentNullException.ThrowIfNull(child);
        if (child.Parent != this)
            throw new InvalidOperationException("Specified control is not a child.");
        else
        {
            ConsoleApplication.Instance.FocusManager.BeforeRemoveElementFromTree(child);
            if (!children.Remove(child))
                throw new InvalidOperationException("Assertion failed.");
            child.Parent = null;

            // Remove it from invalidation queue if already added
            ConsoleApplication.Instance.Renderer.ControlRemovedFromTree(child);

            child.parentChanged();

            Invalidate();
        }
    }

    /// <summary>
    /// Swaps controls z-order by specified indexes.
    /// </summary>
    /// <param name="a">Index of first child</param>
    /// <param name="b">Index of second child</param>
    protected void SwapChildsZOrder(int a, int b)
    {
        if (a < 0 || a >= children.Count) throw new ArgumentException("Incorrect index", nameof(a));
        if (b < 0 || b >= children.Count) throw new ArgumentException("Incorrect index", nameof(b));
        if (a == b) return;

        (children[b], children[a]) = (children[a], children[b]);

        // Add this to zorderCheckControls list
        ConsoleApplication.Instance.Renderer.AddControlToZOrderCheckList(this);
    }

    private void Control_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs args)
    {
        // Focusable controls invalidated automatically when aquire focus
        if (Focusable)
            Invalidate();
    }

    /// <summary>
    /// If one of the child controls of the window loses focus, this handler will be called.
    /// </summary>
    private void Control_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs args)
    {
        // If the current control is a FocusScope, we need to save the element
        // that had focus, to restore it when the FocusScope receives it back
        StoredFocus = IsFocusScope ? args.OldFocus : null;

        // Focusable controls invalidated automatically when lose focus
        if (Focusable)
            Invalidate();
    }

    public Control()
    {
        Children = children.AsReadOnly();
        MinWidth = 0;
        Focusable = false;
        IsFocusScope = false;
        Visibility = Visibility.Visible;
        AddHandler(GotKeyboardFocusEvent, new KeyboardFocusChangedEventHandler(Control_GotKeyboardFocus));
        AddHandler(LostKeyboardFocusEvent, new KeyboardFocusChangedEventHandler(Control_LostKeyboardFocus));
    }

    /// <summary>
    /// The offset of the control's virtual canvas relative to the parent element's canvas.
    /// If the control is completely placed in the parent element and is not clipped by margin,
    /// then ActualOffset is numerically equal to RenderSlotRect.Location. If part of the control is hidden, then
    /// ActualOffset differs from RenderSlotRect.Location.
    /// Takes into account <see cref="Margin"/>, <see cref="HorizontalAlignment"/> and <see cref="VerticalAlignment"/>.
    /// </summary>
    public Vector ActualOffset
    {
        get
        {
            return layoutInfo.actualOffset;
        }
        private set
        {
            layoutInfo.actualOffset = value;
        }
    }

    /// <summary>
    /// Changes layout validity to Render and returns true if this control
    /// should be treated as layout revalidated. (Returns true if layout validity 
    /// has actually changed to Render and there are some subscribers to LayoutRevalidated event).
    /// </summary>
    internal bool SetValidityToRender()
    {
        if (layoutInfo.validity != LayoutValidity.Render)
        {
            layoutInfo.validity = LayoutValidity.Render;

            return (LayoutRevalidated != null);
        }
        return false;
    }

    internal void RaiseInvalidatedEvent()
    {
        LayoutInvalidated?.Invoke(this, EventArgs.Empty);
    }

    internal void RaiseRevalidatedEvent()
    {
        LayoutRevalidated?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Called when the Invalidate() method is called on the control.
    /// </summary>
    public event EventHandler Invalidated;

    /// <summary>
    /// When the LayoutValidity state of the control is reset by the ResetValidity method.
    /// This happens when Renderer.InvalidateLayout() is called, not immediately after calling
    /// control.Invalidate() - the latter just adds it to the queue.
    /// It is not recommended to set handlers for this event for long periods of time, as it can lead to
    /// performance degradation. It's better to unsubscribe as soon as the handler
    /// is no longer needed.
    /// </summary>
    public event EventHandler LayoutInvalidated;

    /// <summary>
    /// When the LayoutValidity state becomes Render.
    /// It is not recommended to set handlers for this event for long periods of time, as it can lead to
    /// performance degradation. It's better to unsubscribe as soon as the handler
    /// is no longer needed.
    /// </summary>
    public event EventHandler LayoutRevalidated;

    public int ActualWidth => RenderSize.Width;

    public int ActualHeight => RenderSize.Height;

    public int MinWidth
    {
        get;
        set;
    }

    public int MaxWidth { get; set; } = int.MaxValue;

    public int MinHeight
    {
        get;
        set;
    }

    /// <summary>
    /// Shows whether control can handle keyboard input or can't.
    /// </summary>
    public bool Focusable
    {
        get;
        set;
    }

    /// <summary>
    /// Specifies the order for keyboard focus move.
    /// Takes 0 by default.
    /// </summary>
    public int TabOrder
    {
        get;
        set;
    }

    /// <summary>
    /// Indicates whether the control can act as a FocusScope.
    /// </summary>
    public bool IsFocusScope { get; set; }

    public int MaxHeight { get; set; } = int.MaxValue;

    public int? Width
    {
        get;
        set;
    }

    public int? Height
    {
        get;
        set;
    }

    public Thickness Margin
    {
        get;
        set;
    }

    public Size DesiredSize
    {
        get => layoutInfo.desiredSize;
        private set => layoutInfo.desiredSize = value;
    }

    private struct MinMax
    {
        /// <summary>
        /// Defines the effective constraints for the current MinHeight/MaxHeight, MinWidth/MaxWidth
        /// and Width/Height values. Min-values cannot be null, default to zero, and cannot be int.MaxValue.
        /// Max-values also cannot be null, default to int.MaxValue.
        /// Width and Height can be unset - in this case the control will occupy as much available
        /// space as possible.
        /// In case of a conflict, priority goes to the Min-property, then explicitly set value (Width or Height),
        /// and finally the Max-property takes effect.
        /// </summary>
        internal MinMax(int minHeight, int maxHeight, int minWidth, int maxWidth, int? width, int? height)
        {
            this.maxHeight = maxHeight;
            this.minHeight = minHeight;
            int? l = height;

            int tmp_height = l ?? int.MaxValue;
            this.maxHeight = Math.Max(Math.Min(tmp_height, this.maxHeight), this.minHeight);

            tmp_height = l ?? 0;
            this.minHeight = Math.Max(Math.Min(this.maxHeight, tmp_height), this.minHeight);

            this.maxWidth = maxWidth;
            this.minWidth = minWidth;
            l = width;

            int tmp_width = l ?? int.MaxValue;
            this.maxWidth = Math.Max(Math.Min(tmp_width, this.maxWidth), this.minWidth);

            tmp_width = l ?? 0;
            this.minWidth = Math.Max(Math.Min(this.maxWidth, tmp_width), this.minWidth);
        }

        internal readonly int minWidth;
        internal readonly int maxWidth;
        internal readonly int minHeight;
        internal readonly int maxHeight;
    }

    /// <summary>
    /// Get the sum of a and b, but
    /// treats int.MaxValue as PositiveInf, and int.MinValue as NegativeInf
    /// 
    /// int.MaxValue + const = int.MaxValue
    /// int.MinValue + const = int.MinValue
    /// int.MaxValue + int.MaxValue = int.MaxValue
    /// int.MaxValue + int.MinValue = Exception
    /// </summary>
    public static int SumWithInf(int a, int b)
    {
        if (a == int.MaxValue || a == int.MinValue)
        {
            assert(b != MinusWithInf(a));
            return a;
        }
        if (b == int.MaxValue || b == int.MinValue)
        {
            assert(a != MinusWithInf(b));
            return a;
        }
        int result = a + b;
        // Check case when sum transforms into one of the "special" values
        assert(result != int.MinValue && result != int.MaxValue);
        return result;
    }

    /// <summary>
    /// Gets the -v but treats int.MaxValue as PositiveInf and int.MinValue as NegativeInf respectively.
    /// Throws exception if v == -int.MinValue (doesn't return int.MaxValue in this case)
    /// </summary>
    /// <param name="v"></param>
    /// <returns></returns>
    public static int MinusWithInf(int v)
    {
        switch (v)
        {
            case int.MaxValue:
                return int.MinValue;
            case int.MinValue:
                return int.MaxValue;
            default:
                int result = -v;
                // Check case when -v transforms into one of the "special" values
                assert(result != int.MinValue && result != int.MaxValue);
                return result;
        }
    }

    public void Measure(Size availableSize)
    {
        if (availableSize.Width < 0 || availableSize.Height < 0)
        {
            throw new ArgumentException("Negative width/height is not allowed");
        }
        if (layoutInfo.validity != LayoutValidity.Nothing)
        {
            return;
        }

        layoutInfo.measureArgument = availableSize;

        if (Visibility == Visibility.Collapsed)
        {
            layoutInfo.unclippedDesiredSize = Size.Empty;
            DesiredSize = Size.Empty;
            return;
        }

        // apply margin
        Thickness margin = Margin;
        int marginWidth = margin.Left + margin.Right;
        int marginHeight = margin.Top + margin.Bottom;

        //  parent size is what parent want us to be
        Size frameworkAvailableSize = new(
            Math.Max(SumWithInf(availableSize.Width, -marginWidth), 0),
            Math.Max(SumWithInf(availableSize.Height, -marginHeight), 0));

        // apply min/max/currentvalue constraints
        MinMax mm = new(MinHeight, MaxHeight, MinWidth, MaxWidth, Width, Height);

        frameworkAvailableSize.Width = Math.Max(mm.minWidth, Math.Min(frameworkAvailableSize.Width, mm.maxWidth));
        frameworkAvailableSize.Height = Math.Max(mm.minHeight, Math.Min(frameworkAvailableSize.Height, mm.maxHeight));

        Size desiredSize = MeasureOverride(frameworkAvailableSize);
        if (desiredSize.Width == int.MaxValue || desiredSize.Height == int.MaxValue)
        {
            throw new InvalidOperationException("MeasureOverride should not return int.MaxValue even for" +
                                                "availableSize = {int.MaxValue, int.MaxValue} argument.");
        }

        //  maximize desiredSize with user provided min size
        desiredSize = new Size(
            Math.Max(desiredSize.Width, mm.minWidth),
            Math.Max(desiredSize.Height, mm.minHeight));

        //here is the "true minimum" desired size - the one that is
        //for sure enough for the control to render its content.
        Size unclippedDesiredSize = desiredSize;

        // User-specified max size starts to "clip" the control here. 
        //Starting from this point desiredSize could be smaller then actually
        //needed to render the whole control
        if (desiredSize.Width > mm.maxWidth)
        {
            desiredSize.Width = mm.maxWidth;
        }

        if (desiredSize.Height > mm.maxHeight)
        {
            desiredSize.Height = mm.maxHeight;
        }

        //  because of negative margins, clipped desired size may be negative.
        //  need to keep it as doubles for that reason and maximize with 0 at the 
        //  very last point - before returning desired size to the parent. 
        int clippedDesiredWidth = desiredSize.Width + marginWidth;
        int clippedDesiredHeight = desiredSize.Height + marginHeight;

        // In overconstrained scenario, parent wins and measured size of the child,
        // including any sizes set or computed, can not be larger then
        // available size. We will clip the guy later. 
        if (clippedDesiredWidth > availableSize.Width)
        {
            clippedDesiredWidth = availableSize.Width;
        }

        if (clippedDesiredHeight > availableSize.Height)
        {
            clippedDesiredHeight = availableSize.Height;
        }

        //  Note: unclippedDesiredSize is needed in ArrangeCore,
        //  because due to the layout protocol, arrange should be called 
        //  with constraints greater or equal to child's desired size
        //  returned from MeasureOverride.
        layoutInfo.unclippedDesiredSize = unclippedDesiredSize;

        DesiredSize = new Size(Math.Max(0, clippedDesiredWidth), Math.Max(0, clippedDesiredHeight));

        if (DesiredSize.Width == int.MaxValue || DesiredSize.Height == int.MaxValue)
        {
            throw new Exception("Desired size cannot have int.MaxValue width/height");
        }
    }

    /// <summary>
    /// Returns the size needed to accommodate the control along with its child elements.
    /// <para>
    /// If the returned size is smaller than availableSize, this can be taken into account by the parent control,
    /// and it can allocate a slot smaller than originally planned. Or it may be
    /// ignored, in which case the control will be placed in a slot larger than actually
    /// needed by the control. The control should account for this scenario (if its
    /// actual placement exceeds expectations).
    /// </para>
    /// <para>
    /// If the returned size is larger than availableSize, then there are again 2 possible outcomes.
    /// In the first case, the parent control according to its layout logic may try to find
    /// additional space for the control and call Measure again with a larger availableSize.
    /// Or, if such logic is not provided in the parent control's layout algorithm,
    /// or if there is no space, - the returned desired size will be recorded in unclipped desired size and at the
    /// arrange stage the control will be placed in the desired size, but it will not actually occupy that much space,
    /// and in the context of the parent control its rendering will be clipped.
    /// </para>
    /// <para>
    /// You cannot return int.MaxValue as the width or height of the returned size.
    /// </para>
    /// <para>
    /// When implementing panels, it is mandatory to call Measure for all child
    /// elements, and the number of calls can be any, but the last call for each control
    /// must be executed with the sizes that will actually be used
    /// when placing the element.
    /// </para>
    /// </summary>
    protected virtual Size MeasureOverride(Size availableSize)
    {
        return new Size(0, 0);
    }

    /// <summary>
    /// Places the control along with child controls in the specified slot.
    /// After the method completes, all properties necessary for rendering the control will be set.
    /// If Arrange was called with sizes smaller than those returned by the control in
    /// MeasureOverride, then the control's rendering will be clipped.
    /// <para>
    /// If Arrange was called with sizes exceeding those requested in MeasureOverride, then
    /// the slot allocated to the control will be larger than expected, and how the additional
    /// space will be used depends on the control's ArrangeOverride logic. If ArrangeOverride returns an older value
    /// (smaller than the current finalSize), then RenderSize will be smaller than RenderSlotRect, and part of the space
    /// allocated for placing the control element will simply not be used by it.
    /// </para>
    /// <para>
    /// Warning! If you implement panel logic in the ArrangeOverride method code, it is very important that
    /// you monitor what arguments you pass to the Arrange method for child elements. If you pass sizes exceeding those
    /// that will be returned from the parent control element's ArrangeOverride, it will mean the following: the parent
    /// control allocates the child more space than available (or exactly all the space), and the child control when rendering will
    /// completely overwrite the parent control element. Of course, it will not go beyond the parent control's slot boundaries
    /// (it will be clipped by the rendering system), but it will also not allow the parent control to draw anything.
    /// </para>
    /// </summary>
    public void Arrange(Rect finalRect)
    {
        if (layoutInfo.validity != LayoutValidity.Nothing) return;

        if (Visibility == Visibility.Collapsed)
        {
            RenderSlotRect = Rect.Empty;
            RenderSize = Size.Empty;
            layoutInfo.layoutClip = calculateLayoutClip();
            layoutInfo.validity = LayoutValidity.MeasureAndArrange;
            return;
        }

        RenderSlotRect = finalRect;

        // If LayoutConstrained==true (parent wins in layout),
        // we might get finalRect.Size smaller then UnclippedDesiredSize. 
        // Stricltly speaking, this may be the case even if LayoutConstrained==false (child wins),
        // since who knows what a particualr parent panel will try to do in error.
        // In this case we will not actually arrange a child at a smaller size,
        // since the logic of the child does not expect to receive smaller size 
        // (if it coudl deal with smaller size, it probably would accept it in MeasureOverride)
        // so lets replace the smaller arreange size with UnclippedDesiredSize 
        // and then clip the guy later. 
        // We will use at least UnclippedDesiredSize to compute arrangeSize of the child, and
        // we will use layoutSlotSize to compute alignments - so the bigger child can be aligned within 
        // smaller slot.

        // Start to compute arrange size for the child. 
        // It starts from layout slot or deisred size if layout slot is smaller then desired, 
        // and then we reduce it by margins, apply Width/Height etc, to arrive at the size
        // that child will get in its ArrangeOverride. 
        Size arrangeSize = finalRect.Size;

        Thickness margin = Margin;
        int marginWidth = margin.Left + margin.Right;
        int marginHeight = margin.Top + margin.Bottom;

        arrangeSize.Width = Math.Max(0, arrangeSize.Width - marginWidth);
        arrangeSize.Height = Math.Max(0, arrangeSize.Height - marginHeight);

        // Next, compare against unclipped, transformed size.
        Size unclippedDesiredSize = layoutInfo.unclippedDesiredSize;

        if (arrangeSize.Width < unclippedDesiredSize.Width)
        {
            arrangeSize.Width = unclippedDesiredSize.Width;
        }

        if (arrangeSize.Height < unclippedDesiredSize.Height)
        {
            arrangeSize.Height = unclippedDesiredSize.Height;
        }

        // Alignment==Stretch --> arrange at the slot size minus margins
        // Alignment!=Stretch --> arrange at the unclippedDesiredSize 
        if (HorizontalAlignment != HorizontalAlignment.Stretch)
        {
            arrangeSize.Width = unclippedDesiredSize.Width;
        }

        if (VerticalAlignment != VerticalAlignment.Stretch)
        {
            arrangeSize.Height = unclippedDesiredSize.Height;
        }

        //Here we use un-clipped InkSize because element does not know that it is
        //clipped by layout system and it should have as much space to render as
        //it returned from its own ArrangeOverride 
        RenderSize = ArrangeOverride(arrangeSize);

        Vector offset = computeAlignmentOffset();

        offset.X += finalRect.X + margin.Left;
        offset.Y += finalRect.Y + margin.Top;

        if (!ActualOffset.Equals(offset))
        {
            ActualOffset = offset;
        }

        layoutInfo.layoutClip = calculateLayoutClip();

        layoutInfo.validity = LayoutValidity.MeasureAndArrange;
    }

    public HorizontalAlignment HorizontalAlignment
    {
        get;
        set;
    }

    public VerticalAlignment VerticalAlignment
    {
        get;
        set;
    }

    /// <summary>
    /// The size for which the control will render its content.
    /// Can be larger than RenderSlotRect due to cases where the control doesn't fit within the boundaries
    /// allocated by the Arrange method. The control will be clipped by the layout system in accordance with RenderSlotRect.
    /// </summary>
    public Size RenderSize
    {
        get => layoutInfo.renderSize;
        private set => layoutInfo.renderSize = value;
    }

    /// <summary>
    /// The slot allocated by the parent control element for rendering.
    /// Set by the argument when calling <see cref="Arrange"/>.
    /// </summary>
    public Rect RenderSlotRect
    {
        get => layoutInfo.renderSlotRect;
        private set => layoutInfo.renderSlotRect = value;
    }

    private Rect calculateLayoutClip()
    {
        Vector offset = computeAlignmentOffset();
        Size clientSize = getClientSize();
        var layoutClip = new Rect(-offset.X, -offset.Y, clientSize.Width, clientSize.Height);
        return applyMaxConstraints(layoutClip);
    }

    internal Rect applyMaxConstraints(Rect layoutClip)
    {
        // If MaxWidth/Height constraints are specified, then from the visible part of layoutClip
        // we keep in the TopLeft corner only what fits in Max. TopLeft is chosen because
        // in the calculation in computeAlignmentOffset() we already expected such an outcome
        // (see the comment inside computeAlignmentOffset)
        var visibleLayoutClip = Rect.Intersect(new Rect(RenderSize), layoutClip);
        MinMax mm = new(MinHeight, MaxHeight, MinWidth, MaxWidth, Width, Height);

        return new Rect(visibleLayoutClip.TopLeft, new Size(
            Math.Min(visibleLayoutClip.Width, mm.maxWidth),
            Math.Min(visibleLayoutClip.Height, mm.maxHeight)));
    }

    /// <summary>
    /// A rectangle inside the virtual canvas of the control where graphics will be output.
    /// Everything else will be clipped in accordance with the set values of the properties
    /// <see cref="Margin"/>, <see cref="HorizontalAlignment"/> and <see cref="VerticalAlignment"/>.
    /// </summary>
    public Rect LayoutClip => layoutInfo.layoutClip;

    private Vector computeAlignmentOffset()
    {
        //
        MinMax mm = new(MinHeight, MaxHeight, MinWidth, MaxWidth, Width, Height);

        Size renderSize = RenderSize;

        //clippedInkSize differs from InkSize only what MaxWidth/Height explicitly clip the
        //otherwise good arrangement. For ex, DS<clientSize but DS>MaxWidth - in this
        //case we should initiate clip at MaxWidth and only show Top-Left portion 
        //of the element limited by Max properties. It is Top-left because in case when we
        //are clipped by container we also degrade to Top-Left, so we are consistent. 
        Size clippedInkSize = new(Math.Min(renderSize.Width, mm.maxWidth),
                                       Math.Min(renderSize.Height, mm.maxHeight));
        Size clientSize = getClientSize();

        return computeAlignmentOffsetCore(clientSize, clippedInkSize);
    }

    // The client size is the size of layout slot decreased by margins. 
    // This is the "window" through which we see the content of the child.
    // Alignments position ink of the child in this "window".
    // Max with 0 is neccessary because layout slot may be smaller then unclipped desired size.
    private Size getClientSize()
    {
        Thickness margin = Margin;
        int marginWidth = margin.Left + margin.Right;
        int marginHeight = margin.Top + margin.Bottom;

        Rect renderSlotRect = RenderSlotRect;

        return new Size(Math.Max(0, renderSlotRect.Width - marginWidth),
                        Math.Max(0, renderSlotRect.Height - marginHeight));
    }

    internal Vector computeAlignmentOffsetCore(Size clientSize, Size inkSize)
    {
        Vector offset = new();

        HorizontalAlignment ha = HorizontalAlignment;
        VerticalAlignment va = VerticalAlignment;

        //this is to degenerate Stretch to Top-Left in case when clipping is about to occur
        //if we need it to be Center instead, simply remove these 2 ifs
        if (ha == HorizontalAlignment.Stretch
            && inkSize.Width > clientSize.Width)
        {
            ha = HorizontalAlignment.Left;
        }

        if (va == VerticalAlignment.Stretch
            && inkSize.Height > clientSize.Height)
        {
            va = VerticalAlignment.Top;
        }
        //end of degeneration of Stretch to Top-Left 

        if (ha == HorizontalAlignment.Center
            || ha == HorizontalAlignment.Stretch)
        {
            offset.X = (clientSize.Width - inkSize.Width) / 2;
        }
        else if (ha == HorizontalAlignment.Right)
        {
            offset.X = clientSize.Width - inkSize.Width;
        }
        else
        {
            offset.X = 0;
        }

        if (va == VerticalAlignment.Center
            || va == VerticalAlignment.Stretch)
        {
            offset.Y = (clientSize.Height - inkSize.Height) / 2;
        }
        else if (va == VerticalAlignment.Bottom)
        {
            offset.Y = clientSize.Height - inkSize.Height;
        }
        else
        {
            offset.Y = 0;
        }

        return offset;
    }

    /// <summary>
    /// The default implementation of <see cref="ArrangeOverride"/> returns the original finalSize.
    /// The default behavior is suitable for all leaf controls (since they don't need to
    /// arrange child elements and their sizes depend only on their own
    /// content). However, for panels, a proper implementation of this method is necessary for
    /// correct and coordinated operation of the layout system.
    /// 
    /// In this method, the child control should arrange all child elements,
    /// calling the <see cref="Arrange"/> method for each of them, and return the size,
    /// which the control with children actually occupied as a result of the arrangement operation. If the control occupied
    /// less space but returned more (for example, just did return finalSize, even though it occupied
    /// less space), then the control will have empty free space, and it will need to be
    /// filled with graphics. Accordingly, when Alignment = Stretch the control will occupy all
    /// available space. If instead you return the real size, then when Alignment = Stretch and
    /// there is excessive space the control will be centered automatically (at the center of RenderSlot
    /// provided by the parent element). If a child element should not
    /// be shown, you need to call <see cref="Arrange"/> on it with an empty rectangle as an argument,
    /// otherwise the old value will remain, and there will likely be garbage in the control's buffer.
    /// 
    /// The value returned by this method is set as the <see cref="RenderSize"/>
    /// of the control, and <see cref="ActualWidth"/> and <see cref="ActualHeight"/> after that
    /// return exactly it.
    /// <param name="finalSize">The final area within the parent that this element
    /// should use to arrange itself and its children.</param>
    /// <returns>The actual size used.</returns>
    /// </summary>
    protected virtual Size ArrangeOverride(Size finalSize)
    {
        return finalSize;
    }

    /// <summary>
    /// Returns a list of controls for which the ResetValidity method was called and which
    /// have subscribers to the LayoutInvalidated event. That is, if a control had the ResetValidity method called,
    /// but it has no subscribers to this event, it should not be in this list.
    /// </summary>
    /// <returns></returns>
    internal void ResetValidity(List<Control> affectedControls)
    {
        // Copy all calculated layout info into lastLayoutInfo
        if (layoutInfo.validity == LayoutValidity.Render)
        {
            lastLayoutInfo.CopyValuesFrom(layoutInfo);
        }
        // Clear layoutInfo.validity (and whole layoutInfo structure to avoid garbage data)
        layoutInfo.ClearValues();

        // Make copy of children collection to avoid troubles with
        // changed children in LayoutInvalidated handlers
        List<Control> childrenCopy = [.. Children];

        // Raise Invalidated event
        if (LayoutInvalidated != null)
        {
            affectedControls.Add(this);
        }

        // Recursively invalidate children, but without add them to queue
        foreach (Control child in childrenCopy)
        {
            child.ResetValidity(affectedControls);
        }
    }

    /// <summary>
    /// Adds this control to the update queue. At the next execution of the UI
    /// update cycle the layout system will call Measure, Arrange and Render. Child controls
    /// whose sizes have not changed will not be redrawn. The same applies to parent controls
    /// if the size of this control has not changed. But this control will be redrawn
    /// on the screen without fail, along with all children, even if nothing has changed in it.
    /// </summary>
    public void Invalidate()
    {
        if (attachedToVisualTree)
        {
            ConsoleApplication.Instance.Renderer.AddControlToInvalidationQueue(this);
            Invalidated?.Invoke(this, EventArgs.Empty);
        }
    }

    public virtual Control GetTopChildAtPoint(Point point)
    {
        return (from child in Children
                where child.RenderSlotRect.Contains(point)
                select child).FirstOrDefault();
    }

    /// <summary>
    /// Translates a point from the coordinate system of source to the coordinate system of dest.
    /// You can specify null for source and dest, in which case the coordinate system
    /// will be the console screen coordinate system.
    /// </summary>
    /// <param name="source">Control relative to which the point is specified, or null if the coordinates are global.</param>
    /// <param name="point">Coordinates of the point relative to source.</param>
    /// <param name="dest">Control relative to which the coordinates of the point need to be calculated.</param>
    /// <returns></returns>
    public static Point TranslatePoint(Control source, Point point, Control dest)
    {
        if (source == null || dest == null)
        {
            if (source == null && dest != null)
            {
                // translating raw point (absolute coords) into relative to dest control point
                Control currentControl = dest;
                for (; ; )
                {
                    Vector actualOffset = currentControl.ActualOffset;
                    point.Offset(-actualOffset.X, -actualOffset.y);
                    if (currentControl.Parent == null)
                    {
                        break;
                    }
                    currentControl = currentControl.Parent;
                }
                return point;
            }
            else if (source != null && dest == null)
            {
                // translating point relative to source into absolute coords
                Control currentControl = source;
                for (; ; )
                {
                    Vector actualOffset = currentControl.ActualOffset;
                    point.Offset(actualOffset.X, actualOffset.y);
                    if (currentControl.Parent == null)
                        break;
                    currentControl = currentControl.Parent;
                }
                return point;
            }
            else
            {
                // both source and dest are null - we shouldn't to do anything
                return point;
            }
        }
        else
        {
            // find common ancestor
            Control ancestor = FindCommonAncestor(source, dest);
            // traverse back from source to common ancestor
            Control currentControl = source;
            while (currentControl != ancestor)
            {
                Vector actualOffset = currentControl.ActualOffset;
                point.Offset(actualOffset.X, actualOffset.y);
                currentControl = currentControl.Parent;
            }
            // traverse back from dest to common ancestor
            currentControl = dest;
            while (currentControl != ancestor)
            {
                Vector actualOffset = currentControl.ActualOffset;
                point.Offset(-actualOffset.X, -actualOffset.y);
                currentControl = currentControl.Parent;
            }
            return point;
        }
    }

    /// <summary>
    /// Returns common ancestor for specified controls pair.
    /// If there are no common ancestor found, null will be returned.
    /// But this situation is impossible because there are only one main control in application.
    /// </summary>
    public static Control FindCommonAncestor(Control a, Control b)
    {
        ArgumentNullException.ThrowIfNull(a);
        ArgumentNullException.ThrowIfNull(b);
        //
        List<Control> visited = [];
        Control refA = a;
        Control refB = b;
        bool f = true;
        for (; ; )
        {
            if (refA == refB)
                return refA;
            if (visited.Contains(refB))
                return refB;
            if (visited.Contains(refA))
                return refA;
            if (refA.Parent == null && refB.Parent == null)
                return null;
            if (f)
            {
                if (refA.Parent != null)
                {
                    visited.Add(refA);
                    refA = refA.Parent;
                }
            }
            else
            {
                if (refB.Parent != null)
                {
                    visited.Add(refB);
                    refB = refB.Parent;
                }
            }
            f = !f;
        }
    }

    public override string ToString()
    {
        return string.Format("{0}: {1}", GetType(), Name);
    }

    /// <summary>
    /// Checks if the point is not overlapped by other controls.
    /// </summary>
    /// <param name="rawPoint"></param>
    /// <returns>True if point is on control, otherwise false.</returns>
    public bool HitTest(Point rawPoint)
    {
        Point point = TranslatePoint(null, rawPoint, Parent);
        // hit testing - calculate position in child according to specified layout attributes
        Vector actualOffset = ActualOffset;
        Rect renderSlotRect = RenderSlotRect;
        Rect virtualSlotRect = new(new Point(actualOffset.x, actualOffset.y), RenderSize);
        if (!LayoutClip.IsEmpty)
        {
            Rect layoutClip = LayoutClip;
            Point location = layoutClip.Location;
            location.Offset(actualOffset.x, actualOffset.y);
            layoutClip.Location = location;
            virtualSlotRect.Intersect(layoutClip);
        }
        virtualSlotRect.Intersect(renderSlotRect);
        return virtualSlotRect.Contains(point);
    }

    /// <summary>
    /// Performs hit testing to a visible part of child control.
    /// Static version of method.
    /// </summary>
    /// <param name="rawPoint"></param>
    /// <param name="parent"></param>
    /// <param name="child"></param>
    /// <returns>True if point is on child, false otherwise.</returns>
    public static bool HitTest(Point rawPoint, Control parent, Control child)
    {
        ArgumentNullException.ThrowIfNull(parent);
        ArgumentNullException.ThrowIfNull(child);
        //
        Point point = TranslatePoint(null, rawPoint, parent);
        // hit testing - calculate position in child according to specified layout attributes
        Vector actualOffset = child.ActualOffset;
        Rect renderSlotRect = child.RenderSlotRect;
        Rect virtualSlotRect = new(new Point(actualOffset.x, actualOffset.y), child.RenderSize);
        if (!child.LayoutClip.IsEmpty)
        {
            Rect layoutClip = child.LayoutClip;
            Point location = layoutClip.Location;
            location.Offset(actualOffset.x, actualOffset.y);
            layoutClip.Location = location;
            virtualSlotRect.Intersect(layoutClip);
        }
        virtualSlotRect.Intersect(renderSlotRect);
        return virtualSlotRect.Contains(point);
    }

    /// <summary>
    /// Checks if the point is not overlapped by other controls.
    /// </summary>
    public bool IsPointVisible(Point point)
    {
        return IsPointVisible(this, point);
    }

    /// <summary>
    /// Checks if the point is not overlapped by other controls.
    /// </summary>
    internal static bool IsPointVisible(Control control, Point point)
    {
        ArgumentNullException.ThrowIfNull(control);
        //
        Rect layoutClip = control.LayoutClip;
        bool visible;
        if (layoutClip.IsEmpty)
        {
            Rect controlVirtualCanvasRect = new(new Point(0, 0), control.RenderSize);
            visible = controlVirtualCanvasRect.Contains(point);
        }
        else
        {
            visible = layoutClip.Contains(point);
        }
        //
        if (!visible)
            return false;
        //
        if (null == control.Parent)
            return true;
        // recursively check the parent
        return IsPointVisible(control.Parent, TranslatePoint(control, point, control.Parent));
    }

    /// <summary>
    /// You should define your rendering logic here.
    /// </summary>
    /// <param name="buffer">Buffer where rendered content will be stored.</param>
    public virtual void Render(RenderingBuffer buffer)
    {
    }

    internal virtual IList<Control> GetChildrenOrderedByZIndex()
    {
        return Children;
    }

    /// <summary>
    /// Sets the position of console cursor.
    /// </summary>
    /// <param name="point">Coords relatively to this control.</param>
    protected void SetCursorPosition(Point point)
    {
        ConsoleApplication.Instance.SetCursorPosition(TranslatePoint(this, point, null));
    }

    protected static void HideCursor()
    {
        ConsoleApplication.Instance.HideCursor();
    }

    protected static void ShowCursor()
    {
        ConsoleApplication.Instance.ShowCursor();
    }

    private bool cursorVisible = false;
    internal bool CursorVisible
    {
        get
        {
            return cursorVisible;
        }
        set
        {
            if (cursorVisible != value)
            {
                cursorVisible = value;
                if (HasFocus)
                {
                    ConsoleApplication.Instance.FocusManager.RefreshMouseCursor();
                }
            }
        }
    }

    private Point cursorPosition = new(0, 0);
    internal Point CursorPosition
    {
        get => cursorPosition;
        set
        {
            if (cursorPosition != value)
            {
                cursorPosition = value;
                if (HasFocus)
                {
                    ConsoleApplication.Instance.FocusManager.RefreshMouseCursor();
                }
            }
        }
    }

    /// <summary>
    /// Writes a string to the buffer, clipping it if necessary (placing two periods at the end).
    /// </summary>
    /// <param name="s">String</param>
    /// <param name="buffer">Output buffer</param>
    /// <param name="x">X-coordinate to start outputting the string from</param>
    /// <param name="y">Y-coordinate</param>
    /// <param name="maxWidth">Available width for output (starting from x). That is, x + maxWidth should not
    /// exceed the ActualWidth of the control</param>
    /// <param name="attr">Attributes</param>
    /// <returns>The number of pixels actually output to the buffer, min(s.len, maxWidth)</returns>
    protected static int RenderString(string s,
        RenderingBuffer buffer,
        int x,
        int y,
        int maxWidth,
        Attr attr)
    {
        for (int i = 0; i < Math.Min(s.Length, maxWidth); i++)
        {
            char c;
            if (i + 2 < maxWidth || i < 2 || s.Length <= maxWidth)
            {
                c = s[i];
            }
            else
            {
                c = '.';
            }
            buffer.SetPixel(x + i, y, c, attr);
        }
        return Math.Min(s.Length, maxWidth);
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void RaisePropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected static void assert(bool assertion)
    {
        if (!assertion) throw new InvalidOperationException("Assertion failed.");
    }

    /// <summary>
    /// Stores a reference to the child window element that last lost focus.
    /// When restoring focus to the window itself, WindowsHost uses this field to
    /// restore focus to the element where it was.
    /// </summary>
    internal Control StoredFocus = null;

    /// <summary>
    /// Determines the child element under the mouse cursor,
    /// and passes focus to it if it is Focusable. And if the right mouse button is pressed and the
    /// control has a context menu, activates it.
    /// </summary>
    protected void PassFocusToChildUnderPoint(MouseEventArgs args)
    {
        Control topControl = VisualTreeHelper.FindTopControlUnderMouse(this, args.GetPosition(this));
        if (topControl != null)
        {
            if (topControl.Focusable)
            {
                ConsoleApplication.Instance.FocusManager.SetFocus(this, topControl);
            }
            if (args.RightButton == MouseButtonState.Pressed)
            {
                if (topControl.ContextMenu != null)
                {
                    var windowsHost = VisualTreeHelper.FindClosestParent<WindowsHost>(this);
                    topControl.ContextMenu.OpenMenu(windowsHost, args.GetPosition(windowsHost));
                }
            }
        }
    }

    /// <summary>
    /// This method is called after control has been created and filled with children.
    /// todo : think about avoiding reentrant Created() calls
    /// </summary>
    public void Created()
    {
        foreach (var child in Children)
        {
            child.Created();
        }
        OnCreated();
    }

    /// <summary>
    /// This method is invoked after control has been created and all children
    /// controls are created too (and children' OnCreated called). So, you can
    /// find any child control in this method and subscribe for events.
    /// </summary>
    protected virtual void OnCreated()
    {
    }

    public ContextMenu ContextMenu { get; set; }
}

