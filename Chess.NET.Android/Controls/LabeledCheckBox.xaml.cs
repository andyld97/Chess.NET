using System.ComponentModel;

namespace Chess.NET.Android.Controls;

public partial class LabeledCheckBox : ContentView
{
    public static readonly BindableProperty IsCheckedProperty = BindableProperty.Create(nameof(IsChecked), typeof(bool), typeof(LabeledCheckBox), false, BindingMode.TwoWay);
    public static readonly BindableProperty TextProperty = BindableProperty.Create(nameof(Text), typeof(string), typeof(LabeledCheckBox), string.Empty);

    public bool IsChecked
    {
        get => (bool)GetValue(IsCheckedProperty);
        set => SetValue(IsCheckedProperty, value);
    }

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public LabeledCheckBox()
    {
        InitializeComponent();
        Container.BindingContext = this;
    }

    private void OnTapped(object? sender, TappedEventArgs e) => IsChecked = !IsChecked;
}