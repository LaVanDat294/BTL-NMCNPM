using NMCNPM.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace NMCNPM.Data
{
    public class TaiKhoanRepository
    {
        private readonly string connectionString =
            ConfigurationManager.ConnectionStrings[
                "HRMConnection"].ConnectionString;

        // Lấy tài khoản theo mã tài khoản

        public TaiKhoan GetByMaTK(string maTK)
        {
            TaiKhoan tk = null;

            string sql = @"
        SELECT
            tk.sMaTK,
            tk.FK_sMaNV,
            tk.sTenDangNhap,
            tk.sMatKhauHash,
            tk.FK_sMaQuyen,
            q.sTenQuyen,
            nv.sTenNV,
            tk.bTrangThai
        FROM dbo.tbl_TaiKhoan tk
        INNER JOIN dbo.tbl_NhanVien nv
            ON tk.FK_sMaNV = nv.sMaNV
        INNER JOIN dbo.tbl_Quyen q
            ON tk.FK_sMaQuyen = q.sMaQuyen
        WHERE tk.sMaTK = @MaTK";

            using (SqlConnection conn =
                new SqlConnection(connectionString))
            using (SqlCommand cmd =
                new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@MaTK", maTK);

                conn.Open();

                using (SqlDataReader rd = cmd.ExecuteReader())
                {
                    if (rd.Read())
                    {
                        tk = new TaiKhoan
                        {
                            MaTK = rd["sMaTK"].ToString(),
                            MaNV = rd["FK_sMaNV"].ToString(),
                            TenDangNhap = rd["sTenDangNhap"].ToString(),
                            MatKhauHash = rd["sMatKhauHash"].ToString(),
                            MaQuyen = rd["FK_sMaQuyen"].ToString(),
                            TenQuyen = rd["sTenQuyen"].ToString(),
                            TenNV = rd["sTenNV"].ToString(),
                            TrangThai = Convert.ToBoolean(
                                rd["bTrangThai"])
                        };
                    }
                }
            }

            return tk;
        }

        public bool CreateAccount(TaiKhoan tk)
        {
            string sql = @"
        INSERT INTO dbo.tbl_TaiKhoan
        (
            sMaTK,
            FK_sMaNV,
            sTenDangNhap,
            sMatKhauHash,
            FK_sMaQuyen,
            bTrangThai
        )
        VALUES
        (
            @MaTK,
            @MaNV,
            @TenDangNhap,
            @MatKhauHash,
            @MaQuyen,
            1
        )";

            using (SqlConnection conn =
                new SqlConnection(connectionString))
            using (SqlCommand cmd =
                new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@MaTK", tk.MaTK);
                cmd.Parameters.AddWithValue("@MaNV", tk.MaNV);
                cmd.Parameters.AddWithValue("@TenDangNhap", tk.TenDangNhap);
                cmd.Parameters.AddWithValue("@MatKhauHash", tk.MatKhauHash);
                cmd.Parameters.AddWithValue("@MaQuyen", tk.MaQuyen);

                conn.Open();

                return cmd.ExecuteNonQuery() > 0;
            }
        }


        public bool HasAnyAccount()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string sql = "SELECT COUNT(*) FROM tbl_TaiKhoan";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    conn.Open();
                    return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
                }
            }
        }


        // Cập nhật mật khẩu mới
        public bool UpdatePassword(
            string maTK, string passwordHash)
        {
            string sql = @"
                UPDATE tbl_TaiKhoan
                SET sMatKhauHash = @MatKhauHash
                WHERE sMaTK = @MaTK
                  AND bTrangThai = 1";

            using (SqlConnection conn =
                new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue(
                    "@MatKhauHash", passwordHash);

                cmd.Parameters.AddWithValue("@MaTK", maTK);

                conn.Open();

                return cmd.ExecuteNonQuery() > 0;
            }
        }
    }
}