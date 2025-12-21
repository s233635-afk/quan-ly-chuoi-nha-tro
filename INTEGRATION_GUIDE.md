# 🔧 INTEGRATION GUIDE - Modern Components

## Quick Start for Developers

### Overview
This guide shows you how to integrate the modern UI components into your forms quickly and easily.

---

## 📋 PREREQUISITE: Add Using Statements

```csharp
using quan_ly_chuoi_nha_tro.GUI;  // Modern components
using System;
using System.Windows.Forms;
using System.Drawing;
```

---

## 🎨 USING ModernTheme

### Apply to Form Background
```csharp
private void InitializeComponent()
{
    BackColor = ModernTheme.Colors.Background;
    Font = ModernTheme.Fonts.NormalFont;
}
```

### Style Buttons by Type
```csharp
// Primary action button
var btnSave = new Button { Text = "Save" };
ModernTheme.StylePrimaryButton(btnSave);

// Success button
var btnAdd = new Button { Text = "Add" };
ModernTheme.StyleSuccessButton(btnAdd);

// Error button
var btnDelete = new Button { Text = "Delete" };
ModernTheme.StyleErrorButton(btnDelete);

// Outline button
var btnCancel = new Button { Text = "Cancel" };
ModernTheme.StyleOutlineButton(btnCancel);
```

### Style Input Controls
```csharp
// TextBox
var txtName = new TextBox();
ModernTheme.StyleTextBox(txtName);

// ComboBox
var cboStatus = new ComboBox();
ModernTheme.StyleComboBox(cboStatus);

// DataGridView
var grid = new DataGridView { DataSource = data };
ModernTheme.StyleDataGridView(grid);
```

### Use Theme Colors in Labels
```csharp
var lblTitle = new Label
{
    Text = "Dashboard",
    Font = ModernTheme.Fonts.PageTitleFont,
    ForeColor = ModernTheme.Colors.TextPrimary
};

var lblSubtitle = new Label
{
    Text = "Welcome back",
    Font = ModernTheme.Fonts.SmallFont,
    ForeColor = ModernTheme.Colors.TextSecondary
};
```

---

## 🧩 USING ModernControls

### ModernButton (Replacements for System.Windows.Forms.Button)
```csharp
var btn = new ModernButton
{
    Text = "➕ Add New",
    BackColor = ModernTheme.Colors.Success,
    ForeColor = ModernTheme.Colors.TextInverse,
    Width = 120,
    Height = 36
};
btn.Click += (s, e) => { /* Action */ };
Controls.Add(btn);
```

### ModernCard (Container Panel)
```csharp
var card = new ModernCard
{
    Width = 400,
    Height = 200,
    Padding = new Padding(ModernTheme.Spacing.LG)
};

var cardTitle = new Label { Text = "Settings" };
card.Controls.Add(cardTitle);
Controls.Add(card);
```

### ModernStatCard (Statistics Display)
```csharp
var statCard = new ModernStatCard(
    title: "Total Rooms",
    value: "125",
    subtitle: "properties in system",
    accentColor: ModernTheme.Colors.Primary
)
{
    Width = 200,
    Height = 120
};
Controls.Add(statCard);

// Later, update the value
statCard.Value = "130";
statCard.Subtitle = "2 new rooms added";
```

### ModernTextBox (Styled Input)
```csharp
var txtInput = new ModernTextBox { Width = 250 };
Controls.Add(txtInput);
```

### ModernComboBox (Styled Dropdown)
```csharp
var cboOptions = new ModernComboBox { Width = 200 };
cboOptions.Items.AddRange(new[] { "Option 1", "Option 2", "Option 3" });
cboOptions.SelectedIndex = 0;
Controls.Add(cboOptions);
```

### ModernDataGridView (Styled Grid)
```csharp
var grid = new ModernDataGridView
{
    DataSource = myDataTable,
    ReadOnly = true,
    SelectionMode = DataGridViewSelectionMode.FullRowSelect,
    Dock = DockStyle.Fill
};
Controls.Add(grid);
```

---

## 🛠️ USING UIHelper

### Show Notifications
```csharp
// Show toast (3 seconds, success color)
UIHelper.ShowToast(this, "Saved successfully!", 3000, ModernTheme.Colors.Success);

// Error notification
UIHelper.ShowToast(this, "Error occurred!", 5000, ModernTheme.Colors.Error);

// Warning notification
UIHelper.ShowToast(this, "Warning: Check data!", 4000, ModernTheme.Colors.Warning);
```

### Create Layouts
```csharp
// Create a form layout with 4 rows, 2 columns
var layout = UIHelper.CreateFormLayout(rows: 4, columns: 2);
layout.Dock = DockStyle.Fill;

var lblName = new Label { Text = "Name:" };
var txtName = new TextBox { Width = 200 };
layout.Controls.Add(lblName, 0, 0);
layout.Controls.Add(txtName, 1, 0);

Controls.Add(layout);
```

### Create Search Box
```csharp
var searchBox = UIHelper.CreateSearchBox((s, e) =>
{
    // Filter when text changes
    ApplyFilter();
});
searchBox.Width = 250;
Controls.Add(searchBox);
```

### Create Section Panels
```csharp
var sectionPanel = UIHelper.CreateSectionPanel("Personal Information");
var content = new Label { Text = "Your content here" };
sectionPanel.Controls.Add(content);
Controls.Add(sectionPanel);
```

### Create Action Bars
```csharp
var actionBar = UIHelper.CreateActionBar(
    ("Save", ModernTheme.Colors.Success, (s, e) => OnSave()),
    ("Cancel", ModernTheme.Colors.Error, (s, e) => this.Close())
);
Controls.Add(actionBar);
```

### Create Info Boxes
```csharp
var infoBox = UIHelper.CreateInfoBox(
    label: "Total Amount",
    value: "1,250,000 VNĐ",
    accentColor: ModernTheme.Colors.Success
);
Controls.Add(infoBox);
```

### Create Badges
```csharp
var badgeActive = UIHelper.CreateBadge("Active", ModernTheme.Colors.Success);
var badgeInactive = UIHelper.CreateBadge("Inactive", ModernTheme.Colors.Error);
Controls.Add(badgeActive);
Controls.Add(badgeInactive);
```

### Enable/Disable Groups of Controls
```csharp
// Disable all controls during loading
UIHelper.EnableControls(this, false);

// ... Do work ...

// Re-enable
UIHelper.EnableControls(this, true);
```

---

## 📋 COMPLETE FORM EXAMPLE

Here's a complete modern form using all components:

```csharp
using quan_ly_chuoi_nha_tro.GUI;
using System;
using System.Windows.Forms;
using System.Drawing;

namespace quan_ly_chuoi_nha_tro.GUI
{
    public class FrmTenantEditorModern : Form
    {
        private AdminDataBLL _bll;
        private int? _tenantId;

        public FrmTenantEditorModern(AdminDataBLL bll, int? tenantId = null)
        {
            _bll = bll;
            _tenantId = tenantId;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            // Form setup
            Text = _tenantId.HasValue ? "Edit Tenant" : "New Tenant";
            Width = 700;
            Height = 600;
            StartPosition = FormStartPosition.CenterParent;
            BackColor = ModernTheme.Colors.Background;
            Font = ModernTheme.Fonts.NormalFont;

            // ========== HEADER ==========
            var headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = ModernTheme.Colors.Primary,
                Padding = new Padding(ModernTheme.Spacing.LG)
            };

            var headerTitle = new Label
            {
                Text = _tenantId.HasValue ? "✏️ EDIT TENANT" : "➕ NEW TENANT",
                Font = ModernTheme.Fonts.Bold(ModernTheme.Fonts.Heading2),
                ForeColor = ModernTheme.Colors.TextInverse,
                Dock = DockStyle.Left
            };

            headerPanel.Controls.Add(headerTitle);

            // ========== CONTENT ==========
            var formLayout = UIHelper.CreateFormLayout(rows: 5, columns: 2);

            // Row 1: Name
            var lblName = new Label { Text = "Full Name:" };
            var txtName = new ModernTextBox { Width = 250 };
            formLayout.Controls.Add(lblName, 0, 0);
            formLayout.Controls.Add(txtName, 1, 0);

            // Row 2: Phone
            var lblPhone = new Label { Text = "Phone Number:" };
            var txtPhone = new ModernTextBox { Width = 250 };
            formLayout.Controls.Add(lblPhone, 0, 1);
            formLayout.Controls.Add(txtPhone, 1, 1);

            // Row 3: Email
            var lblEmail = new Label { Text = "Email:" };
            var txtEmail = new ModernTextBox { Width = 250 };
            formLayout.Controls.Add(lblEmail, 0, 2);
            formLayout.Controls.Add(txtEmail, 1, 2);

            // Row 4: Room
            var lblRoom = new Label { Text = "Room:" };
            var cboRoom = new ModernComboBox { Width = 250 };
            cboRoom.Items.AddRange(new[] { "A101", "A102", "B201", "B202" });
            formLayout.Controls.Add(lblRoom, 0, 3);
            formLayout.Controls.Add(cboRoom, 1, 3);

            // Row 5: Status
            var lblStatus = new Label { Text = "Status:" };
            var cboStatus = new ModernComboBox { Width = 250 };
            cboStatus.Items.AddRange(new[] { "Active", "Inactive", "Suspended" });
            cboStatus.SelectedIndex = 0;
            formLayout.Controls.Add(lblStatus, 0, 4);
            formLayout.Controls.Add(cboStatus, 1, 4);

            // ========== ACTION BUTTONS ==========
            var actionBar = UIHelper.CreateActionBar(
                ("💾 Save", ModernTheme.Colors.Success, (s, e) =>
                {
                    UIHelper.ShowToast(this, "Tenant saved!", 3000, ModernTheme.Colors.Success);
                    DialogResult = DialogResult.OK;
                    Close();
                }),
                ("❌ Cancel", ModernTheme.Colors.Error, (s, e) =>
                {
                    DialogResult = DialogResult.Cancel;
                    Close();
                })
            );

            // ========== ASSEMBLE ==========
            Controls.Add(actionBar);
            Controls.Add(formLayout);
            Controls.Add(headerPanel);
        }
    }
}
```

---

## 🎯 REAL-WORLD PATTERNS

### Pattern 1: List with Add/Edit/Delete
```csharp
// Toolbar
var toolbar = new Panel { Dock = DockStyle.Top, Height = 50, BackColor = ModernTheme.Colors.Surface };
var btnAdd = UIHelper.CreateQuickActionButton("➕", "Add", ModernTheme.Colors.Success);
var btnEdit = UIHelper.CreateQuickActionButton("✏️", "Edit", ModernTheme.Colors.Primary);
var btnDelete = UIHelper.CreateQuickActionButton("🗑️", "Delete", ModernTheme.Colors.Error);
toolbar.Controls.Add(btnDelete);
toolbar.Controls.Add(btnEdit);
toolbar.Controls.Add(btnAdd);

// Grid
var grid = new ModernDataGridView { Dock = DockStyle.Fill, DataSource = data };

// Wire up events
btnAdd.Click += (s, e) => OpenAddDialog();
btnEdit.Click += (s, e) => OpenEditDialog(grid.SelectedRows[0]);
btnDelete.Click += (s, e) => DeleteRow(grid.SelectedRows[0]);

Controls.Add(grid);
Controls.Add(toolbar);
```

### Pattern 2: Filtered List
```csharp
// Filter bar
var filterPanel = new Panel { Dock = DockStyle.Top, Height = 50, BackColor = ModernTheme.Colors.Surface };
var search = UIHelper.CreateSearchBox((s, e) => ApplyFilters());
var cboFilter1 = new ModernComboBox { Width = 150 };
var cboFilter2 = new ModernComboBox { Width = 150 };

filterPanel.Controls.Add(cboFilter2);
filterPanel.Controls.Add(cboFilter1);
filterPanel.Controls.Add(search);

// Grid
var grid = new ModernDataGridView { Dock = DockStyle.Fill };

Controls.Add(grid);
Controls.Add(filterPanel);

void ApplyFilters()
{
    // Filter grid data based on search and combos
}
```

### Pattern 3: Statistics Dashboard
```csharp
// Create stat cards
var stats = new Control[]
{
    new ModernStatCard("Total Items", "1,234", "Items in system", ModernTheme.Colors.Card1),
    new ModernStatCard("In Stock", "890", "Available now", ModernTheme.Colors.Card2),
    new ModernStatCard("Low Stock", "344", "Need reorder", ModernTheme.Colors.Card3),
    new ModernStatCard("Out of Stock", "0", "None currently", ModernTheme.Colors.Card4)
};

// Arrange in grid
var statsPanel = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 150, AutoScroll = true };
foreach (var stat in stats)
    statsPanel.Controls.Add(stat);

Controls.Add(statsPanel);
```

---

## 🔑 KEY TAKEAWAYS

1. **Always use ModernTheme colors** - Don't hardcode colors
2. **Use modern controls** - They're pre-styled
3. **Use UIHelper for common patterns** - Save time and be consistent
4. **Apply theme to existing controls** - Use ModernTheme.Style* methods
5. **Follow spacing guidelines** - Use ModernTheme.Spacing constants
6. **Test across themes** - Ensure colors work together

---

## 🐛 TROUBLESHOOTING

### Colors look wrong
→ Make sure you're using ModernTheme.Colors.* not Color.FromArgb()

### Text is hard to read
→ Check contrast - use TextPrimary for main, TextSecondary for secondary

### Layout breaks on resize
→ Use Dock and AutoSize properties correctly

### Controls not visible
→ Check BackColor vs ForeColor - might be same color

### Performance slow
→ Avoid creating fonts repeatedly - use ModernTheme.Fonts predefined ones

---

## 📚 Additional Resources

- See `ModernTheme.cs` for all colors, fonts, spacing
- See `ModernControls.cs` for component source code
- See `UIHelper.cs` for utility method implementations
- Check example forms: `FrmPaymentManagerModern.cs`, `FrmRoomManagerModern.cs`

---

**Happy coding! 🚀**
