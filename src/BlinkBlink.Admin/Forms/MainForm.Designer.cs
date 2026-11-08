using System.Drawing;
using System.Windows.Forms;

namespace BlinkBlink.Admin.Forms;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
        panelSidebar = new Panel();
        btnReports = new Button();
        btnUsers = new Button();
        btnWarranty = new Button();
        btnProducts = new Button();
        btnSales = new Button();
        lblBrand = new Label();
        panelHeader = new Panel();
        lblHeaderTitle = new Label();
        lblUserRole = new Label();
        panelContent = new Panel();
        lblContentPlaceholder = new Label();
        statusStrip = new StatusStrip();
        statusLabel = new ToolStripStatusLabel();
        panelSidebar.SuspendLayout();
        panelHeader.SuspendLayout();
        panelContent.SuspendLayout();
        statusStrip.SuspendLayout();
        SuspendLayout();
        // 
        // panelSidebar
        // 
        panelSidebar.BackColor = Color.FromArgb(24, 28, 36);
        panelSidebar.Controls.Add(btnReports);
        panelSidebar.Controls.Add(btnUsers);
        panelSidebar.Controls.Add(btnWarranty);
        panelSidebar.Controls.Add(btnProducts);
        panelSidebar.Controls.Add(btnSales);
        panelSidebar.Controls.Add(lblBrand);
        panelSidebar.Dock = DockStyle.Left;
        panelSidebar.Location = new Point(0, 0);
        panelSidebar.Margin = new Padding(4, 4, 4, 4);
        panelSidebar.Name = "panelSidebar";
        panelSidebar.Size = new Size(350, 1068);
        panelSidebar.TabIndex = 0;
        // 
        // btnReports
        // 
        btnReports.BackColor = Color.FromArgb(24, 28, 36);
        btnReports.Cursor = Cursors.Hand;
        btnReports.FlatAppearance.BorderSize = 0;
        btnReports.FlatStyle = FlatStyle.Flat;
        btnReports.Font = new Font("Segoe UI", 10F);
        btnReports.ForeColor = Color.FromArgb(220, 225, 235);
        btnReports.Location = new Point(0, 376);
        btnReports.Margin = new Padding(4, 4, 4, 4);
        btnReports.Name = "btnReports";
        btnReports.Padding = new Padding(22, 0, 10, 0);
        btnReports.Size = new Size(350, 62);
        btnReports.TabIndex = 5;
        btnReports.Text = "📊 Báo Cáo & Thống Kê";
        btnReports.TextAlign = ContentAlignment.MiddleLeft;
        btnReports.UseMnemonic = false;
        btnReports.UseVisualStyleBackColor = false;
        // 
        // btnUsers
        // 
        btnUsers.BackColor = Color.FromArgb(24, 28, 36);
        btnUsers.Cursor = Cursors.Hand;
        btnUsers.FlatAppearance.BorderSize = 0;
        btnUsers.FlatStyle = FlatStyle.Flat;
        btnUsers.Font = new Font("Segoe UI", 10F);
        btnUsers.ForeColor = Color.FromArgb(220, 225, 235);
        btnUsers.Location = new Point(0, 309);
        btnUsers.Margin = new Padding(4, 4, 4, 4);
        btnUsers.Name = "btnUsers";
        btnUsers.Padding = new Padding(22, 0, 10, 0);
        btnUsers.Size = new Size(350, 62);
        btnUsers.TabIndex = 4;
        btnUsers.Text = "👥 Khách Hàng & Nhân Sự";
        btnUsers.TextAlign = ContentAlignment.MiddleLeft;
        btnUsers.UseMnemonic = false;
        btnUsers.UseVisualStyleBackColor = false;
        // 
        // btnWarranty
        // 
        btnWarranty.BackColor = Color.FromArgb(24, 28, 36);
        btnWarranty.Cursor = Cursors.Hand;
        btnWarranty.FlatAppearance.BorderSize = 0;
        btnWarranty.FlatStyle = FlatStyle.Flat;
        btnWarranty.Font = new Font("Segoe UI", 10F);
        btnWarranty.ForeColor = Color.FromArgb(220, 225, 235);
        btnWarranty.Location = new Point(0, 241);
        btnWarranty.Margin = new Padding(4, 4, 4, 4);
        btnWarranty.Name = "btnWarranty";
        btnWarranty.Padding = new Padding(22, 0, 10, 0);
        btnWarranty.Size = new Size(350, 62);
        btnWarranty.TabIndex = 3;
        btnWarranty.Text = "🛡️ Dịch Vụ & Bảo Hành";
        btnWarranty.TextAlign = ContentAlignment.MiddleLeft;
        btnWarranty.UseMnemonic = false;
        btnWarranty.UseVisualStyleBackColor = false;
        // 
        // btnProducts
        // 
        btnProducts.BackColor = Color.FromArgb(24, 28, 36);
        btnProducts.Cursor = Cursors.Hand;
        btnProducts.FlatAppearance.BorderSize = 0;
        btnProducts.FlatStyle = FlatStyle.Flat;
        btnProducts.Font = new Font("Segoe UI", 10F);
        btnProducts.ForeColor = Color.FromArgb(220, 225, 235);
        btnProducts.Location = new Point(0, 174);
        btnProducts.Margin = new Padding(4, 4, 4, 4);
        btnProducts.Name = "btnProducts";
        btnProducts.Padding = new Padding(22, 0, 10, 0);
        btnProducts.Size = new Size(350, 62);
        btnProducts.TabIndex = 2;
        btnProducts.Text = "💎 Sản Phẩm & Kho";
        btnProducts.TextAlign = ContentAlignment.MiddleLeft;
        btnProducts.UseMnemonic = false;
        btnProducts.UseVisualStyleBackColor = false;
        // 
        // btnSales
        // 
        btnSales.BackColor = Color.FromArgb(24, 28, 36);
        btnSales.Cursor = Cursors.Hand;
        btnSales.FlatAppearance.BorderSize = 0;
        btnSales.FlatStyle = FlatStyle.Flat;
        btnSales.Font = new Font("Segoe UI", 10F);
        btnSales.ForeColor = Color.FromArgb(220, 225, 235);
        btnSales.Location = new Point(0, 106);
        btnSales.Margin = new Padding(4, 4, 4, 4);
        btnSales.Name = "btnSales";
        btnSales.Padding = new Padding(22, 0, 10, 0);
        btnSales.Size = new Size(350, 62);
        btnSales.TabIndex = 1;
        btnSales.Text = "\U0001f6d2 Quản Lý Bán Hàng";
        btnSales.TextAlign = ContentAlignment.MiddleLeft;
        btnSales.UseMnemonic = false;
        btnSales.UseVisualStyleBackColor = false;
        // 
        // lblBrand
        // 
        lblBrand.Dock = DockStyle.Top;
        lblBrand.Font = new Font("Segoe UI", 13F, FontStyle.Bold);
        lblBrand.ForeColor = Color.FromArgb(212, 175, 55);
        lblBrand.Location = new Point(0, 0);
        lblBrand.Margin = new Padding(4, 0, 4, 0);
        lblBrand.Name = "lblBrand";
        lblBrand.Size = new Size(350, 100);
        lblBrand.TabIndex = 0;
        lblBrand.Text = "BLINK BLINK\nJEWELRY";
        lblBrand.TextAlign = ContentAlignment.MiddleCenter;
        lblBrand.UseMnemonic = false;
        // 
        // panelHeader
        // 
        panelHeader.BackColor = Color.FromArgb(32, 38, 48);
        panelHeader.Controls.Add(lblHeaderTitle);
        panelHeader.Controls.Add(lblUserRole);
        panelHeader.Dock = DockStyle.Top;
        panelHeader.Location = new Point(350, 0);
        panelHeader.Margin = new Padding(4, 4, 4, 4);
        panelHeader.Name = "panelHeader";
        panelHeader.Size = new Size(1500, 75);
        panelHeader.TabIndex = 1;
        // 
        // lblHeaderTitle
        // 
        lblHeaderTitle.AutoSize = true;
        lblHeaderTitle.Dock = DockStyle.Left;
        lblHeaderTitle.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
        lblHeaderTitle.ForeColor = Color.White;
        lblHeaderTitle.Location = new Point(0, 0);
        lblHeaderTitle.Margin = new Padding(4, 0, 4, 0);
        lblHeaderTitle.Name = "lblHeaderTitle";
        lblHeaderTitle.Padding = new Padding(25, 0, 0, 0);
        lblHeaderTitle.Size = new Size(451, 32);
        lblHeaderTitle.TabIndex = 0;
        lblHeaderTitle.Text = "Bảng Điều Khiển Hệ Thống Quản Trị";
        lblHeaderTitle.TextAlign = ContentAlignment.MiddleLeft;
        lblHeaderTitle.UseMnemonic = false;
        // 
        // lblUserRole
        // 
        lblUserRole.AutoSize = true;
        lblUserRole.Dock = DockStyle.Right;
        lblUserRole.Font = new Font("Segoe UI", 9.5F);
        lblUserRole.ForeColor = Color.FromArgb(170, 180, 195);
        lblUserRole.Location = new Point(1030, 0);
        lblUserRole.Margin = new Padding(4, 0, 4, 0);
        lblUserRole.Name = "lblUserRole";
        lblUserRole.Padding = new Padding(0, 0, 25, 0);
        lblUserRole.Size = new Size(470, 25);
        lblUserRole.TabIndex = 1;
        lblUserRole.Text = "Tài khoản: Quản Trị Viên (Admin) | .NET 8 WinForms";
        lblUserRole.TextAlign = ContentAlignment.MiddleRight;
        lblUserRole.UseMnemonic = false;
        // 
        // panelContent
        // 
        panelContent.BackColor = Color.FromArgb(245, 247, 250);
        panelContent.Controls.Add(lblContentPlaceholder);
        panelContent.Dock = DockStyle.Fill;
        panelContent.Location = new Point(350, 75);
        panelContent.Margin = new Padding(4, 4, 4, 4);
        panelContent.Name = "panelContent";
        panelContent.Size = new Size(1500, 993);
        panelContent.TabIndex = 2;
        // 
        // lblContentPlaceholder
        // 
        lblContentPlaceholder.Dock = DockStyle.Fill;
        lblContentPlaceholder.Font = new Font("Segoe UI", 11F);
        lblContentPlaceholder.ForeColor = Color.FromArgb(140, 150, 165);
        lblContentPlaceholder.Location = new Point(0, 0);
        lblContentPlaceholder.Margin = new Padding(4, 0, 4, 0);
        lblContentPlaceholder.Name = "lblContentPlaceholder";
        lblContentPlaceholder.Size = new Size(1500, 993);
        lblContentPlaceholder.TabIndex = 0;
        lblContentPlaceholder.Text = resources.GetString("lblContentPlaceholder.Text");
        lblContentPlaceholder.TextAlign = ContentAlignment.MiddleCenter;
        // 
        // statusStrip
        // 
        statusStrip.ImageScalingSize = new Size(24, 24);
        statusStrip.Items.AddRange(new ToolStripItem[] { statusLabel });
        statusStrip.Location = new Point(0, 1068);
        statusStrip.Name = "statusStrip";
        statusStrip.Padding = new Padding(1, 0, 18, 0);
        statusStrip.Size = new Size(1850, 32);
        statusStrip.TabIndex = 3;
        // 
        // statusLabel
        // 
        statusLabel.Name = "statusLabel";
        statusLabel.Size = new Size(404, 25);
        statusLabel.Text = "Hệ thống sẵn sàng | Clean Architecture .NET 8 LTS";
        // 
        // MainForm
        // 
        AutoScaleDimensions = new SizeF(10F, 25F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1850, 1100);
        Controls.Add(panelContent);
        Controls.Add(panelHeader);
        Controls.Add(panelSidebar);
        Controls.Add(statusStrip);
        Margin = new Padding(4, 4, 4, 4);
        MinimumSize = new Size(1594, 886);
        Name = "MainForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Blink Blink Jewelry - Quản Trị Hệ Thống";
        WindowState = FormWindowState.Maximized;
        panelSidebar.ResumeLayout(false);
        panelHeader.ResumeLayout(false);
        panelHeader.PerformLayout();
        panelContent.ResumeLayout(false);
        statusStrip.ResumeLayout(false);
        statusStrip.PerformLayout();
        ResumeLayout(false);
        PerformLayout();
    }

    private Panel panelSidebar;
    private Label lblBrand;
    private Button btnSales;
    private Button btnProducts;
    private Button btnWarranty;
    private Button btnUsers;
    private Button btnReports;
    private Panel panelHeader;
    private Label lblHeaderTitle;
    private Label lblUserRole;
    private Panel panelContent;
    private Label lblContentPlaceholder;
    private StatusStrip statusStrip;
    private ToolStripStatusLabel statusLabel;
}
