// Lấy token bảo mật từ Razor
function layAntiForgeryToken() {
    const token = document.querySelector(
        'input[name="__RequestVerificationToken"]'
    );

    return token ? token.value : "";
}

// Gửi yêu cầu chấm công
async function guiYeuCauChamCong(url) {
    const token = layAntiForgeryToken();

    if (!token) {
        alert("Không tìm thấy mã bảo mật. Vui lòng tải lại trang.");
        return;
    }

    try {
        const response = await fetch(url, {
            method: "POST",
            headers: {
                "Content-Type": "application/x-www-form-urlencoded"
            },
            body: new URLSearchParams({
                "__RequestVerificationToken": token
            }),
            credentials: "same-origin"
        });

        if (!response.ok) {
            alert("Không thể gửi yêu cầu chấm công.");
            return;
        }

        const result = await response.json();

        // THAY ĐỔI TẠI ĐÂY:
        if (result.success) {
            // Nếu thành công, không alert nữa mà tải lại dữ liệu mới từ Database luôn
            location.reload();
        } else {
            // Chỉ hiện thông báo (alert) trong trường hợp có lỗi xảy ra (ví dụ: hết giờ, sai ca...)
            alert(result.message);
        }

    } catch (error) {
        console.error(error);
        alert("Có lỗi xảy ra khi kết nối máy chủ.");
    }
}

// Chấm công vào (Bấm là chạy luôn, không hỏi lại)
function handleCheckIn() {
    const url = "/ChamCong/ChamCongVao";
    guiYeuCauChamCong(url);
}

// Chấm công ra (Bấm là chạy luôn, không hỏi lại)
function handleCheckOut() {
    const url = "/ChamCong/ChamCongRa";
    guiYeuCauChamCong(url);
}



document.addEventListener("DOMContentLoaded", function () {
    // 1. Tìm phần tử hiển thị đồng hồ bằng ID
    const clockElement = document.getElementById("realtime-clock");

    if (clockElement) {
        // 2. Tạo hàm cập nhật thời gian
        function updateClock() {
            const now = new Date();

            // Lấy giờ, phút, giây và tự động thêm số 0 ở trước nếu < 10
            const hours = String(now.getHours()).padStart(2, '0');
            const minutes = String(now.getMinutes()).padStart(2, '0');
            const seconds = String(now.getSeconds()).padStart(2, '0');

            // Định dạng chuỗi HH:mm:ss
            clockElement.innerText = `${hours}:${minutes}:${seconds}`;
        }

        // 3. Chạy hàm ngay lập tức khi trang vừa load xong
        updateClock();

        // 4. Thiết lập cứ mỗi 1000ms (1 giây) sẽ cập nhật lại đồng hồ một lần
        setInterval(updateClock, 1000);
    }
});
