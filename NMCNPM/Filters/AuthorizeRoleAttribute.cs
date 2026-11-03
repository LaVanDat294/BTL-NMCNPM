using NMCNPM.Data;
using NMCNPM.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace NMCNPM.Filters
{
    public class AuthorizeRoleAttribute : AuthorizeAttribute
    {
        private readonly string[] allowedRoles;

        public AuthorizeRoleAttribute(params string[] roles)
        {
            allowedRoles = roles ?? new string[0];
        }

        public override void OnAuthorization(
            AuthorizationContext filterContext)
        {
            var http = filterContext.HttpContext;
            var session = http.Session;

            // Nếu Session chưa có thì khôi phục từ Cookie
            if (session["MaTK"] == null)
            {
                HttpCookie cookie = http.Request.Cookies[
                    FormsAuthentication.FormsCookieName];

                if (cookie != null)
                {
                    try
                    {
                        FormsAuthenticationTicket ticket =
                            FormsAuthentication.Decrypt(cookie.Value);

                        if (ticket != null && !ticket.Expired)
                        {
                            string[] data =
                                ticket.UserData.Split('|');

                            if (data.Length == 3)
                            {
                                var repo =
                                    new TaiKhoanRepository();

                                TaiKhoan tk =
                                    repo.GetByMaTK(data[0]);

                                if (tk != null &&
                                    tk.TrangThai &&
                                    tk.MaNV == data[1] &&
                                    tk.MaQuyen == data[2])
                                {
                                    SetSession(session, tk);
                                }
                            }
                        }
                    }
                    catch
                    {
                        // Cookie không hợp lệ
                    }
                }
            }

            // Chưa đăng nhập
            if (session["MaTK"] == null)
            {
                filterContext.Result =
                    new RedirectToRouteResult(
                        new System.Web.Routing.RouteValueDictionary
                        {
                            { "controller", "TaiKhoan" },
                            { "action", "Login" }
                        });

                return;
            }

            // Kiểm tra quyền
            string role = session["MaQuyen"].ToString();

            if (allowedRoles.Length > 0 &&
                !allowedRoles.Contains(role))
            {
                filterContext.Result =
                    new RedirectToRouteResult(
                        new System.Web.Routing.RouteValueDictionary
                        {
                            { "controller", "TaiKhoan" },
                            { "action", "AccessDenied" }
                        });
            }
        }

        private void SetSession(
            HttpSessionStateBase session, TaiKhoan tk)
        {
            session["MaTK"] = tk.MaTK;
            session["MaNV"] = tk.MaNV;
            session["TenDangNhap"] = tk.TenDangNhap;
            session["HoTen"] = tk.TenNV;
            session["MaQuyen"] = tk.MaQuyen;
            session["TenQuyen"] = tk.TenQuyen;
        }
    }
}