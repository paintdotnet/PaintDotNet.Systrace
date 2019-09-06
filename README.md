## Systrace library from Paint.NET

This is a copy/fork of the code I'm using in Paint.NET to do simple runtime tracing in Paint.NET. It's not a complete library for emitting events, it only supports simple begin/end pairs. That's a great place to start, however, and you can get a lot done with it.

These trace files can then be loaded in Chrome by opening a new tab and navigating to `chrome://tracing`. Dragging and dropping trace files into that mini-app works!

### How to use

- Call `Systrace.Initialize(filePath)`. It can be good to do this during your app's startup, but it's not required.

- When you have something you want to trace/measure, utilize `using (Systrace.BeginEvent(name)) { ... }`

- Once you're done, either exit the process, or call `Systrace.Uninitialize()`. Either way is fine. Each event is flushed to disk as it's written. This may need optimizing at some point, e.g. using a dedicated thread for writing events with a `ConcurrentQueue` whatever.

### Performance tips

- The library is optimized to make sure there isn't really any performance cost when tracing isn't enabled. It should only be a `null` check and a pointer-size return value.

- When tracing is initialized, there are per-event allocations. So it may not be good to do a LOT of tracing, as you'll put pressure on the garbage collector. This can probably be optimized quite a bit, too.

- The name you give to `Systrace.BeginEvent()` should a `const` string. Otherwise you could end up with allocations even when tracing isn't enabled. To make sure a string is `const` you can use: 1) `const` strings, 2) inline string literals, 3) nameof() (which are compiled into string literals), and 4) `const` strings concatenated using the `+` operator (which are also compiled into string literals) (and by this I mean you can `+` together the other `const` string types and it's still a `const` string).

- Sometimes, however, you need to use strings that can't be const and must be crafted at runtime using allocations/concatenation. In this case I recommend using this form: `using (Systrace.BeginEvent(Systrace.IsTracing ? ("something had to be allocated: " + arg.ToString()) : string.Empty) { ... }`. This way at least you won't have the allocations when you're not tracing.

### Example

Starting with Paint.NET v4.2.2, you can use the `/enableTracing:filename` command-line argument to enable tracing. You can then open that in `chrome://tracing`, or just look at it in a text editor, to get a feel of the file format.

I've also included some sample traces in the `samples` directory.

Some code:

```
sealed class MainForm : Form
{
    private Button button;
    private Label label;

    // This is how I like to do tracing for a constructor
    public MainForm()
    {
        using (Systrace.BeginEvent(nameof(MainForm) + "::ctor"))
        {
            ...
            InitializeComponent();
            ...
        }
    }

    // Other times you don't really need the class prefix in the event name since the method is always called from the constructor
    private void InitializeComponent()
    {
        using (Systrace.BeginEvent(nameof(InitializeComponent)))
        {
            // You might want to trace for different segments of code
            using (Systrace.BeginEvent("instantiate controls"))
            {
                this.button = new Button();
                this.label = new Label();
            }

            using (Systrace.BeginEvent("initialize controls"))
            {
                this.button.Click += ...;
                this.button.Text = ...;

                this.label.Text = ...;
            }

            using (Systrace.BeginEvent("add controls"))
            {
                SuspendLayout();
                this.Controls.Add(this.button);
                this.Controls.Add(this.label);
                ResumeLayout();
            }
            ...
        }
    }

    // An example where you want a trace event for a specific method
    protected override void OnLayout(LayoutEventArgs levent)
    {
        using (Systrace.BeginEvent(nameof(MainForm) + "::" + nameof(OnLayout))) // "MainForm::OnLayout"
        {
            ... layout code goes here ...
            base.OnLayout(levent);
        }
    }

    // And sometimes you want a trace event that spans method calls ...
    private Systrace.BeginEventScope resizeTracingScope;

    protected override void OnResizeBegin(EventArgs e)
    {
        this.resizeTracingScope = Systrace.BeginEvent(nameof(MainForm) + "::Resize");
        ...
        base.OnResizeBegin(e);
    }

    protected override void OnResizeEnd(EventArgs e)
    {
        ...
        base.OnResizeBegin(e);
        this.resizeTracingScope.Dispose(); // no need to check for null because it's a struct and can't be null
    }
}
```

This is what it looks like, using one of the sample trace from the `samples` directory:

> ![screenshot](/images/screenshot1.png)

### License

MIT license.

### Links

- Trace Event Format documentation: https://docs.google.com/document/d/1CvAClvFfyA5R-PhYUmn5OOQtYMH4h6I0nSsKchNAySU/preview

- Twitter discussion: https://twitter.com/rickbrewPDN/status/1165359190149230592