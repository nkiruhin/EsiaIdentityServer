// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using AspNet.Security.OAuth.Esia;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Security.Cryptography.X509Certificates;

namespace IdentityServer
{
    public class Startup
    {
        public IWebHostEnvironment Environment { get; }

        public Startup(IWebHostEnvironment environment)
        {
            Environment = environment;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            // uncomment, if you want to add an MVC-based UI
            services.AddControllersWithViews();
            services.AddHttpClient();
            var builder = services.AddIdentityServer(options =>
            {
                // see https://identityserver4.readthedocs.io/en/latest/topics/resources.html
                options.EmitStaticAudienceClaim = true;
            })
                .AddInMemoryIdentityResources(Config.IdentityResources)
                .AddInMemoryApiScopes(Config.ApiScopes)
                .AddInMemoryClients(Config.Clients);

            services.AddAuthentication()
                .AddEsia(options =>
                {
                    options.ClientId = "577005"; // идентификатор системы-клиента, обязателен
                    options.SubClientId = "989fbc1c-9691-4804-b73b-f2e135dc7b0f"; // идентификатор в сервисе генерации url
                    //options.ClientCertificate = new X509Certificate2(); // сертификат системы-клиента, обязателен
                    // по умолчанию используются боевые адреса ЕСИА, можно поменять на тестовые:
                    options.AuthorizationEndpoint = EsiaConstants.TestAuthorizationUrl;
                    options.TokenEndpoint = EsiaConstants.TestAccessTokenUrl;
                    options.UserInformationEndpoint = EsiaConstants.TestUserInformationUrl;
                    options.RedirectUrlServiceEndpoint = EsiaConstants.RedirectUrlServiceEndpoint;
                    options.SignServiceEndpoint = EsiaConstants.SignServiceEndpoint;

                    // получение контактных данных пользователя (почта, телефон), по умолчанию отключено
                    // options.FetchContactInfo = true;
                });

            // not recommended for production - you need to store your key material somewhere secure
            builder.AddDeveloperSigningCredential();
            
        }

        public void Configure(IApplicationBuilder app)
        {
            if (Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // uncomment if you want to add MVC
            app.UseStaticFiles();
            app.UseRouting();
            
            app.UseIdentityServer();

            // uncomment, if you want to add MVC
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
            });
        }
    }
}
