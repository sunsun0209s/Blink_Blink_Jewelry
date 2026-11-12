using System;
using System.Reflection;
using System.Windows.Forms;
using BlinkBlink.Admin.Views.UserControls.Catalog;
using BlinkBlink.Admin.Views.UserControls.Warranty;
using Xunit;

namespace BlinkBlink.IntegrationTests;

/// <summary>
/// Kiểm thử tích hợp tự động giao diện Windows Forms:
/// 1. Cấu trúc bảng DataGridView và 11 cột hiển thị phiếu bảo hành
/// 2. 5 nút điều khiển quy trình bảo hành (Tiếp nhận mới, Đang xử lý, Đã xử lý, Hoàn tất & trả khách, Hủy phiếu)
/// 3. Khởi tạo và liên kết CustomerComboItem (mã khách hàng nghiệp vụ vs mã người dùng khóa ngoại DB)
/// 4. Giao diện quản lý nhóm sản phẩm ProductGroupsView
/// </summary>
public class WinFormsUIIntegrationTests
{
    [Fact]
    public void KiemTra_WarrantyView_CauTrucBang_Va_5NutChucNang()
    {
        // Khởi tạo WarrantyView
        var view = new WarrantyView();
        Assert.NotNull(view);
        Assert.Equal(DockStyle.Fill, view.Dock);
        Assert.NotEmpty(view.Controls);

        var flags = BindingFlags.NonPublic | BindingFlags.Instance;

        // 1. Kiểm tra DataGridView hiển thị danh sách bảo hành
        var dgvField = typeof(WarrantyView).GetField("_dgvWarranty", flags);
        Assert.NotNull(dgvField);
        var dgv = dgvField.GetValue(view) as DataGridView;
        Assert.NotNull(dgv);
        Assert.Equal(11, dgv.Columns.Count);

        var expectedColumns = new[]
        {
            "SoPhieuBH", "MaSanPham", "MaKhachHang", "NgayTiepNhan", "NgayHenTra",
            "LyDoBaoHanh", "TinhTrangTiepNhan", "GhiChu", "ChiPhiUocTinh", "ChiPhiThucTe", "TrangThaiXuLy"
        };

        foreach (var colName in expectedColumns)
        {
            Assert.True(dgv.Columns.Contains(colName), $"DataGridView phải chứa cột: {colName}");
        }

        // 2. Kiểm tra 5 nút thao tác chức năng
        var btnNewField = typeof(WarrantyView).GetField("_btnNew", flags);
        var btnProcessingField = typeof(WarrantyView).GetField("_btnProcessing", flags);
        var btnCompleteField = typeof(WarrantyView).GetField("_btnComplete", flags);
        var btnFinishField = typeof(WarrantyView).GetField("_btnFinish", flags);
        var btnCancelField = typeof(WarrantyView).GetField("_btnCancelTicket", flags);

        Assert.NotNull(btnNewField);
        Assert.NotNull(btnProcessingField);
        Assert.NotNull(btnCompleteField);
        Assert.NotNull(btnFinishField);
        Assert.NotNull(btnCancelField);

        var btnNew = btnNewField.GetValue(view) as Button;
        var btnProcessing = btnProcessingField.GetValue(view) as Button;
        var btnComplete = btnCompleteField.GetValue(view) as Button;
        var btnFinish = btnFinishField.GetValue(view) as Button;
        var btnCancel = btnCancelField.GetValue(view) as Button;

        Assert.NotNull(btnNew);
        Assert.NotNull(btnProcessing);
        Assert.NotNull(btnComplete);
        Assert.NotNull(btnFinish);
        Assert.NotNull(btnCancel);

        Assert.Contains("Tiếp Nhận Mới", btnNew.Text);
        Assert.Contains("Đang Xử Lý", btnProcessing.Text);
        Assert.Contains("Đã Xử Lý", btnComplete.Text);
        Assert.Contains("Hoàn Tất & Trả Khách", btnFinish.Text);
        Assert.Contains("Hủy Phiếu", btnCancel.Text);

        // 3. Kiểm tra ComboBox lọc trạng thái
        var cboStatusField = typeof(WarrantyView).GetField("_cboStatus", flags);
        Assert.NotNull(cboStatusField);
        var cboStatus = cboStatusField.GetValue(view) as ComboBox;
        Assert.NotNull(cboStatus);
        Assert.Equal(6, cboStatus.Items.Count);
        Assert.Equal("Tất Cả", cboStatus.Items[0]);
    }

    [Fact]
    public void KiemTra_CustomerComboItem_LuuTruVaMap_MaNguoiDung()
    {
        var itemType = typeof(WarrantyView).GetNestedType("CustomerComboItem", BindingFlags.NonPublic);
        Assert.NotNull(itemType);

        // Trường hợp 1: Khách thành viên (KH001 -> USER_KH001)
        var memberItem = Activator.CreateInstance(itemType, "KH001", "[KH001] Trần Văn An - 0912345678", "0912345678", "USER_KH001");
        Assert.NotNull(memberItem);

        var propMaKh = itemType.GetProperty("MaKhachHang");
        var propMaNd = itemType.GetProperty("MaNguoiDung");
        var propSdt = itemType.GetProperty("SoDienThoai");

        Assert.NotNull(propMaKh);
        Assert.NotNull(propMaNd);
        Assert.NotNull(propSdt);

        Assert.Equal("KH001", propMaKh.GetValue(memberItem));
        Assert.Equal("USER_KH001", propMaNd.GetValue(memberItem));
        Assert.Equal("0912345678", propSdt.GetValue(memberItem));

        // Trường hợp 2: Khách vãng lai (0 -> 0)
        var guestItem = Activator.CreateInstance(itemType, "0", "Khách hàng vãng lai (Chưa có tài khoản)", "", "0");
        Assert.NotNull(guestItem);
        Assert.Equal("0", propMaKh.GetValue(guestItem));
        Assert.Equal("0", propMaNd.GetValue(guestItem));
    }

    [Fact]
    public void KiemTra_ProductGroupsView_KhoiTaoGiaoDien()
    {
        var groupsView = new ProductGroupsView();
        Assert.NotNull(groupsView);
        Assert.Equal(DockStyle.Fill, groupsView.Dock);
        Assert.NotEmpty(groupsView.Controls);

        var flags = BindingFlags.NonPublic | BindingFlags.Instance;

        // 1. Kiểm tra bảng DataGridView hiển thị danh sách nhóm sản phẩm
        var dgvGroupsField = typeof(ProductGroupsView).GetField("_dgvGroups", flags);
        Assert.NotNull(dgvGroupsField);
        var dgvGroups = dgvGroupsField.GetValue(groupsView) as DataGridView;
        Assert.NotNull(dgvGroups);
        Assert.True(dgvGroups.Columns.Count >= 4, "Bảng nhóm phải có đủ các cột: Mã nhóm, Tên nhóm, Mô tả, Số SP.");

        // 2. Kiểm tra các nút hành động trên thanh công cụ
        var btnAddField = typeof(ProductGroupsView).GetField("_btnAddGroup", flags);
        var btnEditField = typeof(ProductGroupsView).GetField("_btnEditGroup", flags);
        var btnRefreshField = typeof(ProductGroupsView).GetField("_btnRefresh", flags);
        Assert.NotNull(btnAddField);
        Assert.NotNull(btnEditField);
        Assert.NotNull(btnRefreshField);

        var btnAdd = btnAddField.GetValue(groupsView) as Button;
        var btnEdit = btnEditField.GetValue(groupsView) as Button;
        Assert.NotNull(btnAdd);
        Assert.NotNull(btnEdit);
        Assert.Contains("Thêm Nhóm", btnAdd.Text);
        Assert.Contains("Sửa Nhóm", btnEdit.Text);

        // 3. Kiểm tra các KPI tổng quan
        var lblKpiTotalField = typeof(ProductGroupsView).GetField("_lblKpiTotalGroups", flags);
        var lblKpiGroupedField = typeof(ProductGroupsView).GetField("_lblKpiGroupedProducts", flags);
        Assert.NotNull(lblKpiTotalField);
        Assert.NotNull(lblKpiGroupedField);
    }
}
