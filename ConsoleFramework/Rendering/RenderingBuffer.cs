using ConsoleFramework.Core;
using ConsoleFramework.Native;
using System;
using System.Diagnostics;
using System.Text;

namespace ConsoleFramework.Rendering;

/// <summary>
/// Stores rendered control content.
/// Supports impositioning and opacity mask matrix.
/// </summary>
public sealed class RenderingBuffer
{
    private CHAR_INFO[,] buffer;
    /// <summary> todo : convert to enum
    /// 0 - opaque pixel
    /// 1 - semi-transparent (displayed as a shadow)
    /// 2 - fully transparent (will be ignored when overlaid on another buffer)
    /// 3 - transparent background (when overlaid on another buffer, characters will take its background) - for button frames, for example
    /// 
    /// 4 - same as 0, but lets mouse events pass through
    /// 5 - same as 1, but lets mouse events pass through
    /// 6 - same as 2, but lets mouse events pass through
    /// 7 - same as 3, but lets mouse events pass through
    /// </summary>
    private int[,] opacityMatrix;
    // todo : add bool hasOpacityAttributes and optimize this
    private int width;
    private int height;

    public int Width
    {
        get
        {
            return width;
        }
    }

    public int Height
    {
        get
        {
            return height;
        }
    }

    public RenderingBuffer()
    {
    }

    public RenderingBuffer(int width, int height)
    {
        buffer = new CHAR_INFO[width, height];
        opacityMatrix = new int[width, height];
        this.width = width;
        this.height = height;
    }

    public void CopyFrom(RenderingBuffer renderingBuffer)
    {
        buffer = new CHAR_INFO[renderingBuffer.width, renderingBuffer.height];
        opacityMatrix = new int[renderingBuffer.width, renderingBuffer.height];
        width = renderingBuffer.width;
        height = renderingBuffer.height;
        //
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                buffer[x, y] = renderingBuffer.buffer[x, y];
                opacityMatrix[x, y] = renderingBuffer.opacityMatrix[x, y];
            }
        }
    }

    /// <summary>
    /// Overlays a child element's buffer onto the current one. The child buffer is virtually overlaid on the current one
    /// in accordance with the passed actualOffset, and then the part of the child buffer that falls into 
    /// renderSlotRect is drawn. renderSlotRect is defined relative to the current buffer (not the child buffer).
    /// layoutClip defines which part of the child buffer will be drawn to the current buffer (clipping,
    /// resulting from the application of Margins and Alignments).
    /// </summary>
    /// <param name="childBuffer"></param>
    /// <param name="actualOffset">The offset of the child element's buffer relative to the current one.</param>
    /// <param name="childRenderSize">The size of the content rendered by the child element - may
    /// be smaller than the size of childBuffer, so it needs to be passed explicitly.</param>
    /// <param name="renderSlotRect">The size and position of the slot allocated to the child element.</param>
    /// <param name="layoutClip">The part of the child buffer that will be drawn - may be
    /// less than or equal to RenderSlotRect.Size. In coordinates, it is relative to childBuffer.</param>
    public void ApplyChild(RenderingBuffer childBuffer, Vector actualOffset,
        Size childRenderSize, Rect renderSlotRect, Rect layoutClip)
    {

        ApplyChild(childBuffer, actualOffset, childRenderSize, renderSlotRect, layoutClip, null);
    }

    /// <summary>
    /// Overload for optimized overlaying in case it's known that only a part of the child
    /// control has changed, identified by the affectedRect parameter.
    /// Only this part of the child control will be processed, and the number of operations will decrease.
    /// </summary>
    /// <param name="childBuffer"></param>
    /// <param name="actualOffset"></param>
    /// <param name="childRenderSize"></param>
    /// <param name="renderSlotRect"></param>
    /// <param name="layoutClip"></param>
    /// <param name="affectedRect">Rectangle in the child control that was changed.</param>
    public void ApplyChild(RenderingBuffer childBuffer, Vector actualOffset,
                           Size childRenderSize, Rect renderSlotRect,
                           Rect layoutClip, Rect? affectedRect)
    {

        // Calculate finalRect - the rectangle relative to parent that needs to be filled
        Rect finalRect = layoutClip;

        if (affectedRect != null)
            finalRect.Intersect(affectedRect.Value);

        // If child.RenderSlotRect is larger than child.RenderSize, and the rendering buffer
        // of the child control is larger than its RenderSize (this happens after reducing
        // the control's size - since the buffer can only grow, not shrink) -
        // then we need to either pass child.RenderSize to the ApplyChild method and
        // perform intersection in advance
        finalRect.Intersect(new Rect(new Point(0, 0), childRenderSize));

        // Because cannot call Offset() method of empty rect
        if (finalRect.IsEmpty) return;

        finalRect.Offset(actualOffset);
        finalRect.Intersect(renderSlotRect);

        // Also need to account for the dimensions of the current control's buffer
        finalRect.Intersect(new Rect(new Point(0, 0), new Size(width, height)));

        for (int x = finalRect.Left; x < finalRect.Right; x++)
        {
            int parentX = x;
            int childX = parentX - actualOffset.x;
            for (int y = finalRect.Top; y < finalRect.Bottom; y++)
            {
                int parentY = y;
                int childY = parentY - actualOffset.y;

                CHAR_INFO charInfo = childBuffer.buffer[childX, childY];
                int opacity = childBuffer.opacityMatrix[childX, childY];

                // For fully transparent pixels of the parent buffer - we assign both the pixel value
                // and the opacity value, the rest is up to the next parent
                if (opacityMatrix[parentX, parentY] == 2 || opacityMatrix[parentX, parentY] == 6)
                {
                    buffer[parentX, parentY] = charInfo;
                    opacityMatrix[parentX, parentY] = opacity;
                }
                else
                {
                    // In other cases the parent buffer's opacity remains, and
                    // the pixel itself depends on the opacity of the child element
                    if (opacity == 0 || opacity == 4)
                    {
                        buffer[parentX, parentY] = charInfo;
                    }
                    else if (opacity == 1 || opacity == 5)
                    {
                        charInfo.Attributes = Colors.Blend(Color.DarkGray, Color.Black);
                        charInfo.UnicodeChar = buffer[parentX, parentY].UnicodeChar;
                        buffer[parentX, parentY] = charInfo;
                    }
                    else if (opacity == 3 || opacity == 7)
                    {
                        // Take the background attributes of the character from the parent buffer
                        Attr parentAttr = buffer[parentX, parentY].Attributes;
                        if ((parentAttr & Attr.BACKGROUND_BLUE) == Attr.BACKGROUND_BLUE)
                        {
                            charInfo.Attributes |= Attr.BACKGROUND_BLUE;
                        }
                        else
                        {
                            charInfo.Attributes &= ~Attr.BACKGROUND_BLUE;
                        }
                        if ((parentAttr & Attr.BACKGROUND_GREEN) == Attr.BACKGROUND_GREEN)
                        {
                            charInfo.Attributes |= Attr.BACKGROUND_GREEN;
                        }
                        else
                        {
                            charInfo.Attributes &= ~Attr.BACKGROUND_GREEN;
                        }
                        if ((parentAttr & Attr.BACKGROUND_RED) == Attr.BACKGROUND_RED)
                        {
                            charInfo.Attributes |= Attr.BACKGROUND_RED;
                        }
                        else
                        {
                            charInfo.Attributes &= ~Attr.BACKGROUND_RED;
                        }
                        if ((parentAttr & Attr.BACKGROUND_INTENSITY) == Attr.BACKGROUND_INTENSITY)
                        {
                            charInfo.Attributes |= Attr.BACKGROUND_INTENSITY;
                        }
                        else
                        {
                            charInfo.Attributes &= ~Attr.BACKGROUND_INTENSITY;
                        }
                        buffer[parentX, parentY] = charInfo;
                    }
                }
            }
        }
    }

    public void SetPixelSafe(int x, int y, char c)
    {
        if (buffer.GetLength(0) > x && buffer.GetLength(1) > y)
            SetPixel(x, y, c);
    }

    public void SetPixelSafe(int x, int y, Attr attr)
    {
        if (buffer.GetLength(0) > x && buffer.GetLength(1) > y)
            SetPixel(x, y, attr);
    }

    public void SetPixelSafe(int x, int y, char c, Attr attr)
    {
        if (buffer.GetLength(0) > x && buffer.GetLength(1) > y)
            SetPixel(x, y, c, attr);
    }

    public void SetPixel(int x, int y, char c)
    {
        buffer[x, y].UnicodeChar = c;
    }

    public void SetPixel(int x, int y, Attr attr)
    {
        buffer[x, y].Attributes = attr;
    }

    public void SetPixel(int x, int y, char c, Attr attr)
    {
        buffer[x, y].UnicodeChar = c;
        buffer[x, y].Attributes = attr;
    }

    public void SetOpacity(int x, int y, int opacity)
    {
        if (opacity < 0 || opacity > 7)
            throw new ArgumentException(null, nameof(opacity));
        //
        opacityMatrix[x, y] = opacity;
    }

    public void SetOpacityRect(int x, int y, int w, int h, int opacity)
    {
        if (opacity < 0 || opacity > 7)
            throw new ArgumentException(null, nameof(opacity));
        for (int i = 0; i < w; i++)
        {
            int _x = x + i;
            for (int j = 0; j < h; j++)
            {
                opacityMatrix[_x, y + j] = opacity;
            }
        }
    }

    public void FillRectangle(int x, int y, int w, int h, char c, Attr attributes)
    {
        for (int _x = 0; _x < w; _x++)
        {
            for (int _y = 0; _y < h; _y++)
            {
                SetPixel(x + _x, y + _y, c, attributes);
            }
        }
    }

    /// <summary>
    /// Copies affectedRect from the buffer to the console screen, taking into account that the buffer
    /// is positioned on the console screen with an offset.
    /// </summary>
    /// <param name="canvas"></param>
    /// <param name="affectedRect">Changed area relative to this.</param>
    /// <param name="offset">At what point on the screen the control is positioned (see <see cref="Renderer.RootElementRect"/>).</param>
    public void CopyToPhysicalCanvas(PhysicalCanvas canvas, Rect affectedRect, Point offset)
    {
        Rect rectToCopy = affectedRect;
        Rect bufferRect = new(new Point(0, 0), new Size(width, height));
        Rect canvasRect = new(new Point(-offset.X, -offset.Y), canvas.Size);
        rectToCopy.Intersect(canvasRect);
        rectToCopy.Intersect(bufferRect);
        //
        for (int x = 0; x < rectToCopy.width; x++)
        {
            int bufferX = x + rectToCopy.x;
            int canvasX = x + rectToCopy.x + offset.x;
            for (int y = 0; y < rectToCopy.height; y++)
            {
                int bufferY = y + rectToCopy.y;
                int canvasY = y + rectToCopy.y + offset.y;
                CHAR_INFO charInfo = buffer[bufferX, bufferY];
                canvas[canvasX][canvasY].Assign(charInfo);
            }
        }
    }

    /// <summary>
    /// Renderer should call this method before any control render.
    /// </summary>
    public void Clear()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                buffer[x, y] = new CHAR_INFO();
                opacityMatrix[x, y] = 0;
            }
        }
    }

    public void DumpOpacityMatrix()
    {
        for (int y = 0; y < height; y++)
        {
            StringBuilder sb = new();
            for (int x = 0; x < width; x++)
            {
                sb.Append(opacityMatrix[x, y]);
            }
            Debug.WriteLine(sb);
        }
    }

    /// <summary>
    /// Checks if affectedRect contains pixels with the opacity value set.
    /// This is necessary to ensure correct blending with parent buffers in case of
    /// partial screen updates (if not accounted for, the screen state can mix
    /// new pixels with old ones that were obtained during the previous rendering call).
    /// </summary>
    public bool ContainsOpacity(Rect affectedRect)
    {
        for (int x = 0; x < affectedRect.width; x++)
        {
            for (int y = 0; y < affectedRect.height; y++)
            {
                if (opacityMatrix[x + affectedRect.x, y + affectedRect.y] != 0)
                {
                    return true;
                }
            }
        }
        return false;
    }

    /// <summary>
    /// Returns the opacity code at the specified point.
    /// </summary>
    public int GetOpacityAt(int x, int y)
    {
        return opacityMatrix[x, y];
    }

    public void RenderStringSafe(string s, int x, int y, Attr attr)
    {
        for (int i = 0; i < s.Length; i++)
        {
            SetPixelSafe(x + i, y, s[i], attr);
        }
    }
}
