namespace Owf.Sd.Jwt.Abstracts;

public interface IIssuer
{
    public string Issue(Dictionary<string, object> claims, string issuerJwk);
}
