using System.ComponentModel;

namespace Chess.NET.Android.Controls.Dialogs;

public partial class DialogHeader : ContentView
{
    public static readonly BindableProperty TitleProperty = BindableProperty.Create(nameof(Title), typeof(string), typeof(DialogHeader), string.Empty);

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public event EventHandler? CloseRequested;

    public DialogHeader()
    {
        InitializeComponent();
        Container.BindingContext = this;
    }

    private void OnCloseTapped(object? sender, TappedEventArgs e) => CloseRequested?.Invoke(this, EventArgs.Empty);
}