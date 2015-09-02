using System;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Filters;
using MvvmTools.Web.Models;

namespace MvvmTools.Web.Attributes
{
    internal class IdentityBasicAuthenticationAttribute : Attribute, IAuthenticationFilter
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        public bool AllowMultiple => false;

        public async Task AuthenticateAsync(HttpAuthenticationContext context, CancellationToken cancellationToken)
        {
            
        }

        public async Task ChallengeAsync(HttpAuthenticationChallengeContext context, CancellationToken cancellationToken)
        {
            
        }
    }
}
