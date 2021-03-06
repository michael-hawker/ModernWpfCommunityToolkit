// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using ModernWpf.Toolkit.Extensions;
using ModernWpf.Toolkit.UI.Helpers;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ModernWpf.Toolkit.UI.Controls
{
    /// <summary>
    /// The <see cref="Eyedropper"/> control can pick up a color from anywhere in your application.
    /// </summary>
    public partial class Eyedropper
    {
        private void UpdateEyedropper(Point position)
        {
            if (_appScreenshot == null)
            {
                return;
            }

            #region Updating Layout Transform

            // Updating Y values
            if (position.Y > _rootGrid.ActualHeight / 2)
            {
                _layoutTransform.Y = position.Y - ActualHeight;
            }
            else
            {
                _layoutTransform.Y = position.Y;
            }

            // Updating X values
            if (position.X > _rootGrid.ActualWidth - (ActualWidth / 2) - 15)
            {
                _layoutTransform.X = _rootGrid.ActualWidth - ActualWidth - 15;
            }
            else if (position.X < (ActualWidth / 2) + 15)
            {
                _layoutTransform.X = 15;
            }
            else
            {
                _layoutTransform.X = position.X - (ActualWidth / 2);
            }

            #endregion

            var x = (int)Math.Ceiling(Math.Min(_appScreenshot.PixelWidth - 1, Math.Max(position.X, 0)));
            var y = (int)Math.Ceiling(Math.Min(_appScreenshot.PixelHeight - 1, Math.Max(position.Y, 0)));
            Color = _appScreenshot.GetPixelColor(x, y);
            UpdatePreview(x, y);
        }

        private void UpdateWorkArea()
        {
            if (_targetGrid == null)
            {
                return;
            }

            var content = (FrameworkElement)OwnerWindow.Content;
            if (WorkArea == default)
            {
                _targetGrid.Margin = default;
            }
            else
            {
                var left = WorkArea.Left;
                var top = WorkArea.Top;
                double right = content.ActualWidth - WorkArea.Right;
                double bottom = content.ActualHeight - WorkArea.Bottom;

                _targetGrid.Margin = new Thickness(left, top, right, bottom);
            }
        }

        private void UpdatePreview(int centerX, int centerY)
        {
            var halfPixelCountPerRow = (PixelCountPerRow - 1) / 2;
            var startX = centerX - halfPixelCountPerRow;
            var startY = centerY - halfPixelCountPerRow;
            var endX = centerX + halfPixelCountPerRow + 1;
            var endY = centerY + halfPixelCountPerRow + 1;

            var size = new Size(PreviewPixelsPerRawPixel, PreviewPixelsPerRawPixel);
            Point startPoint = new Point(0, 0);

            var brush = Brushes.Transparent;
            var drawingGroup = new DrawingGroup();

            for (var y = startY; y < endY; y++)
            {
                startPoint.X = 0;
                for (var x = startX; x < endX; x++)
                {
                    var rectangleGeometry = new RectangleGeometry() { Rect = startPoint.ToRect(size) };
                    var geometryDrawing = new GeometryDrawing(_appScreenshot.GetPixelBrush(x, y), new Pen(brush, 1.0), rectangleGeometry);
                    drawingGroup.Children.Add(geometryDrawing);

                    startPoint.X += PreviewPixelsPerRawPixel;
                }

                startPoint.Y += PreviewPixelsPerRawPixel;
            }

            var drawingImage = new DrawingImage(drawingGroup);
            drawingImage.Freeze();
            Preview = drawingImage;
        }

        internal void UpdateAppScreenshot()
        {            
            FrameworkElement content = (FrameworkElement)OwnerWindow.Content;
            int width = (int)Math.Ceiling(content.ActualWidth);
            int height = (int)Math.Ceiling(content.ActualHeight);

            try
            {
                var renderTargetBitmap = new RenderTargetBitmap(
                    width, height,
                    96, 96, PixelFormats.Pbgra32);
                renderTargetBitmap.Render(content);

                _appScreenshot = BitmapFrame.Create(renderTargetBitmap);
                _appScreenshot.Freeze();
            }
            catch (OutOfMemoryException ex)
            {
                Debug.WriteLine(ex);
            }
        }
    }
}
