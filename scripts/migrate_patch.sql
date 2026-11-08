USE [BlinkBlinkJewelryDb];
GO

-- 1. Bổ sung cột TyLeLoiNhuan nếu chưa có
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.SanPhamTrangSuc') AND name = 'TyLeLoiNhuan')
BEGIN
    ALTER TABLE dbo.SanPhamTrangSuc ADD TyLeLoiNhuan DECIMAL(5,2) NOT NULL CONSTRAINT DF_SanPhamTrangSuc_TyLeLoiNhuan DEFAULT 0;
    PRINT N'Đã thêm cột TyLeLoiNhuan vào bảng SanPhamTrangSuc.';
END
ELSE
BEGIN
    PRINT N'Cột TyLeLoiNhuan đã tồn tại.';
END
GO

-- 2. Tự động tính tỷ lệ lợi nhuận cho các sản phẩm hiện có nếu giá bán > giá nhập
UPDATE dbo.SanPhamTrangSuc
SET TyLeLoiNhuan = ROUND(((GiaBan - GiaNhap) / GiaNhap) * 100.0, 2)
WHERE GiaNhap > 0 AND GiaBan >= GiaNhap AND TyLeLoiNhuan = 0;
PRINT N'Đã cập nhật TyLeLoiNhuan cho các sản phẩm hiện hữu.';
GO

-- 3. Đảm bảo cột TrangThai trong PhieuNhapKho là NVARCHAR(50)
IF EXISTS (
    SELECT 1 FROM sys.columns c
    JOIN sys.types t ON c.user_type_id = t.user_type_id
    WHERE c.object_id = OBJECT_ID('dbo.PhieuNhapKho') 
      AND c.name = 'TrangThai' 
      AND t.name IN ('bit', 'tinyint', 'int')
)
BEGIN
    -- Tìm và xóa default constraint cũ nếu có
    DECLARE @defName NVARCHAR(256);
    SELECT @defName = d.name
    FROM sys.default_constraints d
    JOIN sys.columns c ON d.parent_object_id = c.object_id AND d.parent_column_id = c.column_id
    WHERE d.parent_object_id = OBJECT_ID('dbo.PhieuNhapKho') AND c.name = 'TrangThai';

    IF @defName IS NOT NULL
    BEGIN
        EXEC('ALTER TABLE dbo.PhieuNhapKho DROP CONSTRAINT ' + @defName);
    END

    -- Thêm cột tạm để chuyển dữ liệu
    ALTER TABLE dbo.PhieuNhapKho ADD TrangThai_New NVARCHAR(50) NULL;
    EXEC('UPDATE dbo.PhieuNhapKho SET TrangThai_New = CASE WHEN TrangThai = 1 THEN N''DaDuyet'' ELSE N''DaHuy'' END');
    ALTER TABLE dbo.PhieuNhapKho DROP COLUMN TrangThai;
    EXEC sp_rename 'dbo.PhieuNhapKho.TrangThai_New', 'TrangThai', 'COLUMN';
    ALTER TABLE dbo.PhieuNhapKho ALTER COLUMN TrangThai NVARCHAR(50) NOT NULL;
    ALTER TABLE dbo.PhieuNhapKho ADD CONSTRAINT DF_PhieuNhapKho_TrangThai DEFAULT N'DaDuyet' FOR TrangThai;
    PRINT N'Đã chuyển đổi kiểu cột TrangThai sang NVARCHAR(50).';
END
ELSE
BEGIN
    PRINT N'Cột TrangThai đã là NVARCHAR hoặc chưa cần chuyển đổi.';
END
GO

