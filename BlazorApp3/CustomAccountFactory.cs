using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;

namespace BlazorApp3
{
    public class CustomAccountFactory
     : AccountClaimsPrincipalFactory<RemoteUserAccount>
    {
        private readonly ILogger<CustomAccountFactory> logger;
        private readonly IServiceProvider serviceProvider;

        public CustomAccountFactory(IAccessTokenProviderAccessor accessor,
            IServiceProvider serviceProvider,
            ILogger<CustomAccountFactory> logger)
            : base(accessor)
        {
            this.serviceProvider = serviceProvider;
            this.logger = logger;
        }

        public async override ValueTask<ClaimsPrincipal> CreateUserAsync(
            RemoteUserAccount account,
            RemoteAuthenticationUserOptions options)
        {
            var initialUser = await base.CreateUserAsync(account, options);

            if (initialUser.Identity.IsAuthenticated)
            {
                var userIdentity = (ClaimsIdentity)initialUser.Identity;

                try
                {
                    var graphClient = ActivatorUtilities
                        .CreateInstance<GraphServiceClient>(serviceProvider);
                    var request = graphClient.Me.Request();
                    var user = await request.GetAsync();

                    if (user != null)
                    {
                        userIdentity.AddClaim(new Claim("mobilephone",
                            user.MobilePhone));
                    }
                }
                catch (ServiceException exception)
                {
                    logger.LogError("Graph API service failure: {Message}",
                        exception.Message);
                }
            }

            return initialUser;
        }
    }
}
