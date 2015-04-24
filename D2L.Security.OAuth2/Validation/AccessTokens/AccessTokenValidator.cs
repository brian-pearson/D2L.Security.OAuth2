﻿using System;
using System.IdentityModel.Tokens;
using System.Threading.Tasks;

using D2L.Security.OAuth2.Keys;
using D2L.Security.OAuth2.Keys.Remote;
using D2L.Security.OAuth2.Validation.Exceptions;

namespace D2L.Security.OAuth2.Validation.AccessTokens {
	internal sealed class AccessTokenValidator : IAccessTokenValidator {

		internal const string ALLOWED_SIGNATURE_ALGORITHM = "RS256";

		private readonly IPublicKeyProvider m_publicKeyProvider;
		private readonly JwtSecurityTokenHandler m_tokenHandler = new JwtSecurityTokenHandler();

		public AccessTokenValidator(
			IPublicKeyProvider publicKeyProvider
		) {
			m_publicKeyProvider = publicKeyProvider;
		}

		async Task<IValidationResponse> IAccessTokenValidator.ValidateAsync(
			Uri jwksEndPoint,
			string token
		) {
			
			var unvalidatedToken = (JwtSecurityToken)m_tokenHandler.ReadToken(
				token
			);
			
			if( unvalidatedToken.SignatureAlgorithm != ALLOWED_SIGNATURE_ALGORITHM ) {
				string message = string.Format(
					"Signature algorithm '{0}' is not supported.  Permitted algorithm is '{1}'",
					unvalidatedToken.SignatureAlgorithm,
					ALLOWED_SIGNATURE_ALGORITHM
				);
				throw new InvalidSignatureAlgorithmException( message );
			}

			if( !unvalidatedToken.Header.ContainsKey( "kid" ) ) {
				throw new MissingKeyIdException( "KeyId not found in token" );
			}

			string keyId = unvalidatedToken.Header["kid"].ToString();
			Guid id;
			if( !Guid.TryParse( keyId, out id ) ) {
				throw new Exception( "ffooof TODO" );
			}
			IAccessToken accessToken = null;

			using( D2LSecurityToken signingToken = await m_publicKeyProvider.GetSecurityTokenAsync(
				jwksEndPoint: jwksEndPoint,
				keyId: id
			).SafeAsync() ) {

				var validationParameters = new TokenValidationParameters() {
					ValidateAudience = false,
					ValidateIssuer = false,
					RequireSignedTokens = true,
					IssuerSigningToken = signingToken
				};
				
				try {

					SecurityToken securityToken;
					m_tokenHandler.ValidateToken(
						token,
						validationParameters,
						out securityToken
						);
					accessToken = new AccessToken( (JwtSecurityToken) securityToken );

				} catch( SecurityTokenExpiredException ) {

					return new ValidationResponse(
						ValidationStatus.Expired,
						accessToken: null );
				}
			}

			return new ValidationResponse(
				ValidationStatus.Success,
				accessToken
			);

		}
	}
}