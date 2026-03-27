using ConsoleFramework.Core;
using System;

namespace ConsoleFramework.Controls
{

    /// <summary>
    /// Fully describes the layout state of a control.
    /// </summary>
    internal class LayoutInfo : IEquatable<LayoutInfo>
    {
        public Size measureArgument;
        // If this field has not changed, then we can consider that the control has not changed its size.
        public Size unclippedDesiredSize;
        public Size desiredSize;
        // Essentially this is the arrangeArgument
        public Rect renderSlotRect;
        public Size renderSize;
        public Rect layoutClip;
        public Vector actualOffset;
        public LayoutValidity validity = LayoutValidity.Nothing;

        public void CopyValuesFrom(LayoutInfo layoutInfo)
        {
            measureArgument = layoutInfo.measureArgument;
            unclippedDesiredSize = layoutInfo.unclippedDesiredSize;
            desiredSize = layoutInfo.desiredSize;
            renderSlotRect = layoutInfo.renderSlotRect;
            renderSize = layoutInfo.renderSize;
            layoutClip = layoutInfo.layoutClip;
            actualOffset = layoutInfo.actualOffset;
            validity = layoutInfo.validity;
        }

        public void ClearValues()
        {
            measureArgument = new Size();
            unclippedDesiredSize = new Size();
            desiredSize = new Size();
            renderSlotRect = new Rect();
            renderSize = new Size();
            layoutClip = new Rect();
            actualOffset = new Vector();
            validity = LayoutValidity.Nothing;
        }

        // All members except 'validity'
        public bool Equals(LayoutInfo? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return other.measureArgument.Equals(measureArgument)
                   && other.unclippedDesiredSize.Equals(unclippedDesiredSize)
                   && other.desiredSize.Equals(desiredSize)
                   && other.renderSlotRect.Equals(renderSlotRect)
                   && other.renderSize.Equals(renderSize)
                   && other.layoutClip.Equals(layoutClip)
                   && other.actualOffset.Equals(actualOffset);
        }

        public override bool Equals(object? obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(LayoutInfo)) return false;
            return Equals((LayoutInfo)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(
                measureArgument,
                unclippedDesiredSize,
                desiredSize,
                renderSlotRect,
                renderSize,
                layoutClip,
                actualOffset);
        }
    }
}

