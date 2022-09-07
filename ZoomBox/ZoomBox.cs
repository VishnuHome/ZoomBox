using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace NetEti.CustomControls
{
    /// <summary>
    /// Stellt ein ContentControl mit zusätzlichen Eigenschaften zur Verfügung:
    /// Zoom-Funktionen über Strg-Mausrad, vertikales Scrollen über Mausrad,
    /// horizontales Scrollen über Umschalt-Mausrad, Normalansicht über rechte Maustaste,
    /// Komplettansicht über Strg-rechte Maustaste.
    /// </summary>
    /// <remarks>
    /// File: ZoomBox.cs
    /// Autor: Erik Nagel, NetEti
    ///<br></br>
    /// 23.07.2013 Erik Nagel: erstellt
    /// </remarks>
    public class ZoomBox : ContentControl
    {
        #region public members

        #region Properties

        /// <summary>
        /// Holt oder setzt die Sichtbarkeit eines optionalen horizontalen Scrollbalkens.
        /// </summary>
        public static DependencyProperty HorizontalScrollBarVisibilityProperty =
                ScrollViewer.HorizontalScrollBarVisibilityProperty.AddOwner(typeof(ZoomBox));
        /// <summary>
        /// Holt oder setzt die Sichtbarkeit eines optionalen horizontalen Scrollbalkens.
        /// </summary>
        public ScrollBarVisibility HorizontalScrollBarVisibility
        {
            get
            {
                return (ScrollBarVisibility)GetValue(HorizontalScrollBarVisibilityProperty);
            }
            set
            {
                SetValue(HorizontalScrollBarVisibilityProperty, value);
            }
        }

        /// <summary>
        /// Holt oder setzt die Sichtbarkeit eines optionalen vertikalen Scrollbalkens.
        /// </summary>
        public static DependencyProperty VerticalScrollBarVisibilityProperty =
            ScrollViewer.VerticalScrollBarVisibilityProperty.AddOwner(typeof(ZoomBox));
        /// <summary>
        /// Holt oder setzt die Sichtbarkeit eines optionalen vertikalen Scrollbalkens.
        /// </summary>
        public ScrollBarVisibility VerticalScrollBarVisibility
        {
            get
            {
                return (ScrollBarVisibility)GetValue(VerticalScrollBarVisibilityProperty);
            }
            set
            {
                SetValue(VerticalScrollBarVisibilityProperty, value);
            }
        }

        /// <summary>
        /// Holt oder setzt den minimalen Scale-Faktor.
        /// Der minimale Scale Faktor gibt an, wie stark Elemente verkleinert werden können.
        /// Dadurch wird indirekt auch die maximale Anzahl Elemente begrenzt, die sich
        /// gleichzeitig auf dem Bildschirm befinden können. Bei sehr großen Jobs mit sehr
        /// vielen Controls ist das für die Performance der WPF-Anwendung entscheidend.
        /// Default: 0.3.
        /// </summary>
        public double MinimalScaleFactor
        {
            get
            {
                return this._minimalScaleFactor;
            }
            set
            {
                this._minimalScaleFactor = value;
            }
        }

        /// <summary>
        /// Holt oder setzt die horizontale Scrollweite in geräteunabhängigen Pixeln.
        /// Default: 0.0.
        /// </summary>
        public double HorizontalScroll
        {
            get
            {
                return this._scrollViewer.HorizontalOffset;
            }
            set
            {
                this._scrollViewer.ScrollToHorizontalOffset(value);
            }
        }

        /// <summary>
        /// Holt oder setzt die vertikale Scrollweite in geräteunabhängigen Pixeln.
        /// Default: 0.0.
        /// </summary>
        public double VerticalScroll
        {
            get
            {
                return this._scrollViewer.VerticalOffset;
            }
            set
            {
                this._scrollViewer.ScrollToVerticalOffset(value);
            }
        }

        /// <summary>
        /// Returnt True, wenn die horizontale Scrollbar gerade sichtbar ist.
        /// </summary>
        public bool IsHorizontalScrollbarVisible
        {
            get
            {
                return this._scrollViewer.ComputedHorizontalScrollBarVisibility == Visibility.Visible;
            }
        }

        /// <summary>
        /// Returnt True, wenn die vertikale Scrollbar gerade sichtbar ist.
        /// </summary>
        public bool IsVerticalScrollbarVisible
        {
            get
            {
                return this._scrollViewer.ComputedVerticalScrollBarVisibility == Visibility.Visible;
            }
        }

        #endregion Properties

        /// <summary>
        /// Liefert die aktuellen Zoom-Einstellungen der ZoomBox.
        /// </summary>
        /// <returns>ScaleTransform mit horizontalem und vertikalem Vergößerungs-/Verkleinerungsfaktor (Originalgröße jeweils 1.0)</returns>
        public ScaleTransform GetScale()
        {
            return this._scaleTransform;
        }

        /// <summary>
        /// Bereitet das Setzen der aktuellen Zoom-Einstellungen der ZoomBox vor.
        /// Beim nächsten ZoomBox_LayoutUpdated werden diese dann übernommen.
        /// </summary>
        /// <param name="presetScaleTransform">Vorbelegung für den Vergößerungs-/Verkleinerungsfaktor (Originalgröße = 1.0, 1.0).</param>
        public void PresetScale(ScaleTransform presetScaleTransform)
        {
            this._presetScaleTransform = presetScaleTransform;
        }

        /// <summary>
        /// Setzt die aktuellen Zoom-Einstellungen der ZoomBox.
        /// </summary>
        /// <param name="newScaleX">Der horizontale Vergößerungs-/Verkleinerungsfaktor (Originalgröße = 1.0).</param>
        /// <param name="newScaleY">Der vertikale Vergößerungs-/Verkleinerungsfaktor (Originalgröße = 1.0).</param>
        public void SetScale(double newScaleX, double newScaleY)
        {
            if (newScaleX < this.MinimalScaleFactor)
            {
                newScaleX = this.MinimalScaleFactor;
            }
            if (newScaleY < this.MinimalScaleFactor)
            {
                newScaleY = this.MinimalScaleFactor;
            }
            if (this._scaleTransform != null)
            {
                this._scaleTransform.ScaleX = newScaleX;
                this._scaleTransform.ScaleY = newScaleY;

                if (this._scrollViewer != null)
                {
                    Point centerOfViewport = new Point(this._scrollViewer.ViewportWidth / 2, this._scrollViewer.ViewportHeight / 2);
                    this._lastCenterPositionOnTarget = this._scrollViewer.TranslatePoint(centerOfViewport, this._grid);
                }
            }
            else
            {
                this._scaleTransform = new ScaleTransform(newScaleX, newScaleY);
                this.PresetScale(new ScaleTransform(newScaleX, newScaleY));
            }
        }

        #endregion public members

        #region initialization

        static ZoomBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ZoomBox), new FrameworkPropertyMetadata(typeof(ZoomBox)));
        }

        /// <summary>
        /// Standard-Konstruktor.
        /// </summary>
        public ZoomBox()
        {
            // this.Initialized += this.initialized; // zu früh.
            //this.Loaded += this.loaded; // feuert nicht bei Browser-Page.
            this._initAspectsDone = false;
            this._presetScaleTransform = null;
            this.MinimalScaleFactor = 0.3;
            this.LayoutUpdated += ZoomBox_LayoutUpdated;
        }

        private ScrollViewer _scrollViewer;
        private ScaleTransform _scaleTransform;
        private ScaleTransform _presetScaleTransform;
        private Grid _grid;
        private Point? _lastCenterPositionOnTarget;
        private Point? lastMousePositionOnTarget;
        private bool _initAspectsDone;
        private double _minimalScaleFactor;

        private void initialized(object sender, System.EventArgs e)
        {
            // this.InitAspects(); // zu früh.
        }

        private void loaded(object sender, RoutedEventArgs e)
        {
            // this.InitAspects(); // feuert nicht bei Browser-Page.
        }

        private void ZoomBox_LayoutUpdated(object sender, System.EventArgs e)
        {
            if (!this._initAspectsDone)
            {
                this.InitAspects();
            }
            if (this._presetScaleTransform != null)
            {
                this.SetScale(this._presetScaleTransform.ScaleX, this._presetScaleTransform.ScaleY);
                this._presetScaleTransform = null;
            }
        }

        private void InitAspects()
        {
            this._scrollViewer = (ScrollViewer)this.Template?.FindName("scrollViewer", this);
            this._scaleTransform = (ScaleTransform)this.Template?.FindName("scaleTransform", this);
            this._grid = (Grid)this.Template?.FindName("grid", this);
            if (this._scrollViewer != null)
            {
                this._scrollViewer.ScrollChanged += OnScrollViewerScrollChanged;
                this._scrollViewer.PreviewMouseWheel += this.mouseWheelDispatcher;
                this._scrollViewer.PreviewMouseRightButtonDown += this.previewMouseRightButtonDown;
                this._initAspectsDone = true;
            }
        }

        #endregion initialization

        #region EventHandlers

        // Überschreibt OnPreviewMouseWheel für die Implementierung
        // der erweiterten Mausrad-Funtionen.
        // Mausrad gedreht:
        //   bei gedrückter Umschalt-Taste wird horizontal gescrollt,
        //   bei gedrückter Strg-Taste wird der Tree vergrößert (Rad nach oben)
        //       oder verkleinert (Rad nach unten),
        //   ansonsten wird vertikal gescrollt (Standard-Funktionalität).
        private void mouseWheelDispatcher(object sender, MouseWheelEventArgs e)
        {
            if ((Keyboard.Modifiers & ModifierKeys.Control) > 0)
            {
                this.transform(sender, e);
                e.Handled = true;
            }
            else
            {
                if ((Keyboard.Modifiers & ModifierKeys.Shift) > 0)
                {
                    this.horizontalScroll(e);
                }
                else
                {
                    this.verticalScroll(e);
                }
            }
        }

        /// <summary>
        /// Rechte Maustaste: 
        ///   zusammen mit Strg: verkleinert alles, sodass alles im Fenster sichtbar ist.
        ///   zusammen mit Umschalt: setzt alles auf Anfang - kein Zoom, Position links oben.
        /// </summary>
        /// <param name="sender">Element, in dem das Event zuerst auftritt.</param>
        /// <param name="e">Weitergehende Informationen zum Event.</param>
        private void previewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if ((Keyboard.Modifiers & ModifierKeys.Control) > 0)
            {
                this.transformAllIntoView();
            }
            else
            {
                if ((Keyboard.Modifiers & ModifierKeys.Shift) > 0)
                {
                    this.reset();
                }
            }
        }

        #endregion EventHandlers

        #region zoom

        private void transform(object sender, MouseWheelEventArgs e)
        {
            this.lastMousePositionOnTarget = Mouse.GetPosition(this._grid);

            if (e.Delta > 0)
            {
                //this._slider.Value += 1;
                //this.setNewScale(this._scaleTransform.ScaleX + .2, this._scaleTransform.ScaleY + .2);
                this.SetScale(this._scaleTransform.ScaleX * 1.2, this._scaleTransform.ScaleY * 1.2);
            }
            if (e.Delta < 0)
            {
                //this._slider.Value -= 1;
                //this.setNewScale(this._scaleTransform.ScaleX - .2, this._scaleTransform.ScaleY - .2);
                this.SetScale(this._scaleTransform.ScaleX / 1.2, this._scaleTransform.ScaleY / 1.2);
            }

            e.Handled = true;
        }

        private void transformAllIntoView()
        {
            double oldScaleX = this._scaleTransform.ScaleX;
            double oldScaleY = this._scaleTransform.ScaleY;
            double newScaleX = oldScaleX * this._scrollViewer.ViewportWidth / (this._scrollViewer.ExtentWidth);
            double newScaleY = oldScaleY * this._scrollViewer.ViewportHeight / (this._scrollViewer.ExtentHeight);
            double newScale = newScaleX < newScaleY ? newScaleX : newScaleY;
            this.SetScale(newScale, newScale);
            this._scrollViewer.ScrollToHome();
        }

        #endregion zoom

        #region scroll

        // Scrollt in der ZoomBox horizontal. Wird über Shift+Mousewheel ausgelöst.
        private void horizontalScroll(MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
            {
                this._scrollViewer.LineLeft();
                this._scrollViewer.LineLeft();
                this._scrollViewer.LineLeft();
            }
            else
            {
                this._scrollViewer.LineRight();
                this._scrollViewer.LineRight();
                this._scrollViewer.LineRight();
            }
            e.Handled = true;
        }

        // Scrollt in der ZoomBox horizontal. Wird über Mousewheel ausgelöst.
        private void verticalScroll(MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
            {
                this._scrollViewer.LineUp();
                this._scrollViewer.LineUp();
                this._scrollViewer.LineUp();
            }
            else
            {
                this._scrollViewer.LineDown();
                this._scrollViewer.LineDown();
                this._scrollViewer.LineDown();
            }
            e.Handled = true;
        }

        private void OnScrollViewerScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (e.ExtentHeightChange != 0 || e.ExtentWidthChange != 0)
            {
                // Hier kommt die Steuerung nur bei Aufklappen oder Zuklappen eines Knotens oder Größenänderung durch Strg+Mausrad an.
                // Rechts-links-Scrollen (Umschalt+Mausrad) oder hoch-runter-Scrollen (Mausrad) kommt hier nicht hin.
                Point? targetBefore = null;
                Point? targetNow = null;

                if (!this.lastMousePositionOnTarget.HasValue)
                {
                    /*
                    // Aufklappen oder Zuklappen - hier liegt die Fehlerquelle (Bildschirm springt)
                    if (this.lastCenterPositionOnTarget.HasValue)
                    {
                        Point centerOfViewport = new Point(this._scrollViewer.ViewportWidth / 2, this._scrollViewer.ViewportHeight / 2);
                        Point centerOfTargetNow = this._scrollViewer.TranslatePoint(centerOfViewport, this._grid);

                        targetBefore = this.lastCenterPositionOnTarget;
                        // 13.10.2018 Erik Nagel+- auskommentiert, führte zu unkontrollierten Sprüngen der Anzeige // targetNow = centerOfTargetNow;
                        // 14.10.2018 doch wieder ohne Bedingung, muss in der Berechnung gelöst werrden
                        //if (!targetNow.HasValue) // 13.10.2018 Erik Nagel+
                        //{
                            targetNow = centerOfTargetNow;
                        //} // 13.10.2018 Erik Nagel-
                    }
                    */
                }
                else
                {
                    // Größenänderung durch Strg-Mausrad
                    targetBefore = this.lastMousePositionOnTarget;
                    targetNow = Mouse.GetPosition(this._grid);

                    this.lastMousePositionOnTarget = null;
                }

                if (targetBefore.HasValue && targetNow.HasValue)
                {
                    double dXInTargetPixels = targetNow.Value.X - targetBefore.Value.X;
                    double dYInTargetPixels = targetNow.Value.Y - targetBefore.Value.Y;

                    //double multiplicatorX = e.ExtentWidth / this._grid.Width;
                    //double multiplicatorY = e.ExtentHeight / this._grid.Height;
                    double multiplicatorX = this._scaleTransform.ScaleX;
                    double multiplicatorY = this._scaleTransform.ScaleY;

                    double newOffsetX = this._scrollViewer.HorizontalOffset - dXInTargetPixels * multiplicatorX;
                    double newOffsetY = this._scrollViewer.VerticalOffset - dYInTargetPixels * multiplicatorY;

                    if (double.IsNaN(newOffsetX) || double.IsNaN(newOffsetY))
                    {
                        return;
                    }

                    this._scrollViewer.ScrollToHorizontalOffset(newOffsetX);
                    this._scrollViewer.ScrollToVerticalOffset(newOffsetY);
                }
            }
        }

        /*
        private void OnScrollViewerScrollChanged(object sender, ScrollChangedEventArgs e)
        {
          if (e.ExtentHeightChange != 0 || e.ExtentWidthChange != 0)
          {
            Point? targetBefore = null;
            Point? targetNow = null;

            if (!lastMousePositionOnTarget.HasValue)
            {
              if (lastCenterPositionOnTarget.HasValue)
              {
                var centerOfViewport = new Point(this._scrollViewer.ViewportWidth / 2, this._scrollViewer.ViewportHeight / 2);
                Point centerOfTargetNow = this._scrollViewer.TranslatePoint(centerOfViewport, this._grid);

                targetBefore = lastCenterPositionOnTarget;
                targetNow = centerOfTargetNow;
              }
            }
            else
            {
              targetBefore = lastMousePositionOnTarget;
              targetNow = Mouse.GetPosition(this._grid);

              lastMousePositionOnTarget = null;
            }

            if (targetBefore.HasValue)
            {
              double dXInTargetPixels = targetNow.Value.X - targetBefore.Value.X;
              double dYInTargetPixels = targetNow.Value.Y - targetBefore.Value.Y;

              double multiplicatorX = e.ExtentWidth / this._grid.Width;
              double multiplicatorY = e.ExtentHeight / this._grid.Height;

              double newOffsetX = this._scrollViewer.HorizontalOffset - dXInTargetPixels * multiplicatorX;
              double newOffsetY = this._scrollViewer.VerticalOffset - dYInTargetPixels * multiplicatorY;

              if (double.IsNaN(newOffsetX) || double.IsNaN(newOffsetY))
              {
                return;
              }

              this._scrollViewer.ScrollToHorizontalOffset(newOffsetX);
              this._scrollViewer.ScrollToVerticalOffset(newOffsetY);
            }
          }
        }
        */

        #endregion scroll

        #region reset layout

        // Setzt die TreeView auf den Ursprungszustand
        // zurück (kein Zoom, Position links oben).
        private void reset()
        {
            lastMousePositionOnTarget = Mouse.GetPosition(this._grid);
            this.SetScale(1.0, 1.0);
            this._scrollViewer.ScrollToHome();
            lastMousePositionOnTarget = null; // 14.10.2018 Erik Nagel+-
        }

        #endregion reset layout
    }
}
