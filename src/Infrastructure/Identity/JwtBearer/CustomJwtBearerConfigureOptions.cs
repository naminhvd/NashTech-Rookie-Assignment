﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Globalization;
using Infrastructure.Identity.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Identity.JwtBearer;

/// <summary>
/// Custom version of Microsofts' 
/// Microsoft.AspNetCore.Authentication.<see cref="CustomJwtBearerConfigureOptions"/>.
/// </summary>
internal sealed class CustomJwtBearerConfigureOptions : IConfigureNamedOptions<CustomJwtBearerOptions>
{
    private readonly IAuthenticationConfigurationProvider authenticationConfigurationProvider;
    private readonly IDataProtectionProvider dataProtectionProvider;
    private static readonly Func<string, TimeSpan> invariantTimeSpanParse = (string timespanString) => TimeSpan.Parse(timespanString, CultureInfo.InvariantCulture);

    private const string primaryPurpose = "JWTBearerToken";

    /// <summary>
    /// Initializes a new <see cref="CustomJwtBearerConfigureOptions"/> given the configuration
    /// provided by the <paramref name="configurationProvider"/>.
    /// </summary>
    /// <param name="configurationProvider">An <see cref="IAuthenticationConfigurationProvider"/> instance.</param>\
    public CustomJwtBearerConfigureOptions(
        IAuthenticationConfigurationProvider configurationProvider,
        IDataProtectionProvider protectionProvider)
    {
        authenticationConfigurationProvider = configurationProvider;
        dataProtectionProvider = protectionProvider;
    }

    /// <inheritdoc />
    public void Configure(string? name, CustomJwtBearerOptions options)
    {
        if (string.IsNullOrEmpty(name))
        {
            return;
        }

        options.BearerTokenProtector = new TicketDataFormat(dataProtectionProvider.CreateProtector(primaryPurpose, name, "BearerToken"));
        options.RefreshTokenProtector = new TicketDataFormat(dataProtectionProvider.CreateProtector(primaryPurpose, name, "RefreshToken"));

        var configSection = authenticationConfigurationProvider.GetSchemeConfiguration(name);

        if (configSection is null || !configSection.GetChildren().Any())
        {
            return;
        }

        var issuer = configSection[nameof(TokenValidationParameters.ValidIssuer)];
        var issuers = configSection.GetSection(nameof(TokenValidationParameters.ValidIssuers)).GetChildren().Select(iss => iss.Value).ToList();
        var audience = configSection[nameof(TokenValidationParameters.ValidAudience)];
        var audiences = configSection.GetSection(nameof(TokenValidationParameters.ValidAudiences)).GetChildren().Select(aud => aud.Value).ToList();

        options.Authority = configSection[nameof(options.Authority)] ?? options.Authority;
        options.BackchannelTimeout = StringHelpers.ParseValueOrDefault(configSection[nameof(options.BackchannelTimeout)], invariantTimeSpanParse, options.BackchannelTimeout);
        options.Challenge = configSection[nameof(options.Challenge)] ?? options.Challenge;
        options.ForwardAuthenticate = configSection[nameof(options.ForwardAuthenticate)] ?? options.ForwardAuthenticate;
        options.ForwardChallenge = configSection[nameof(options.ForwardChallenge)] ?? options.ForwardChallenge;
        options.ForwardDefault = configSection[nameof(options.ForwardDefault)] ?? options.ForwardDefault;
        options.ForwardForbid = configSection[nameof(options.ForwardForbid)] ?? options.ForwardForbid;
        options.ForwardSignIn = configSection[nameof(options.ForwardSignIn)] ?? options.ForwardSignIn;
        options.ForwardSignOut = configSection[nameof(options.ForwardSignOut)] ?? options.ForwardSignOut;
        options.IncludeErrorDetails = StringHelpers.ParseValueOrDefault(configSection[nameof(options.IncludeErrorDetails)], bool.Parse, options.IncludeErrorDetails);
        options.MapInboundClaims = StringHelpers.ParseValueOrDefault(configSection[nameof(options.MapInboundClaims)], bool.Parse, options.MapInboundClaims);
        options.MetadataAddress = configSection[nameof(options.MetadataAddress)] ?? options.MetadataAddress;
        options.RefreshInterval = StringHelpers.ParseValueOrDefault(configSection[nameof(options.RefreshInterval)], invariantTimeSpanParse, options.RefreshInterval);
        options.RefreshOnIssuerKeyNotFound = StringHelpers.ParseValueOrDefault(configSection[nameof(options.RefreshOnIssuerKeyNotFound)], bool.Parse, options.RefreshOnIssuerKeyNotFound);
        options.RequireHttpsMetadata = StringHelpers.ParseValueOrDefault(configSection[nameof(options.RequireHttpsMetadata)], bool.Parse, options.RequireHttpsMetadata);
        options.SaveToken = StringHelpers.ParseValueOrDefault(configSection[nameof(options.SaveToken)], bool.Parse, options.SaveToken);
        options.TokenValidationParameters = new()
        {
            ValidateIssuer = issuers.Count > 0,
            ValidIssuers = issuers,
            ValidIssuer = issuer,
            ValidateAudience = audiences.Count > 0,
            ValidAudiences = audiences,
            ValidAudience = audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKeys = GetIssuerSigningKeys(configSection, issuers),
        };
        options.BearerTokenExpiration = StringHelpers.ParseValueOrDefault(configSection[nameof(options.BearerTokenExpiration)], TimeSpan.Parse, options.BearerTokenExpiration);
        options.RefreshTokenExpiration = StringHelpers.ParseValueOrDefault(configSection[nameof(options.RefreshTokenExpiration)], TimeSpan.Parse, options.RefreshTokenExpiration);
    }

    private static IEnumerable<SecurityKey> GetIssuerSigningKeys(IConfiguration configuration, List<string?> issuers)
    {
        foreach (var issuer in issuers)
        {
            var signingKey = configuration.GetSection("SigningKeys")
                .GetChildren()
                .SingleOrDefault(key => key["Issuer"] == issuer);
            if (signingKey is not null && signingKey["Value"] is string keyValue)
            {
                yield return new SymmetricSecurityKey(Convert.FromBase64String(keyValue));
            }
        }
    }

    /// <inheritdoc />
    public void Configure(CustomJwtBearerOptions options)
    {
        Configure(Options.DefaultName, options);
    }
}