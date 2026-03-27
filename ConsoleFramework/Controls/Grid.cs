using ConsoleFramework.Core;
using ConsoleFramework.Rendering;
using ConsoleFramework.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ConsoleFramework.Controls;

[ContentProperty("Controls")]
public class Grid : Control
{
    private readonly List<ColumnDefinition> columnDefinitions = [];
    private readonly List<RowDefinition> rowDefinitions = [];
    private readonly UIElementCollection children;
    private int[] columnsWidths;
    private int[] rowsHeights;

    public List<ColumnDefinition> ColumnDefinitions
    {
        get { return columnDefinitions; }
    }

    public List<RowDefinition> RowDefinitions
    {
        get { return rowDefinitions; }
    }

    public UIElementCollection Controls { get { return children; } }

    public Grid()
    {
        children = new UIElementCollection(this);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        if (ColumnDefinitions.Count == 0 || RowDefinitions.Count == 0)
            return Size.Empty;
        Control[,] matrix = new Control[ColumnDefinitions.Count, RowDefinitions.Count];
        for (int x = 0; x < ColumnDefinitions.Count; x++)
        {
            for (int y = 0; y < RowDefinitions.Count; y++)
            {
                if (Children.Count > y * ColumnDefinitions.Count + x)
                {
                    matrix[x, y] = Children[y * ColumnDefinitions.Count + x];
                }
                else
                {
                    matrix[x, y] = null;
                }
            }
        }

        // If PositiveInfinity is passed as availableSize, we simply ignore Star-elements,
        // treating them the same as Auto and performing normal placement
        bool interpretStarAsAuto = availableSize.Width == int.MaxValue
                                   || availableSize.Height == int.MaxValue;

        // First, perform Measure of all controls taking into account the constraints
        // defined in ColumnDefinitions and RowDefinitions
        for (int x = 0; x < ColumnDefinitions.Count; x++)
        {
            ColumnDefinition columnDefinition = ColumnDefinitions[x];

            int width = columnDefinition.Width.GridUnitType == GridUnitType.Pixel
                ? columnDefinition.Width.Value : int.MaxValue;

            for (int y = 0; y < RowDefinitions.Count; y++)
            {
                RowDefinition rowDefinition = RowDefinitions[y];

                int height = rowDefinition.Height.GridUnitType == GridUnitType.Pixel
                    ? rowDefinition.Height.Value : int.MaxValue;

                // Apply min-max constraints
                if (columnDefinition.MinWidth != null && width < columnDefinition.MinWidth.Value)
                    width = columnDefinition.MinWidth.Value;
                if (columnDefinition.MaxWidth != null && width > columnDefinition.MaxWidth.Value)
                    width = columnDefinition.MaxWidth.Value;

                if (rowDefinition.MinHeight != null && height < rowDefinition.MinHeight.Value)
                    height = rowDefinition.MinHeight.Value;
                if (rowDefinition.MaxHeight != null && height > rowDefinition.MaxHeight.Value)
                    height = rowDefinition.MaxHeight.Value;

                matrix[x, y]?.Measure(new Size(width, height));
            }
        }

        // Now for each column (non-Star) we need to calculate the maximum Width, and for
        // each row - the maximum Height - these values will become respectively
        // the width and height of cells defined by row and column coordinates

        columnsWidths = new int[ColumnDefinitions.Count];

        for (int x = 0; x < ColumnDefinitions.Count; x++)
        {
            if (ColumnDefinitions[x].Width.GridUnitType != GridUnitType.Star || interpretStarAsAuto)
            {
                int maxWidth = ColumnDefinitions[x].Width.GridUnitType == GridUnitType.Pixel
                                   ? ColumnDefinitions[x].Width.Value
                                   : 0;
                // Take MinWidth into account. There's no need to specifically account for MaxWidth, since we
                // already did that on the first Measure, and DesiredSize cannot be larger than MaxWidth
                if (ColumnDefinitions[x].MinWidth != null && maxWidth < ColumnDefinitions[x].MinWidth.Value)
                    maxWidth = ColumnDefinitions[x].MinWidth.Value;
                for (int y = 0; y < RowDefinitions.Count; y++)
                {
                    if (matrix[x, y] != null)
                        if (matrix[x, y].DesiredSize.Width > maxWidth)
                            maxWidth = matrix[x, y].DesiredSize.Width;
                }
                columnsWidths[x] = maxWidth;
            }
        }

        rowsHeights = new int[RowDefinitions.Count];

        for (int y = 0; y < RowDefinitions.Count; y++)
        {
            if (RowDefinitions[y].Height.GridUnitType != GridUnitType.Star || interpretStarAsAuto)
            {
                int maxHeight = RowDefinitions[y].Height.GridUnitType == GridUnitType.Pixel
                                    ? RowDefinitions[y].Height.Value
                                    : 0;
                if (RowDefinitions[y].MinHeight != null && maxHeight < RowDefinitions[y].MinHeight.Value)
                    maxHeight = RowDefinitions[y].MinHeight.Value;
                for (int x = 0; x < ColumnDefinitions.Count; x++)
                {
                    if (matrix[x, y] != null)
                        if (matrix[x, y].DesiredSize.Height > maxHeight)
                            maxHeight = matrix[x, y].DesiredSize.Height;
                }
                rowsHeights[y] = maxHeight;
            }
        }

        // Now calculate the sizes of Star-columns and Star-rows
        if (!interpretStarAsAuto)
        {
            int totalWidthStars = 0;
            foreach (var columnDefinition in ColumnDefinitions)
            {
                if (columnDefinition.Width.GridUnitType == GridUnitType.Star)
                {
                    totalWidthStars += columnDefinition.Width.Value;
                }
            }
            int remainingWidth = Math.Max(0, availableSize.Width - columnsWidths.Sum());
            for (int x = 0; x < ColumnDefinitions.Count; x++)
            {
                ColumnDefinition columnDefinition = ColumnDefinitions[x];
                if (columnDefinition.Width.GridUnitType == GridUnitType.Star)
                {
                    columnsWidths[x] = remainingWidth * columnDefinition.Width.Value / totalWidthStars;
                }
            }

            int totalHeightStars = 0;
            foreach (var rowDefinition in RowDefinitions)
            {
                if (rowDefinition.Height.GridUnitType == GridUnitType.Star)
                {
                    totalHeightStars += rowDefinition.Height.Value;
                }
            }
            int remainingHeight = Math.Max(0, availableSize.Height - rowsHeights.Sum());
            for (int y = 0; y < RowDefinitions.Count; y++)
            {
                RowDefinition rowDefinition = RowDefinitions[y];
                if (rowDefinition.Height.GridUnitType == GridUnitType.Star)
                {
                    rowsHeights[y] = remainingHeight * rowDefinition.Height.Value / totalHeightStars;
                }
            }
        }

        // Final repeated call to Measure for all children with already determined sizes,
        // those that will be used during placement
        for (int x = 0; x < ColumnDefinitions.Count; x++)
        {
            int width = columnsWidths[x];
            for (int y = 0; y < RowDefinitions.Count; y++)
            {
                int height = rowsHeights[y];
                matrix[x, y]?.Measure(new Size(width, height));
            }
        }

        return new Size(columnsWidths.Sum(), rowsHeights.Sum());
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        int currentX = 0;
        for (int x = 0; x < columnsWidths.Length; x++)
        {
            int currentY = 0;
            for (int y = 0; y < rowsHeights.Length; y++)
            {
                if (Children.Count > y * columnsWidths.Length + x)
                {
                    Children[y * columnsWidths.Length + x].Arrange(new Rect(
                        new Point(currentX, currentY),
                        new Size(columnsWidths[x], rowsHeights[y])
                        ));
                }
                currentY += rowsHeights[y];
            }
            currentX += columnsWidths[x];
        }
        return new Size(columnsWidths.Sum(), rowsHeights.Sum());
    }

    public override void Render(RenderingBuffer buffer)
    {
        buffer.SetOpacityRect(0, 0, ActualWidth, ActualHeight, 2);
    }
}
