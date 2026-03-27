using ConsoleFramework.Controls;
using ConsoleFramework.Core;
using System;
using System.Collections.Generic;

namespace ConsoleFramework.Rendering;

/// <summary>
/// Central point of the console framework layout system.
/// </summary>
public sealed class Renderer
{

    private Rect rootElementRect;

    /// <summary>
    /// The rectangular area relative to the console screen where the Root Element will be placed.
    /// </summary>
    public Rect RootElementRect
    {
        get { return rootElementRect; }
        set
        {
            if (rootElementRect != value)
            {
                rootElementRect = value;
                if (null != RootElement)
                    AddControlToInvalidationQueue(RootElement);
            }
        }
    }

    private Control rootElement;
    public Control RootElement
    {
        get
        {
            return rootElement;
        }
        set
        {
            if (rootElement != value)
            {
                rootElement?.ControlUnsetAsRootElement();
                rootElement = value;
                rootElement?.ControlSetAsRootElement();
            }
        }
    }

    public PhysicalCanvas Canvas
    {
        get;
        set;
    }

    // Buffers containing only control rendering representation itself
    private readonly Dictionary<Control, RenderingBuffer> buffers = [];
    // Buffers containing full control render (with children render applied)
    private readonly Dictionary<Control, RenderingBuffer> fullBuffers = [];
    // Queue of controls marked for layout invalidation
    private readonly List<Control> invalidatedControls = [];

    /// <summary>
    /// Controls whose child elements have changes in Z-Order
    /// (Z-Order only; if child elements were added or deleted then it will automatically
    /// be invalidated, and there's no need to add it to this list).
    /// </summary>
    private readonly List<Control> zOrderCheckControls = [];

    public bool AnyControlInvalidated
    {
        get { return invalidatedControls.Count != 0; }
    }

    // List of controls whose full render buffer content has been updated
    // Valid only when UpdateLayout is called; cleared after FinallyApplyChangesToCanvas is called
    private readonly List<Control> renderingUpdatedControls = [];

    private enum AffectType
    {
        LayoutInvalidated,
        LayoutRevalidated
    }

    private struct ControlAffectInfo(Control control, Renderer.AffectType affectType)
    {
        public readonly Control control = control;
        public readonly AffectType affectType = affectType;
    }

    /// <summary>
    /// Applies all changes accumulated during previous UpdateLayout calls to the screen.
    /// </summary>
    public void FinallyApplyChangesToCanvas(bool forceRepaintAll = false)
    {
        Rect affectedRect = Rect.Empty;

        // Propagate updated rendered buffers to parent elements and eventually to Canvas
        foreach (Control control in renderingUpdatedControls)
        {
            Rect currentAffectedRect = applyChangesToCanvas(control, new Rect(new Point(0, 0), control.RenderSize));
            affectedRect.Union(currentAffectedRect);
        }
        if (forceRepaintAll)
        {
            affectedRect = new Rect(rootElementRect.Size);
        }

        // Flush stored image (with this.RootElementRect offset)
        if (!affectedRect.IsEmpty)
        {
            // Affected rect relative to canvas
            Rect affectedRectAbsolute = new(affectedRect.x + RootElementRect.x, affectedRect.y + RootElementRect.y, affectedRect.width, affectedRect.height);

            // Clip according to real canvas size
            affectedRectAbsolute.Intersect(new Rect(new Point(0, 0), Canvas.Size));

            Canvas.Flush(affectedRectAbsolute);
        }

        // If anything changed in layout - update displaying cursor state
        if (renderingUpdatedControls.Count > 0)
        {
            ConsoleApplication.Instance.FocusManager.RefreshMouseCursor();
        }

        // Prepare for next layout pass
        renderingUpdatedControls.Clear();
    }

    /// <summary>
    /// Recalculates the layout for all controls added to the revalidation queue.
    /// Determines which controls need to be redrawn and calls Render on them.
    /// Determines which areas of the screen need to be updated and redraws
    /// the console screen.
    /// </summary>
    public void UpdateLayout()
    {
        List<ControlAffectInfo> affectedControls = [];

        // Invalidate layout and fill renderingUpdatedControls list
        invalidateLayout(affectedControls);

        // Raise all invalidated and revalidated events of affected controls with subscribers
        foreach (ControlAffectInfo affectInfo in affectedControls)
        {
            if (affectInfo.affectType == AffectType.LayoutInvalidated)
                affectInfo.control.RaiseInvalidatedEvent();
            else if (affectInfo.affectType == AffectType.LayoutRevalidated)
                affectInfo.control.RaiseRevalidatedEvent();
        }

        // Iterate through zOrderCheckControls, for each control checking all its children -
        // has their overlappedRect changed? If yes, and changed so that
        // most of the child control became visible - add that control to the list
        // renderingUpdatedControls. Their content will then be output to the screen in the FinallyApplyChangesToCanvas method.
        foreach (Control zorderCheckControl in zOrderCheckControls)
        {
            refreshChildrenLastOverlappedRects(zorderCheckControl, true);
        }

        // Clear list to prepare for next layout pass
        zOrderCheckControls.Clear();
    }

    /// <summary>
    /// Updates LastOverlappedRect for all controls that are direct
    /// children of parent, in accordance with their Z-Order. If addToInvalidatedIfChanged = true,
    /// then child elements whose OverlappedRect decreased compared to the previous
    /// value will be added to the renderingUpdatedControls list.
    /// </summary>
    private void refreshChildrenLastOverlappedRects(Control parent,
                                                     bool addToInvalidatedIfChanged)
    {
        for (int i = 0; i < parent.Children.Count; i++)
        {
            Control control = parent.Children[i];
            // Relative to parent
            Rect controlRect = control.RenderSlotRect;
            // Relative to control
            Rect overlappedRect = Rect.Empty;

            // We only check siblings that have higher Z-Order
            for (int j = i + 1; j < parent.Children.Count; j++)
            {
                Control sibling = parent.Children[j];
                if (sibling != control)
                {
                    if (controlRect.IntersectsWith(sibling.RenderSlotRect))
                    {
                        Rect controlRectCopy = controlRect;
                        controlRectCopy.Intersect(sibling.RenderSlotRect);
                        if (!controlRectCopy.IsEmpty)
                        {
                            controlRectCopy.Offset(-controlRect.X, -controlRect.Y);
                            overlappedRect.Union(controlRectCopy);
                        }
                    }
                }
            }

            if (addToInvalidatedIfChanged)
            {
                Rect lastOverlappedRectCopy = control.LastOverlappedRect;
                lastOverlappedRectCopy.Union(overlappedRect);

                // Only add to invalidated if new rect is not inside old
                if (lastOverlappedRectCopy != overlappedRect)
                {
                    renderingUpdatedControls.Add(control);
                }
            }

            control.LastOverlappedRect = overlappedRect;
        }
    }

    /// <summary>
    /// Gets the full render buffer for the specified control and applies it sequentially
    /// to all parent elements, up to displaying on the screen.
    /// Returns the rectangle needed for revalidation on the screen (affected rect).
    /// Takes into account the Z-Order of sibling controls (if a parent control has multiple children, they may overlap
    /// each other).
    /// The first call is made with affectedRect = control.RenderSize.
    /// </summary>
    /// <returns>Affected rectangle in canvas should be copyied to console screen.</returns>
    private Rect applyChangesToCanvas(Control control, Rect affectedRect)
    {
        // if the layout system determined the size of the child control, exceeding the size of the slot
        // (this can happen if the child control ignores the arguments passed to MeasureOverride
        // and ArrangeOverride), then an affectedRect coming from outside the bounds of
        // the current control's RenderSize may arrive here, and we must perform intersection for correct overlay
        affectedRect.Intersect(new Rect(new Point(0, 0), control.RenderSize));
        RenderingBuffer fullBuffer = getOrCreateFullBufferForControl(control);
        if (control.Parent != null)
        {
            RenderingBuffer fullParentBuffer = getOrCreateFullBufferForControl(control.Parent);
            // if the control's buffer contains opacity pixels in affectedRect, we are forced to reinitialize
            // the parent's buffer entirely (not calling Render, of course, but by overlaying child buffers)
            if (fullBuffer.ContainsOpacity(affectedRect))
            {
                fullParentBuffer.Clear();
                fullParentBuffer.CopyFrom(getOrCreateBufferForControl(control.Parent));
                foreach (Control child in control.Parent.Children)
                {
                    if (child.Visibility == Visibility.Visible)
                    {
                        RenderingBuffer childBuffer = getOrCreateFullBufferForControl(child);
                        fullParentBuffer.ApplyChild(childBuffer, child.ActualOffset,
                            child.RenderSize, child.RenderSlotRect, child.LayoutClip);
                    }
                }
            }

            if (control.Visibility == Visibility.Visible)
            {
                if (affectedRect == new Rect(new Point(0, 0), control.RenderSize))
                {
                    fullParentBuffer.ApplyChild(fullBuffer, control.ActualOffset,
                        control.RenderSize, control.RenderSlotRect, control.LayoutClip);
                }
                else
                {
                    fullParentBuffer.ApplyChild(fullBuffer, control.ActualOffset,
                        control.RenderSize, control.RenderSlotRect, control.LayoutClip,
                        affectedRect);
                }
            }

            // Determine neighbors of the control that can overlap it
            IList<Control> neighbors = control.Parent.GetChildrenOrderedByZIndex();

            // Restore the image over the updated control if
            // there are controls lying higher in z-order
            int controlIndex = neighbors.IndexOf(control);
            // Starting from controlIndex + 1 in the list are controls with z-index greater than the current control's z-index
            for (int i = controlIndex + 1; i < neighbors.Count; i++)
            {
                Control neighbor = neighbors[i];
                fullParentBuffer.ApplyChild(getOrCreateFullBufferForControl(neighbor),
                    neighbor.ActualOffset, neighbor.RenderSize,
                    neighbor.RenderSlotRect, neighbor.LayoutClip);
            }
            Rect parentAffectedRect = control.RenderSlotRect;
            parentAffectedRect.Intersect(new Rect(affectedRect.x + control.ActualOffset.x,
                                                  affectedRect.y + control.ActualOffset.y,
                                                  affectedRect.width,
                                                  affectedRect.height));
            // No point in continuing to climb up the tree if the control is definitely not visible
            if (parentAffectedRect.IsEmpty)
            {
                return Rect.Empty;
            }
            return applyChangesToCanvas(control.Parent, parentAffectedRect);
        }
        else
        {
            if (control != RootElement)
                throw new InvalidOperationException("Assertion failed.");

            // We've reached the console screen
            fullBuffer.CopyToPhysicalCanvas(Canvas, affectedRect, RootElementRect.TopLeft);
            return affectedRect;
        }
    }

    /// <summary>
    /// Recalculates the layout for all controls added to the revalidation queue.
    /// After the control's layout is calculated, rendering is performed.
    /// Rendering is only done when the control's size changes or
    /// the control is explicitly marked as having changed its image. In other cases
    /// cached buffers containing already rendered images are used.
    /// </summary>
    /// <param name="affectedControls"></param>
    private void invalidateLayout(List<ControlAffectInfo> affectedControls)
    {
        List<Control> resettedControls = [];
        List<Control> revalidatedControls = [];
        while (invalidatedControls.Count != 0)
        {
            // Dequeue next control
            Control control = invalidatedControls[^1];
            invalidatedControls.RemoveAt(invalidatedControls.Count - 1);

            // Set previous results of layout passes dirty
            control.ResetValidity(resettedControls);
            if (resettedControls.Count > 0)
            {
                foreach (Control resettedControl in resettedControls)
                {
                    affectedControls.Add(new ControlAffectInfo(resettedControl, AffectType.LayoutInvalidated));
                }
                resettedControls.Clear();
            }

            //
            updateLayout(control, revalidatedControls);
            if (revalidatedControls.Count > 0)
            {
                foreach (Control revalidatedControl in revalidatedControls)
                {
                    affectedControls.Add(new ControlAffectInfo(revalidatedControl, AffectType.LayoutRevalidated));
                }
                revalidatedControls.Clear();
            }
        }
    }

    private static bool checkDesiredSizeNotChangedRecursively(Control control)
    {
        if (control.lastLayoutInfo.unclippedDesiredSize != control.layoutInfo.unclippedDesiredSize)
        {
            return false;
        }
        foreach (Control child in control.Children)
        {
            if (!checkDesiredSizeNotChangedRecursively(child))
                return false;
        }
        return true;
    }

    private void updateLayout(Control control, List<Control> revalidatedControls)
    {
        LayoutInfo lastLayoutInfo = control.lastLayoutInfo;
        // Work with the parent control element
        if (control.Parent != null)
        {
            bool needUpdateParentLayout = true;
            // If the size of the current control has not changed, the revalidation state is not propagated
            // up the element tree, and we move on to working with child elements.
            // Otherwise we add the parent element to the end of the revalidation queue, and
            // return control.
            if (lastLayoutInfo.validity != LayoutValidity.Nothing)
            {
                control.Measure(lastLayoutInfo.measureArgument);
                //                    if (lastLayoutInfo.unclippedDesiredSize == control.layoutInfo.unclippedDesiredSize) {
                if (checkDesiredSizeNotChangedRecursively(control))
                {
                    needUpdateParentLayout = false;
                }
            }
            if (needUpdateParentLayout)
            {
                // mark the parent control for invalidation too and enqueue them
                control.Parent.Invalidate();
                // We can finish with this element, since we've already added
                // its parent to the end of the queue, and we will return to it next time anyway
                return;
            }
        }
        // Work with child control elements
        // Call Measure&Arrange for the current control with the latest argument values
        if (lastLayoutInfo.validity == LayoutValidity.Nothing && control.Parent != null)
        {
            throw new InvalidOperationException("Assertion failed.");
        }
        // rootElement - special case
        if (control.Parent == null)
        {
            if (control != RootElement)
            {
                throw new InvalidOperationException("Control has no parent but is not known rootElement.");
            }
            control.Measure(RootElementRect.Size);
            control.Arrange(RootElementRect);
        }
        else
        {
            control.Measure(lastLayoutInfo.measureArgument);
            control.Arrange(lastLayoutInfo.renderSlotRect);
        }
        // update render buffers of current control and its children
        RenderingBuffer buffer = getOrCreateBufferForControl(control);
        RenderingBuffer fullBuffer = getOrCreateFullBufferForControl(control);
        // replace buffers if control has grown
        LayoutInfo layoutInfo = control.layoutInfo;
        if (layoutInfo.renderSize.width > buffer.Width || layoutInfo.renderSize.height > buffer.Height)
        {
            buffer = new RenderingBuffer(layoutInfo.renderSize.width, layoutInfo.renderSize.height);
            fullBuffer = new RenderingBuffer(layoutInfo.renderSize.width, layoutInfo.renderSize.height);
            buffers[control] = buffer;
            fullBuffers[control] = fullBuffer;
        }
        buffer.Clear();
        if (control.RenderSize.Width != 0 && control.RenderSize.Height != 0)
            control.Render(buffer);
        // Check child controls - if their layoutInfo has not changed compared to the last time,
        // we can take their last renderBuffer without updating and apply it to the current control
        fullBuffer.CopyFrom(buffer);
        IList<Control> children = control.Children;
        foreach (Control child in children)
        {
            if (child.Visibility == Visibility.Visible)
            {
                RenderingBuffer fullChildBuffer = processControl(child, revalidatedControls);
                fullBuffer.ApplyChild(fullChildBuffer, child.ActualOffset,
                    child.RenderSize,
                    child.RenderSlotRect, child.LayoutClip);
            }
            else
            {
                // So that the next Invalidate overwrites lastLayoutInfo
                if (child.SetValidityToRender())
                {
                    revalidatedControls.Add(child);
                }
            }
        }

        // Save overlappingRect for each control child
        refreshChildrenLastOverlappedRects(control, false);

        if (control.SetValidityToRender())
        {
            revalidatedControls.Add(control);
        }
        addControlToRenderingUpdatedList(control);
    }

    /// <summary>
    /// Adds the specified control to the list of controls for which the full rendering buffer has been updated.
    /// </summary>
    private void addControlToRenderingUpdatedList(Control control)
    {
        renderingUpdatedControls.Add(control);
    }

    private static bool checkRenderingWasNotChangedRecursively(Control control)
    {
        if (!control.lastLayoutInfo.Equals(control.layoutInfo)
            || control.lastLayoutInfo.validity != LayoutValidity.Render) return false;
        foreach (Control child in control.Children)
        {
            if (!checkRenderingWasNotChangedRecursively(child)) return false;
        }
        return true;
    }

    private RenderingBuffer processControl(Control control, List<Control> revalidatedControls)
    {
        RenderingBuffer buffer = getOrCreateBufferForControl(control);
        RenderingBuffer fullBuffer = getOrCreateFullBufferForControl(control);
        //
        LayoutInfo lastLayoutInfo = control.lastLayoutInfo;
        LayoutInfo layoutInfo = control.layoutInfo;
        //
        control.Measure(lastLayoutInfo.measureArgument);
        control.Arrange(lastLayoutInfo.renderSlotRect);
        // if lastLayoutInfo eq layoutInfo we can use last rendered buffer
        if (checkRenderingWasNotChangedRecursively(control))
        {
            if (control.SetValidityToRender())
            {
                revalidatedControls.Add(control);
            }
            return fullBuffer;
        }
        // replace buffers if control has grown
        if (layoutInfo.renderSize.width > buffer.Width || layoutInfo.renderSize.height > buffer.Height)
        {
            buffer = new RenderingBuffer(layoutInfo.renderSize.width, layoutInfo.renderSize.height);
            fullBuffer = new RenderingBuffer(layoutInfo.renderSize.width, layoutInfo.renderSize.height);
            buffers[control] = buffer;
            fullBuffers[control] = fullBuffer;
        }
        // otherwise we should assemble full rendered buffer using childs
        buffer.Clear();
        if (control.RenderSize.Width != 0 && control.RenderSize.Height != 0)
            control.Render(buffer);
        //
        fullBuffer.CopyFrom(buffer);
        foreach (Control child in control.Children)
        {
            if (child.Visibility == Visibility.Visible)
            {
                RenderingBuffer fullChildBuffer = processControl(child, revalidatedControls);
                fullBuffer.ApplyChild(fullChildBuffer, child.ActualOffset,
                    child.RenderSize, child.RenderSlotRect, child.LayoutClip);
            }
            else
            {
                // So that the next Invalidate for this control
                // overwrites lastLayoutInfo
                if (child.SetValidityToRender())
                {
                    revalidatedControls.Add(child);
                }
            }
        }

        // Save overlappingRect for each control child
        refreshChildrenLastOverlappedRects(control, false);

        if (control.SetValidityToRender())
        {
            revalidatedControls.Add(control);
        }
        return fullBuffer;
    }

    internal void AddControlToInvalidationQueue(Control control)
    {
        ArgumentNullException.ThrowIfNull(control);
        if (!invalidatedControls.Contains(control))
        {
            // Add to queue only if it has parent or it is root element
            if (control.Parent != null || control == RootElement)
            {
                invalidatedControls.Add(control);
            }
        }
    }

    private RenderingBuffer getOrCreateBufferForControl(Control control)
    {
        if (buffers.TryGetValue(control, out RenderingBuffer value))
        {
            return value;
        }
        else
        {
            RenderingBuffer buffer = new(control.ActualWidth, control.ActualHeight);
            buffers.Add(control, buffer);
            return buffer;
        }
    }

    private RenderingBuffer getOrCreateFullBufferForControl(Control control)
    {
        if (fullBuffers.TryGetValue(control, out RenderingBuffer value))
        {
            return value;
        }
        else
        {
            RenderingBuffer buffer = new(control.ActualWidth, control.ActualHeight);
            fullBuffers.Add(control, buffer);
            return buffer;
        }
    }

    /// <summary>
    /// Returns the opacity code of the control at the specified point.
    /// This is necessary to determine the control that will be the source of the mouse event.
    /// </summary>
    internal int getControlOpacityAt(Control control, int x, int y)
    {
        // If the control being hovered has invisible children that have never
        // been rendered, then the buffer dictionary for such children will be empty.
        // We return 6 for such children - as if they are completely transparent
        if (!buffers.TryGetValue(control, out RenderingBuffer value))
        {
            return 6;
        }
        return value.GetOpacityAt(x, y);
    }

    /// <summary>
    /// Called when control is removed from visual tree.
    /// It is necessary to remove it from invalidated queue if they are there.
    /// </summary>
    internal void ControlRemovedFromTree(Control child)
    {
        invalidatedControls.Remove(child);
        foreach (var nestedChild in child.Children)
        {
            ControlRemovedFromTree(nestedChild);
        }
    }

    /// <summary>
    /// Called when some of control's children changed their z-order.
    /// (Not when added or removed - just changed between them).
    /// This call allows layout system to detect when need to refresh
    /// display image if no controls invalidated but z-order changed.
    /// </summary>
    internal void AddControlToZOrderCheckList(Control control)
    {
        zOrderCheckControls.Add(control);
    }
}

