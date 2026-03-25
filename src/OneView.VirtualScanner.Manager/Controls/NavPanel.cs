namespace OneView.VirtualScanner.Manager.Controls;

public class NavPanel : Panel
{
    public event EventHandler<string>? PageSelected;

    private readonly string[] _pages = { "Profiles", "Current Job", "Behavior", "Diagnostics" };
    private string _selected = "Profiles";
    private readonly List<Button> _buttons = new();

    public NavPanel()
    {
        Width = 150;
        Dock = DockStyle.Left;
        BackColor = Color.FromArgb(40, 40, 48);
        Padding = new Padding(0, 8, 0, 0);
        BuildButtons();
    }

    private void BuildButtons()
    {
        foreach (var page in _pages)
        {
            var btn = new Button
            {
                Text = page,
                Dock = DockStyle.Top,
                Height = 44,
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.Silver,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(12, 0, 0, 0),
                Font = new Font("Segoe UI", 9.5f),
                Cursor = Cursors.Hand,
                Tag = page
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(60, 60, 72);
            btn.Click += (_, _) => Select(page);
            _buttons.Add(btn);
        }

        // Add in reverse order because Dock=Top stacks bottom-up
        for (int i = _buttons.Count - 1; i >= 0; i--)
            Controls.Add(_buttons[i]);

        // Header label
        var header = new Label
        {
            Text = "OneView\nVirtual Scanner",
            Dock = DockStyle.Top,
            Height = 56,
            ForeColor = Color.White,
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Segoe UI", 9f, FontStyle.Bold),
            BackColor = Color.FromArgb(28, 28, 36)
        };
        Controls.Add(header);

        Highlight(_selected);
    }

    private void Select(string page)
    {
        _selected = page;
        Highlight(page);
        PageSelected?.Invoke(this, page);
    }

    private void Highlight(string page)
    {
        foreach (var b in _buttons)
        {
            bool active = (string)b.Tag! == page;
            b.BackColor = active ? Color.FromArgb(0, 120, 215) : Color.Transparent;
            b.ForeColor = active ? Color.White : Color.Silver;
        }
    }
}
