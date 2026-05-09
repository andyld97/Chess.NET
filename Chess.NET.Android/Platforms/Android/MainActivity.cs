using Android.App;
using Android.Content.PM;
using Android.Content.Res;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Views;
using AndroidX.Core.View;
using System.Diagnostics;
using A = Android;
using Activity = Android.App.Activity;

namespace Chess.NET.Android
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            StatusBarInsetsListener.SetStatusBarBackground(this);
        }
    }

    /// <summary>
    /// This is required to googles really cool idea that APPs targetting API = 35, edge to edge is automatically enabled:<br/>
    /// https://developer.android.com/develop/ui/views/layout/edge-to-edge#enable-edge-to-edge-display<br />
    /// Thanks google, I invested ~ 1 hour here to solve this, since the other method is deprecated (OF COURSE!!)
    /// </summary>
    public class StatusBarInsetsListener : Java.Lang.Object, IOnApplyWindowInsetsListener
    {
        private readonly Activity activity;

        public StatusBarInsetsListener(Activity activity)
        {
            this.activity = activity;
        }

        public WindowInsetsCompat OnApplyWindowInsets(A.Views.View v, WindowInsetsCompat insets)
        {
            var statusBarInsets = insets.GetInsets(WindowInsetsCompat.Type.StatusBars());
            var statusBarHeight = statusBarInsets.Top;

            v.SetPadding(0, statusBarHeight, 0, 0);

            v.Background = new ColorDrawable(new A.Graphics.Color(activity.Resources.GetColor(Resource.Color.colorPrimaryDark, null)));
            return insets;
        }

        public static void SetStatusBarBackground(Activity activity)
        {
            var decorView = activity.Window.DecorView;

            // Listener setzen
            ViewCompat.SetOnApplyWindowInsetsListener(decorView, new StatusBarInsetsListener(activity));
            ViewCompat.RequestApplyInsets(decorView);

            // Optional: Statusleisten-Icons anpassen
            var controller = WindowCompat.GetInsetsController(activity.Window, decorView);
            controller.AppearanceLightStatusBars = false; // false = weiße Icons
        }
    }

}
