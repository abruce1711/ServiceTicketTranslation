using Hangfire.Dashboard;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace EmailTranslation.Hangfire
{
    public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
    {
        public IEnumerable<HangfireUserCredentials> Users { get; }

        public HangfireAuthorizationFilter(IEnumerable<HangfireUserCredentials> users)
        {
            Users = users;
        }

        public bool Authorize(DashboardContext dashboardContext)
        {
            var context = dashboardContext.GetHttpContext();

            string header = context.Request.Headers["Authorization"];

            if (!string.IsNullOrWhiteSpace(header))
            {
                AuthenticationHeaderValue authValues = AuthenticationHeaderValue.Parse(header);

                if ("Basic".Equals(authValues.Scheme, StringComparison.InvariantCultureIgnoreCase))
                {
                    string parameter = Encoding.UTF8.GetString(Convert.FromBase64String(authValues.Parameter));
                    var parts = parameter.Split(':');

                    if (parts.Length > 1)
                    {
                        string username = parts[0];
                        string password = parts[1];

                        if ((!string.IsNullOrWhiteSpace(username)) && (!string.IsNullOrWhiteSpace(password)))
                        {
                            return Users.Any(user => user.ValidateUser(username, password)) || Challenge(context);
                        }
                    }
                }
            }

            return Challenge(context);
        }

        private bool Challenge(HttpContext context)
        {
            context.Response.StatusCode = 401;
            context.Response.Headers.Append("WWW-Authenticate", "Basic realm=\"Hangfire Dashboard\"");

            context.Response.WriteAsync("Authentication is required.");

            return false;
        }
    }
}
