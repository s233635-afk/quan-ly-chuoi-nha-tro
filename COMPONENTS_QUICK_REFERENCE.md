# 🎨 MODERN COMPONENTS QUICK REFERENCE

## ModernTheme

### Colors

**Primary Colors**
```csharp
ModernTheme.Colors.Primary          // #0078D7 - Main blue
ModernTheme.Colors.PrimaryDark      // #005AAA - Darker blue
ModernTheme.Colors.PrimaryLight     // #E6F3FF - Light blue background
```

**Secondary Colors**
```csharp
ModernTheme.Colors.Secondary        // #6B4FBB - Purple
ModernTheme.Colors.SecondaryLight   // #F3EDFF - Light purple
```

**Status Colors**
```csharp
ModernTheme.Colors.Success          // #2E7D32 - Green (✅ positive)
ModernTheme.Colors.SuccessLight     // #EDF7E9 - Light green
ModernTheme.Colors.Warning          // #FBC804 - Yellow (⚠️ warning)
ModernTheme.Colors.WarningLight     // #FFF9E7 - Light yellow
ModernTheme.Colors.Error            // #D32F2F - Red (❌ error)
ModernTheme.Colors.ErrorLight       // #FDedED - Light red
ModernTheme.Colors.Info             // #1967D2 - Blue (ℹ️ info)
ModernTheme.Colors.InfoLight        // #E3F2FD - Light blue
```

**Neutral Colors**
```csharp
ModernTheme.Colors.Background       // #FFFFFF - White
ModernTheme.Colors.Surface          // #F8F9FA - Light gray
ModernTheme.Colors.Border           // #E0E0E0 - Border gray
ModernTheme.Colors.Divider          // #E6E6E6 - Divider gray
```

**Text Colors**
```csharp
ModernTheme.Colors.TextPrimary      // #212121 - Main text (dark)
ModernTheme.Colors.TextSecondary    // #757575 - Secondary text
ModernTheme.Colors.TextTertiary     // #9E9E9E - Tertiary text
ModernTheme.Colors.TextDisabled     // #BDBDBD - Disabled text
ModernTheme.Colors.TextInverse      // #FFFFFF - Text on dark bg
```

**Theme Colors**
```csharp
ModernTheme.Colors.Card1            // #3F51B5 - Blue
ModernTheme.Colors.Card2            // #198754 - Green
ModernTheme.Colors.Card3            // #FF7043 - Orange
ModernTheme.Colors.Card4            // #E91E63 - Pink
ModernTheme.Colors.Card5            // #2196F3 - Light Blue
ModernTheme.Colors.Card6            // #9C27B0 - Purple
ModernTheme.Colors.Card7            // #FF9800 - Orange-Yellow
ModernTheme.Colors.Card8            // #4CAF50 - Light Green
```

### Fonts

**Sizes (in pt)**
```csharp
ModernTheme.Fonts.PageTitle         // 24pt - Main page title
ModernTheme.Fonts.Heading1          // 20pt - Large heading
ModernTheme.Fonts.Heading2          // 18pt - Medium heading
ModernTheme.Fonts.Heading3          // 16pt - Small heading
ModernTheme.Fonts.Large             // 13pt - Large text
ModernTheme.Fonts.Normal            // 11pt - Normal text
ModernTheme.Fonts.Small             // 9pt  - Small text
ModernTheme.Fonts.Tiny              // 8pt  - Tiny text
```

**Font Creation**
```csharp
ModernTheme.Fonts.Regular(12)       // Regular font at 12pt
ModernTheme.Fonts.Bold(14)          // Bold font at 14pt
ModernTheme.Fonts.Italic(10)        // Italic font at 10pt
ModernTheme.Fonts.SemiBold(13)      // Bold font at 13pt
```

**Predefined Fonts**
```csharp
ModernTheme.Fonts.PageTitleFont     // Bold 24pt
ModernTheme.Fonts.Heading1Font      // Bold 20pt
ModernTheme.Fonts.Heading2Font      // Bold 18pt
ModernTheme.Fonts.Heading3Font      // Bold 16pt
ModernTheme.Fonts.LargeFont         // Regular 13pt
ModernTheme.Fonts.NormalFont        // Regular 11pt
ModernTheme.Fonts.SmallFont         // Regular 9pt
ModernTheme.Fonts.TinyFont          // Regular 8pt
```

### Spacing

```csharp
ModernTheme.Spacing.XS              // 4px
ModernTheme.Spacing.SM              // 8px
ModernTheme.Spacing.MD              // 12px
ModernTheme.Spacing.LG              // 16px
ModernTheme.Spacing.XL              // 24px
ModernTheme.Spacing.XXL             // 32px
```

### Styling Methods

**Button Styling**
```csharp
ModernTheme.StyleButton(btn, color);           // Generic button
ModernTheme.StylePrimaryButton(btn);           // Primary style
ModernTheme.StyleSuccessButton(btn);           // Success style
ModernTheme.StyleErrorButton(btn);             // Error style
ModernTheme.StyleWarningButton(btn);           // Warning style
ModernTheme.StyleOutlineButton(btn, color);    // Outline style
```

**Control Styling**
```csharp
ModernTheme.StyleTextBox(textBox);
ModernTheme.StyleComboBox(comboBox);
ModernTheme.StyleDataGridView(grid);
ModernTheme.StyleLabel(label, fontSize, isBold);
ModernTheme.StylePanel(panel, bgColor);
```

**Helper Methods**
```csharp
ModernTheme.CreateCardPanel(width, height);
ModernTheme.CreateHeaderPanel(title);
ModernTheme.CreateToolbarPanel();
```

---

## ModernControls

### ModernButton
```csharp
var btn = new ModernButton
{
    Text = "Click Me",
    BackColor = ModernTheme.Colors.Primary,
    ForeColor = ModernTheme.Colors.TextInverse,
    Width = 120,
    Height = 36,
    HoverColor = ModernTheme.Colors.PrimaryDark,
    PressedColor = ModernTheme.Colors.PrimaryDark
};
```

### ModernCard
```csharp
var card = new ModernCard
{
    Width = 300,
    Height = 200,
    Padding = new Padding(ModernTheme.Spacing.LG)
};
card.Controls.Add(someControl);
```

### ModernStatCard
```csharp
var statCard = new ModernStatCard(
    title: "Total Users",
    value: "1,234",
    subtitle: "Active users",
    accentColor: ModernTheme.Colors.Success
);

// Update later
statCard.Title = "New Title";
statCard.Value = "2,000";
statCard.Subtitle = "Updated subtitle";
statCard.AccentColor = ModernTheme.Colors.Error;
```

### ModernTextBox
```csharp
var txtInput = new ModernTextBox
{
    Width = 250,
    Margin = new Padding(ModernTheme.Spacing.SM)
};
```

### ModernComboBox
```csharp
var cbo = new ModernComboBox { Width = 200 };
cbo.Items.AddRange(new[] { "Option 1", "Option 2", "Option 3" });
cbo.SelectedIndex = 0;
```

### ModernLabel
```csharp
var lbl = new ModernLabel(
    fontSize: ModernTheme.Fonts.Normal,
    isBold: true
);
lbl.Text = "Bold Label";
```

### ModernDataGridView
```csharp
var grid = new ModernDataGridView
{
    DataSource = myData,
    ReadOnly = true,
    SelectionMode = DataGridViewSelectionMode.FullRowSelect,
    Dock = DockStyle.Fill
};
```

### ModernPanel
```csharp
var panel = new ModernPanel
{
    Dock = DockStyle.Fill,
    Padding = new Padding(ModernTheme.Spacing.MD)
};
```

### ModernProgressBar
```csharp
var progress = new ModernProgressBar
{
    Minimum = 0,
    Maximum = 100,
    Value = 50,
    ForeColor = ModernTheme.Colors.Primary
};
progress.Value = 75; // Update
```

---

## UIHelper Methods

### Notifications
```csharp
// Show toast notification
UIHelper.ShowToast(form, message, durationMs, bgColor);

// Examples:
UIHelper.ShowToast(this, "Saved!", 3000, ModernTheme.Colors.Success);
UIHelper.ShowToast(this, "Error!", 5000, ModernTheme.Colors.Error);
UIHelper.ShowToast(this, "Warning!", 4000, ModernTheme.Colors.Warning);
```

### Layout Creation
```csharp
// Form layout grid
var layout = UIHelper.CreateFormLayout(rows: 5, columns: 2);
layout.Controls.Add(label, col, row);
layout.Controls.Add(control, col, row);

// Section panel with title
var section = UIHelper.CreateSectionPanel("Section Title");
section.Controls.Add(content);
```

### Dialog Creation
```csharp
// Modern dialog window
var dialog = UIHelper.CreateModernDialog("Title", width: 600, height: 400);
```

### Input Controls
```csharp
// Modern search box
var search = UIHelper.CreateSearchBox((s, e) => OnSearch());

// Quick action buttons
var btn = UIHelper.CreateQuickActionButton("📊", "Dashboard", 
    ModernTheme.Colors.Primary);
```

### Action Bars
```csharp
// Button row (bottom of form)
var actionBar = UIHelper.CreateActionBar(
    ("Save", ModernTheme.Colors.Success, (s, e) => Save()),
    ("Cancel", ModernTheme.Colors.Error, (s, e) => Cancel())
);
Controls.Add(actionBar);
```

### Information Display
```csharp
// Info box with label and value
var infoBox = UIHelper.CreateInfoBox(
    label: "Amount",
    value: "1,000,000 VNĐ",
    accentColor: ModernTheme.Colors.Success
);

// Badge labels
var badge = UIHelper.CreateBadge("New", ModernTheme.Colors.Error);
```

### Control Management
```csharp
// Enable/disable all controls in form
UIHelper.EnableControls(this, true);   // Enable
UIHelper.EnableControls(this, false);  // Disable
```

### Tabs
```csharp
// Modern tab control
var tabs = UIHelper.CreateModernTabControl();
tabs.Dock = DockStyle.Fill;
```

---

## COLOR COMBINATIONS (Good Pairings)

### Dashboard/Statistics
```csharp
Primary + PrimaryLight           // Header + content area
Success + SuccessLight          // Green theme
Error + ErrorLight              // Red theme
Warning + WarningLight          // Yellow theme
```

### Lists/Grids
```csharp
Surface + TextPrimary           // Background + text
Border + Divider                // Borders
PrimaryLight + Primary          // Selection highlight
```

### Forms
```csharp
Background + TextPrimary        // Form + labels
Surface + Border                // Input fields
Primary + TextInverse           // Buttons
```

### Cards/Panels
```csharp
Background + Border             // Card outline
Success + SuccessLight          // Success cards
Error + ErrorLight              // Error cards
Card1-8 + White                 // Stat cards
```

---

## USAGE EXAMPLES

### Simple Form with Button
```csharp
BackColor = ModernTheme.Colors.Background;

var btn = new ModernButton
{
    Text = "Save",
    BackColor = ModernTheme.Colors.Success
};
btn.Click += OnSave;
Controls.Add(btn);
```

### Grid with Toolbar
```csharp
// Toolbar
var toolbar = new Panel 
{ 
    Dock = DockStyle.Top, 
    Height = 50,
    BackColor = ModernTheme.Colors.Surface 
};

var btnAdd = UIHelper.CreateQuickActionButton("➕", "Add", 
    ModernTheme.Colors.Success);
toolbar.Controls.Add(btnAdd);

// Grid
var grid = new ModernDataGridView { Dock = DockStyle.Fill };

Controls.Add(grid);
Controls.Add(toolbar);
```

### Statistics Dashboard
```csharp
var stats = new Panel { Dock = DockStyle.Top, Height = 150 };
var flow = new FlowLayoutPanel { Dock = DockStyle.Fill };

flow.Controls.Add(new ModernStatCard("Items", "100", "In Stock", 
    ModernTheme.Colors.Card1));
flow.Controls.Add(new ModernStatCard("Orders", "25", "Pending", 
    ModernTheme.Colors.Card2));
flow.Controls.Add(new ModernStatCard("Revenue", "$50K", "This Month", 
    ModernTheme.Colors.Card3));

stats.Controls.Add(flow);
Controls.Add(stats);
```

### Input Form
```csharp
var layout = UIHelper.CreateFormLayout(rows: 3, columns: 2);

var lblName = new Label { Text = "Name:" };
var txtName = new ModernTextBox { Width = 250 };
layout.Controls.Add(lblName, 0, 0);
layout.Controls.Add(txtName, 1, 0);

var lblEmail = new Label { Text = "Email:" };
var txtEmail = new ModernTextBox { Width = 250 };
layout.Controls.Add(lblEmail, 0, 1);
layout.Controls.Add(txtEmail, 1, 1);

var actionBar = UIHelper.CreateActionBar(
    ("Submit", ModernTheme.Colors.Success, (s, e) => OnSubmit()),
    ("Reset", ModernTheme.Colors.Warning, (s, e) => OnReset())
);

Controls.Add(actionBar);
Controls.Add(layout);
```

---

## KEYBOARD SHORTCUTS (to implement)

```csharp
// Add to form
protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
{
    if (keyData == Keys.F5)
    {
        Refresh(); return true;
    }
    if (keyData == (Keys.Control | Keys.S))
    {
        Save(); return true;
    }
    return base.ProcessCmdKey(ref msg, keyData);
}
```

---

## PERFORMANCE TIPS

1. ✅ **Reuse fonts** - Use ModernTheme.Fonts predefined
2. ✅ **Reuse colors** - Use ModernTheme.Colors
3. ✅ **Use async** - Async/await for data loading
4. ✅ **Cache data** - Don't reload unnecessarily
5. ✅ **Lazy load** - Load images and data on demand
6. ✅ **Virtual scrolling** - For large datasets

---

**Last Updated:** December 21, 2025
