# Hướng dẫn Build và Upload bản Playable lên Luna

Dưới đây là các bước cơ bản để build và upload Playable Ad từ dự án Unity lên hệ thống Luna (Luna Create Hub).

## 1. Yêu cầu trước khi build
- Đảm bảo bạn đã cài đặt Plugin Luna cho Unity trong dự án này.
- Đã đăng nhập vào tài khoản Luna của bạn trong cửa sổ Luna UI ở Unity Editor.
- Kiểm tra các thiết lập Playable (Settings, Networks) để đảm bảo cấu hình chính xác cho các nền tảng mạng quảng cáo mục tiêu (ironSource, AppLovin, v.v.).

## 2. Cách Build (Develop Build)
1. Trên thanh menu của Unity, chọn **Luna** -> **Luna UI**.
2. Chuyển sang tab **Develop** (hoặc tab **Build** tuỳ phiên bản Luna).
3. Nhấn nút **Build** để Luna bắt đầu biên dịch dự án Unity của bạn ra chuẩn HTML5.
4. Sau khi quá trình build hoàn tất, Luna thường sẽ cung cấp tuỳ chọn chạy thử trực tiếp trên trình duyệt (localhost) để bạn kiểm tra các chức năng và layout.

## 3. Cách Upload lên Luna Create Hub
1. Mở cửa sổ **Luna UI** trong Unity.
2. Chuyển sang tab **Upload** (hoặc tìm nút Upload).
3. Điền các thông tin cần thiết (nếu được yêu cầu):
   - **App / Concept Name**: Đặt tên cho bản playable để dễ quản lý.
4. Nhấn nút **Upload**. Hãy chờ vài phút để quá trình tải lên máy chủ Luna hoàn tất.
5. Sau khi thành công, bạn sẽ nhận được thông báo hoặc đường dẫn trực tiếp đi đến Luna Create Hub.

## 4. Quản lý trên Luna Create Hub
1. Mở trình duyệt và truy cập vào [Luna Create Hub](https://create.lunalabs.io/).
2. Đăng nhập với tài khoản Luna của bạn.
3. Tìm kiếm bản Playable vừa upload trong thư mục dự án tương ứng ở mục **Concepts** hoặc **Apps**.
4. Tại đây, bạn có thể:
   - Chỉnh sửa các biến (Playable Variables).
   - Kiểm tra và review bản Playable trên các kích thước màn hình thiết bị khác nhau.
   - **Export (Xuất bản)** ra các file tối ưu riêng cho từng mạng quảng cáo (như ironSource, Unity Ads, AppLovin, Mintegral, v.v.).
