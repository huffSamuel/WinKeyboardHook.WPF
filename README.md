# WinKeyboardHook.WPF
---
A .NET managed wrapper for low-level keyboard events.


[![Nuget](https://img.shields.io/nuget/v/AoBD.WinKeyboardHook.WPF.svg)](https://www.nuget.org/packages/AobD.WinKeyboardHook.WPF/)

## Goals
This project was adapted from [Open.WinKeyboardHook](https://github.com/lontivero/Open.WinKeyboardHook) and intends to remove dependencies on Windows.Forms and offer some additional features.

## Usage
See below or the provided [demo](src/AobD.WinKeyboardHook.Demo/).

### ViewModel
---
```c#
using AobD.WinKeyboardHook.WPF;

class MyViewModel : INotifyPropetyChanged
{
    private IKeyboardInterceptor _interceptor;
    
    public MyViewModel()
    {
        _interceptor = new KeyboardInterceptor();
        _interceptor.KeyDown += OnKeyDown;
        _interceptor.KeyUp += OnKeyUp;
    }
    
    public void HookKeyboard()
    {
        _interceptor.StartCapturing();
    }
    
    public void UnhookKeyboard()
    {
        _interceptor.StopCapturing();
    }
    
    private void OnKeyUp(object sender, KeyEventArgs e)
    {
        // handle key up events here
    }
    
    private void OnKeyDown(object sender, KeyEventArgs e)
    {
        // handle key down events here
    }
}   
```

### View
---
Take careful note of the Window's MouseDown event; this is required to un-focus the control that will be capturing keyboard input.

```c#
// XAML
<Window MouseDown="Window_MouseDown">
    // Misc setup here
    <TextBox GotKeyboardFocus="tb_GotFocus"
             LostKeyboardFocus="tb_LostFocus"/>
</Window>

// Code-behind
private void tb_GotFocus(object sender, RoutedEventArgs e)
{
    (DataContext as MyViewModel).StartCapturing();
}

private void tb_LostFocus(object sender, RoutedEventArgs e)
{
    (DataContext as MyViewModel).StopCapturing();
}

private void Window_MouseDown(object sender, MouseButtonEventArgs e)
{
    Keyboard.ClearFocus();
}
```
## Support
Please submit issues via the issue tracker.

## Contributing
Pull requests are welcome. For major changes please open an issue first to discuss your proposed changes.
