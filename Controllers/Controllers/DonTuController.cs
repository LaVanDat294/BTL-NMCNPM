{

		private readonly string _connectionString;

public DonTuController(IConfiguration configuration)
{
	_connectionString =
		configuration.GetConnectionString("DefaultConnection")!;
}

// Kiểm tra nhân viên đã đăng nhập
private string? LayMaNV()
{
	return HttpContext.Session.GetString("sMaNV");
}

// Kiểm tra quyền quản lý
private bool LaQuanLy()
{
	string? quyen = HttpContext.Session.GetString("sMaQuyen");

	return quyen == "Q_MANAGER" || quyen == "Q_ADMIN";
}

// 1. Danh sách đơn của nhân viên
public async Task<IActionResult> Index()
{
	string? maNV = LayMaNV();

	if (string.IsNullOrEmpty(maNV))
		return RedirectToAction("Login", "Account");

	var danhSach = new List<DonTu>();

	using (SqlConnection conn = new SqlConnection(_connectionString))
	{
		string sql = @"
                    SELECT *
                    FROM vw_DanhSachDonTu
                    WHERE FK_sMaNV = @MaNV
                    ORDER BY dNgayGui DESC";

		using SqlCommand cmd = new SqlCommand(sql, conn);

		cmd.Parameters.Add("@MaNV", SqlDbType.VarChar, 20)
			.Value = maNV;

		await conn.OpenAsync();

		using SqlDataReader reader = await cmd.ExecuteReaderAsync();

		while (await reader.ReadAsync())
		{
			danhSach.Add(new DonTu
			{
				sMaDon = reader["sMaDon"].ToString()!,
				FK_sMaNV = reader["FK_sMaNV"].ToString()!,
				sTenNguoiTao = reader["sTenNguoiTao"].ToString(),
				sLoaiDon = reader["sLoaiDon"].ToString()!,
				dNgayGui = Convert.ToDateTime(reader["dNgayGui"]),
				sLyDo = reader["sLyDo"] == DBNull.Value
					? "" : reader["sLyDo"].ToString()!,
				sTrangThai = reader["sTrangThai"].ToString(),
				sNguoiDuyet = reader["sNguoiDuyet"] == DBNull.Value
					? null : reader["sNguoiDuyet"].ToString(),
				sTenNguoiDuyet = reader["sTenNguoiDuyet"] == DBNull.Value
					? null : reader["sTenNguoiDuyet"].ToString()
			});
		}
	}

	return View(danhSach);
}

// 2. Hiển thị form tạo đơn
[HttpGet]
public IActionResult TaoDon()
{
	if (string.IsNullOrEmpty(LayMaNV()))
		return RedirectToAction("Login", "Account");

	return View();
}

// 3. Xử lý tạo đơn
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> TaoDon(DonTu model)
{
	string? maNV = LayMaNV();

	if (string.IsNullOrEmpty(maNV))
		return RedirectToAction("Login", "Account");

	string[] loaiDonHopLe =
	{
				"Nghỉ phép",
				"Đổi ca",
				"Giải trình"
			};

	if (!loaiDonHopLe.Contains(model.sLoaiDon))
		ModelState.AddModelError("sLoaiDon", "Loại đơn không hợp lệ.");

	if (!ModelState.IsValid)
		return View(model);

	model.sMaDon = "DT" + Guid.NewGuid()
		.ToString("N").Substring(0, 10).ToUpper();

	using (SqlConnection conn = new SqlConnection(_connectionString))
	{
		string sql = @"
                    INSERT INTO tbl_DonTu
                    (
                        sMaDon,
                        FK_sMaNV,
                        sLoaiDon,
                        dNgayGui,
                        sLyDo,
                        sTrangThai,
                        sNguoiDuyet
                    )
                    VALUES
                    (
                        @MaDon,
                        @MaNV,
                        @LoaiDon,
                        GETDATE(),
                        @LyDo,
                        N'Chờ duyệt',
                        NULL
                    )";

		using SqlCommand cmd = new SqlCommand(sql, conn);

		cmd.Parameters.Add("@MaDon", SqlDbType.VarChar, 20)
			.Value = model.sMaDon;

		cmd.Parameters.Add("@MaNV", SqlDbType.VarChar, 20)
			.Value = maNV;

		cmd.Parameters.Add("@LoaiDon", SqlDbType.NVarChar, 50)
			.Value = model.sLoaiDon;

		cmd.Parameters.Add("@LyDo", SqlDbType.NVarChar, 500)
			.Value = model.sLyDo;

		await conn.OpenAsync();
		await cmd.ExecuteNonQueryAsync();
	}

	TempData["ThongBao"] = "Tạo đơn thành công! Đơn đang chờ duyệt.";

	return RedirectToAction("Index");
}

// 4. Danh sách đơn chờ phê duyệt
[HttpGet]
public async Task<IActionResult> PheDuyet()
{
	if (string.IsNullOrEmpty(LayMaNV()))
		return RedirectToAction("Login", "Account");

	if (!LaQuanLy())
		return Forbid();

	var danhSach = new List<DonTu>();

	using (SqlConnection conn = new SqlConnection(_connectionString))
	{
		string sql = @"
                    SELECT *
                    FROM vw_DanhSachDonTu
                    WHERE sTrangThai = N'Chờ duyệt'
                    ORDER BY dNgayGui ASC";

		using SqlCommand cmd = new SqlCommand(sql, conn);

		await conn.OpenAsync();

		using SqlDataReader reader = await cmd.ExecuteReaderAsync();

		while (await reader.ReadAsync())
		{
			danhSach.Add(new DonTu
			{
				sMaDon = reader["sMaDon"].ToString()!,
				FK_sMaNV = reader["FK_sMaNV"].ToString()!,
				sTenNguoiTao = reader["sTenNguoiTao"].ToString(),
				sLoaiDon = reader["sLoaiDon"].ToString()!,
				dNgayGui = Convert.ToDateTime(reader["dNgayGui"]),
				sLyDo = reader["sLyDo"] == DBNull.Value
					? "" : reader["sLyDo"].ToString()!,
				sTrangThai = reader["sTrangThai"].ToString()
			});
		}
	}

	return View(danhSach);
}

// 5. Phê duyệt hoặc từ chối đơn
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> XuLyDon(string maDon, string trangThai)
{
	string? maNV = LayMaNV();

	if (string.IsNullOrEmpty(maNV))
		return RedirectToAction("Login", "Account");

	if (!LaQuanLy())
		return Forbid();

	if (trangThai != "Đã duyệt" && trangThai != "Từ chối")
		return BadRequest("Trạng thái không hợp lệ.");

	using (SqlConnection conn = new SqlConnection(_connectionString))
	{
		string sql = @"
                    UPDATE tbl_DonTu
                    SET
                        sTrangThai = @TrangThai,
                        sNguoiDuyet = @NguoiDuyet,
                        dNgayDuyet = GETDATE()
                    WHERE sMaDon = @MaDon
                      AND sTrangThai = N'Chờ duyệt'
                      AND FK_sMaNV <> @NguoiDuyet";

		using SqlCommand cmd = new SqlCommand(sql, conn);

		cmd.Parameters.Add("@TrangThai", SqlDbType.NVarChar, 50)
			.Value = trangThai;

		cmd.Parameters.Add("@NguoiDuyet", SqlDbType.VarChar, 20)
			.Value = maNV;

		cmd.Parameters.Add("@MaDon", SqlDbType.VarChar, 20)
			.Value = maDon;

		await conn.OpenAsync();

		int rows = await cmd.ExecuteNonQueryAsync();

		TempData["ThongBao"] = rows > 0
			? "Xử lý đơn thành công!"
			: "Không tìm thấy đơn hoặc đơn đã được xử lý.";
	}

	return RedirectToAction("PheDuyet");
}
    }