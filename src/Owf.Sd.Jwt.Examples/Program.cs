// See https://aka.ms/new-console-template for more information
using Owf.Sd.Jwt.Examples;

var issuer = new Issuer();
var claims = new Dictionary<string, object>
{
    { "nickname", "Thomas" }
};

var issuerJwk = @"{""kty"":""EC"",""crv"":""P-256"",""x"":""BHId3zoDv6pDgOUh8rKdloUZ0YumRTcaVDCppUPoYgk"",""y"":""g3QIDhaWEksYtZ9OWjNHn9a6-i_P9o5_NrdISP0VWDU"",""d"":""KpTnMOHEpskXvuXHFCfiRtGUHUZ9Dq5CCcZQ-19rYs4""}";
var token = issuer.Issue(claims, issuerJwk);
Console.WriteLine(token);
