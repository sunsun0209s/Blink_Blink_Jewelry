/**
 * ====================================================================================
 * BLINK BLINK HAUTE JOAILLERIE - XỬ LÝ TƯƠNG TÁC GIAO DIỆN (STOREFRONT CLIENT SCRIPT)
 * Quy chuẩn: Prototype-First, tương tác trực quan trước khi gắn logic API thật
 * Dữ liệu mẫu đồng bộ 100% với SeedData.sql và sơ đồ thực thể Clean Architecture
 * ====================================================================================
 */

// 1. Danh mục sản phẩm trang sức nạp động 100% từ CSDL SQL Server qua RESTful API /api/SanPham
let catalogProducts = [];

// Trạng thái phân trang danh mục và tìm kiếm kiệt tác chuẩn Taste Skill
let currentCatalogPage = 1;
let currentCatalogPageSize = 8;
let currentFilteredProducts = [];

/**
 * Hàm gọi RESTful API tập trung với cơ chế xử lý lỗi và mã truy vết thích ứng.
 * @param {string} url - Đường dẫn API
 * @param {RequestInit} [options] - Cấu hình Fetch
 * @returns {Promise<{ok: boolean, status: number, data: any, traceId: string}>}
 */
async function apiRequest(url, options = {}) {
    const headers = new Headers(options.headers || {});
    if (!headers.has('Accept')) headers.set('Accept', 'application/json');
    if (options.body && !headers.has('Content-Type')) headers.set('Content-Type', 'application/json');

    const response = await fetch(url, { ...options, headers });
    const data = await response.json().catch(() => ({}));
    const traceId = data.traceId || data.TraceId || response.headers.get('X-Correlation-Id') || '';

    if (response.status === 403 || data.code === 'AUTH_ACCOUNT_LOCKED') {
        showToast(data.message || 'Tài khoản của quý khách hiện đang bị tạm khóa.', 'error');
        setTimeout(() => {
            if (typeof logoutUser === 'function') logoutUser();
        }, 1200);
    } else if (!response.ok) {
        const code = data.code || data.Code || 'HTTP_' + response.status;
        console.error(`[BlinkBlink.Web] API lỗi (${url}) | Status=${response.status} | Code=${code} | TraceId=${traceId}`, data.message || '');
    }

    return { ok: response.ok, status: response.status, data, traceId };
}

/**
 * Tải toàn bộ danh mục sản phẩm từ CSDL SQL Server qua API
 */
async function fetchCatalogFromApi() {
    try {
        const res = await fetch('/api/SanPham');
        if (res.ok) {
            const json = await res.json();
            if (json.success && Array.isArray(json.data) && json.data.length > 0) {
                catalogProducts = json.data;
                console.log(`[BlinkBlink.Web] Đã nạp thành công ${catalogProducts.length} sản phẩm từ CSDL SQL Server.`);
                return catalogProducts;
            }
        }
    } catch (e) {
        console.error('Không thể kết nối API /api/SanPham:', e);
    }
    return [];
}

// 2. Dữ liệu mẫu phiếu tiếp nhận bảo hành dịch vụ kim hoàn (SeedData.sql)
const sampleWarrantyTickets = {
    'BH2001': {
        soPhieu: 'BH2001',
        maKhachHang: 'USER_KH001',
        tenKhachHang: 'Phạm Bích Ngọc',
        maSanPham: 'SP001',
        tenSanPham: 'Nhẫn Solitaire Diamond 18K',
        ngayTiepNhan: '12/03/2026',
        ngayHenTra: '14/03/2026',
        chiPhi: 0,
        trangThai: 'DaHoanTat',
        trangThaiText: 'Đã hoàn tất',
        lyDo: 'Làm sạch, đánh bóng siêu âm và kiểm tra chấu giữ viên kim cương chủ',
        ghiChu: 'Trang sức đã được phục hồi độ sáng bóng 99%, sẵn sàng giao trả tại showroom Quận 1.'
    },
    'BH2002': {
        soPhieu: 'BH2002',
        maKhachHang: 'USER_KH002',
        tenKhachHang: 'Hoàng Tuấn Anh',
        maSanPham: 'SP002',
        tenSanPham: 'Dây Chuyền Eternal Heart Ruby',
        ngayTiepNhan: '18/03/2026',
        ngayHenTra: '22/03/2026',
        chiPhi: 150000,
        trangThai: 'DangBaoHanh',
        trangThaiText: 'Đang bảo dưỡng',
        lyDo: 'Đứt khoen móc khóa cài dây chuyền vàng hồng, yêu cầu hàn laser thẩm mỹ',
        ghiChu: 'Nghệ nhân đang gia công hàn nhiệt laser tại xưởng chế tác Blink Blink.'
    }
};



// 4. Trạng thái người dùng đăng nhập hiện tại

// Trạng thái người dùng đăng nhập hiện tại
let currentUser = null;

// Khóa định danh lưu trữ giỏ hàng tạm cho khách vãng lai (CHƯA ĐĂNG NHẬP)
const GUEST_CART_STORAGE_KEY = 'blink_guest_cart';

// 3. Trạng thái giỏ hàng trong bộ nhớ làm việc hiện tại
let cart = [];

/**
 * Nạp dữ liệu giỏ hàng:
 * - Nếu ĐÃ ĐĂNG NHẬP: Nạp trực tiếp từ RESTful API /api/GioHang/{maKhachHang} trên CSDL SQL Server (TUYỆT ĐỐI KHÔNG DÙNG localStorage).
 * - Nếu CHƯA ĐĂNG NHẬP (Khách vãng lai): Nạp từ localStorage qua khóa blink_guest_cart.
 */
async function loadCart() {
    if (currentUser) {
        // Đã đăng nhập: Đảm bảo không còn dữ liệu guest cart trong localStorage
        try {
            const customerId = currentUser.maKhachHang || currentUser.maNguoiDung;
            const res = await fetch(`/api/GioHang/${encodeURIComponent(customerId)}`);
            if (res.ok) {
                const json = await res.json();
                if (json.success && json.data && Array.isArray(json.data.danhSachSanPham)) {
                    cart = json.data.danhSachSanPham.map(item => {
                        const baseProd = catalogProducts.find(p => p.maSanPham === item.maSanPham);
                        const unitPrice = item.giaBan || (baseProd ? (baseProd.giaSauGiam || baseProd.giaBan) : 0);
                        const origPrice = item.giaGoc || (baseProd ? baseProd.giaBan : unitPrice);
                        const discount = item.phanTramGiamGia !== undefined ? item.phanTramGiamGia : (baseProd?.phanTramGiamGia || 0);

                        return {
                            maSanPham: item.maSanPham,
                            tenSanPham: item.tenSanPham || baseProd?.tenSanPham || item.maSanPham,
                            chatLieu: item.chatLieu || baseProd?.chatLieu || '',
                            size: item.size || baseProd?.size || '',
                            giaBan: unitPrice,
                            giaGoc: origPrice,
                            phanTramGiamGia: discount,
                            soLuong: item.soLuong,
                            hinhAnh: item.hinhAnh || baseProd?.hinhAnh || ''
                        };
                    });
                } else {
                    cart = [];
                }
            }
        } catch (e) {
            console.error('Không thể nạp giỏ hàng từ máy chủ:', e);
            cart = [];
        }
    } else {
        // Khách vãng lai: Nạp từ localStorage
        try {
            const raw = localStorage.getItem(GUEST_CART_STORAGE_KEY);
            if (raw) {
                const parsed = JSON.parse(raw);
                if (Array.isArray(parsed)) {
                    cart = parsed;
                }
            } else {
                cart = [];
            }
        } catch (e) {
            console.error('Không thể phục hồi giỏ hàng khách vãng lai:', e);
            cart = [];
        }
    }
    updateCartUI();
}

/**
 * Lưu trạng thái giỏ hàng:
 * - Nếu ĐÃ ĐĂNG NHẬP: Gửi yêu cầu lưu trữ vào CSDL qua POST /api/GioHang/cap-nhat (TUYỆT ĐỐI KHÔNG GHI localStorage).
 * - Nếu CHƯA ĐĂNG NHẬP: Lưu vào localStorage qua khóa blink_guest_cart.
 */
async function saveCart() {
    if (currentUser) {
        // Đã đăng nhập: Xóa sạch guest cart khỏi localStorage để không nhầm lẫn
        localStorage.removeItem(GUEST_CART_STORAGE_KEY);

        try {
            const customerId = currentUser.maKhachHang || currentUser.maNguoiDung;
            await apiRequest('/api/GioHang/cap-nhat', {
                method: 'POST',
                body: JSON.stringify({
                    maKhachHang: customerId,
                    danhSachSanPham: cart.map(item => ({
                        maSanPham: item.maSanPham,
                        soLuong: item.soLuong
                    }))
                })
            });
        } catch (e) {
            console.error('Không thể đồng bộ giỏ hàng lên máy chủ:', e);
        }
    } else {
        // Khách vãng lai: Lưu vào localStorage
        try {
            localStorage.setItem(GUEST_CART_STORAGE_KEY, JSON.stringify(cart));
        } catch (e) {
            console.error('Không thể lưu giỏ hàng khách vãng lai:', e);
        }
    }
}

// Định dạng tiền tệ VND
function formatCurrency(amount) {
    return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(amount);
}

// Khởi chạy khi tài liệu DOM sẵn sàng
document.addEventListener('DOMContentLoaded', async () => {
    initAuth();
    initHeaderSearch();
    initNavigationAndScrollSpy();
    await fetchCatalogFromApi();
    // Thiết lập kích thước trang mặc định: 12 sản phẩm/trang trên trang tìm kiếm, 8 sản phẩm/trang trên trang chủ
    if (window.location.pathname.toLowerCase().includes('timkiem') || document.querySelector('.search-page-container')) {
        currentCatalogPageSize = 12;
    } else {
        currentCatalogPageSize = 8;
    }
    initCategoryFilters();
    applyProductFilters();
    await loadCart();
    initEventListeners();
});

/**
 * Hiển thị lưới sản phẩm kim hoàn với nhãn giảm giá và giá sau giảm
 * @param {Array} products - Danh sách sản phẩm cần vẽ lên lưới
 */
/**
 * Gom nhóm sản phẩm để hiển thị trên Web (Hướng 1):
 * - Sản phẩm có nhóm (maNhom != "0"): Gom lại thành 1 mục đại diện mang Tên nhóm (tenNhom), kèm danh sách các size.
 * - Sản phẩm không có nhóm (maNhom == "0"): Giữ nguyên từng sản phẩm riêng lẻ mang Tên sản phẩm (tenSanPham).
 * @param {Array} products - Danh sách sản phẩm sau lọc
 * @returns {Array} Danh sách đã gom nhóm hiển thị
 */
function groupProductsForDisplay(products) {
    if (!products || products.length === 0) return [];

    const displayList = [];
    const groupedMap = new Map();

    products.forEach(p => {
        const maNhom = p.maNhom || p.MaNhom || '0';
        if (maNhom && maNhom !== '0') {
            const groupName = p.tenNhom || p.TenNhom || p.tenSanPham;
            if (!groupedMap.has(maNhom)) {
                const groupItem = {
                    ...p,
                    isGroup: true,
                    tenHienThi: groupName,
                    variants: [p],
                    allSizes: p.size ? [p.size] : []
                };
                groupedMap.set(maNhom, groupItem);
                displayList.push(groupItem);
            } else {
                const groupItem = groupedMap.get(maNhom);
                groupItem.variants.push(p);
                if (p.size && !groupItem.allSizes.includes(p.size)) {
                    groupItem.allSizes.push(p.size);
                }
                const stock = p.soLuongTon !== undefined ? p.soLuongTon : (p.SoLuongTon !== undefined ? p.SoLuongTon : 0);
                if (stock > 0 && p.trangThai === 'SanSang') {
                    groupItem.conHang = true;
                    if (groupItem.soLuongTon <= 0 || groupItem.trangThai !== 'SanSang') {
                        groupItem.maSanPham = p.maSanPham;
                        groupItem.giaBan = p.giaBan;
                        groupItem.giaSauGiam = p.giaSauGiam;
                        groupItem.phanTramGiamGia = p.phanTramGiamGia;
                        groupItem.hinhAnh = p.hinhAnh || groupItem.hinhAnh;
                        groupItem.trangThai = p.trangThai;
                    }
                }
                groupItem.soLuongTon = (groupItem.soLuongTon || 0) + stock;
            }
        } else {
            displayList.push({
                ...p,
                isGroup: false,
                tenHienThi: p.tenSanPham,
                variants: [p],
                allSizes: p.size ? [p.size] : []
            });
        }
    });

    return displayList;
}

function initProductGrid(products) {
    const gridContainer = document.getElementById('products-grid');
    if (!gridContainer) return;

    gridContainer.innerHTML = '';

    if (!products || products.length === 0) {
        renderNoResults(gridContainer);
        return;
    }

    products.forEach(p => {
        const card = document.createElement('div');
        card.className = 'product-card';

        const stock = p.soLuongTon !== undefined ? p.soLuongTon : (p.SoLuongTon !== undefined ? p.SoLuongTon : 1);
        const isOutOfStock = stock <= 0 || p.trangThai === 'HetHang';
        const hasSale = p.phanTramGiamGia && p.phanTramGiamGia > 0;

        let tagHtml = '';
        if (isOutOfStock) {
            tagHtml = '<span class="product-status-tag out-of-stock">Tạm Hết Hàng</span>';
        } else if (hasSale) {
            tagHtml = `<span class="product-status-tag sale">-${p.phanTramGiamGia}%</span>`;
        } else if (p.isGroup) {
            tagHtml = '<span class="product-status-tag" style="background: rgba(212,175,55,0.18); color: var(--gold-light); border: 1px solid var(--gold-primary);">Theo Nhóm</span>';
        } else {
            tagHtml = '<span class="product-status-tag">Kiệt Tác</span>';
        }

        const effectivePrice = p.giaSauGiam || p.giaBan;
        const priceHtml = hasSale
            ? `
                <div class="product-price-group">
                    <span class="price-original-strikethrough">${formatCurrency(p.giaBan)}</span>
                    <span class="product-price-tabular has-sale">${formatCurrency(effectivePrice)}</span>
                </div>
              `
            : `<span class="product-price-tabular">${formatCurrency(p.giaBan)}</span>`;

        const displayName = p.tenHienThi || (p.isGroup ? (p.tenNhom || p.tenSanPham) : p.tenSanPham);
        const displaySizes = (p.allSizes && p.allSizes.length > 1)
            ? p.allSizes.join(' • ')
            : (p.size || 'Tiêu chuẩn');

        const cardBuyButton = isOutOfStock
            ? `
                <button class="btn-card-addcart is-disabled" disabled title="Kiệt tác hiện đang tạm hết hàng tại showroom">
                    <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/></svg>
                    Hết Hàng
                </button>
              `
            : (p.isGroup && p.allSizes && p.allSizes.length > 1)
                ? `
                    <button class="btn-card-addcart" onclick="openQuickView('${p.maSanPham}')" title="Chọn kích cỡ trong nhóm ${displayName}">
                        <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M6 2L3 6v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2V6l-3-4z"/><line x1="3" y1="6" x2="21" y2="6"/><path d="M16 10a4 4 0 0 1-8 0"/></svg>
                        Chọn Size
                    </button>
                  `
                : `
                    <button class="btn-card-addcart" onclick="addToCart('${p.maSanPham}')">
                        <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M6 2L3 6v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2V6l-3-4z"/><line x1="3" y1="6" x2="21" y2="6"/><path d="M16 10a4 4 0 0 1-8 0"/></svg>
                        Chọn Mua
                    </button>
                  `;

        card.innerHTML = `
            <div class="product-thumb-wrap">
                <img src="${p.hinhAnh || '/images/hero-jewelry.jpg'}" alt="${displayName}" loading="lazy" onerror="this.onerror=null;this.src='/images/hero-jewelry.jpg';" />
                ${tagHtml}
                <div class="product-card-actions">
                    <button class="btn-card-quickview" onclick="openQuickView('${p.maSanPham}')">
                        <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8z"/><circle cx="12" cy="12" r="3"/></svg>
                        Xem Nhanh
                    </button>
                    ${cardBuyButton}
                </div>
            </div>
            <div class="product-info-body">
                <span class="product-category-meta">${p.tenDanhMuc || 'Trang Sức Cao Cấp'}</span>
                <h3 class="product-name-title">${displayName}</h3>
                <p class="product-material-desc">${p.chatLieu}</p>
                <div class="product-footer-row">
                    <span class="product-size-badge" title="${p.isGroup ? 'Kích cỡ sẵn có trong nhóm' : 'Kích cỡ kiệt tác'}">${displaySizes}</span>
                    ${priceHtml}
                </div>
            </div>
        `;
        gridContainer.appendChild(card);
    });
}

/**
 * Chuẩn hóa chuỗi tìm kiếm tiếng Việt: loại bỏ dấu thanh và ký tự đặc biệt, chuyển chữ thường
 * @param {string} str - Chuỗi tiếng Việt cần chuẩn hóa
 * @returns {string} Chuỗi không dấu chuẩn hóa
 */
function normalizeSearchText(str) {
    if (!str) return '';
    return str
        .normalize('NFD')
        .replace(/[\u0300-\u036f]/g, '')
        .replace(/đ/g, 'd')
        .replace(/Đ/g, 'D')
        .toLowerCase()
        .trim();
}

/**
 * Hiển thị trạng thái rỗng và đề xuất kiệt tác liên quan khi không tìm thấy kết quả
 * @param {HTMLElement} container - Khung chứa lưới sản phẩm
 */
function renderNoResults(container) {
    const activeCategoryBtn = document.querySelector('.filter-pill-btn.active');
    const activeCategory = activeCategoryBtn ? activeCategoryBtn.getAttribute('data-category') : 'ALL';
    const searchInput = document.getElementById('catalog-search-input');
    const keyword = searchInput ? searchInput.value.trim() : '';

    // Tìm kiếm các sản phẩm gợi ý liên quan:
    // 1. Ưu tiên sản phẩm cùng danh mục đang chọn (nếu có)
    // 2. Hoặc lấy các sản phẩm có ưu đãi / bán chạy nổi bật từ catalogProducts
    let related = [];
    if (activeCategory && activeCategory !== 'ALL') {
        related = catalogProducts.filter(p => p.maDanhMuc === activeCategory);
    }
    if (related.length === 0) {
        related = [...catalogProducts].sort((a, b) => (b.phanTramGiamGia || 0) - (a.phanTramGiamGia || 0));
    }
    const topRelated = related.slice(0, 3);

    const emptyWrapper = document.createElement('div');
    emptyWrapper.className = 'search-empty-state';
    emptyWrapper.id = 'search-empty-state';

    emptyWrapper.innerHTML = `
        <div class="empty-state-icon-wrap">
            <svg width="32" height="32" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8">
                <circle cx="11" cy="11" r="8"/>
                <line x1="21" y1="21" x2="16.65" y2="16.65"/>
                <line x1="8" y1="11" x2="14" y2="11"/>
            </svg>
        </div>
        <h3 class="empty-state-title">Không tìm thấy sản phẩm phù hợp</h3>
        <p class="empty-state-desc">
            Rất tiếc, không có kiệt tác trang sức nào khớp với tiêu chí tìm kiếm ${keyword ? `"${keyword}"` : 'hiện tại'} của quý khách. Quý khách vui lòng thử tìm kiếm bằng từ khóa khác hoặc đặt lại bộ lọc để khám phá toàn bộ bộ sưu tập.
        </p>
        <div class="empty-state-actions">
            <button type="button" class="btn-luxury-outline" onclick="resetAllFilters()">
                <svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" style="margin-right: 6px;"><path d="M3 12a9 9 0 1 0 9-9 9.75 9.75 0 0 0-6.74 2.74L3 8"/><path d="M3 3v5h5"/></svg>
                Đặt Lại Bộ Lọc & Khám Phá Tuyển Tập
            </button>
        </div>

        ${topRelated.length > 0 ? `
            <div class="related-recommendations-section">
                <div class="recommendations-header-block">
                    <span class="recommendations-pretitle">Gợi Ý Dành Riêng Cho Quý Khách</span>
                    <h4 class="recommendations-heading">Kiệt Tác Kim Hoàn Được Yêu Thích Nhất</h4>
                </div>
                <div class="products-editorial-grid" id="related-products-grid"></div>
            </div>
        ` : ''}
    `;

    container.appendChild(emptyWrapper);

    // Gắn thẻ sản phẩm vào lưới gợi ý
    if (topRelated.length > 0) {
        const relatedGrid = emptyWrapper.querySelector('#related-products-grid');
        if (relatedGrid) {
            topRelated.forEach(p => {
                const card = document.createElement('div');
                card.className = 'product-card';

                const hasSale = p.phanTramGiamGia && p.phanTramGiamGia > 0;
                const tagHtml = hasSale
                    ? `<span class="product-status-tag sale">-${p.phanTramGiamGia}%</span>`
                    : `<span class="product-status-tag">Kiệt Tác</span>`;

                const effectivePrice = p.giaSauGiam || p.giaBan;
                const priceHtml = hasSale
                    ? `
                        <div class="product-price-group">
                            <span class="price-original-strikethrough">${formatCurrency(p.giaBan)}</span>
                            <span class="product-price-tabular has-sale">${formatCurrency(effectivePrice)}</span>
                        </div>
                      `
                    : `<span class="product-price-tabular">${formatCurrency(p.giaBan)}</span>`;

                card.innerHTML = `
                    <div class="product-thumb-wrap">
                        <img src="${p.hinhAnh || '/images/hero-jewelry.jpg'}" alt="${p.tenSanPham}" loading="lazy" onerror="this.onerror=null;this.src='/images/hero-jewelry.jpg';" />
                        ${tagHtml}
                        <div class="product-card-actions">
                            <button class="btn-card-quickview" onclick="openQuickView('${p.maSanPham}')">
                                <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8z"/><circle cx="12" cy="12" r="3"/></svg>
                                Xem Nhanh
                            </button>
                            <button class="btn-card-addcart" onclick="addToCart('${p.maSanPham}')">
                                <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M6 2L3 6v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2V6l-3-4z"/><line x1="3" y1="6" x2="21" y2="6"/><path d="M16 10a4 4 0 0 1-8 0"/></svg>
                                Chọn Mua
                            </button>
                        </div>
                    </div>
                    <div class="product-info-body">
                        <span class="product-category-meta">${p.tenDanhMuc || 'Trang Sức Cao Cấp'}</span>
                        <h3 class="product-name-title">${p.tenSanPham}</h3>
                        <p class="product-material-desc">${p.chatLieu}</p>
                        <div class="product-footer-row">
                            <span class="product-size-badge">${p.size || 'Tiêu chuẩn'}</span>
                            ${priceHtml}
                        </div>
                    </div>
                `;
                relatedGrid.appendChild(card);
            });
        }
    }
}

/**
 * Khởi tạo bộ lọc danh mục và ô tìm kiếm
 */
function initCategoryFilters() {
    const filterButtons = document.querySelectorAll('.filter-pill-btn');
    const searchInput = document.getElementById('catalog-search-input');
    const searchClearBtn = document.getElementById('catalog-search-clear');
    const priceSelect = document.getElementById('catalog-filter-price');
    const materialSelect = document.getElementById('catalog-filter-material');
    const sortSelect = document.getElementById('catalog-sort-order');

    // Lắng nghe click các pill danh mục
    filterButtons.forEach(btn => {
        btn.addEventListener('click', () => {
            filterButtons.forEach(b => b.classList.remove('active'));
            btn.classList.add('active');
            applyProductFilters();
        });
    });

    // Lắng nghe gõ từ khóa tìm kiếm
    if (searchInput) {
        searchInput.addEventListener('input', (e) => {
            if (searchClearBtn) {
                searchClearBtn.style.display = e.target.value.trim() ? 'flex' : 'none';
            }
            applyProductFilters();
        });
    }

    // Nút xóa nhanh từ khóa tìm kiếm
    if (searchClearBtn && searchInput) {
        searchClearBtn.addEventListener('click', () => {
            searchInput.value = '';
            searchClearBtn.style.display = 'none';
            searchInput.focus();
            applyProductFilters();
        });
    }

    // Lắng nghe thay đổi các select lọc mở rộng
    if (priceSelect) {
        priceSelect.addEventListener('change', () => applyProductFilters());
    }
    if (materialSelect) {
        materialSelect.addEventListener('change', () => applyProductFilters());
    }
    if (sortSelect) {
        sortSelect.addEventListener('change', () => applyProductFilters());
    }

    // Tự động nhận diện từ khóa và danh mục từ URL (nếu có)
    const urlParams = new URLSearchParams(window.location.search);
    const initialKeyword = urlParams.get('tuKhoa') || urlParams.get('q');
    const initialCategory = urlParams.get('danhMuc');

    if (initialCategory) {
        filterButtons.forEach(btn => {
            if (btn.getAttribute('data-category') === initialCategory) {
                filterButtons.forEach(b => b.classList.remove('active'));
                btn.classList.add('active');
            }
        });
    }

    if (initialKeyword && searchInput) {
        searchInput.value = initialKeyword;
        if (searchClearBtn) {
            searchClearBtn.style.display = 'flex';
        }
    }

    // Cập nhật số lượng sản phẩm ban đầu
    updateFilterStatusCount(catalogProducts.length);
}

/**
 * Áp dụng toàn bộ bộ lọc đa chiều: từ khóa, danh mục, mức giá, chất liệu, sắp xếp
 */
function applyProductFilters(resetPage = true) {
    if (resetPage) {
        currentCatalogPage = 1;
    }
    let filtered = [...catalogProducts];

    // 1. Lọc theo danh mục
    const activeCategoryBtn = document.querySelector('.filter-pill-btn.active');
    const category = activeCategoryBtn ? activeCategoryBtn.getAttribute('data-category') : 'ALL';
    if (category && category !== 'ALL') {
        filtered = filtered.filter(p => p.maDanhMuc === category);
    }

    // 2. Lọc theo từ khóa tìm kiếm (hỗ trợ không dấu, có dấu, mã SP, chất liệu)
    const searchInput = document.getElementById('catalog-search-input');
    const keyword = searchInput ? searchInput.value.trim() : '';
    if (keyword !== '') {
        const rawQuery = keyword.toLowerCase();
        const normQuery = normalizeSearchText(keyword);

        filtered = filtered.filter(p => {
            const rawTen = (p.tenSanPham || '').toLowerCase();
            const rawNhom = (p.tenNhom || '').toLowerCase();
            const rawChatLieu = (p.chatLieu || '').toLowerCase();
            const rawMa = (p.maSanPham || '').toLowerCase();
            const rawSize = (p.size || '').toLowerCase();
            const rawDanhMuc = (p.tenDanhMuc || '').toLowerCase();

            const normTen = normalizeSearchText(p.tenSanPham);
            const normNhom = normalizeSearchText(p.tenNhom);
            const normChatLieu = normalizeSearchText(p.chatLieu);
            const normMa = normalizeSearchText(p.maSanPham);
            const normDanhMuc = normalizeSearchText(p.tenDanhMuc);

            return (
                rawTen.includes(rawQuery) ||
                rawNhom.includes(rawQuery) ||
                rawChatLieu.includes(rawQuery) ||
                rawMa.includes(rawQuery) ||
                rawSize.includes(rawQuery) ||
                rawDanhMuc.includes(rawQuery) ||
                normTen.includes(normQuery) ||
                normNhom.includes(normQuery) ||
                normChatLieu.includes(normQuery) ||
                normMa.includes(normQuery) ||
                normDanhMuc.includes(normQuery)
            );
        });
    }

    // 3. Lọc theo khoảng giá bán (tính theo giá sau giảm nếu có ưu đãi)
    const priceSelect = document.getElementById('catalog-filter-price');
    const priceRange = priceSelect ? priceSelect.value : 'ALL';
    if (priceRange !== 'ALL') {
        filtered = filtered.filter(p => {
            const price = p.giaSauGiam || p.giaBan || 0;
            if (priceRange === 'UNDER_20M') return price < 20000000;
            if (priceRange === '20M_40M') return price >= 20000000 && price <= 40000000;
            if (priceRange === 'OVER_40M') return price > 40000000;
            return true;
        });
    }

    // 4. Lọc theo chất liệu
    const materialSelect = document.getElementById('catalog-filter-material');
    const material = materialSelect ? materialSelect.value : 'ALL';
    if (material !== 'ALL') {
        filtered = filtered.filter(p => {
            const normCL = normalizeSearchText(p.chatLieu);
            if (material === 'KIM_CUONG') return normCL.includes('kim cuong') || normCL.includes('diamond');
            if (material === 'VANG_18K') return normCL.includes('vang') || normCL.includes('18k') || normCL.includes('750');
            if (material === 'BACH_KIM') return normCL.includes('bach kim') || normCL.includes('platin');
            if (material === 'RUBY_SAPPHIRE') return normCL.includes('ruby') || normCL.includes('sapphire') || normCL.includes('da quy');
            if (material === 'NGOC_TRAI') return normCL.includes('ngoc trai') || normCL.includes('pearl');
            return true;
        });
    }

    // 5. Sắp xếp thứ tự hiển thị
    const sortSelect = document.getElementById('catalog-sort-order');
    const sortOrder = sortSelect ? sortSelect.value : 'DEFAULT';
    if (sortOrder === 'PRICE_ASC') {
        filtered.sort((a, b) => (a.giaSauGiam || a.giaBan) - (b.giaSauGiam || b.giaBan));
    } else if (sortOrder === 'PRICE_DESC') {
        filtered.sort((a, b) => (b.giaSauGiam || b.giaBan) - (a.giaSauGiam || a.giaBan));
    } else if (sortOrder === 'DISCOUNT') {
        filtered.sort((a, b) => (b.phanTramGiamGia || 0) - (a.phanTramGiamGia || 0));
    }

    // Cập nhật số lượng và render lưới sản phẩm
    // Gom nhóm sản phẩm theo Hướng 1: có nhóm thì hiện nhóm, không có nhóm thì hiện sản phẩm
    const groupedDisplayList = groupProductsForDisplay(filtered);

    // Cập nhật số lượng và render lưới sản phẩm
    updateFilterStatusCount(groupedDisplayList.length);
    updateActiveFilterBadges(category, keyword, priceRange, material);
    // Lưu danh sách sản phẩm sau lọc để phân trang
    currentFilteredProducts = groupedDisplayList;

    const totalItems = groupedDisplayList.length;
    const totalPages = Math.ceil(totalItems / currentCatalogPageSize) || 1;
    if (currentCatalogPage > totalPages) {
        currentCatalogPage = totalPages;
    }
    if (currentCatalogPage < 1) {
        currentCatalogPage = 1;
    }

    const startIndex = (currentCatalogPage - 1) * currentCatalogPageSize;
    const endIndex = Math.min(startIndex + currentCatalogPageSize, totalItems);
    const pagedItems = groupedDisplayList.slice(startIndex, endIndex);

    initProductGrid(pagedItems);
    renderCatalogPagination(totalItems, currentCatalogPage, currentCatalogPageSize);
}

/**
 * Cập nhật số lượng sản phẩm hiển thị trên thanh trạng thái
 * @param {number} count - Số lượng kết quả
 */
/**
 * Vẽ thanh phân trang sản phẩm theo Taste Skill kim hoàn cao cấp
 * Giải thích: Chia nhỏ tập kết quả giúp giảm tải DOM, tăng tốc độ render ảnh và nâng tầm trải nghiệm sang trọng.
 * @param {number} totalItems - Tổng số sản phẩm sau khi lọc
 * @param {number} currentPage - Số thứ tự trang hiện tại (1-based)
 * @param {number} pageSize - Số sản phẩm trên mỗi trang
 */
function renderCatalogPagination(totalItems, currentPage, pageSize) {
    const paginationContainer = document.getElementById('products-pagination');
    if (!paginationContainer) return;

    if (!totalItems || totalItems === 0) {
        paginationContainer.style.display = 'none';
        paginationContainer.innerHTML = '';
        return;
    }

    const totalPages = Math.ceil(totalItems / pageSize) || 1;
    const startIndex = (currentPage - 1) * pageSize;
    const endIndex = Math.min(startIndex + pageSize, totalItems);

    paginationContainer.style.display = 'flex';

    // 1. Dòng tổng quan số lượng hiển thị (Tabular summary)
    const summaryHtml = `
        <div class="pagination-summary">
            <span>Hiển thị</span>
            <span class="summary-highlight">${startIndex + 1} - ${endIndex}</span>
            <span>trong tổng số</span>
            <span class="summary-highlight">${totalItems}</span>
            <span>kiệt tác</span>
        </div>
    `;

    // 2. Cụm nút chuyển trang (Pagination Controls)
    let buttonsHtml = '';

    // Nút Trang trước
    const prevDisabled = currentPage <= 1 ? 'disabled' : '';
    buttonsHtml += `
        <button type="button" class="pagination-btn pagination-btn-nav" ${prevDisabled} onclick="changeCatalogPage(${currentPage - 1})" title="Trang trước">
            <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><polyline points="15 18 9 12 15 6"/></svg>
            Trước
        </button>
    `;

    // Dãy số trang với thuật toán thu gọn thông minh (Ellipsis)
    if (totalPages <= 7) {
        for (let i = 1; i <= totalPages; i++) {
            const activeClass = i === currentPage ? 'active' : '';
            buttonsHtml += `<button type="button" class="pagination-btn ${activeClass}" onclick="changeCatalogPage(${i})">${i}</button>`;
        }
    } else {
        const pages = [];
        if (currentPage <= 4) {
            for (let i = 1; i <= 5; i++) pages.push(i);
            pages.push('...');
            pages.push(totalPages);
        } else if (currentPage >= totalPages - 3) {
            pages.push(1);
            pages.push('...');
            for (let i = totalPages - 4; i <= totalPages; i++) pages.push(i);
        } else {
            pages.push(1);
            pages.push('...');
            pages.push(currentPage - 1);
            pages.push(currentPage);
            pages.push(currentPage + 1);
            pages.push('...');
            pages.push(totalPages);
        }

        pages.forEach(p => {
            if (p === '...') {
                buttonsHtml += `<span class="pagination-ellipsis">&hellip;</span>`;
            } else {
                const activeClass = p === currentPage ? 'active' : '';
                buttonsHtml += `<button type="button" class="pagination-btn ${activeClass}" onclick="changeCatalogPage(${p})">${p}</button>`;
            }
        });
    }

    // Nút Trang sau
    const nextDisabled = currentPage >= totalPages ? 'disabled' : '';
    buttonsHtml += `
        <button type="button" class="pagination-btn pagination-btn-nav" ${nextDisabled} onclick="changeCatalogPage(${currentPage + 1})" title="Trang kế tiếp">
            Sau
            <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><polyline points="9 18 15 12 9 6"/></svg>
        </button>
    `;

    const controlsHtml = `<div class="pagination-controls">${buttonsHtml}</div>`;

    // 3. Khối tùy chọn kích thước trang linh hoạt
    const sizeSelectorHtml = `
        <div class="pagination-size-wrapper">
            <label for="pagination-page-size">Mỗi trang:</label>
            <select id="pagination-page-size" class="pagination-size-select" onchange="changeCatalogPageSize(Number(this.value))">
                <option value="8" ${pageSize === 8 ? 'selected' : ''}>8 mẫu</option>
                <option value="12" ${pageSize === 12 ? 'selected' : ''}>12 mẫu</option>
                <option value="24" ${pageSize === 24 ? 'selected' : ''}>24 mẫu</option>
                <option value="48" ${pageSize === 48 ? 'selected' : ''}>48 mẫu</option>
            </select>
        </div>
    `;

    paginationContainer.innerHTML = summaryHtml + controlsHtml + sizeSelectorHtml;
}

/**
 * Chuyển đến trang chỉ định và tự động cuộn nhẹ lên đầu danh sách sản phẩm
 * @param {number} newPage - Trang đích cần hiển thị
 */
function changeCatalogPage(newPage) {
    const totalPages = Math.ceil(currentFilteredProducts.length / currentCatalogPageSize) || 1;
    if (newPage < 1 || newPage > totalPages || newPage === currentCatalogPage) return;

    currentCatalogPage = newPage;
    applyProductFilters(false);

    // Cuộn mượt về đầu danh mục sản phẩm có tính toán trừ khoảng cách của thanh navbar cố định
    const targetElement = document.getElementById('products-grid') || document.getElementById('catalog');
    if (targetElement) {
        const headerOffset = 100;
        const elementPosition = targetElement.getBoundingClientRect().top;
        const offsetPosition = elementPosition + window.pageYOffset - headerOffset;
        window.scrollTo({
            top: Math.max(0, offsetPosition),
            behavior: 'smooth'
        });
    }
}

/**
 * Điều chỉnh số lượng sản phẩm trên mỗi trang và reset về trang 1
 * @param {number} newSize - Số sản phẩm mỗi trang (8, 12, 24, 48)
 */
function changeCatalogPageSize(newSize) {
    if (!newSize || newSize <= 0) return;
    currentCatalogPageSize = Number(newSize);
    currentCatalogPage = 1;
    applyProductFilters(false);
}

function updateFilterStatusCount(count) {
    const countEl = document.getElementById('filtered-count');
    if (countEl) {
        countEl.textContent = count;
    }
}

/**
 * Hiển thị các nhãn tag bộ lọc đang được kích hoạt
 */
function updateActiveFilterBadges(category, keyword, priceRange, material) {
    const tagsContainer = document.getElementById('active-filter-tags');
    if (!tagsContainer) return;

    tagsContainer.innerHTML = '';
    const tags = [];

    if (keyword) tags.push(`Từ khóa: "${keyword}"`);
    if (category && category !== 'ALL') {
        const catBtn = document.querySelector(`.filter-pill-btn[data-category="${category}"]`);
        if (catBtn) tags.push(`Danh mục: ${catBtn.textContent.trim()}`);
    }
    if (priceRange === 'UNDER_20M') tags.push('< 20 triệu');
    if (priceRange === '20M_40M') tags.push('20 - 40 triệu');
    if (priceRange === 'OVER_40M') tags.push('> 40 triệu');

    if (material === 'KIM_CUONG') tags.push('Kim Cương');
    if (material === 'VANG_18K') tags.push('Vàng 18K');
    if (material === 'BACH_KIM') tags.push('Bạch Kim');
    if (material === 'RUBY_SAPPHIRE') tags.push('Đá Quý');
    if (material === 'NGOC_TRAI') tags.push('Ngọc Trai');

    tags.forEach(t => {
        const badge = document.createElement('span');
        badge.className = 'filter-badge-tag';
        badge.textContent = t;
        tagsContainer.appendChild(badge);
    });
}

/**
 * Đặt lại toàn bộ bộ lọc và từ khóa tìm kiếm về trạng thái mặc định
 */
function resetAllFilters() {
    const searchInput = document.getElementById('catalog-search-input');
    const searchClearBtn = document.getElementById('catalog-search-clear');
    const priceSelect = document.getElementById('catalog-filter-price');
    const materialSelect = document.getElementById('catalog-filter-material');
    const sortSelect = document.getElementById('catalog-sort-order');
    const filterButtons = document.querySelectorAll('.filter-pill-btn');

    if (searchInput) searchInput.value = '';
    if (searchClearBtn) searchClearBtn.style.display = 'none';

    filterButtons.forEach(b => {
        b.classList.remove('active');
        if (b.getAttribute('data-category') === 'ALL') {
            b.classList.add('active');
        }
    });

    if (priceSelect) priceSelect.value = 'ALL';
    if (materialSelect) materialSelect.value = 'ALL';
    if (sortSelect) sortSelect.value = 'DEFAULT';

    applyProductFilters();
    showToast('Đã đặt lại toàn bộ bộ lọc kiệt tác trang sức.');
}

/**
 * Hàm tương thích ngược với luồng code cũ
 */
function filterProducts(category, keyword) {
    applyProductFilters();
}

/**
 * Thêm sản phẩm vào giỏ hàng (Áp dụng giá sau giảm nếu có ưu đãi)
 * @param {string} maSanPham - Mã sản phẩm kim hoàn cần thêm
 */
async function addToCart(maSanPham) {
    const product = catalogProducts.find(p => p.maSanPham === maSanPham);
    if (!product) return;

    const stock = product.soLuongTon !== undefined ? product.soLuongTon : (product.SoLuongTon !== undefined ? product.SoLuongTon : 1);
    if (stock <= 0 || product.trangThai === 'HetHang') {
        showToast(`Kiệt tác "${product.tenSanPham}" hiện đang tạm hết hàng tại showroom. Quý khách vui lòng chọn mẫu khác hoặc liên hệ nhân viên tư vấn.`);
        return;
    }

    const effectivePrice = product.giaSauGiam || product.giaBan;
    const existingIndex = cart.findIndex(item => item.maSanPham === maSanPham);
    if (existingIndex > -1) {
        if (cart[existingIndex].soLuong + 1 > stock) {
            showToast(`Số lượng trong giỏ đã đạt mức tồn kho tối đa (${stock} món) của kiệt tác này.`);
            return;
        }
        cart[existingIndex].soLuong += 1;
    } else {
        cart.push({
            maSanPham: product.maSanPham,
            tenSanPham: product.tenSanPham,
            chatLieu: product.chatLieu,
            size: product.size,
            giaBan: effectivePrice,
            giaGoc: product.giaBan,
            phanTramGiamGia: product.phanTramGiamGia || 0,
            soLuong: 1,
            hinhAnh: product.hinhAnh
        });
    }

    await saveCart();
    updateCartUI();
    showToast(`Đã thêm "${product.tenSanPham}" vào giỏ hàng thành công!`);
}

/**
 * Cập nhật giao diện ngăn kéo giỏ hàng
 */
function updateCartUI() {
    const badge = document.getElementById('cart-badge');
    const itemsContainer = document.getElementById('cart-items-list');
    const subtotalEl = document.getElementById('cart-subtotal');
    const totalCount = cart.reduce((sum, item) => sum + item.soLuong, 0);

    if (badge) {
        badge.textContent = totalCount;
    }

    if (!itemsContainer) return;

    if (cart.length === 0) {
        itemsContainer.innerHTML = `
            <div class="cart-empty-message">
                <svg width="48" height="48" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5"><path d="M6 2L3 6v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2V6l-3-4z"/><line x1="3" y1="6" x2="21" y2="6"/><path d="M16 10a4 4 0 0 1-8 0"/></svg>
                <p>Giỏ hàng của quý khách hiện đang trống.</p>
                <button class="btn-luxury-outline" style="margin-top: 16px; font-size: 0.78rem;" onclick="toggleCartDrawer(false)">Khám Phá Bộ Sưu Tập</button>
            </div>
        `;
        if (subtotalEl) subtotalEl.textContent = formatCurrency(0);
        return;
    }

    itemsContainer.innerHTML = '';
    let total = 0;

    cart.forEach(item => {
        total += item.giaBan * item.soLuong;
        const row = document.createElement('div');
        row.className = 'cart-item-row';

        let priceHtml = `<div class="cart-item-price">${formatCurrency(item.giaBan)}</div>`;
        if (item.phanTramGiamGia > 0 && item.giaGoc && item.giaGoc > item.giaBan) {
            priceHtml = `
                <div class="cart-item-price">
                    <span class="cart-item-price-original">${formatCurrency(item.giaGoc)}</span>
                    <span class="cart-item-price-sale">${formatCurrency(item.giaBan)}</span>
                </div>
            `;
        }

        row.innerHTML = `
            <img class="cart-item-thumb" src="${item.hinhAnh || '/images/hero-jewelry.jpg'}" alt="${item.tenSanPham}" onerror="this.onerror=null;this.src='/images/hero-jewelry.jpg';" />
            <div class="cart-item-info">
                <h4 class="cart-item-name">${item.tenSanPham}</h4>
                <div class="cart-item-meta">${item.size} • ${item.chatLieu}</div>
                ${priceHtml}
                <div class="cart-item-qty-row">
                    <div class="qty-control-box">
                        <button class="btn-qty-adj" onclick="changeQty('${item.maSanPham}', -1)">-</button>
                        <span class="qty-display-num">${item.soLuong}</span>
                        <button class="btn-qty-adj" onclick="changeQty('${item.maSanPham}', 1)">+</button>
                    </div>
                    <button class="btn-remove-item" onclick="removeFromCart('${item.maSanPham}')">Xoá</button>
                </div>
            </div>
        `;
        itemsContainer.appendChild(row);
    });

    if (subtotalEl) {
        subtotalEl.textContent = formatCurrency(total);
    }
}

/**
 * Tăng hoặc giảm số lượng món đồ trong giỏ
 */
async function changeQty(maSanPham, delta) {
    const item = cart.find(i => i.maSanPham === maSanPham);
    if (!item) return;

    item.soLuong += delta;
    if (item.soLuong <= 0) {
        await removeFromCart(maSanPham);
    } else {
        await saveCart();
        updateCartUI();
    }
}

/**
 * Xóa một món khỏi giỏ hàng
 */
async function removeFromCart(maSanPham) {
    cart = cart.filter(i => i.maSanPham !== maSanPham);
    await saveCart();
    updateCartUI();
    showToast('Đã xóa món đồ khỏi giỏ hàng.');
}

/**
 * Bật/tắt ngăn kéo giỏ hàng (Cart Drawer)
 */
function toggleCartDrawer(open) {
    const overlay = document.getElementById('cart-drawer-overlay');
    if (!overlay) return;

    if (open) {
        overlay.classList.add('active');
        document.body.style.overflow = 'hidden';
    } else {
        overlay.classList.remove('active');
        document.body.style.overflow = '';
    }
}

/**
 * Mở hộp thoại xem nhanh sản phẩm (Quick View Modal)
 */
function openQuickView(maSanPham) {
    const product = catalogProducts.find(p => p.maSanPham === maSanPham);
    if (!product) return;

    const modal = document.getElementById('quickview-modal');
    if (!modal) return;

    const modalImg = document.getElementById('modal-img');
    if (modalImg) {
        modalImg.src = product.hinhAnh || '/images/hero-jewelry.jpg';
        modalImg.onerror = function() { this.onerror = null; this.src = '/images/hero-jewelry.jpg'; };
    }
    const maNhomModal = product.maNhom || product.MaNhom || '0';
    const modalDisplayName = (maNhomModal && maNhomModal !== '0' && (product.tenNhom || product.TenNhom))
        ? (product.tenNhom || product.TenNhom)
        : product.tenSanPham;
    document.getElementById('modal-cat').textContent = product.tenDanhMuc || 'Trang Sức Cao Cấp';
    document.getElementById('modal-title').textContent = modalDisplayName;

    const priceContainer = document.getElementById('modal-price');
    if (priceContainer) {
        if (product.phanTramGiamGia > 0 && product.giaSauGiam) {
            priceContainer.innerHTML = `
                <div style="display: flex; align-items: baseline; gap: 10px; flex-wrap: wrap;">
                    <span class="modal-price-original">${formatCurrency(product.giaBan)}</span>
                    <span class="product-price-tabular has-sale" style="font-size: 1.35rem;">${formatCurrency(product.giaSauGiam)}</span>
                    <span class="product-status-tag sale" style="position: static; display: inline-block;">-${product.phanTramGiamGia}%</span>
                </div>
            `;
        } else {
            priceContainer.innerHTML = `<span class="product-price-tabular" style="font-size: 1.35rem;">${formatCurrency(product.giaBan)}</span>`;
        }
    }

    document.getElementById('modal-material').textContent = product.chatLieu;
    document.getElementById('modal-size').textContent = product.size || 'Tiêu chuẩn';
    document.getElementById('modal-desc').textContent = product.moTa || `Kiệt tác trang sức ${product.tenSanPham} chế tác tinh xảo từ ${product.chatLieu}.`;

    // Hiển thị trạng thái tồn kho thực tế từ CSDL
    const stockEl = document.getElementById('modal-stock');
    const stock = product.soLuongTon !== undefined ? product.soLuongTon : (product.SoLuongTon !== undefined ? product.SoLuongTon : 1);
    const isOutOfStock = stock <= 0 || product.trangThai === 'HetHang';
    if (stockEl) {
        if (isOutOfStock) {
            stockEl.textContent = 'Tạm hết hàng tại showroom';
            stockEl.style.color = '#FF8A80';
        } else if (stock <= 3) {
            stockEl.textContent = `Chỉ còn ${stock} món tại showroom`;
            stockEl.style.color = 'var(--gold-primary)';
        } else {
            stockEl.textContent = `Còn hàng (${stock} món sẵn sàng giao)`;
            stockEl.style.color = '#81C784';
        }
    }

    // Hiển thị danh sách nhóm kích cỡ cùng mẫu chế tác
    const groupWrap = document.getElementById('modal-group-sizes');
    const groupList = document.getElementById('modal-group-sizes-list');
    const maNhom = product.maNhom || product.MaNhom || '0';

    if (groupWrap && groupList) {
        if (maNhom && maNhom !== '0') {
            const groupVariants = catalogProducts.filter(item => (item.maNhom || item.MaNhom) === maNhom);
            if (groupVariants.length > 1) {
                groupList.innerHTML = groupVariants.map(variant => {
                    const isActive = variant.maSanPham === product.maSanPham;
                    const vStock = variant.soLuongTon !== undefined ? variant.soLuongTon : (variant.SoLuongTon !== undefined ? variant.SoLuongTon : 1);
                    const vOutOfStock = vStock <= 0 || variant.trangThai === 'HetHang';
                    const activeClass = isActive ? 'is-active' : '';
                    const stockClass = vOutOfStock ? 'is-out-of-stock' : '';
                    const label = variant.size ? variant.size : variant.maSanPham;

                    return `<button type="button" class="modal-size-pill ${activeClass} ${stockClass}" onclick="openQuickView('${variant.maSanPham}')" title="${vOutOfStock ? 'Tạm hết hàng' : 'Có sẵn ' + vStock + ' món'}">${label}</button>`;
                }).join('');
                groupWrap.style.display = 'block';
            } else {
                groupWrap.style.display = 'none';
            }
        } else {
            groupWrap.style.display = 'none';
        }
    }

    // Xử lý nút Thêm vào giỏ hàng theo tồn kho
    const addBtn = document.getElementById('modal-btn-add');
    if (addBtn) {
        if (isOutOfStock) {
            addBtn.disabled = true;
            addBtn.className = 'btn-luxury-primary is-disabled';
            addBtn.style.opacity = '0.5';
            addBtn.style.cursor = 'not-allowed';
            addBtn.textContent = 'Tạm Hết Hàng';
            addBtn.onclick = null;
        } else {
            addBtn.disabled = false;
            addBtn.className = 'btn-luxury-primary';
            addBtn.style.opacity = '1';
            addBtn.style.cursor = 'pointer';
            addBtn.textContent = 'Thêm Vào Giỏ Hàng';
            addBtn.onclick = () => {
                addToCart(product.maSanPham);
                closeQuickView();
            };
        }
    }

    modal.classList.add('active');
    document.body.style.overflow = 'hidden';
}

/**
 * Đóng hộp thoại xem nhanh
 */
function closeQuickView() {
    const modal = document.getElementById('quickview-modal');
    if (modal) {
        modal.classList.remove('active');
        document.body.style.overflow = '';
    }
}

/**
 * Tra cứu bảo hành kim hoàn
 */
async function lookupWarranty(ticketCode) {
    const input = document.getElementById('warranty-search-input');
    const code = (ticketCode || (input ? input.value : '')).trim().toUpperCase();

    if (!code) {
        showToast('Vui lòng nhập mã phiếu bảo hành (VD: BH2001, BH2002)');
        return;
    }

    if (input) input.value = code;

    const card = document.getElementById('warranty-result-card');
    let ticket = null;

    // 1. Tra cứu trực tiếp từ CSDL SQL Server qua RESTful API
    try {
        const res = await fetch(`/api/BaoHanh/${encodeURIComponent(code)}`);
        if (res.ok) {
            const json = await res.json();
            if (json.success && json.data) {
                ticket = json.data;
            }
        }
    } catch (e) {
        console.warn(`[BlinkBlink.Web] Lỗi kết nối /api/BaoHanh/${code}:`, e);
    }

    // 2. Dự phòng kho mẫu nếu mất kết nối hoặc dữ liệu cục bộ
    if (!ticket && sampleWarrantyTickets[code]) {
        ticket = sampleWarrantyTickets[code];
    }

    if (!ticket) {
        showToast(`Không tìm thấy thông tin cho mã phiếu: ${code}`);
        if (card) card.style.display = 'none';
        return;
    }

    // Nạp dữ liệu vào thẻ bảo hành
    const elTicketNo = document.getElementById('wb-ticket-no');
    const elCustomer = document.getElementById('wb-customer');
    const elProduct = document.getElementById('wb-product');
    const elDateIn = document.getElementById('wb-date-in');
    const elDateOut = document.getElementById('wb-date-out');
    const elCost = document.getElementById('wb-cost');
    const elNote = document.getElementById('wb-note');

    if (elTicketNo) elTicketNo.textContent = ticket.soPhieu || ticket.soPhieuBH || code;
    if (elCustomer) elCustomer.textContent = ticket.tenKhachHang || 'Khách hàng Blink Blink';
    if (elProduct) elProduct.textContent = ticket.tenSanPham || 'Kiệt tác trang sức';
    if (elDateIn) elDateIn.textContent = ticket.ngayTiepNhan || '-';
    if (elDateOut) elDateOut.textContent = ticket.ngayHenTra || 'Đang cập nhật';

    const costVal = ticket.chiPhi !== undefined ? ticket.chiPhi : (ticket.chiPhiBaoHanh || 0);
    if (elCost) elCost.textContent = costVal === 0 ? 'Miễn phí bảo dưỡng' : formatCurrency(costVal);

    const noteParts = [
        ticket.lyDo || ticket.lyDoBaoHanh,
        ticket.tinhTrangTiepNhan ? `Tình trạng: ${ticket.tinhTrangTiepNhan}` : '',
        ticket.ghiChu
    ].filter(Boolean);
    if (elNote) elNote.textContent = noteParts.join(' — ') || 'Đang được nghệ nhân Blink Blink chăm sóc và gia công.';

    const badgeEl = document.getElementById('wb-status-badge');
    if (badgeEl) {
        const rawStatus = (ticket.trangThaiXuLy || ticket.trangThai || '').toUpperCase();
        if (rawStatus === 'DAHOANTAT' || rawStatus === 'DATRAKHACH') {
            badgeEl.className = 'badge-status-completed';
            badgeEl.textContent = 'Đã Hoàn Tất Dịch Vụ';
        } else if (rawStatus === 'DAXULY') {
            badgeEl.className = 'badge-status-completed';
            badgeEl.textContent = 'Đã Xử Lý Xong • Chờ Giao Khách';
        } else if (rawStatus === 'DAHUY') {
            badgeEl.className = 'badge-status-cancelled';
            badgeEl.textContent = 'Phiếu Đã Hủy';
        } else if (rawStatus === 'TIEPNHAN') {
            badgeEl.className = 'badge-status-processing';
            badgeEl.textContent = 'Đã Tiếp Nhận Yêu Cầu';
        } else {
            badgeEl.className = 'badge-status-processing';
            badgeEl.textContent = 'Đang Gia Công Bảo Dưỡng';
        }
    }

    if (card) {
        card.style.display = 'block';
        card.scrollIntoView({ behavior: 'smooth', block: 'nearest' });
    }

    showToast(`Đã tìm thấy thông tin bảo hành ${code}`);
}

/**
 * Khởi tạo các sự kiện lắng nghe giao diện chung
 */
function initEventListeners() {
    // Nút mở/đóng giỏ hàng
    const openCartBtn = document.getElementById('btn-open-cart');
    const closeCartBtn = document.getElementById('btn-close-cart');
    const overlay = document.getElementById('cart-drawer-overlay');

    if (openCartBtn) {
        openCartBtn.addEventListener('click', () => toggleCartDrawer(true));
    }
    if (closeCartBtn) {
        closeCartBtn.addEventListener('click', () => toggleCartDrawer(false));
    }
    if (overlay) {
        overlay.addEventListener('click', (e) => {
            if (e.target === overlay) toggleCartDrawer(false);
        });
    }

    // Modal xem nhanh
    const closeModalBtn = document.getElementById('btn-close-modal');
    const modalBackdrop = document.getElementById('quickview-modal');
    if (closeModalBtn) {
        closeModalBtn.addEventListener('click', closeQuickView);
    }
    if (modalBackdrop) {
        modalBackdrop.addEventListener('click', (e) => {
            if (e.target === modalBackdrop) closeQuickView();
        });
    }

    // Nút tra cứu bảo hành
    const btnSearchWarranty = document.getElementById('btn-search-warranty');
    if (btnSearchWarranty) {
        btnSearchWarranty.addEventListener('click', () => lookupWarranty());
    }

    // Nút thanh toán đơn hàng (Mở form thông tin người nhận)
    const btnCheckout = document.getElementById('btn-proceed-checkout');
    if (btnCheckout) {
        btnCheckout.addEventListener('click', () => {
            if (cart.length === 0) {
                showToast('Giỏ hàng chưa có sản phẩm nào để đặt hàng.');
                return;
            }
            toggleCartDrawer(false);
            openCheckoutDialog();
        });
    }
}

/**
 * Khởi tạo tương tác thanh tìm kiếm co giãn trên Header (Expandable Search Bar)
 */
function initHeaderSearch() {
    const searchContainer = document.getElementById('header-search-container');
    const searchInput = document.getElementById('header-search-input');
    const searchTrigger = document.getElementById('btn-header-search-trigger');
    const searchClear = document.getElementById('btn-header-search-clear');
    const searchWrap = document.getElementById('header-search-wrap') || searchContainer;

    if (!searchContainer || !searchInput || !searchTrigger) return;

    let hoverTimeout = null;

    // Khi chuột rê vào vùng tìm kiếm: Mở rộng ngay lập tức và hủy lệnh đóng dở dang
    searchWrap.addEventListener('mouseenter', () => {
        if (hoverTimeout) {
            clearTimeout(hoverTimeout);
            hoverTimeout = null;
        }
        searchContainer.classList.add('is-hovered');
        searchContainer.classList.add('expanded');
    });

    // Khi chuột rời khỏi vùng tìm kiếm: Trì hoãn 280ms trước khi thu gọn, triệt tiêu 100% hiện tượng rung lắc/giật ở mép viền
    searchWrap.addEventListener('mouseleave', () => {
        searchContainer.classList.remove('is-hovered');
        // Nếu ô input đang được focus hoặc đang có nội dung tìm kiếm thì giữ nguyên không đóng
        if (document.activeElement === searchInput || searchInput.value.trim().length > 0) {
            return;
        }
        hoverTimeout = setTimeout(() => {
            searchContainer.classList.remove('expanded');
        }, 280);
    });

    // Khi focus vào input: Duy trì mở cố định
    searchInput.addEventListener('focus', () => {
        if (hoverTimeout) {
            clearTimeout(hoverTimeout);
            hoverTimeout = null;
        }
        searchContainer.classList.add('expanded');
    });

    // Khi blur khỏi input: Nếu rỗng và chuột không hover thì thu gọn
    searchInput.addEventListener('blur', () => {
        if (!searchInput.value.trim() && !searchContainer.matches(':hover') && !searchWrap.matches(':hover')) {
            hoverTimeout = setTimeout(() => {
                searchContainer.classList.remove('expanded');
            }, 200);
        }
    });

    // Hàm thực hiện điều hướng sang trang Tìm Kiếm
    const performSearch = () => {
        const query = searchInput.value.trim();
        if (query) {
            window.location.href = `/TimKiem?tuKhoa=${encodeURIComponent(query)}`;
        } else {
            window.location.href = '/TimKiem';
        }
    };

    // Khi click vào icon kính lúp
    searchTrigger.addEventListener('click', (e) => {
        e.stopPropagation();
        if (!searchContainer.classList.contains('expanded')) {
            searchContainer.classList.add('expanded');
            searchInput.focus();
        } else {
            // Đã mở rộng: Nếu có từ khóa thì tìm kiếm, nếu không thì focus
            if (searchInput.value.trim()) {
                performSearch();
            } else {
                searchInput.focus();
            }
        }
    });

    // Khi người dùng bấm phím Enter trong ô tìm kiếm Header
    searchInput.addEventListener('keydown', (e) => {
        if (e.key === 'Enter') {
            e.preventDefault();
            performSearch();
        } else if (e.key === 'Escape') {
            searchInput.value = '';
            searchContainer.classList.remove('expanded');
            if (searchClear) searchClear.style.display = 'none';
        }
    });

    // Lắng nghe gõ phím để hiển thị nút xóa nhanh
    searchInput.addEventListener('input', () => {
        if (searchClear) {
            searchClear.style.display = searchInput.value.length > 0 ? 'inline-block' : 'none';
        }
    });

    // Nút xóa nhanh từ khóa trên Header
    if (searchClear) {
        searchClear.addEventListener('click', (e) => {
            e.stopPropagation();
            searchInput.value = '';
            searchClear.style.display = 'none';
            searchInput.focus();
        });
    }

    // Khi click ra ngoài thanh tìm kiếm: Tự động thu gọn nếu ô nhập rỗng
    document.addEventListener('click', (e) => {
        if (!searchWrap.contains(e.target)) {
            if (!searchInput.value.trim()) {
                searchContainer.classList.remove('expanded');
            }
        }
    });
}

/**
 * Khởi tạo điều hướng menu và cập nhật gạch chân active (.nav-link-item a::after)
 */
function initNavigationAndScrollSpy() {
    const navLinks = document.querySelectorAll('.nav-link-item a');
    const isSearchPage = window.location.pathname.toLowerCase().includes('/timkiem') || 
                         window.location.pathname.toLowerCase().includes('/tim-kiem');

    // 1. Nếu đang ở trang Tìm Kiếm riêng biệt
    if (isSearchPage) {
        navLinks.forEach(link => {
            link.classList.remove('active');
            const href = (link.getAttribute('href') || '').toLowerCase();
            if (href.includes('timkiem') || href.includes('tim-kiem')) {
                link.classList.add('active');
            }
        });
        return; // Không chạy scroll spy của trang chủ trên trang tìm kiếm
    }

    // 2. Nếu đang ở Trang Chủ: Đặt mặc định Trang Chủ active nếu chưa có hash
    if (!window.location.hash) {
        const homeLink = document.querySelector('.nav-link-item a[href="/#top"], .nav-link-item a[href="#top"]');
        if (homeLink) {
            navLinks.forEach(l => l.classList.remove('active'));
            homeLink.classList.add('active');
        }
    }

    // 3. Xử lý click chuyển tab mượt mà
    navLinks.forEach(link => {
        link.addEventListener('click', (e) => {
            const href = link.getAttribute('href') || '';
            if (href.includes('#')) {
                const hash = href.split('#')[1];
                const targetElement = document.getElementById(hash);
                if (targetElement) {
                    e.preventDefault();
                    navLinks.forEach(l => l.classList.remove('active'));
                    link.classList.add('active');
                    targetElement.scrollIntoView({ behavior: 'smooth' });
                    // Cập nhật URL hash mà không gây giật trang
                    history.pushState(null, '', `#${hash}`);
                }
            } else if (!href.startsWith('http')) {
                navLinks.forEach(l => l.classList.remove('active'));
                link.classList.add('active');
            }
        });
    });

    // 4. ScrollSpy: Tự động chuyển gạch chân active khi người dùng cuộn trang
    const sectionIds = ['top', 'collections', 'catalog', 'warranty'];
    const sections = sectionIds
        .map(id => document.getElementById(id))
        .filter(el => el !== null);

    if (sections.length > 0 && 'IntersectionObserver' in window) {
        const observerOptions = {
            root: null,
            rootMargin: '-20% 0px -55% 0px',
            threshold: 0.1
        };

        const observer = new IntersectionObserver((entries) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    const currentId = entry.target.id;
                    navLinks.forEach(link => {
                        const href = link.getAttribute('href') || '';
                        if (href.endsWith(`#${currentId}`)) {
                            navLinks.forEach(l => l.classList.remove('active'));
                            link.classList.add('active');
                        }
                    });
                }
            });
        }, observerOptions);

        sections.forEach(sec => observer.observe(sec));
    }
}

/**
 * Mở hộp thoại thông tin đặt hàng online thương mại cao cấp
 */
function openCheckoutDialog() {
    if (!cart || cart.length === 0) {
        showToast('Giỏ hàng chưa có sản phẩm nào để đặt hàng.', 'warning');
        return;
    }

    // Nếu khách hàng chưa đăng nhập: Yêu cầu đăng nhập lịch sự để bảo đảm cấp Chứng thư Kim hoàn & Bảo hành
    if (!currentUser) {
        showToast('Để cấp Chứng thư Kim hoàn & Bảo hành trọn đời, Quý khách vui lòng đăng nhập hoặc đăng ký thành viên.', 'warning');
        window.shouldResumeCheckout = true;
        openAuthModal('login');
        return;
    }

    // Điền sẵn thông tin khách hàng nếu đã có
    const nameInput = document.getElementById('checkout-name');
    const phoneInput = document.getElementById('checkout-phone');
    const addressInput = document.getElementById('checkout-address');

    if (nameInput) nameInput.value = currentUser.hoTen || '';
    if (phoneInput) phoneInput.value = currentUser.soDienThoai || '';
    if (addressInput && !addressInput.value) {
        addressInput.value = '45 Lê Lợi, Phường Bến Nghé, Quận 1, TP. Hồ Chí Minh';
    }

    // Nạp danh sách kiệt tác tóm tắt trong modal đặt hàng
    renderCheckoutSummary();

    // Ẩn banner lỗi nếu còn
    const errorBanner = document.getElementById('checkout-error-banner');
    if (errorBanner) errorBanner.style.display = 'none';

    // Mở modal đặt hàng
    const checkoutModal = document.getElementById('checkout-modal');
    if (checkoutModal) {
        checkoutModal.classList.add('active');
    }
}

/**
 * Đóng modal đặt hàng
 */
function closeCheckoutModal() {
    const checkoutModal = document.getElementById('checkout-modal');
    if (checkoutModal) {
        checkoutModal.classList.remove('active');
    }
}

/**
 * Hiển thị tóm tắt danh sách sản phẩm và tổng tiền trong modal đặt hàng
 */
function renderCheckoutSummary() {
    const container = document.getElementById('checkout-items-scroll');
    const countBadge = document.getElementById('checkout-items-count');
    const subtotalEl = document.getElementById('checkout-subtotal');
    const finalTotalEl = document.getElementById('checkout-final-total');

    if (!container) return;

    const totalCount = cart.reduce((sum, item) => sum + item.soLuong, 0);
    const subtotal = cart.reduce((sum, item) => sum + item.giaBan * item.soLuong, 0);

    if (countBadge) countBadge.textContent = totalCount;
    if (subtotalEl) subtotalEl.textContent = formatCurrency(subtotal);
    if (finalTotalEl) finalTotalEl.textContent = formatCurrency(subtotal);

    container.innerHTML = cart.map(item => {
        const itemImg = (item.hinhAnh && item.hinhAnh.trim()) ? item.hinhAnh.trim() : '/images/hero-jewelry.jpg';
        const hasDiscount = item.phanTramGiamGia && item.phanTramGiamGia > 0;
        const origPrice = item.giaGoc || item.giaBan;
        const priceDisplay = hasDiscount
            ? `<div class="checkout-mini-price">
                   <span style="text-decoration: line-through; font-size: 0.75rem; color: var(--text-muted); margin-right: 4px;">${formatCurrency(origPrice * item.soLuong)}</span>
                   <span>${formatCurrency(item.giaBan * item.soLuong)}</span>
                   <span class="product-status-tag sale" style="font-size: 0.65rem; padding: 1px 4px; margin-left: 4px; position: static;">-${item.phanTramGiamGia}%</span>
               </div>`
            : `<div class="checkout-mini-price">${formatCurrency(item.giaBan * item.soLuong)}</div>`;

        return `
        <div class="checkout-mini-item">
            <img src="${itemImg}" 
                 alt="${item.tenSanPham}" 
                 class="checkout-mini-img"
                 loading="lazy"
                 onerror="this.onerror=null; this.src='/images/hero-jewelry.jpg';" />
            <div class="checkout-mini-info">
                <div class="checkout-mini-title">${item.tenSanPham}</div>
                <div class="checkout-mini-meta">Số lượng: ${item.soLuong} • Size: ${item.size || item.kichCo || 'Tiêu chuẩn'}</div>
            </div>
            ${priceDisplay}
        </div>
    `;
    }).join('');
}

/**
 * Xử lý biểu mẫu Đặt Hàng và gửi yêu cầu tạo hóa đơn lên RESTful API
 * @param {Event} event - Sự kiện submit form
 */
async function handleCheckoutSubmit(event) {
    if (event) event.preventDefault();

    const name = document.getElementById('checkout-name')?.value.trim();
    const phone = document.getElementById('checkout-phone')?.value.trim();
    const address = document.getElementById('checkout-address')?.value.trim();
    const paymentRadio = document.querySelector('input[name="checkout-payment-method"]:checked');
    const paymentMethod = paymentRadio ? paymentRadio.value : 'Chuyển khoản ngân hàng';
    const notes = document.getElementById('checkout-notes')?.value.trim() || '';

    const errorBanner = document.getElementById('checkout-error-banner');
    const errorText = document.getElementById('checkout-error-text');

    if (!name || !phone || !address) {
        if (errorBanner && errorText) {
            errorText.textContent = 'Vui lòng điền đầy đủ Họ tên, Số điện thoại và Địa chỉ nhận hàng.';
            errorBanner.style.display = 'flex';
        }
        showToast('Vui lòng điền đầy đủ thông tin nhận hàng.', 'warning');
        return;
    }

    const submitBtn = document.getElementById('btn-submit-order');
    const submitBtnText = document.getElementById('btn-submit-order-text');
    if (submitBtn) submitBtn.disabled = true;
    if (submitBtnText) submitBtnText.textContent = 'Đang tiếp nhận và niêm phong đơn hàng...';

    try {
        const customerId = currentUser.maKhachHang || currentUser.maNguoiDung;
        const total = cart.reduce((sum, item) => sum + item.giaBan * item.soLuong, 0);

        const { ok, status, data: result, traceId } = await apiRequest('/api/BanHang/dat-hang', {
            method: 'POST',
            body: JSON.stringify({
                MaKhachHang: customerId,
                DiaChiGiaoHang: address,
                SoDienThoaiNhan: phone,
                PhuongThucThanhToan: paymentMethod,
                GhiChu: notes
            })
        });

        if (status === 403) {
            closeCheckoutModal();
            return;
        }

        if (ok && result.success) {
            closeCheckoutModal();

            // Dọn sạch giỏ hàng trên client và server
            cart = [];
            updateCartUI();

            // Trích xuất mã đơn hàng thật từ database qua result.Data
            const donMoi = result.data || result.Data || {};
            const maDonHang = donMoi.maHoaDon || donMoi.MaHoaDon || ('HD' + Date.now().toString().slice(-6));
            const tongThanhToan = donMoi.tongThanhToan || donMoi.TongThanhToan || total;

            const orderCertificate = {
                maHoaDon: maDonHang,
                ngayLap: new Date().toLocaleString('vi-VN'),
                nguoiNhan: `${name} (${phone})`,
                diaChi: address,
                phuongThuc: paymentMethod,
                tongTien: tongThanhToan
            };

            openSuccessModal(orderCertificate);
            showToast('Kiệt tác kim hoàn của Quý khách đã được tiếp nhận thành công!', 'success');
        } else {
            const baseMsg = result.message || 'Không thể tạo đơn hàng. Quý khách vui lòng thử lại sau.';
            const msg = status >= 500 && traceId ? `${baseMsg} (Mã tra cứu: ${traceId})` : baseMsg;
            if (errorBanner && errorText) {
                errorText.textContent = msg;
                errorBanner.style.display = 'flex';
            }
            showToast(msg, 'error');
        }
    } catch (err) {
        console.error('Lỗi khi gửi yêu cầu đặt hàng:', err);
        const errMsg = 'Lỗi kết nối tới máy chủ xưởng kim hoàn. Vui lòng thử lại sau.';
        if (errorBanner && errorText) {
            errorText.textContent = errMsg;
            errorBanner.style.display = 'flex';
        }
        showToast(errMsg, 'error');
    } finally {
        if (submitBtn) submitBtn.disabled = false;
        if (submitBtnText) submitBtnText.textContent = 'Hoàn Tất Đặt Hàng Ngay';
    }
}

/**
 * Mở modal chúc mừng và hiển thị Chứng Thư Đơn Hàng Hoàng Gia
 * @param {Object} order - Thông tin hóa đơn đã lưu
 */
function openSuccessModal(order) {
    const codeEl = document.getElementById('success-order-code');
    const timeEl = document.getElementById('success-order-time');
    const nameEl = document.getElementById('success-recipient-name');
    const addrEl = document.getElementById('success-recipient-addr');
    const methodEl = document.getElementById('success-payment-method');
    const totalEl = document.getElementById('success-order-total');

    if (codeEl) codeEl.textContent = order.maHoaDon || 'HD-NEW';
    if (timeEl) timeEl.textContent = order.ngayLap || new Date().toLocaleString('vi-VN');
    if (nameEl) nameEl.textContent = order.nguoiNhan || '-';
    if (addrEl) addrEl.textContent = order.diaChi || '-';
    if (methodEl) methodEl.textContent = order.phuongThuc || 'Chuyển khoản ngân hàng';
    if (totalEl) totalEl.textContent = formatCurrency(order.tongTien || 0);

    const successModal = document.getElementById('order-success-modal');
    if (successModal) {
        successModal.classList.add('active');
    }
}

/**
 * Đóng modal chúc mừng đơn hàng
 */
function closeSuccessModal() {
    const successModal = document.getElementById('order-success-modal');
    if (successModal) {
        successModal.classList.remove('active');
    }
}

/**
 * Đóng modal chúc mừng và mở ngăn kéo lịch sử đơn hàng
 */
function viewOrderInDrawer() {
    closeSuccessModal();
    openOrdersDrawer();
}

/**
 * Hiển thị thông báo Toast thương mại cao cấp đa trạng thái
 * @param {string} message - Nội dung thông báo
 * @param {'info'|'success'|'warning'|'error'} [type='info'] - Phân loại trạng thái
 */
function showToast(message, type = 'info') {
    let container = document.getElementById('luxury-toast-container');
    if (!container) {
        container = document.createElement('div');
        container.id = 'luxury-toast-container';
        container.className = 'luxury-toast-container';
        document.body.appendChild(container);
    }

    const toast = document.createElement('div');
    toast.className = `luxury-toast-msg toast-${type}`;

    let iconSvg = '';
    if (type === 'success') {
        iconSvg = `<svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="#10b981" stroke-width="2.2"><path d="M22 11.08V12a10 10 0 1 1-5.93-9.14"/><polyline points="22 4 12 14.01 9 11.01"/></svg>`;
    } else if (type === 'error') {
        iconSvg = `<svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="#ef4444" stroke-width="2.2"><circle cx="12" cy="12" r="10"/><line x1="12" y1="8" x2="12" y2="12"/><line x1="12" y1="16" x2="12.01" y2="16"/></svg>`;
    } else if (type === 'warning') {
        iconSvg = `<svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="#f59e0b" stroke-width="2.2"><path d="M10.29 3.86L1.82 18a2 2 0 0 0 1.71 3h16.94a2 2 0 0 0 1.71-3L13.71 3.86a2 2 0 0 0-3.42 0z"/><line x1="12" y1="9" x2="12" y2="13"/><line x1="12" y1="17" x2="12.01" y2="17"/></svg>`;
    } else {
        iconSvg = `<svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="var(--gold-primary)" stroke-width="2.2"><polygon points="6 3 18 3 22 9 12 22 2 9"/></svg>`;
    }

    toast.innerHTML = `
        ${iconSvg}
        <span>${message}</span>
        <div class="toast-progress-bar"></div>
    `;

    container.appendChild(toast);

    setTimeout(() => {
        toast.style.opacity = '0';
        toast.style.transform = 'translateY(10px)';
        toast.style.transition = 'all 0.3s ease';
        setTimeout(() => toast.remove(), 300);
    }, 3800);
}

/**
 * Hiển thị banner phản hồi lỗi/thành công bên trong form xác thực (Login/Register)
 * @param {'login'|'register'} paneId - Tab cần hiển thị
 * @param {string} message - Nội dung thông báo
 * @param {'error'|'success'} [type='error'] - Phân loại
 */
function showAuthFeedback(paneId, message, type = 'error') {
    const banner = document.getElementById(`${paneId}-feedback-banner`);
    const textEl = document.getElementById(`${paneId}-feedback-text`);
    if (banner && textEl) {
        textEl.textContent = message;
        banner.className = `auth-feedback-banner ${type}`;
        banner.style.display = 'flex';
    }
}

/**
 * Xóa banner phản hồi của tab xác thực
 * @param {'login'|'register'} paneId - Tab cần ẩn
 */
function clearAuthFeedback(paneId) {
    const banner = document.getElementById(`${paneId}-feedback-banner`);
    if (banner) {
        banner.style.display = 'none';
    }
}

// ====================================================================================
// KHỐI XỬ LÝ XÁC THỰC KHÁCH HÀNG & TRA CỨU ĐƠN HÀNG (AUTHENTICATION & ORDER TRACKING)
// Tuân thủ Clean Architecture và Prototype-First: Ưu tiên trải nghiệm mượt mà,
// kết hợp gọi REST API với cơ chế dự phòng dữ liệu mẫu chuẩn SeedData.sql
// ====================================================================================

/**
 * Khởi tạo hệ thống xác thực người dùng từ bộ nhớ trình duyệt (localStorage)
 */
function initAuth() {
    // 1. Phục hồi trạng thái phiên đăng nhập nếu đã lưu trước đó
    const savedUserJson = localStorage.getItem('blink_currentUser');
    if (savedUserJson) {
        try {
            const savedUser = JSON.parse(savedUserJson);
            if (savedUser && (savedUser.tenDangNhap || savedUser.hoTen)) {
                applyLoggedInUser(savedUser);
                // Đã đăng nhập: Tuyệt đối không dùng localStorage cho giỏ hàng
                localStorage.removeItem(GUEST_CART_STORAGE_KEY);
            }
        } catch (e) {
            console.error('Không thể phục hồi phiên người dùng:', e);
            localStorage.removeItem('blink_currentUser');
        }
    } else {
        // Khách vãng lai: Đảm bảo wrapper và menu ở trạng thái ẩn hoàn toàn
        const wrapper = document.querySelector('.user-auth-action-wrapper');
        if (wrapper) wrapper.classList.remove('logged-in');
        closeUserDropdown();
    }

    // 2. Sự kiện nhấn nút Tài Khoản trên Header
    const authBtn = document.getElementById('btn-auth-user');
    const dropdown = document.getElementById('user-dropdown-menu');

    if (authBtn) {
        authBtn.addEventListener('click', (e) => {
            e.stopPropagation();
            if (currentUser) {
                // Đã đăng nhập: Bật/tắt menu dropdown cá nhân
                if (dropdown) dropdown.classList.toggle('open');
            } else {
                // Chưa đăng nhập: Đảm bảo đóng dropdown và mở Modal đăng nhập
                closeUserDropdown();
                openAuthModal('login');
            }
        });
    }

    // 3. Đóng dropdown khi nhấn ra ngoài khu vực menu
    document.addEventListener('click', (e) => {
        if (dropdown && dropdown.classList.contains('open')) {
            if (!dropdown.contains(e.target) && e.target !== authBtn && !authBtn.contains(e.target)) {
                closeUserDropdown();
            }
        }
    });

    // 4. Sự kiện đóng Modal Đăng nhập / Đăng ký
    const closeAuthBtn = document.getElementById('btn-close-auth');
    const authBackdrop = document.getElementById('auth-modal');

    if (closeAuthBtn) {
        closeAuthBtn.addEventListener('click', closeAuthModal);
    }
    if (authBackdrop) {
        authBackdrop.addEventListener('click', (e) => {
            if (e.target === authBackdrop) closeAuthModal();
        });
    }

    // 5. Sự kiện đóng Modal Đặt hàng, Modal Thành công & Modal Hồ sơ khi click ngoài nền
    const checkoutModal = document.getElementById('checkout-modal');
    if (checkoutModal) {
        checkoutModal.addEventListener('click', (e) => {
            if (e.target === checkoutModal) closeCheckoutModal();
        });
    }

    const successModal = document.getElementById('order-success-modal');
    if (successModal) {
        successModal.addEventListener('click', (e) => {
            if (e.target === successModal) closeSuccessModal();
        });
    }

    const profileModal = document.getElementById('profile-modal');
    if (profileModal) {
        profileModal.addEventListener('click', (e) => {
            if (e.target === profileModal) closeProfileModal();
        });
    }

    // 6. Tự động xóa phản hồi lỗi khi người dùng chỉnh sửa ô nhập liệu
    const loginInputs = ['login-username', 'login-password'];
    loginInputs.forEach(id => {
        const el = document.getElementById(id);
        if (el) el.addEventListener('input', () => clearAuthFeedback('login'));
    });

    const regInputs = ['reg-fullname', 'reg-phone', 'reg-dob', 'reg-username', 'reg-password'];
    regInputs.forEach(id => {
        const el = document.getElementById(id);
        if (el) el.addEventListener('input', () => clearAuthFeedback('register'));
    });

    const checkoutInputs = ['checkout-name', 'checkout-phone', 'checkout-address'];
    checkoutInputs.forEach(id => {
        const el = document.getElementById(id);
        if (el) {
            el.addEventListener('input', () => {
                const banner = document.getElementById('checkout-error-banner');
                if (banner) banner.style.display = 'none';
            });
        }
    });

    const profileInputs = ['profile-fullname', 'profile-phone', 'profile-dob'];
    profileInputs.forEach(id => {
        const el = document.getElementById(id);
        if (el) {
            el.addEventListener('input', () => {
                const banner = document.getElementById('profile-feedback-banner');
                if (banner) banner.style.display = 'none';
            });
        }
    });

    // 7. Sự kiện đóng Ngăn kéo Đơn hàng
    const closeOrdersBtn = document.getElementById('btn-close-orders');
    const ordersOverlay = document.getElementById('orders-drawer-overlay');

    if (closeOrdersBtn) {
        closeOrdersBtn.addEventListener('click', closeOrdersDrawer);
    }
    if (ordersOverlay) {
        ordersOverlay.addEventListener('click', (e) => {
            if (e.target === ordersOverlay) closeOrdersDrawer();
        });
    }
}

/**
 * Mở hộp thoại Đăng nhập / Đăng ký
 * @param {'login'|'register'} defaultTab - Tab hiển thị mặc định
 */
function openAuthModal(defaultTab = 'login') {
    const modal = document.getElementById('auth-modal');
    if (!modal) return;

    clearAuthFeedback('login');
    clearAuthFeedback('register');
    switchAuthTab(defaultTab);
    modal.classList.add('active');
    document.body.style.overflow = 'hidden';
}

/**
 * Đóng hộp thoại Đăng nhập / Đăng ký
 */
function closeAuthModal() {
    const modal = document.getElementById('auth-modal');
    if (modal) {
        modal.classList.remove('active');
        document.body.style.overflow = '';
    }
}

/**
 * Chuyển đổi qua lại giữa Tab Đăng nhập và Tab Đăng ký
 * @param {'login'|'register'} tab - Tên tab cần kích hoạt
 */
function switchAuthTab(tab) {
    const tabLoginBtn = document.getElementById('tab-btn-login');
    const tabRegBtn = document.getElementById('tab-btn-register');
    const paneLogin = document.getElementById('pane-login');
    const paneReg = document.getElementById('pane-register');

    clearAuthFeedback('login');
    clearAuthFeedback('register');

    if (tab === 'login') {
        if (tabLoginBtn) tabLoginBtn.classList.add('active');
        if (tabRegBtn) tabRegBtn.classList.remove('active');
        if (paneLogin) paneLogin.classList.add('active');
        if (paneReg) paneReg.classList.remove('active');
    } else {
        if (tabLoginBtn) tabLoginBtn.classList.remove('active');
        if (tabRegBtn) tabRegBtn.classList.add('active');
        if (paneLogin) paneLogin.classList.remove('active');
        if (paneReg) paneReg.classList.add('active');
    }
}

/**
 * Xử lý biểu mẫu Đăng nhập tài khoản khách hàng
 * Gửi yêu cầu xác thực trực tiếp tới RESTful API POST /api/XacThuc/dang-nhap
 * @param {Event} [event] - Sự kiện submit form
 */
async function handleLoginSubmit(event) {
    if (event) event.preventDefault();

    const usernameInput = document.getElementById('login-username');
    const passwordInput = document.getElementById('login-password');

    const username = usernameInput ? usernameInput.value.trim() : '';
    const password = passwordInput ? passwordInput.value : '';

    if (!username || !password) {
        showAuthFeedback('login', 'Vui lòng nhập đầy đủ tên đăng nhập và mật khẩu.');
        showToast('Vui lòng nhập đầy đủ thông tin đăng nhập.', 'warning');
        return;
    }

    try {
        const response = await fetch('/api/XacThuc/dang-nhap', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Accept': 'application/json'
            },
            body: JSON.stringify({
                TenDangNhap: username,
                MatKhau: password
            })
        });

        const result = await response.json();

        if (response.ok && result.success) {
            clearAuthFeedback('login');
            const data = result.data || result;
            const userObj = {
                maNguoiDung: data.maNguoiDung || data.MaNguoiDung || ('USER_' + username),
                maKhachHang: data.maKhachHang || data.MaKhachHang || 'KH001',
                hoTen: data.hoTen || data.HoTen || username,
                tenDangNhap: username,
                soDienThoai: data.soDienThoai || data.SoDienThoai || '',
                maVaiTro: data.maVaiTro || data.MaVaiTro || 'KHACHHANG'
            };

            applyLoggedInUser(userObj);
            localStorage.setItem('blink_currentUser', JSON.stringify(userObj));
            await syncGuestCartAndReload(userObj);
            closeAuthModal();
            showToast(`Chào mừng quý khách ${userObj.hoTen} trở lại với Blink Blink!`, 'success');

            // Nếu người dùng trước đó đang ấn đặt hàng, tự động tiếp tục mở modal đặt hàng
            if (window.shouldResumeCheckout) {
                window.shouldResumeCheckout = false;
                setTimeout(() => openCheckoutDialog(), 350);
            }
        } else {
            const errorMsg = result.message || 'Tên đăng nhập hoặc mật khẩu không chính xác.';
            showAuthFeedback('login', errorMsg);
            showToast(errorMsg, 'error');
        }
    } catch (err) {
        console.error('Lỗi khi gọi API đăng nhập:', err);
        const netError = 'Không thể kết nối tới máy chủ xác thực. Vui lòng kiểm tra lại đường truyền.';
        showAuthFeedback('login', netError);
        showToast(netError, 'error');
    }
}

/**
 * Xử lý biểu mẫu Đăng ký tài khoản khách hàng mới
 * Gửi yêu cầu lưu trữ trực tiếp vào CSDL qua API POST /api/KhachHang/dang-ky
 * @param {Event} [event] - Sự kiện submit form
 */
async function handleRegisterSubmit(event) {
    if (event) event.preventDefault();

    const hoTen = document.getElementById('reg-fullname')?.value.trim();
    const sdt = document.getElementById('reg-phone')?.value.trim();
    const ngaySinh = document.getElementById('reg-dob')?.value;
    const username = document.getElementById('reg-username')?.value.trim();
    const password = document.getElementById('reg-password')?.value;

    if (!hoTen || !sdt || !username || !password) {
        showAuthFeedback('register', 'Vui lòng điền đầy đủ các trường thông tin bắt buộc (*).');
        showToast('Vui lòng điền đầy đủ các trường thông tin.', 'warning');
        return;
    }

    try {
        const response = await fetch('/api/KhachHang/dang-ky', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Accept': 'application/json'
            },
            body: JSON.stringify({
                HoTen: hoTen,
                SoDienThoai: sdt,
                NgaySinh: ngaySinh || '1995-01-01',
                TenDangNhap: username,
                MatKhau: password
            })
        });

        const result = await response.json();

        if (response.ok && result.success) {
            clearAuthFeedback('register');
            showToast(`Đăng ký thành công! Đang thiết lập phiên hội viên...`, 'success');

            // Tự động gọi xác thực đăng nhập để thiết lập phiên làm việc
            try {
                const loginRes = await fetch('/api/XacThuc/dang-nhap', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json', 'Accept': 'application/json' },
                    body: JSON.stringify({ TenDangNhap: username, MatKhau: password })
                });

                if (loginRes.ok) {
                    const loginData = await loginRes.json();
                    const info = loginData.data || loginData;
                    const loggedUser = {
                        maNguoiDung: info.maNguoiDung || info.MaNguoiDung,
                        maKhachHang: info.maKhachHang || info.MaKhachHang,
                        hoTen: info.hoTen || info.HoTen || hoTen,
                        tenDangNhap: username,
                        soDienThoai: sdt,
                        maVaiTro: 'KHACHHANG'
                    };
                    applyLoggedInUser(loggedUser);
                    localStorage.setItem('blink_currentUser', JSON.stringify(loggedUser));
                    await syncGuestCartAndReload(loggedUser);
                    closeAuthModal();
                    showToast(`Chào mừng ${hoTen} gia nhập hội viên Blink Blink Jewelry!`, 'success');

                    // Nếu trước đó đang ấn đặt hàng, tự động tiếp tục mở modal đặt hàng
                    if (window.shouldResumeCheckout) {
                        window.shouldResumeCheckout = false;
                        setTimeout(() => openCheckoutDialog(), 350);
                    }
                    return;
                }
            } catch {
                // Nếu login tự động gặp trở ngại, chuyển sang tab đăng nhập cho khách tự đăng nhập
            }

            switchAuthTab('login');
            const loginUserInput = document.getElementById('login-username');
            if (loginUserInput) loginUserInput.value = username;
            showToast('Đăng ký tài khoản thành công! Quý khách vui lòng đăng nhập.', 'success');
        } else {
            const regError = result.message || 'Không thể đăng ký tài khoản. Vui lòng kiểm tra lại thông tin.';
            showAuthFeedback('register', regError);
            showToast(regError, 'error');
        }
    } catch (err) {
        console.error('Lỗi khi gọi API đăng ký:', err);
        const netError = 'Không thể kết nối tới máy chủ đăng ký. Vui lòng thử lại sau.';
        showAuthFeedback('register', netError);
        showToast(netError, 'error');
    }
}

/**
 * Cập nhật giao diện Header và menu người dùng sau khi đã xác thực
 * @param {Object} user - Thông tin thực thể người dùng
 */
function applyLoggedInUser(user) {
    currentUser = user;

    const authBtn = document.getElementById('btn-auth-user');
    const wrapper = document.querySelector('.user-auth-action-wrapper');
    const nameLabel = document.getElementById('user-display-name');
    const menuFullName = document.getElementById('menu-user-fullname');
    const menuUserId = document.getElementById('menu-user-id');

    if (nameLabel) {
        nameLabel.textContent = user.hoTen || user.HoTen || user.tenDangNhap;
    }

    if (menuFullName) {
        menuFullName.textContent = user.hoTen || user.HoTen || 'Quý Khách';
    }

    if (menuUserId) {
        const idText = user.maKhachHang || user.MaKhachHang || user.maNguoiDung || 'VIP';
        menuUserId.textContent = `Hội Viên Danh Dự #${idText}`;
    }

    if (authBtn) {
        authBtn.classList.add('logged-in');
        authBtn.setAttribute('title', `Tài khoản: ${user.hoTen} - Nhấn để mở menu tùy chọn`);
    }

    if (wrapper) {
        wrapper.classList.add('logged-in');
    }
}

/**
 * Đóng menu dropdown cá nhân
 */
function closeUserDropdown() {
    const dropdown = document.getElementById('user-dropdown-menu');
    if (dropdown) {
        dropdown.classList.remove('open');
        dropdown.classList.remove('active');
    }
}

/**
 * Đăng xuất người dùng, đưa giao diện về trạng thái vãng lai ngay lập tức
 */
async function logoutUser() {
    currentUser = null;
    localStorage.removeItem('blink_currentUser');

    const authBtn = document.getElementById('btn-auth-user');
    const wrapper = document.querySelector('.user-auth-action-wrapper');
    const nameLabel = document.getElementById('user-display-name');
    const menuFullName = document.getElementById('menu-user-fullname');
    const menuUserId = document.getElementById('menu-user-id');

    // 1. Khôi phục nhãn trên nút Header về trạng thái chưa đăng nhập
    if (nameLabel) {
        nameLabel.textContent = 'Đăng Nhập / Đăng Ký';
    }

    // 2. Reset thông tin họ tên và mã hội viên trong dropdown menu về mặc định
    if (menuFullName) {
        menuFullName.textContent = 'Khách Hàng';
    }
    if (menuUserId) {
        menuUserId.textContent = 'Thành viên Blink Blink';
    }

    // 3. Gỡ bỏ toàn bộ class trạng thái logged-in để menu không bao giờ bung ra khi chưa đăng nhập
    if (authBtn) {
        authBtn.classList.remove('logged-in');
        authBtn.removeAttribute('title');
    }

    if (wrapper) {
        wrapper.classList.remove('logged-in');
    }

    // 4. Giỏ hàng của tài khoản đã được lưu an toàn trên CSDL; reset RAM và nạp lại giỏ hàng vãng lai nếu có
    cart = [];
    await loadCart();
    updateCartUI();

    // 5. Đóng ngay dropdown và ngăn kéo đơn hàng
    closeUserDropdown();
    closeOrdersDrawer();
    showToast('Quý khách đã đăng xuất tài khoản thành công.');
}

/**
 * Hợp nhất giỏ hàng tạm của khách vãng lai (nếu có) lên CSDL và nạp lại giỏ hàng chính thức của tài khoản
 * Đảm bảo: Sau khi đăng nhập, giỏ hàng không dùng localStorage nữa!
 * @param {Object} user - Đối tượng người dùng vừa đăng nhập
 */
async function syncGuestCartAndReload(user) {
    const customerId = user.maKhachHang || user.maNguoiDung;
    const guestCartRaw = localStorage.getItem(GUEST_CART_STORAGE_KEY);
    if (guestCartRaw) {
        try {
            const guestItems = JSON.parse(guestCartRaw);
            if (Array.isArray(guestItems) && guestItems.length > 0) {
                // Đẩy các sản phẩm khách vãng lai đã chọn lên CSDL
                await fetch('/api/GioHang/cap-nhat', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json', 'Accept': 'application/json' },
                    body: JSON.stringify({
                        maKhachHang: customerId,
                        danhSachSanPham: guestItems.map(i => ({ maSanPham: i.maSanPham, soLuong: i.soLuong }))
                    })
                });
            }
        } catch (e) {
            console.error('Không thể đồng bộ giỏ hàng vãng lai lên máy chủ:', e);
        }
        // Xóa sạch giỏ hàng vãng lai khỏi localStorage
        localStorage.removeItem(GUEST_CART_STORAGE_KEY);
    }

    // Nạp giỏ hàng chính thức của tài khoản từ CSDL SQL Server (HOÀN TOÀN KHÔNG DÙNG localStorage)
    await loadCart();
}

/**
 * Mở Ngăn kéo xem danh sách lịch sử đơn hàng của người dùng hiện tại
 * Nạp động 100% từ RESTful API /api/BanHang/khach-hang/{maKH} kết nối CSDL SQL Server thật
 */
async function openOrdersDrawer() {
    closeUserDropdown();

    if (!currentUser) {
        showToast('Vui lòng đăng nhập để tra cứu lịch sử mua hàng.', 'warning');
        openAuthModal('login');
        return;
    }

    const overlay = document.getElementById('orders-drawer-overlay');
    if (overlay) {
        overlay.classList.add('active');
        document.body.style.overflow = 'hidden';
    }

    await refreshCustomerOrders();
}

/**
 * Đóng Ngăn kéo danh sách đơn hàng
 */
function closeOrdersDrawer() {
    const overlay = document.getElementById('orders-drawer-overlay');
    if (overlay) {
        overlay.classList.remove('active');
        document.body.style.overflow = '';
    }
}

/**
 * Làm mới danh sách đơn hàng của khách hàng trực tiếp từ máy chủ CSDL
 */
async function refreshCustomerOrders() {
    if (!currentUser) return;

    const container = document.getElementById('orders-list-container');
    const btnRefresh = document.getElementById('btn-refresh-orders');

    if (btnRefresh) btnRefresh.classList.add('spinning');
    if (container) {
        container.innerHTML = `
            <div class="orders-loading-state">
                <div class="orders-loading-spinner"></div>
                <span>Đang kết nối xưởng kim hoàn nạp đơn hàng...</span>
            </div>
        `;
    }

    try {
        const custId = currentUser.maKhachHang || currentUser.MaKhachHang || currentUser.maNguoiDung;
        const res = await fetch(`/api/BanHang/khach-hang/${encodeURIComponent(custId)}`, {
            headers: { 'Accept': 'application/json' }
        });

        if (res.ok) {
            const json = await res.json();
            if (json.success && Array.isArray(json.data)) {
                renderCustomerOrders(json.data, currentUser);
                return;
            }
        }
        renderCustomerOrders([], currentUser);
    } catch (e) {
        console.error('Không thể nạp đơn hàng từ API:', e);
        if (container) {
            container.innerHTML = `
                <div style="text-align: center; padding: 40px 20px; color: #b91c1c;">
                    <p style="font-size: 0.88rem; margin-bottom: 12px;">Không thể kết nối máy chủ để nạp đơn hàng.</p>
                    <button type="button" class="btn-luxury-outline" onclick="refreshCustomerOrders()">Thử Lại</button>
                </div>
            `;
        }
    } finally {
        if (btnRefresh) {
            setTimeout(() => btnRefresh.classList.remove('spinning'), 400);
        }
    }
}

/**
 * Vẽ danh sách hóa đơn đơn hàng của khách hàng lên Drawer từ dữ liệu CSDL SQL Server thật
 * @param {Array} orders - Mảng các đối tượng HoaDon từ API
 * @param {Object} user - Đối tượng khách hàng đang đăng nhập
 */
function renderCustomerOrders(orders, user) {
    const container = document.getElementById('orders-list-container');
    if (!container) return;

    if (!orders || orders.length === 0) {
        container.innerHTML = `
            <div style="text-align: center; padding: 60px 24px; color: var(--text-muted);">
                <svg width="48" height="48" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5" style="margin-bottom: 12px; opacity: 0.6;">
                    <path d="M6 2L3 6v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2V6l-3-4z"/>
                    <line x1="3" y1="6" x2="21" y2="6"/>
                    <path d="M16 10a4 4 0 0 1-8 0"/>
                </svg>
                <h4 style="font-family: var(--font-heading); font-size: 1.15rem; color: var(--dark-obsidian); margin-bottom: 8px;">Chưa Có Giao Dịch Nào</h4>
                <p style="font-size: 0.85rem; line-height: 1.5;">Hội viên ${user.hoTen} chưa có đơn đặt hàng kim hoàn nào được ghi nhận trên hệ thống.</p>
                <button type="button" class="btn-luxury-outline" style="margin-top: 18px; font-size: 0.78rem;" onclick="closeOrdersDrawer()">
                    Khám Phá Tuyệt Tác Trang Sức
                </button>
            </div>
        `;
        return;
    }

    container.innerHTML = '';

    orders.forEach(order => {
        const card = document.createElement('div');
        card.className = 'order-card-item';

        // Xác định huy hiệu và tên hiển thị trạng thái giao dịch
        let badgeClass = 'status-badge-choduyet';
        let statusText = '⏳ Chờ Xưởng Duyệt';
        const st = (order.trangThaiDonHang || order.trangThai || '').toLowerCase();

        if (st === 'dangdonggoi') {
            badgeClass = 'status-badge-dangdonggoi';
            statusText = '📦 Đang Đóng Gói';
        } else if (st === 'danggiao') {
            badgeClass = 'status-badge-danggiao';
            statusText = '🚚 Đang Vận Chuyển';
        } else if (st === 'dagiao') {
            badgeClass = 'status-badge-dagiao';
            statusText = '✅ Đã Bàn Giao Thành Công';
        } else if (st === 'dahuy') {
            badgeClass = 'status-badge-dahuy';
            statusText = '❌ Đã Hủy Đơn';
        }

        // Định dạng ngày lập hóa đơn
        let dateStr = 'Mới đặt gần đây';
        if (order.ngayLap) {
            try {
                const d = new Date(order.ngayLap);
                dateStr = `${d.getDate().toString().padStart(2, '0')}/${(d.getMonth() + 1).toString().padStart(2, '0')}/${d.getFullYear()} ${d.getHours().toString().padStart(2, '0')}:${d.getMinutes().toString().padStart(2, '0')}`;
            } catch {
                dateStr = order.ngayLap;
            }
        }

        // Danh sách sản phẩm trong hóa đơn (từ chiTietHoaDons nạp từ CSDL SQL Server thật)
        const items = order.chiTietHoaDons || order.sanPham || [];
        const itemsHtml = items.map(ct => {
            const sp = ct.sanPham || ct.SanPham || {};
            const prodName = sp.tenSanPham || sp.TenSanPham || ct.tenSanPham || ct.TenSanPham || ct.maSanPham || ct.MaSanPham || 'Kiệt tác trang sức';
            const material = sp.chatLieu || sp.ChatLieu || ct.chatLieu || ct.ChatLieu || 'Kiệt tác kim hoàn cao cấp';
            const rawImg = (sp.hinhAnh || sp.HinhAnh || ct.hinhAnh || ct.HinhAnh || '').trim();
            const img = rawImg !== '' ? rawImg : '/images/hero-jewelry.jpg';
            const qty = ct.soLuong || ct.SoLuong || 1;
            const origPrice = ct.donGia || ct.DonGia || 0;
            const discountPercent = ct.phanTramGiamGia !== undefined ? ct.phanTramGiamGia : (ct.PhanTramGiamGia || 0);
            const discountedPrice = discountPercent > 0 ? (origPrice * (100 - discountPercent) / 100) : origPrice;

            const priceDisplay = discountPercent > 0
                ? `<div style="font-size: 0.78rem; color: var(--gold-hover); margin-top: 2px;">
                       <span style="text-decoration: line-through; color: var(--text-muted); margin-right: 4px;">${formatCurrency(origPrice)}</span>
                       <span style="font-weight: 600;">${formatCurrency(discountedPrice)}</span>
                       <span class="product-status-tag sale" style="font-size: 0.65rem; padding: 1px 4px; margin-left: 4px; position: static; display: inline-block;">-${discountPercent}%</span>
                       <span style="color: var(--text-muted);"> × ${qty}</span>
                   </div>`
                : `<div style="font-size: 0.78rem; color: var(--gold-hover); margin-top: 2px;">
                       ${formatCurrency(origPrice)} × ${qty}
                   </div>`;

            return `
                <div class="order-mini-product">
                    <img src="${img}" alt="${prodName}" class="order-mini-img" loading="lazy" onerror="this.onerror=null; this.src='/images/hero-jewelry.jpg';" />
                    <div class="order-mini-info">
                        <div style="font-weight: 600; color: var(--dark-obsidian);">${prodName}</div>
                        <div style="font-size: 0.75rem; color: var(--text-muted);">${material}</div>
                        ${priceDisplay}
                    </div>
                </div>
            `;
        }).join('');

        const finalTotal = order.tongThanhToan !== undefined ? order.tongThanhToan : (order.tongTien || 0);
        const origTotal = order.tongTienHang !== undefined ? order.tongTienHang : (finalTotal + (order.tienGiamGia || 0));
        const discount = order.tienGiamGia || 0;
        const address = order.diaChiGiaoHang || order.diaChi || 'Nhận tại xưởng Blink Blink';
        const phone = order.soDienThoaiNhan || order.soDienThoai || '';
        const payment = order.phuongThucThanhToan || order.phuongThuc || 'Chuyển khoản';

        card.innerHTML = `
            <div class="order-card-head">
                <div>
                    <span class="order-id-label">${order.maHoaDon}</span>
                    <div class="order-date-text">Ngày đặt: ${dateStr}</div>
                </div>
                <span class="${badgeClass}">${statusText}</span>
            </div>

            <div class="order-items-preview">
                ${itemsHtml}
            </div>

            <div class="order-meta-info-row">
                <div><strong>Nơi nhận:</strong> ${address}</div>
                ${phone ? `<div><strong>Điện thoại:</strong> ${phone}</div>` : ''}
                <div><strong>Phương thức:</strong> ${payment}</div>
                ${origTotal > finalTotal ? `<div><strong>Tổng tiền gốc:</strong> ${formatCurrency(origTotal)}</div>` : ''}
                ${discount > 0 ? `<div style="color: #2E7D32;"><strong>Ưu đãi giảm giá SP:</strong> -${formatCurrency(discount)}</div>` : ''}
            </div>

            <div class="order-foot-row">
                <span class="order-foot-label">Tổng thanh toán:</span>
                <span class="order-foot-total">${formatCurrency(finalTotal)}</span>
            </div>
        `;

        container.appendChild(card);
    });
}

/**
 * Mở hộp thoại Hồ Sơ Thành Viên Kim Hoàn
 */
async function openProfileModal() {
    closeUserDropdown();

    if (!currentUser) {
        showToast('Vui lòng đăng nhập để xem thông tin hồ sơ.', 'warning');
        openAuthModal('login');
        return;
    }

    const modal = document.getElementById('profile-modal');
    if (!modal) return;

    // Tự động điền dữ liệu người dùng
    const custId = currentUser.maKhachHang || currentUser.MaKhachHang || currentUser.maNguoiDung;
    const username = currentUser.tenDangNhap || currentUser.TenDangNhap || '';
    const fullname = currentUser.hoTen || currentUser.HoTen || '';
    const phone = currentUser.soDienThoai || currentUser.SoDienThoai || '';

    const elCustId = document.getElementById('profile-cust-id');
    const elDisplayCustId = document.getElementById('profile-display-cust-id');
    const elUsername = document.getElementById('profile-username');
    const elFullname = document.getElementById('profile-fullname');
    const elPhone = document.getElementById('profile-phone');
    const elDob = document.getElementById('profile-dob');
    const banner = document.getElementById('profile-feedback-banner');

    if (banner) banner.style.display = 'none';
    if (elCustId) elCustId.value = custId;
    if (elDisplayCustId) elDisplayCustId.textContent = custId;
    if (elUsername) elUsername.value = username;
    if (elFullname) elFullname.value = fullname;
    if (elPhone) elPhone.value = phone;

    // Gắn bộ lọc chỉ cho phép nhập số vào ô Số điện thoại
    if (elPhone && !elPhone.hasAttribute('data-has-filter')) {
        elPhone.setAttribute('data-has-filter', 'true');
        elPhone.addEventListener('input', function() {
            this.value = this.value.replace(/[^0-9]/g, '');
            if (banner) banner.style.display = 'none';
        });
    }
    if (elFullname && !elFullname.hasAttribute('data-has-filter')) {
        elFullname.setAttribute('data-has-filter', 'true');
        elFullname.addEventListener('input', function() {
            if (banner) banner.style.display = 'none';
        });
    }

    // Cố gắng tra cứu ngày sinh chính xác từ API nếu chưa có
    try {
        const res = await fetch(`/api/KhachHang/${encodeURIComponent(custId)}`);
        if (res.ok) {
            const json = await res.json();
            if (json.success && json.data) {
                const d = json.data;
                if (elFullname && d.hoTen) elFullname.value = d.hoTen;
                if (elPhone && d.soDienThoai) elPhone.value = d.soDienThoai;
                if (elDob && d.ngaySinh) {
                    elDob.value = d.ngaySinh.split('T')[0];
                }
                if (d.trangThai === false) {
                    showToast('Tài khoản của quý khách hiện đang bị tạm khóa.', 'error');
                    closeProfileModal();
                    setTimeout(() => logoutUser(), 1200);
                    return;
                }
            }
        }
    } catch (e) {
        console.error('Không thể nạp hồ sơ từ API:', e);
    }

    modal.style.display = 'flex';
    modal.classList.add('active');
    document.body.style.overflow = 'hidden';
}

/**
 * Đóng hộp thoại Hồ Sơ Thành Viên Kim Hoàn
 */
function closeProfileModal() {
    const modal = document.getElementById('profile-modal');
    if (modal) {
        modal.classList.remove('active');
        modal.style.display = 'none';
        document.body.style.overflow = '';
    }
}

/**
 * Xử lý cập nhật thông tin cá nhân khách hàng gửi lên RESTful API
 * @param {Event} event - Sự kiện submit form
 */
async function handleProfileSubmit(event) {
    if (event) event.preventDefault();

    const custId = document.getElementById('profile-cust-id')?.value.trim();
    const fullname = document.getElementById('profile-fullname')?.value.trim();
    const phone = document.getElementById('profile-phone')?.value.trim();
    const dob = document.getElementById('profile-dob')?.value.trim();

    const banner = document.getElementById('profile-feedback-banner');
    const bannerText = document.getElementById('profile-feedback-text');

    // 1. Kiểm tra Họ và tên
    if (!fullname || fullname.length < 2) {
        if (banner && bannerText) {
            bannerText.textContent = 'Họ và tên quý khách phải có ít nhất 2 ký tự.';
            banner.style.display = 'flex';
        }
        showToast('Họ và tên không hợp lệ. Vui lòng nhập đầy đủ.', 'warning');
        document.getElementById('profile-fullname')?.focus();
        return;
    }

    // 2. Kiểm tra định dạng Số điện thoại Việt Nam (10 chữ số, đầu 03, 05, 07, 08, 09)
    const phoneRegex = /^(0[3|5|7|8|9])[0-9]{8}$/;
    if (!phone || !phoneRegex.test(phone)) {
        if (banner && bannerText) {
            bannerText.textContent = 'Số điện thoại không hợp lệ. Vui lòng nhập đúng 10 chữ số (VD: 0901234567).';
            banner.style.display = 'flex';
        }
        showToast('Số điện thoại không hợp lệ. Vui lòng nhập đúng 10 số (VD: 0901234567).', 'warning');
        document.getElementById('profile-phone')?.focus();
        return;
    }

    // 3. Kiểm tra ngày sinh (nếu có chọn) không vượt quá ngày hiện tại
    if (dob) {
        const birthDate = new Date(dob);
        const today = new Date();
        if (birthDate > today) {
            if (banner && bannerText) {
                bannerText.textContent = 'Ngày sinh không thể lớn hơn ngày hiện tại.';
                banner.style.display = 'flex';
            }
            showToast('Ngày sinh không hợp lệ.', 'warning');
            return;
        }
    }

    const saveBtn = document.getElementById('btn-save-profile');
    if (saveBtn) {
        saveBtn.disabled = true;
        saveBtn.textContent = 'Đang lưu thay đổi hồ sơ...';
    }

    try {
        const response = await fetch('/api/KhachHang/cap-nhat', {
            method: 'PUT',
            headers: {
                'Content-Type': 'application/json',
                'Accept': 'application/json'
            },
            body: JSON.stringify({
                MaKhachHang: custId,
                HoTen: fullname,
                SoDienThoai: phone,
                NgaySinh: dob ? new Date(dob).toISOString() : new Date(1995, 0, 1).toISOString()
            })
        });

        const result = await response.json();

        if (response.status === 403) {
            closeProfileModal();
            showToast(result.message || 'Tài khoản của quý khách hiện đang bị tạm khóa.', 'error');
            setTimeout(() => logoutUser(), 1200);
            return;
        }

        if (response.ok && result.success) {
            // Cập nhật thông tin trong đối tượng currentUser và localStorage
            currentUser.hoTen = fullname;
            currentUser.soDienThoai = phone;
            localStorage.setItem('blink_currentUser', JSON.stringify(currentUser));
            applyLoggedInUser(currentUser);

            closeProfileModal();
            showToast('Cập nhật hồ sơ thành viên kim hoàn thành công!', 'success');
        } else {
            const msg = result.message || 'Không thể cập nhật hồ sơ. Vui lòng thử lại.';
            if (banner && bannerText) {
                bannerText.textContent = msg;
                banner.style.display = 'flex';
            }
            showToast(msg, 'error');
        }
    } catch (err) {
        console.error('Lỗi khi cập nhật hồ sơ:', err);
        const errMsg = 'Lỗi kết nối tới máy chủ xưởng kim hoàn. Vui lòng thử lại sau.';
        if (banner && bannerText) {
            bannerText.textContent = errMsg;
            banner.style.display = 'flex';
        }
        showToast(errMsg, 'error');
    } finally {
        if (saveBtn) {
            saveBtn.disabled = false;
            saveBtn.textContent = 'Lưu Thay Đổi Hồ Sơ';
        }
    }
}

