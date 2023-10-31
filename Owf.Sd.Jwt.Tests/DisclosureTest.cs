using System.Text.Json;

namespace Owf.Sd.Jwt.Tests
{
    public class DisclosureTest
    {
        [Fact]
        public void Disclosure_Ctor()
        {
            var salt = "_26bc4LT-ac6q2KI6cBW5es";
            var claimName = "family_name";
            var claimValue = "Möbius";

            // The expected disclosure here is the version of "No white space".
            // (The implementation of Disclosure does not insert redundant
            // white spaces when it builds JSON internally.)
            var expectedDisclosure = "WyJfMjZiYzRMVC1hYzZxMktJNmNCVzVlcyIsImZhbWlseV9uYW1lIiwiTcO2Yml1cyJd";

            var disclosure = Disclosure.Create(salt, claimName, claimValue);
            var actualDisclosure = disclosure.GetBase64Url();

            Assert.Equal(expectedDisclosure, actualDisclosure);
        }

        [Fact]
        public void Disclosure_FromBase64Url ()
        {
            var salt = "_26bc4LT-ac6q2KI6cBW5es";
            var claimName = "family_name";
            var claimValue = "Möbius";
            var base64Disclosure = "WyJfMjZiYzRMVC1hYzZxMktJNmNCVzVlcyIsICJmYW1pbHlfbmFtZSIsICJNw7ZiaXVzIl0";

            var disclosure = Disclosure.FromBase64Url(base64Disclosure);
            Assert.NotNull(disclosure);
            Assert.Equal(salt, disclosure.Salt);
            Assert.Equal(claimName, disclosure.ClaimName);
            Assert.Equivalent(JsonSerializer.Serialize(claimValue), JsonSerializer.Serialize(disclosure.ClaimValue));
        }

        [Fact]
        public void Disclosure_TestArray()
        {
            var claimValue = "my_array_element";

            // Disclosure that represents an array element.
            Disclosure disclosure1 = Disclosure.Create(claimValue);
            var disclosure1Str = disclosure1.GetBase64Url();

            var disclosure2 = Disclosure.FromBase64Url(disclosure1Str);

            Assert.Null(disclosure2?.ClaimName);
            Assert.Equivalent(JsonSerializer.Serialize(claimValue), JsonSerializer.Serialize(disclosure2?.ClaimValue));
        }

        [Fact]
        public void Disclosure_TestArray_Elements()
        {
            var dc = "WyJsa2x4RjVqTVlsR1RQVW92TU5JdkNBIiwgIkZSIl0";
            var salt = "lklxF5jMYlGTPUovMNIvCA";
            var value = "FR";
            var digest = "w0I8EKcdCtUPkGCNUrfwVp2xEgNjtoIDlOxc9-PlOhs";

            // Create a disclosure for an array element.
            var disclosure = Disclosure.FromBase64Url(dc);

            Assert.Equivalent(JsonSerializer.Serialize(salt), JsonSerializer.Serialize(disclosure?.Salt));
            Assert.Null(disclosure?.ClaimName);
            Assert.Equivalent(JsonSerializer.Serialize(value), JsonSerializer.Serialize(disclosure?.ClaimValue));

            // Create a Map that represents an array element.
            var elements = disclosure?.ToArrayElement();
            Assert.True(elements?.ContainsKey(SDConstants.KEY_THREE_DOTS));
            Assert.Equal(digest, elements?.GetValueOrDefault(SDConstants.KEY_THREE_DOTS));
        }
    }
}