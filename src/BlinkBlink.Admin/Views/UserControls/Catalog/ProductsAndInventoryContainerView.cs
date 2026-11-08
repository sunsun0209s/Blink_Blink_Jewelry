using System;
using System.Drawing;
using System.Windows.Forms;
using BlinkBlink.Admin.Views.Helpers;
using BlinkBlink.Admin.Views.UserControls.Inventory;

namespace BlinkBlink.Admin.Views.UserControls.Catalog;

/// <summary>
/// Giao diện Container tích hợp 3 phân hệ cốt lõi: Quản lý sản phẩm, Danh mục trang sức và Quản lý nhập kho.
/// Thiết kế thanh Sub-navigation tabs sang trọng chuẩn Blink Blink Jewelry theo Taste Skill.
/// </summary>
public class ProductsAndInventoryContainerView : UserControl
{
    private readonly Panel _pnlTabsBar;
    private readonly Panel _pnlContentHost;

    private readonly Button _btnTabProducts;
    private readonly Button _btnTabProductGroups;
    private readonly Button _btnTabCategories;
    private readonly Button _btnTabInventory;
    private readonly Button _btnTabAudit;

    private Button? _currentActiveTab;
    private UserControl? _currentActiveView;

    private ProductsView? _productsView;
    private ProductGroupsView? _productGroupsView;
    private CategoryView? _categoryView;
    private InventoryInwardView? _inventoryView;
    private InventoryAuditView? _inventoryAuditView;

    public ProductsAndInventoryContainerView()
    {
        this.Dock = DockStyle.Fill;
        this.BackColor = UITheme.CanvasBackground;
        this.Font = UITheme.RegularFont;

        // Sub-tabs Bar
        _pnlTabsBar = new Panel
        {
            Dock = DockStyle.Top,
            Height = 48,
            BackColor = UITheme.DarkHeader,
            Padding = new Padding(16, 4, 16, 0)
        };

        var flpTabs = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            BackColor = Color.Transparent
        };

        _btnTabProducts = CreateSubTabButton("💎 Quản Lý Sản Phẩm Trang Sức");
        _btnTabProductGroups = CreateSubTabButton("🏷️ Quản Lý Nhóm Sản Phẩm");
        _btnTabCategories = CreateSubTabButton("📁 Danh Mục Sản Phẩm");
        _btnTabInventory = CreateSubTabButton("📦 Quản Lý Nhập Kho Hàng");
        _btnTabAudit = CreateSubTabButton("📋 Kiểm Kê Kho Hàng");

        _btnTabProducts.Click += (s, e) => SwitchTab(_btnTabProducts, GetProductsView());
        _btnTabProductGroups.Click += (s, e) => SwitchTab(_btnTabProductGroups, GetProductGroupsView());
        _btnTabCategories.Click += (s, e) => SwitchTab(_btnTabCategories, GetCategoryView());
        _btnTabInventory.Click += (s, e) => SwitchTab(_btnTabInventory, GetInventoryView());
        _btnTabAudit.Click += (s, e) => SwitchTab(_btnTabAudit, GetInventoryAuditView());

        flpTabs.Controls.Add(_btnTabProducts);
        flpTabs.Controls.Add(_btnTabProductGroups);
        flpTabs.Controls.Add(_btnTabCategories);
        flpTabs.Controls.Add(_btnTabInventory);
        flpTabs.Controls.Add(_btnTabAudit);
        _pnlTabsBar.Controls.Add(flpTabs);

        // Content Host Panel
        _pnlContentHost = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = UITheme.CanvasBackground
        };

        this.Controls.Add(_pnlContentHost);
        this.Controls.Add(_pnlTabsBar);

        // Khởi động tại tab Quản lý Sản Phẩm
        SwitchTab(_btnTabProducts, GetProductsView());
    }

    private Button CreateSubTabButton(string text)
    {
        var btn = new Button
        {
            Text = text,
            Height = 40,
            AutoSize = true,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
            ForeColor = Color.FromArgb(203, 213, 225),
            BackColor = Color.Transparent,
            Cursor = Cursors.Hand,
            UseMnemonic = false,
            Margin = new Padding(0, 0, 8, 0),
            Padding = new Padding(16, 0, 16, 0)
        };
        btn.FlatAppearance.BorderSize = 0;
        btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(40, 48, 62);
        return btn;
    }

    private ProductsView GetProductsView() => _productsView ??= new ProductsView();
    private ProductGroupsView GetProductGroupsView() => _productGroupsView ??= new ProductGroupsView();
    private CategoryView GetCategoryView() => _categoryView ??= new CategoryView();
    private InventoryInwardView GetInventoryView() => _inventoryView ??= new InventoryInwardView();
    private InventoryAuditView GetInventoryAuditView() => _inventoryAuditView ??= new InventoryAuditView();

    private void SwitchTab(Button tabButton, UserControl targetView)
    {
        if (_currentActiveTab != null)
        {
            _currentActiveTab.ForeColor = Color.FromArgb(203, 213, 225);
            _currentActiveTab.BackColor = Color.Transparent;
        }

        _currentActiveTab = tabButton;
        _currentActiveTab.ForeColor = UITheme.PrimaryGold;
        _currentActiveTab.BackColor = Color.FromArgb(40, 48, 62);

        _pnlContentHost.SuspendLayout();
        _pnlContentHost.Controls.Clear();
        targetView.Dock = DockStyle.Fill;
        _pnlContentHost.Controls.Add(targetView);
        _pnlContentHost.ResumeLayout(true);

        _currentActiveView = targetView;

        // Tự động làm mới dữ liệu liên phân hệ để đảm bảo tính nhất quán
        if (targetView is ProductsView pv)
        {
            pv.LoadProductsData();
        }
        else if (targetView is ProductGroupsView pgv)
        {
            pgv.LoadGroupData();
        }
        else if (targetView is CategoryView cv)
        {
            cv.LoadCategoryData();
        }
        else if (targetView is InventoryInwardView iv)
        {
            iv.LoadReceiptsData();
        }
        else if (targetView is InventoryAuditView iav)
        {
            iav.LoadAuditData();
        }
    }
}
