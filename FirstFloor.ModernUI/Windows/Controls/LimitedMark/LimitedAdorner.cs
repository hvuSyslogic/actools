﻿using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace FirstFloor.ModernUI.Windows.Controls.LimitedMark {
    internal class LimitedAdorner : Adorner {
        private readonly FrameworkElement _contentPresenter;

        public LimitedAdorner(UIElement adornedElement)
                : base(adornedElement) {
            IsHitTestVisible = true;
            
            _contentPresenter = new TextBlock {
                Text = "LIMITED",
                Style = FindResource("LimitedMarkStyle") as Style
            };

            if (_contentPresenter == null) return;

            SetBinding(VisibilityProperty, new Binding("IsVisible") {
                Source = adornedElement,
                Converter = new BooleanToVisibilityConverter()
            });
        }

        protected override void OnPreviewMouseLeftButtonUp(MouseButtonEventArgs e) {
            ModernDialog.ShowMessage("Limited Mode");
            OnMouseLeftButtonUp(e);
        }

        protected override int VisualChildrenCount => 1;
        
        protected override Visual GetVisualChild(int index) {
            return _contentPresenter;
        }
        
        protected override Size MeasureOverride(Size constraint) {
            _contentPresenter?.Measure(AdornedElement.RenderSize);
            return AdornedElement.RenderSize;
        }
        
        protected override Size ArrangeOverride(Size finalSize) {
            _contentPresenter?.Arrange(new Rect(finalSize));
            return finalSize;
        }
    }
}