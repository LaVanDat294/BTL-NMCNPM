using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace NMCNPM.Filters
{
    public class AuthorizeRoleAttribute : AuthorizeAttribute, IAuthorizationFilter
    {
        private readonly string[] _roles;

        public AuthorizeRoleAttribute(params string[] roles)
        {
            _roles = roles;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;

            // Đã bổ sung kiểm tra null cho user.Identity
            if (user.Identity == null || !user.Identity.IsAuthenticated)
            {
                // Chưa đăng nhập thì chuyển về Login
                context.Result = new RedirectToActionResult("Login", "Account", new { returnUrl = context.HttpContext.Request.Path });
                return;
            }

            // Kiểm tra role (Mã quyền: Q_ADMIN, Q_MANAGER, Q_STAFF)
            var userRole = user.FindFirst("MaQuyen")?.Value;

            if (string.IsNullOrEmpty(userRole) || !_roles.Contains(userRole))
            {
                // Có đăng nhập nhưng không đủ quyền
                context.Result = new RedirectToActionResult("AccessDenied", "Account", null);
            }
        }
    }
}