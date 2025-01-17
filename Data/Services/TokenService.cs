﻿using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using SmartBuyApi.DataBase.Tables;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SmartBuyApi.Data.Services
{
	public class TokenService
	{
		private const int ExpirationMinutes = 90;
		private readonly IConfiguration _configuration;

		public TokenService(IConfiguration configuration)
		{
			_configuration = configuration;
		}
		public string CreateToken(SmartUser user)
		{
			var expiration = DateTime.UtcNow.AddMinutes(ExpirationMinutes);
			var token = CreateJwtToken(
				CreateClaims(user),
				CreateSigningCredentials(),
				expiration
			);
			var tokenHandler = new JwtSecurityTokenHandler();
			return tokenHandler.WriteToken(token);
		}

		private JwtSecurityToken CreateJwtToken(List<Claim> claims, SigningCredentials credentials,
			DateTime expiration) =>
			new(
				_configuration["Jwt:Issuer"],
				_configuration["Jwt:Audience"],
				claims,
				expires: expiration,
				signingCredentials: credentials
			);

		private List<Claim> CreateClaims(SmartUser user)
		{
			try
			{
				var claims = new List<Claim>
				{
					new Claim(JwtRegisteredClaimNames.Sub, _configuration["Jwt:Subject"]),
					new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
					new Claim(JwtRegisteredClaimNames.Iat, DateTime.UtcNow.ToString(CultureInfo.InvariantCulture)),
					new Claim(ClaimTypes.NameIdentifier, user.Id),
					new Claim(ClaimTypes.Name, user.UserName),
					new Claim(ClaimTypes.Email, user.Email)
				};
				return claims;
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				throw;
			}
		}
		private SigningCredentials CreateSigningCredentials()
		{
			return new SigningCredentials(
				new SymmetricSecurityKey(
					Encoding.UTF8.GetBytes(_configuration["Jwt:Key"])
				),
				SecurityAlgorithms.HmacSha256
			);
		}
	}
}
