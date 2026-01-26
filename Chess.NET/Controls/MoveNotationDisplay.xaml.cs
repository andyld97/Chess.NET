using Chess.NET.Model;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

namespace Chess.NET.Controls
{
    /// <summary>
    /// Interaktionslogik für MoveNotationDisplay.xaml
    /// </summary>
    public partial class MoveNotationDisplay : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private MoveNotation move1 = null!, move2 = null!;

        public delegate void onJumpToMove(int index);
        public event onJumpToMove? OnJumpToMove;

        public MoveNotation Move1
        {
            get => move1;
            set
            {
                if (value != move1)
                {
                    move1 = value;
                    PanelMove1.DataContext = Move1;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Move1)));
                }
            }
        }

        public MoveNotation Move2
        {
            get => move2;
            set
            {
                if (value != move2)
                {
                    move2 = value;
                    PanelMove2.DataContext = Move2;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Move2)));

                    if (move2 != null)
                        PanelMove2.Visibility = System.Windows.Visibility.Visible;
                }
            }
        }

        public MoveNotationDisplay(bool isStart = false)
        {
            InitializeComponent();
            PanelMove1.DataContext = Move1;
            PanelMove2.DataContext = Move2;

            PanelMove2.Visibility = System.Windows.Visibility.Collapsed;
            if (isStart)
            {
                PanelMove1.Visibility = System.Windows.Visibility.Collapsed;
                PanelBegin.Visibility = System.Windows.Visibility.Visible;
            }
        }

        private void PanelMove1_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (Move1 == null)
                return;

            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
                OnJumpToMove?.Invoke(Move1.Count - 1);
        }

        private void PanelBegin_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
                OnJumpToMove?.Invoke(-1);
        }

        private void PanelMove2_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (Move2 == null)
                return;

            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
                OnJumpToMove?.Invoke(Move2.Count - 1);
        }
    }

    #region Converter

    public class MoveToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is MoveNotation mv)
                return mv.Piece.Type.ToBitmap(mv.Piece.Color);

            return null!;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class MoveToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is MoveNotation mv)
                return mv.FormatMove(false, true);

            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class MoveCountConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int n)
                return (n + 1) / 2;

            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    #endregion
}
