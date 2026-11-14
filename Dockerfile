# ========================================================
# DOCKERFILE CHO MÔI TRƯỜNG NGƯỜI DÙNG TRIỂN KHAI (PRODUCTION)
# Kỹ thuật: Multi-stage build (Đa tầng)
# Tối ưu hóa: Dung lượng siêu nhẹ, bảo mật cao, tách biệt môi trường build và runtime
# ========================================================

# --------------------------------------------------------
# TẦNG 1: BIÊN DỊCH VÀ ĐÓNG GÓI MÃ NGUỒN (BUILD STAGE)
# --------------------------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Sao chép file kịch bản CSDL để phục vụ cơ chế tự động nhúng tài nguyên vào thư viện Infrastructure
COPY BlinkBlinkJewelry.sql /
COPY BlinkBlinkJewelry.sql /src/
COPY src/BlinkBlink.Domain/*.csproj ./src/BlinkBlink.Domain/
COPY src/BlinkBlink.Application/*.csproj ./src/BlinkBlink.Application/
COPY src/BlinkBlink.Infrastructure/*.csproj ./src/BlinkBlink.Infrastructure/
COPY src/BlinkBlink.Web/*.csproj ./src/BlinkBlink.Web/

# Khôi phục các gói thư viện NuGet
RUN dotnet restore src/BlinkBlink.Web/BlinkBlink.Web.csproj

# Sao chép toàn bộ mã nguồn các tầng kiến trúc Clean Architecture
COPY src/ ./src/

# Chuyển vào thư mục Web và biên dịch gói phát hành tối ưu
WORKDIR /src/src/BlinkBlink.Web
RUN dotnet publish BlinkBlink.Web.csproj -c Release -o /app/publish /p:UseAppHost=false

# --------------------------------------------------------
# TẦNG 2: MÔI TRƯỜNG CHẠY THỰC TẾ TINH GỌN (RUNTIME STAGE)
# --------------------------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Thiết lập các biến môi trường Production
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080
ENV DOTNET_PRINT_TELEMETRY_MESSAGE=false

# Mở cổng tiêu chuẩn 8080 của ASP.NET Core 8
EXPOSE 8080

# Chỉ sao chép các tệp nhị phân đã phát hành từ tầng build, loại bỏ toàn bộ mã nguồn và công cụ SDK
COPY --from=build /app/publish .

# Tạo thư mục chứa các tệp nhật ký thích ứng xoay vòng theo ngày
RUN mkdir -p /app/logs

# Khởi chạy ứng dụng Web mua sắm và máy chủ RESTful API
ENTRYPOINT ["dotnet", "BlinkBlink.Web.dll"]
