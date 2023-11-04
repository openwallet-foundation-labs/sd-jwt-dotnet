namespace Owf.Sd.Jwt.Tests;

public class SDJWTTests
{
    [Fact]
    public void SDJW_CrJwt_Only()
    {
        // Credential JWT
        var crJwt = "a.b.c";

        // SD-JWT with no disclosures.
        var input = $"{crJwt}~";

        // Parse the input.
        var sdJwt = SDJWT.Parse(input);

        // Credential JWT
        Assert.Equal(crJwt, sdJwt?.CredentialJwt);

        // Disclosures
        Assert.Equal(0, sdJwt?.Disclosures.Count);

        // Binding JWT
        Assert.Null(sdJwt?.BindingJwt);
        
        // String representation
        Assert.Equal(input, sdJwt?.ToString());
    }

    [Fact]
    public void SDJW_CrJwt_Disclosure()
    {
        var crJwt = "a.b.c";
        var dc0 = "WyI0d3dqUzlyMm4tblBxdzNpTHR0TkFBIiwgInN0cmVldF9hZGRyZXNzIiwgIlNjaHVsc3RyLiAxMiJd";

        // SD-JWT with 1 disclosure.
        var input = $"{crJwt}~{dc0}~";

        // Parse the input.
        var sdJwt = SDJWT.Parse(input);

        // Credential JWT
        Assert.Equal(crJwt, sdJwt?.CredentialJwt);

        // Disclosures
        Assert.Equal(1, sdJwt?.Disclosures.Count);

        var disclosure0 = sdJwt?.Disclosures[0];
        Assert.Equal(dc0, disclosure0?.ToString());

        // Binding JWT
        Assert.Null(sdJwt?.BindingJwt);

        // String representation
        Assert.Equal(input, sdJwt?.ToString());
    }

    [Fact]
    public void SDJW_CrJwt_Multiple_Disclosures()
    {
        var crJwt = "a.b.c";
        var dc0 = "WyI0d3dqUzlyMm4tblBxdzNpTHR0TkFBIiwgInN0cmVldF9hZGRyZXNzIiwgIlNjaHVsc3RyLiAxMiJd";
        var dc1 = "WyJXcEtIQmVTa3A5U2MyNVV4a1F1RmNRIiwgImxvY2FsaXR5IiwgIlNjaHVscGZvcnRhIl0";

        // SD-JWT with 2 disclosures.
        var input = $"{crJwt}~{dc0}~{dc1}~";

        // Parse the input.
        var sdJwt = SDJWT.Parse(input);

        // Credential JWT
        Assert.Equal(crJwt, sdJwt?.CredentialJwt);

        // Disclosures
        Assert.Equal(2, sdJwt?.Disclosures.Count);

        var disclosure0 = sdJwt?.Disclosures[0];
        Assert.Equal(dc0, disclosure0?.ToString());

        var disclosure1 = sdJwt?.Disclosures[1];
        Assert.Equal(dc1, disclosure1?.ToString());

        // Binding JWT
        Assert.Null(sdJwt?.BindingJwt);

        // String representation
        Assert.Equal(input, sdJwt?.ToString());
    }

    [Fact]
    public void SDJW_CrJwt_BindingJwt()
    {
        var crJwt = "a.b.c";
        var bdJwt = "d.e.f";
        
        // SD-JWT with 2 disclosures and binding jwt.
        var input = $"{crJwt}~{bdJwt}";

        // Parse the input.
        var sdJwt = SDJWT.Parse(input);

        // Credential JWT
        Assert.Equal(crJwt, sdJwt?.CredentialJwt);

        // Disclosures
        Assert.Equal(0, sdJwt?.Disclosures.Count);

        // Binding JWT
        Assert.Equal(bdJwt, sdJwt?.BindingJwt);

        // String representation
        Assert.Equal(input, sdJwt?.ToString());
    }


    [Fact]
    public void SDJW_CrJwt_Multiple_Disclosures_BindingJwt()
    {
        var crJwt = "a.b.c";
        var bdJwt = "d.e.f";
        var dc0 = "WyI0d3dqUzlyMm4tblBxdzNpTHR0TkFBIiwgInN0cmVldF9hZGRyZXNzIiwgIlNjaHVsc3RyLiAxMiJd";
        var dc1 = "WyJXcEtIQmVTa3A5U2MyNVV4a1F1RmNRIiwgImxvY2FsaXR5IiwgIlNjaHVscGZvcnRhIl0";

        // SD-JWT with 2 disclosures and binding jwt.
        var input = $"{crJwt}~{dc0}~{dc1}~{bdJwt}";

        // Parse the input.
        var sdJwt = SDJWT.Parse(input);

        // Credential JWT
        Assert.Equal(crJwt, sdJwt?.CredentialJwt);

        // Disclosures
        Assert.Equal(2, sdJwt?.Disclosures.Count);

        var disclosure0 = sdJwt?.Disclosures[0];
        Assert.Equal(dc0, disclosure0?.ToString());

        var disclosure1 = sdJwt?.Disclosures[1];
        Assert.Equal(dc1, disclosure1?.ToString());

        // Binding JWT
        Assert.Equal(bdJwt, sdJwt?.BindingJwt);

        // String representation
        Assert.Equal(input, sdJwt?.ToString());
    }

}
