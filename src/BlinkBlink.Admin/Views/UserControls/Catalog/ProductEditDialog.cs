using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using BlinkBlink.Admin.Services;
using BlinkBlink.Admin.Views.Helpers;
using BlinkBlink.Application.DTOs.Catalog;
using BlinkBlink.Domain.Entities.Catalog;

namespace BlinkBlink.Admin.Views.UserControls.Catalog;

/// <summary>
/// Hộp thoại modal thêm mới và chỉnh sửa thông tin sản phẩm trang sức.
/// Tự động thích ứng kích thước hiển thị chống đè nút trên màn hình độ phân giải cao (High DPI 125% - 150%).
/// Hỗ trợ chọn tệp ảnh thực tế, sao chép ảnh vào thư mục tĩnh web, xem trước ảnh đa nguồn và chọn danh mục động từ CSDL.
/// </summary>
public class ProductEditDialog : Form
{
    private readonly bool _isEditMode;
    private readonly SanPhamTrangSuc? _targetProduct;
    private readonly AdminServiceScope _services;

    private TextBox _txtMaSP = null!;
    private TextBox _txtTenSP = null!;
    private ComboBox _cboDanhMuc = null!;
    private TextBox _txtChatLieu = null!;
    private TextBox _txtSize = null!;
    private TextBox _txtGiaNhap = null!;
    private TextBox _txtGiaBan = null!;
    private NumericUpDown _nudTyLeLoiNhuan = null!;
    private NumericUpDown _nudPhanTramGiamGia = null!;

    private PictureBox _picPreview = null!;
    private TextBox _txtHinhAnhUrl = null!;
    private Label _lblImagePath = null!;
    private string _selectedImagePath = string.Empty;
    private bool _isInternalUpdating;

    public ProductEditDialog(bool isEditMode, SanPhamTrangSuc? targetProduct = null)
    {
        _isEditMode = isEditMode;
        _targetProduct = targetProduct;
        _services = AdminServiceScope.Instance;

        InitializeDialogLayout();
        LoadCategoryOptions();
        BindInitialData();
    }

    private void InitializeDialogLayout()
    {
        this.Text = _isEditMode ? "Chỉnh Sửa Thông Tin Trang Sức - Blink Blink Jewelry" : "Thêm Mới Sản Phẩm Trang Sức - Blink Blink Jewelry";
        this.Width = 880;
        this.Height = 750;
        this.StartPosition = FormStartPosition.CenterParent;
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.BackColor = Color.White;
        this.Font = UITheme.RegularFont;

        // Top Header
        var pnlHeader = new Panel
        {
            Dock = DockStyle.Top,
            Height = 80,
            BackColor = UITheme.DarkHeader,
            Padding = new Padding(24, 16, 24, 10)
        };

        var lblHeaderTitle = new Label
        {
            Text = _isEditMode ? "CHỈNH SỬA CHI TIẾT SẢN PHẨM TRANG SỨC" : "KHAI BÁO MẶT HÀNG TRANG SỨC MỚI",
            Font = UITheme.HeaderFont,
            ForeColor = UITheme.PrimaryGold,
            Dock = DockStyle.Top,
            Height = 26,
            UseMnemonic = false
        };

        var lblHeaderSub = new Label
        {
            Text = _isEditMode 
                ? "Cập nhật tên, danh mục, thuộc tính kim hoàn, đơn giá bán niêm yết, % giảm giá và hình ảnh thực tế."
                : "Sản phẩm mới khai báo có tồn kho = 0. Giá nhập sẽ được tự động tính bình quân gia quyền khi tạo phiếu nhập kho.",
            Font = new Font("Segoe UI", 9F, FontStyle.Regular),
            ForeColor = Color.FromArgb(203, 213, 225),
            Dock = DockStyle.Fill,
            UseMnemonic = false
        };

        pnlHeader.Controls.Add(lblHeaderSub);
        pnlHeader.Controls.Add(lblHeaderTitle);

        // Bottom Footer (Chứa các nút hành động, căn lề FlowLayoutPanel chống va chạm lấn đè)
        var pnlFooter = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 72,
            BackColor = UITheme.CanvasBackground,
            Padding = new Padding(24, 16, 24, 16)
        };

        var flpButtons = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            WrapContents = false,
            AutoSize = false
        };

        var btnCancel = UITheme.CreateFlatButton("Hủy Bỏ", Color.FromArgb(226, 232, 240), UITheme.TextPrimary);
        btnCancel.Width = 115;
        btnCancel.Height = 40;
        btnCancel.Margin = new Padding(12, 0, 0, 0);
        btnCancel.Click += (s, e) => this.Close();

        var btnSave = UITheme.CreateFlatButton(_isEditMode ? "Lưu Thay Đổi" : "Lưu Sản Phẩm", UITheme.PrimaryGold, Color.Black);
        btnSave.Width = 155;
        btnSave.Height = 40;
        btnSave.Margin = new Padding(0, 0, 0, 0);
        btnSave.Click += BtnSave_Click;

        flpButtons.Controls.Add(btnCancel);
        flpButtons.Controls.Add(btnSave);
        pnlFooter.Controls.Add(flpButtons);

        // Content Body (Chia 2 cột trái: nhập liệu rộng rãi, phải: hình ảnh)
        var pnlBody = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(24, 20, 24, 10),
            BackColor = Color.White
        };

        // Cột trái: Nhập thông số sản phẩm
        var pnlLeft = new Panel
        {
            Location = new Point(24, 15),
            Width = 490,
            Height = 550
        };

        int curY = 0;
        void AddFormRow(string labelText, Control inputControl)
        {
            var lbl = new Label
            {
                Text = labelText,
                Location = new Point(0, curY + 4),
                Width = 175,
                ForeColor = UITheme.TextSecondary,
                Font = UITheme.RegularFont,
                UseMnemonic = false
            };
            inputControl.Location = new Point(180, curY);
            inputControl.Width = 300;
            inputControl.Font = UITheme.RegularFont;

            pnlLeft.Controls.Add(lbl);
            pnlLeft.Controls.Add(inputControl);
            curY += 46;
        }

        _txtMaSP = new TextBox { Height = 30 };
        if (_isEditMode)
        {
            _txtMaSP.ReadOnly = true;
            _txtMaSP.BackColor = Color.FromArgb(241, 245, 249);
            _txtMaSP.ForeColor = UITheme.TextSecondary;
        }

        _txtTenSP = new TextBox { Height = 30, PlaceholderText = "Nhập tên mẫu trang sức..." };
        _cboDanhMuc = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Height = 30 };
        _txtChatLieu = new TextBox { Height = 30, PlaceholderText = "Ví dụ: Vàng trắng 18K, Kim cương 5.4 ly" };
        _txtSize = new TextBox { Height = 30, PlaceholderText = "Ví dụ: Size 14 hoặc Size 45cm" };

        AddFormRow("Mã Sản Phẩm (*):", _txtMaSP);
        AddFormRow("Tên Trang Sức (*):", _txtTenSP);
        AddFormRow("Danh Mục (*):", _cboDanhMuc);
        AddFormRow("Chất Liệu Kim Hoàn:", _txtChatLieu);
        AddFormRow("Kích Cỡ / Size:", _txtSize);

        if (_isEditMode)
        {
            var txtNhom = new TextBox
            {
                Height = 30,
                ReadOnly = true,
                BackColor = Color.FromArgb(241, 245, 249),
                ForeColor = UITheme.TextSecondary
            };
            if (_targetProduct != null && !string.IsNullOrWhiteSpace(_targetProduct.MaNhom) && _targetProduct.MaNhom != "0")
            {
                txtNhom.Text = $"Mã nhóm: {_targetProduct.MaNhom}";
            }
            else
            {
                txtNhom.Text = "Chưa thuộc về nhóm nào (Mã nhóm: 0)";
            }
            AddFormRow("Nhóm Sản Phẩm:", txtNhom);

            // Trong chế độ Chỉnh sửa: Hiển thị giá nhập bình quân (chỉ xem), % lợi nhuận để tự động tính đơn giá bán
            _txtGiaNhap = new TextBox 
            { 
                Height = 30, 
                Font = UITheme.TabularNumericFont, 
                ReadOnly = true,
                BackColor = Color.FromArgb(241, 245, 249),
                ForeColor = UITheme.TextSecondary
            };

            _nudTyLeLoiNhuan = new NumericUpDown
            {
                Height = 30,
                Font = UITheme.TabularNumericFont,
                Minimum = 0,
                Maximum = 1000,
                DecimalPlaces = 1,
                Value = 0,
                Increment = 5
            };
            _nudTyLeLoiNhuan.ValueChanged += (s, e) =>
            {
                var gNhap = _targetProduct?.GiaNhap ?? 0;
                var gBan = gNhap > 0 ? Math.Round(gNhap * (1 + _nudTyLeLoiNhuan.Value / 100m)) : 0;
                _txtGiaBan.Text = gBan.ToString("0");
            };

            _txtGiaBan = new TextBox 
            { 
                Height = 30, 
                Font = UITheme.TabularNumericFont,
                ReadOnly = true,
                BackColor = Color.FromArgb(241, 245, 249),
                ForeColor = UITheme.PrimaryGold
            };
            
            _nudPhanTramGiamGia = new NumericUpDown
            {
                Height = 30,
                Font = UITheme.TabularNumericFont,
                Minimum = 0,
                Maximum = 100,
                Value = 0,
                Increment = 1
            };

            AddFormRow("Giá Nhập Bình Quân (VND):", _txtGiaNhap);
            AddFormRow("% Lợi Nhuận (0 - 1000%):", _nudTyLeLoiNhuan);
            AddFormRow("Đơn Giá Bán (Tự Động) (*):", _txtGiaBan);
            AddFormRow("% Giảm Giá (0 - 100%):", _nudPhanTramGiamGia);
        }
        else
        {
            // Trong chế độ Thêm mới: Tùy chỉnh % lợi nhuận (mặc định 0%), giá bán bị khóa không cho nhập tay
            _txtGiaNhap = new TextBox { Text = "0", Visible = false };

            _nudTyLeLoiNhuan = new NumericUpDown
            {
                Height = 30,
                Font = UITheme.TabularNumericFont,
                Minimum = 0,
                Maximum = 1000,
                DecimalPlaces = 1,
                Value = 0,
                Increment = 5
            };

            _txtGiaBan = new TextBox 
            { 
                Height = 30, 
                Font = UITheme.TabularNumericFont,
                Text = "0",
                ReadOnly = true,
                BackColor = Color.FromArgb(241, 245, 249),
                ForeColor = UITheme.PrimaryGold
            };

            _nudPhanTramGiamGia = new NumericUpDown { Value = 0, Visible = false };

            AddFormRow("% Lợi Nhuận Ban Đầu (%):", _nudTyLeLoiNhuan);
            AddFormRow("Đơn Giá Bán (Tự Động):", _txtGiaBan);

            var pnlHint = new Panel
            {
                Location = new Point(0, curY + 6),
                Width = 480,
                Height = 90,
                BackColor = Color.FromArgb(248, 250, 252),
                Padding = new Padding(12)
            };
            var lblHint = new Label
            {
                Text = "💡 Lưu ý cơ chế định giá tự động:\n- Giá nhập bình quân được tính tự động từ các phiếu nhập kho đã được Admin phê duyệt.\n- Giá bán tự động = Giá nhập bình quân × (1 + % Lợi nhuận).\n- Ô đơn giá bán bị khóa không cho nhập tay.",
                ForeColor = UITheme.TextSecondary,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Italic),
                Dock = DockStyle.Fill
            };
            pnlHint.Controls.Add(lblHint);
            pnlLeft.Controls.Add(pnlHint);
        }

        // Cột phải: Hình ảnh sản phẩm & Xem trước (Preview)
        var pnlRight = new Panel
        {
            Location = new Point(535, 15),
            Width = 300,
            Height = 550
        };

        var lblPicTitle = new Label
        {
            Text = "HÌNH ẢNH SẢN PHẨM",
            Location = new Point(0, 4),
            Width = 280,
            Font = UITheme.SubHeaderFont,
            ForeColor = UITheme.TextPrimary,
            UseMnemonic = false
        };

        _picPreview = new PictureBox
        {
            Location = new Point(0, 30),
            Width = 280,
            Height = 205,
            BorderStyle = BorderStyle.FixedSingle,
            SizeMode = PictureBoxSizeMode.Zoom,
            BackColor = Color.FromArgb(248, 250, 252)
        };

        var lblUrl = new Label
        {
            Text = "Đường dẫn tệp hoặc URL ảnh online:",
            Location = new Point(0, 242),
            Width = 280,
            Height = 18,
            ForeColor = UITheme.TextSecondary,
            Font = new Font("Segoe UI", 8.25F, FontStyle.Regular),
            UseMnemonic = false
        };

        _txtHinhAnhUrl = new TextBox
        {
            Location = new Point(0, 262),
            Width = 280,
            Height = 28,
            Font = UITheme.RegularFont,
            PlaceholderText = "https://... hoặc /images/..."
        };
        _txtHinhAnhUrl.TextChanged += (s, e) =>
        {
            if (!_isInternalUpdating)
            {
                _selectedImagePath = _txtHinhAnhUrl.Text.Trim();
                LoadPreviewImage(_selectedImagePath);
            }
        };

        var btnChooseImage = UITheme.CreateFlatButton("📁 Chọn Tệp Ảnh Từ Máy Tính...", UITheme.DarkHeader, Color.White);
        btnChooseImage.Location = new Point(0, 298);
        btnChooseImage.Width = 280;
        btnChooseImage.Height = 36;
        btnChooseImage.Click += BtnChooseImage_Click;

        var btnRemoveImage = UITheme.CreateFlatButton("✕ Xóa Ảnh", Color.FromArgb(241, 245, 249), UITheme.DangerColor);
        btnRemoveImage.Location = new Point(0, 340);
        btnRemoveImage.Width = 280;
        btnRemoveImage.Height = 32;
        btnRemoveImage.Click += (s, e) =>
        {
            _isInternalUpdating = true;
            _selectedImagePath = string.Empty;
            _txtHinhAnhUrl.Text = string.Empty;
            _isInternalUpdating = false;

            _picPreview.Image?.Dispose();
            _picPreview.Image = null;
            _lblImagePath.Text = "Chưa có hình ảnh";
        };

        _lblImagePath = new Label
        {
            Location = new Point(0, 378),
            Width = 280,
            Height = 44,
            ForeColor = UITheme.TextSecondary,
            Font = new Font("Segoe UI", 8.25F, FontStyle.Italic),
            Text = "Chưa có hình ảnh",
            UseMnemonic = false
        };

        pnlRight.Controls.Add(lblPicTitle);
        pnlRight.Controls.Add(_picPreview);
        pnlRight.Controls.Add(lblUrl);
        pnlRight.Controls.Add(_txtHinhAnhUrl);
        pnlRight.Controls.Add(btnChooseImage);
        pnlRight.Controls.Add(btnRemoveImage);
        pnlRight.Controls.Add(_lblImagePath);

        pnlBody.Controls.Add(pnlLeft);
        pnlBody.Controls.Add(pnlRight);

        this.Controls.Add(pnlBody);
        this.Controls.Add(pnlFooter);
        this.Controls.Add(pnlHeader);
    }

    private void LoadCategoryOptions()
    {
        _cboDanhMuc.Items.Clear();
        var categories = _services.DanhMucRepo.LayTatCa();
        if (categories.Count > 0)
        {
            foreach (var cat in categories)
            {
                _cboDanhMuc.Items.Add(new ComboBoxItem(cat.TenDanhMuc, cat.MaDanhMuc));
            }

            // Mặc định khi tạo sản phẩm mới sẽ để là 'Chưa phân loại' (DM00) theo CONGVIECTIEPTHEO
            var defaultIdx = -1;
            for (int i = 0; i < _cboDanhMuc.Items.Count; i++)
            {
                if (_cboDanhMuc.Items[i] is ComboBoxItem item &&
                    string.Equals(item.Value, "DM00", StringComparison.OrdinalIgnoreCase))
                {
                    defaultIdx = i;
                    break;
                }
            }
            _cboDanhMuc.SelectedIndex = defaultIdx >= 0 ? defaultIdx : 0;
        }
        else
        {
            _cboDanhMuc.Items.Add(new ComboBoxItem("Chưa phân loại (DM00)", "DM00"));
            _cboDanhMuc.SelectedIndex = 0;
        }
    }

    private void BindInitialData()
    {
        if (_isEditMode && _targetProduct != null)
        {
            _txtMaSP.Text = _targetProduct.MaSanPham;
            _txtTenSP.Text = _targetProduct.TenSanPham;
            _txtChatLieu.Text = _targetProduct.ChatLieu;
            _txtSize.Text = _targetProduct.Size;
            _txtGiaNhap.Text = _targetProduct.GiaNhap.ToString("N0", new CultureInfo("vi-VN"));
            _nudTyLeLoiNhuan.Value = Math.Clamp(_targetProduct.TyLeLoiNhuan, 0, 1000);
            _txtGiaBan.Text = _targetProduct.GiaBan.ToString("0");
            _nudPhanTramGiamGia.Value = Math.Clamp(_targetProduct.PhanTramGiamGia, 0, 100);

            // Chọn danh mục tương ứng
            for (int i = 0; i < _cboDanhMuc.Items.Count; i++)
            {
                if (_cboDanhMuc.Items[i] is ComboBoxItem item &&
                    string.Equals(item.Value, _targetProduct.MaDanhMuc, StringComparison.OrdinalIgnoreCase))
                {
                    _cboDanhMuc.SelectedIndex = i;
                    break;
                }
            }

            // Nạp hình ảnh nếu có
            if (!string.IsNullOrWhiteSpace(_targetProduct.HinhAnh))
            {
                _isInternalUpdating = true;
                _selectedImagePath = _targetProduct.HinhAnh;
                _txtHinhAnhUrl.Text = _selectedImagePath;
                _isInternalUpdating = false;

                LoadPreviewImage(_selectedImagePath);
            }
        }
        else
        {
            // Tự sinh mã mới
            var allProducts = _services.SanPhamRepo.LayTatCa();
            _txtMaSP.Text = $"SP{allProducts.Count + 1:D3}";
            _txtTenSP.Text = "";
            _txtChatLieu.Text = "";
            _txtSize.Text = "";
        }
    }

    private void BtnChooseImage_Click(object? sender, EventArgs e)
    {
        using var ofd = new OpenFileDialog
        {
            Title = "Chọn Hình Ảnh Trang Sức Từ Máy Tính",
            Filter = "Tệp Hình Ảnh (*.jpg;*.jpeg;*.png;*.webp)|*.jpg;*.jpeg;*.png;*.webp|Tất cả tệp (*.*)|*.*",
            RestoreDirectory = true
        };

        if (ofd.ShowDialog() == DialogResult.OK)
        {
            _selectedImagePath = ofd.FileName;
            _isInternalUpdating = true;
            _txtHinhAnhUrl.Text = _selectedImagePath;
            _isInternalUpdating = false;

            LoadPreviewImage(_selectedImagePath);
        }
    }

    private async void LoadPreviewImage(string pathOrUrl)
    {
        if (string.IsNullOrWhiteSpace(pathOrUrl))
        {
            _picPreview.Image?.Dispose();
            _picPreview.Image = null;
            _lblImagePath.Text = "Chưa có hình ảnh";
            return;
        }

        var trimmed = pathOrUrl.Trim();
        try
        {
            // Dạng 1: Đường link trực tuyến (http:// hoặc https://)
            if (trimmed.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                trimmed.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                _lblImagePath.Text = "Đang tải ảnh online...";
                using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
                var bytes = await client.GetByteArrayAsync(trimmed);
                using var ms = new MemoryStream(bytes);
                _picPreview.Image?.Dispose();
                _picPreview.Image = Image.FromStream(ms);
                _lblImagePath.Text = "✓ Ảnh trực tuyến (URL Web)";
                return;
            }

            // Dạng 2: Đường dẫn web tương đối (/images/products/...)
            if (trimmed.StartsWith("/images/", StringComparison.OrdinalIgnoreCase))
            {
                var folder = GetWebProductsImageFolder();
                if (!string.IsNullOrEmpty(folder))
                {
                    var fileName = Path.GetFileName(trimmed);
                    var fullPath = Path.Combine(folder, fileName);
                    if (File.Exists(fullPath))
                    {
                        using var fs = new FileStream(fullPath, FileMode.Open, FileAccess.Read);
                        using var ms = new MemoryStream();
                        fs.CopyTo(ms);
                        ms.Position = 0;
                        _picPreview.Image?.Dispose();
                        _picPreview.Image = Image.FromStream(ms);
                        _lblImagePath.Text = $"✓ Ảnh kho hệ thống: {fileName}";
                        return;
                    }
                }
            }

            // Dạng 3: File cục bộ trên máy tính
            if (File.Exists(trimmed))
            {
                using var fs = new FileStream(trimmed, FileMode.Open, FileAccess.Read);
                using var ms = new MemoryStream();
                fs.CopyTo(ms);
                ms.Position = 0;
                _picPreview.Image?.Dispose();
                _picPreview.Image = Image.FromStream(ms);
                _lblImagePath.Text = $"✓ File máy tính: {Path.GetFileName(trimmed)}";
                return;
            }

            _lblImagePath.Text = "Không tìm thấy file ảnh tương ứng.";
        }
        catch
        {
            _lblImagePath.Text = "Không thể nạp xem trước ảnh.";
        }
    }

    /// <summary>
    /// Tìm thư mục lưu trữ ảnh sản phẩm tĩnh trong BlinkBlink.Web (wwwroot/images/products)
    /// </summary>
    private static string? GetWebProductsImageFolder()
    {
        var current = AppDomain.CurrentDomain.BaseDirectory;
        while (!string.IsNullOrEmpty(current))
        {
            var webFolder = Path.Combine(current, "src", "BlinkBlink.Web");
            if (Directory.Exists(webFolder))
            {
                var target = Path.Combine(webFolder, "wwwroot", "images", "products");
                if (!Directory.Exists(target))
                {
                    Directory.CreateDirectory(target);
                }
                return target;
            }
            var parent = Directory.GetParent(current);
            if (parent == null) break;
            current = parent.FullName;
        }
        return null;
    }

    /// <summary>
    /// Chuẩn hóa và lưu trữ ảnh: Tự động copy file máy cục bộ vào wwwroot/images/products
    /// và sinh đường dẫn tương đối dạng web (/images/products/...) để Web và Admin đều nạp được.
    /// </summary>
    private static string ProcessAndNormalizeImage(string inputImage, string maSP)
    {
        if (string.IsNullOrWhiteSpace(inputImage))
        {
            return string.Empty;
        }

        var trimmed = inputImage.Trim();

        // 1. Nếu là đường link trực tuyến (http:// hoặc https://)
        if (trimmed.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
            trimmed.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            return trimmed;
        }

        // 2. Nếu đã là đường dẫn web tương đối chuẩn (/images/products/...)
        if (trimmed.StartsWith("/images/", StringComparison.OrdinalIgnoreCase))
        {
            return trimmed;
        }

        // 3. Nếu là tệp file trên máy tính của người dùng
        if (File.Exists(trimmed))
        {
            var webProductsFolder = GetWebProductsImageFolder();
            if (!string.IsNullOrEmpty(webProductsFolder))
            {
                try
                {
                    var ext = Path.GetExtension(trimmed).ToLowerInvariant();
                    if (string.IsNullOrEmpty(ext)) ext = ".png";

                    var rawName = Path.GetFileNameWithoutExtension(trimmed);
                    var safeName = new string(rawName.Where(c => char.IsLetterOrDigit(c) || c == '_' || c == '-').ToArray());
                    if (string.IsNullOrWhiteSpace(safeName)) safeName = "item";

                    var destFileName = $"{maSP}_{safeName}{ext}";
                    var destFullPath = Path.Combine(webProductsFolder, destFileName);

                    File.Copy(trimmed, destFullPath, true);

                    return $"/images/products/{destFileName}";
                }
                catch
                {
                    return trimmed;
                }
            }
        }

        return trimmed;
    }

    private void BtnSave_Click(object? sender, EventArgs e)
    {
        var maSP = _txtMaSP.Text.Trim();
        var tenSP = _txtTenSP.Text.Trim();
        var selectedItem = _cboDanhMuc.SelectedItem as ComboBoxItem;
        var maDM = selectedItem?.Value ?? "DM01";
        var chatLieu = _txtChatLieu.Text.Trim();
        var size = _txtSize.Text.Trim();

        if (string.IsNullOrWhiteSpace(maSP))
        {
            MessageBox.Show("Vui lòng nhập Mã Sản Phẩm.", "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            _txtMaSP.Focus();
            return;
        }

        if (string.IsNullOrWhiteSpace(tenSP))
        {
            MessageBox.Show("Vui lòng nhập Tên Sản Phẩm.", "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            _txtTenSP.Focus();
            return;
        }

        // Chuẩn hóa và tự động lưu trữ ảnh vào thư mục tĩnh của Web
        var rawImagePath = string.IsNullOrWhiteSpace(_txtHinhAnhUrl.Text) ? _selectedImagePath : _txtHinhAnhUrl.Text.Trim();
        var processedImage = ProcessAndNormalizeImage(rawImagePath, maSP);

        if (_isEditMode)
        {
            // Kiểm tra ràng buộc nhóm sản phẩm theo CONGVIECTIEPTHEO.md
            if (_targetProduct != null && !string.IsNullOrWhiteSpace(_targetProduct.MaNhom) && _targetProduct.MaNhom != "0")
            {
                // 1. Kiểm tra trùng size
                if (_services.QuanLyNhomSanPham.KiemTraTrungSizeTrongNhom(_targetProduct.MaSanPham, size, out _))
                {
                    MessageBox.Show("size của sản phẩm đã bị trùng với sản phẩm khác trong nhóm", "Cảnh báo trùng size", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    _txtSize.Text = _targetProduct.Size ?? string.Empty;
                    return;
                }

                // Lưu ý: Theo yêu cầu mới, bỏ chức năng sửa tên sản phẩm trong nhóm sẽ làm thay đổi tên nhóm và các sản phẩm trong nhóm.
                // Việc đổi tên sản phẩm ở đây chỉ cập nhật riêng cho sản phẩm này.
            }

            var tyLeLoiNhuan = _nudTyLeLoiNhuan.Value;
            var giaNhap = _targetProduct?.GiaNhap ?? 0;
            // Tự động tính giá bán theo tỷ lệ lợi nhuận: Giá bán = Giá nhập * (1 + % Lợi nhuận)
            var giaBan = giaNhap > 0 ? Math.Round(giaNhap * (1 + tyLeLoiNhuan / 100m)) : 0;
            var phanTramGiamGia = (int)_nudPhanTramGiamGia.Value;

            var updateDto = new CapNhatSanPhamDTO
            {
                MaSanPham = maSP,
                TenSanPham = tenSP,
                MaDanhMuc = maDM,
                ChatLieu = chatLieu,
                Size = size,
                GiaNhap = giaNhap,
                GiaBan = giaBan,
                TyLeLoiNhuan = tyLeLoiNhuan,
                HinhAnh = processedImage,
                PhanTramGiamGia = phanTramGiamGia
            };

            var success = _services.QuanLySanPham.CapNhatThongTin(updateDto);
            if (success)
            {
                MessageBox.Show($"Cập nhật thành công thông tin sản phẩm [{maSP}]!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                MessageBox.Show("Không thể cập nhật sản phẩm. Vui lòng kiểm tra lại kết nối cơ sở dữ liệu.", "Lỗi cập nhật", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        else
        {
            var createDto = new SanPhamDTO
            {
                MaSanPham = maSP,
                TenSanPham = tenSP,
                MaDanhMuc = maDM,
                ChatLieu = chatLieu,
                Size = size,
                GiaNhap = 0,
                GiaBan = 0,
                TyLeLoiNhuan = _nudTyLeLoiNhuan.Value,
                HinhAnh = processedImage,
                PhanTramGiamGia = 0
            };

            var success = _services.QuanLySanPham.ThemSanPhamMoi(createDto);
            if (success)
            {
                MessageBox.Show($"Đã tạo mới sản phẩm [{maSP}] thành công!\n\nLưu ý: Sản phẩm vừa tạo có trạng thái 'Hết Hàng' (Tồn kho = 0). Vui lòng vào phân hệ 'Nhập Kho' để lập phiếu nhập hàng, hệ thống sẽ tự động tính giá vốn bình quân gia quyền và chuyển trạng thái sang 'Sẵn Sàng Bán'.", "Tạo mới thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                MessageBox.Show("Không thể tạo sản phẩm mới. Có thể do trùng lặp mã sản phẩm hoặc dữ liệu không hợp lệ.", "Lỗi tạo mới", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    /// <summary>
    /// Lớp hỗ trợ hiển thị danh mục trong ComboBox
    /// </summary>
    private class ComboBoxItem
    {
        public string Text { get; }
        public string Value { get; }

        public ComboBoxItem(string text, string value)
        {
            Text = text;
            Value = value;
        }

        public override string ToString() => Text;
    }
}
