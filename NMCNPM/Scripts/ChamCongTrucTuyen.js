// Lấy mã bảo mật AntiForgeryToken
function layAntiForgeryToken() {
    var token = document.querySelector(
        'input[name="__RequestVerificationToken"]'
    );

    return token ? token.value : "";
}

// Lấy hai nút chấm công
function layNutChamCong() {
    return {
        nutVao: document.querySelector(".btn-checkin"),
        nutRa: document.querySelector(".btn-checkout")
    };
}

// Cập nhật trạng thái hiển thị
function capNhatTrangThai(trangThai, trangThaiVao, trangThaiRa) {
    var statusElement = document.getElementById("attendance-status");

    if (!statusElement) {
        return;
    }

    var noiDung = trangThai || "";

    if (trangThaiVao) {
        noiDung += " - " + trangThaiVao;
    }

    if (trangThaiRa) {
        noiDung += " - " + trangThaiRa;
    }

    var span = statusElement.querySelector("span");

    if (span) {
        span.textContent = noiDung;
    } else {
        statusElement.textContent = "Trạng thái: " + noiDung;
    }
}

// Cập nhật giờ vào
function capNhatGioVao(gioVao) {
    var element = document.getElementById("check-in-time");

    if (element) {
        element.textContent = gioVao || "--:--:--";
    }
}

// Cập nhật giờ ra
function capNhatGioRa(gioRa) {
    var element = document.getElementById("check-out-time");

    if (element) {
        element.textContent = gioRa || "--:--:--";
    }
}

// Xử lý yêu cầu chấm công
async function guiYeuCauChamCong(url, loaiChamCong) {
    var token = layAntiForgeryToken();
    var nut = layNutChamCong();

    if (!token) {
        alert("Không tìm thấy mã bảo mật. Vui lòng tải lại trang.");
        return;
    }

    if (!nut.nutVao || !nut.nutRa) {
        alert("Không tìm thấy nút chấm công trên giao diện.");
        return;
    }

    // Lưu trạng thái ban đầu để khôi phục nếu có lỗi
    var trangThaiNutVao = nut.nutVao.disabled;
    var trangThaiNutRa = nut.nutRa.disabled;

    // Khóa nút trong khi gửi yêu cầu, tránh bấm nhiều lần
    nut.nutVao.disabled = true;
    nut.nutRa.disabled = true;

    try {
        var response = await fetch(url, {
            method: "POST",
            headers: {
                "Content-Type": "application/x-www-form-urlencoded"
            },
            body: new URLSearchParams({
                "__RequestVerificationToken": token
            }),
            credentials: "same-origin"
        });

        var result = await response.json();

        if (!response.ok) {
            throw new Error(
                result.message || "Không thể gửi yêu cầu chấm công."
            );
        }

        if (!result.success) {
            alert(result.message || "Chấm công không thành công.");

            // Khôi phục trạng thái nút nếu máy chủ từ chối
            nut.nutVao.disabled = trangThaiNutVao;
            nut.nutRa.disabled = trangThaiNutRa;

            return;
        }

        var data = result.data;

        if (!data) {
            throw new Error("Máy chủ không trả về dữ liệu chấm công.");
        }

        // CẬP NHẬT SAU KHI CHẤM CÔNG VÀO
        if (loaiChamCong === "vao") {
            capNhatGioVao(data.gioVao);

            // Khóa nút vào, mở nút ra
            nut.nutVao.disabled = true;
            nut.nutRa.disabled = false;

            capNhatTrangThai(
                data.trangThai,
                data.trangThaiVao,
                null
            );

            alert(result.message || "Chấm công vào thành công.");
        }

        // CẬP NHẬT SAU KHI CHẤM CÔNG RA
        else if (loaiChamCong === "ra") {
            capNhatGioVao(data.gioVao);
            capNhatGioRa(data.gioRa);

            // Khóa cả hai nút
            nut.nutVao.disabled = true;
            nut.nutRa.disabled = true;

            capNhatTrangThai(
                data.trangThai,
                data.trangThaiVao,
                data.trangThaiRa
            );

            alert(result.message || "Chấm công ra thành công.");
        }

    } catch (error) {
        console.error("Lỗi chấm công:", error);

        alert(error.message || "Có lỗi xảy ra khi kết nối máy chủ.");

        // Khôi phục trạng thái nút khi xảy ra lỗi
        nut.nutVao.disabled = trangThaiNutVao;
        nut.nutRa.disabled = trangThaiNutRa;
    }
}

// Chấm công vào
function handleCheckIn() {
    guiYeuCauChamCong(
        "/ChamCong/ChamCongVao",
        "vao"
    );
}

// Chấm công ra
function handleCheckOut() {
    guiYeuCauChamCong(
        "/ChamCong/ChamCongRa",
        "ra"
    );
}

// ĐỒNG HỒ THỜI GIAN THỰC
document.addEventListener("DOMContentLoaded", function () {
    var clockElement = document.getElementById("realtime-clock");

    if (!clockElement) {
        return;
    }

    function updateClock() {
        var now = new Date();

        var hours = String(now.getHours()).padStart(2, "0");
        var minutes = String(now.getMinutes()).padStart(2, "0");
        var seconds = String(now.getSeconds()).padStart(2, "0");

        clockElement.textContent =
            hours + ":" + minutes + ":" + seconds;
    }

    updateClock();

    setInterval(updateClock, 1000);
});