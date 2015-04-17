﻿using System;
using System.Collections.Generic;
using System.Security.Claims;
using D2L.Security.OAuth2.Scopes;

namespace D2L.Security.OAuth2.Validation.AccessTokens {
	internal sealed class ValidatedTokenToD2LPrincipalAdapter : ID2LPrincipal {

		private readonly DateTime m_expiry;
		private readonly string m_userId;
		private readonly string m_tenantId;
		private readonly IEnumerable<Scope> m_scopes;
		private readonly PrincipalType m_type;
		private readonly IEnumerable<Claim> m_allClaims;
		
		private readonly string m_xsrf;
		private readonly string m_accessToken;
		private readonly string m_accessTokenId;

		internal ValidatedTokenToD2LPrincipalAdapter( IValidatedToken validatedToken, string accessToken ) {
			m_expiry = validatedToken.Expiry;
			m_tenantId = validatedToken.GetTenantId();
			m_scopes = validatedToken.GetScopes();

			m_userId = validatedToken.GetUserId();
			m_type = string.IsNullOrEmpty( m_userId ) ? PrincipalType.Service : PrincipalType.User;

			m_xsrf = validatedToken.GetXsrfToken();
			m_accessToken = accessToken;
			m_allClaims = validatedToken.Claims;
			m_accessTokenId = validatedToken.GetAccessTokenId();
		}

		DateTime ID2LPrincipal.AccessTokenExpiry {
			get { return m_expiry; }
		}

		string ID2LPrincipal.UserId {
			get { return m_userId; }
		}
		
		string ID2LPrincipal.TenantId {
			get { return m_tenantId; }
		}
		
		IEnumerable<Scope> ID2LPrincipal.Scopes {
			get { return m_scopes; }
		}

		PrincipalType ID2LPrincipal.Type {
			get { return m_type; }
		}

		string ID2LPrincipal.Xsrf {
			get { return m_xsrf;  }
		}

		string ID2LPrincipal.AccessToken {
			get { return m_accessToken; }
		}

		IEnumerable<Claim> ID2LPrincipal.AllClaims {
			get { return m_allClaims; }
		}

		string ID2LPrincipal.AccessTokenId {
			get { return m_accessTokenId; }
		}
	}
}
