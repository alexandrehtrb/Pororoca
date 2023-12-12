using System.Text;
using Pororoca.Domain.Features.ResponseParsing;
using Xunit;
using static Pororoca.Domain.Features.ResponseParsing.PororocaHttpMultipartResponseBodyReader;

namespace Pororoca.Domain.Tests.Features.ResponseParsing;

public static class PororocaHttpMultipartResponseBodyReaderTests
{
    #region BYTE ARRAY SPLIT

    [Fact]
    public static void Should_split_byte_array_correctly_when_splitter_is_not_present()
    {
        var input = "aiaiaiaiai"u8;
        var splitter = "oi"u8;

        var parts = input.Split(splitter);
        byte[] part = Assert.Single(parts);
        Assert.Equal("aiaiaiaiai"u8.ToArray(), part);
    }

    [Fact]
    public static void Should_split_byte_array_correctly_when_splitter_is_present_in_middle()
    {
        var input = "aiaiaioiaiai"u8;
        var splitter = "oi"u8;

        var parts = input.Split(splitter);
        Assert.Equal(2, parts.Count);
        Assert.Equal("aiaiai"u8.ToArray(), parts[0]);
        Assert.Equal("aiai"u8.ToArray(), parts[1]);
    }

    [Fact]
    public static void Should_split_byte_array_correctly_when_splitter_is_present_in_start()
    {
        var input = "oiaiaiai"u8;
        var splitter = "oi"u8;

        var parts = input.Split(splitter);
        byte[] part = Assert.Single(parts);
        Assert.Equal("aiaiai"u8.ToArray(), part);
    }

    [Fact]
    public static void Should_split_byte_array_correctly_when_splitter_is_present_in_end()
    {
        var input = "aiaioi"u8;
        var splitter = "oi"u8;

        var parts = input.Split(splitter);
        byte[] part = Assert.Single(parts);
        Assert.Equal("aiai"u8.ToArray(), part);
    }

    [Fact]
    public static void Should_split_byte_array_correctly_when_splitter_is_present_in_start_and_middle()
    {
        var input = "oiaiaiaioiaiai"u8;
        var splitter = "oi"u8;

        var parts = input.Split(splitter);
        Assert.Equal(2, parts.Count);
        Assert.Equal("aiaiai"u8.ToArray(), parts[0]);
        Assert.Equal("aiai"u8.ToArray(), parts[1]);
    }

    [Fact]
    public static void Should_split_byte_array_correctly_when_splitter_is_present_in_middle_and_end()
    {
        var input = "aiaiaioiaiaioi"u8;
        var splitter = "oi"u8;

        var parts = input.Split(splitter);
        Assert.Equal(2, parts.Count);
        Assert.Equal("aiaiai"u8.ToArray(), parts[0]);
        Assert.Equal("aiai"u8.ToArray(), parts[1]);
    }

    [Fact]
    public static void Should_split_byte_array_correctly_when_splitter_is_present_in_start_middle_and_end()
    {
        var input = "oiaiaiaioiaiaioi"u8;
        var splitter = "oi"u8;

        var parts = input.Split(splitter);
        Assert.Equal(2, parts.Count);
        Assert.Equal("aiaiai"u8.ToArray(), parts[0]);
        Assert.Equal("aiai"u8.ToArray(), parts[1]);
    }

    [Fact]
    public static void Should_split_byte_array_correctly_when_splitter_is_present_more_than_2_parts_closed()
    {
        var input = "oiaiaiaioiaiaioiqwqwqwqwoizmzmzmzoi"u8;
        var splitter = "oi"u8;

        var parts = input.Split(splitter);
        Assert.Equal(4, parts.Count);
        Assert.Equal("aiaiai"u8.ToArray(), parts[0]);
        Assert.Equal("aiai"u8.ToArray(), parts[1]);
        Assert.Equal("qwqwqwqw"u8.ToArray(), parts[2]);
        Assert.Equal("zmzmzmz"u8.ToArray(), parts[3]);
    }

    [Fact]
    public static void Should_split_byte_array_correctly_when_splitter_is_present_more_than_2_parts_open()
    {
        var input = "oiaiaiaioiaiaioiqwqwqwqwoizmzmzmz"u8;
        var splitter = "oi"u8;

        var parts = input.Split(splitter);
        Assert.Equal(4, parts.Count);
        Assert.Equal("aiaiai"u8.ToArray(), parts[0]);
        Assert.Equal("aiai"u8.ToArray(), parts[1]);
        Assert.Equal("qwqwqwqw"u8.ToArray(), parts[2]);
        Assert.Equal("zmzmzmz"u8.ToArray(), parts[3]);
    }

    #endregion

    [Fact]
    public static void Should_read_multipart_boundary_properly() =>
        Assert.Equal(
            "--7af8b434-4b56-4daa-b6e4-2d453518b69e",
            ReadMultipartBoundary("Content-Type: multipart/form-data; boundary=\"7af8b434-4b56-4daa-b6e4-2d453518b69e\""));

    [Fact]
    public static void Should_return_empty_multipart_boundary_if_not_found() =>
        Assert.Equal(string.Empty, ReadMultipartBoundary("Content-Type: multipart/byterange"));

    [Fact]
    public static void Should_read_multipart_response_parts_correctly()
    {
        // GIVEN
        byte[] fullBinaryBody = File.ReadAllBytes(GetTestFilePath("multipartformdata"));

        // WHEN
        var parts = ReadMultipartResponseParts(fullBinaryBody, "--6fdbe94b-214a-4c2d-91be-88ecb66ccdcd");

        // THEN
        Assert.NotNull(parts);
        Assert.Equal(2, parts.Length);

        Assert.Equal([
            new("Content-Type", "text/plain"),
            new("Content-Disposition", "form-data; name=a")],
            parts[0].Headers.ToArray());
        Assert.Equal("oi", Encoding.UTF8.GetString(parts[0].BinaryBody));

        Assert.Equal([
            new("Content-Type", "image/gif"),
            new("Content-Disposition", "form-data; name=arq; filename=pirate.gif")],
            parts[1].Headers.ToArray());
        Assert.Equal("R0lGODlhQABAAMQRAAAAAAgIAAgQGBAQEFoAAIRjOZR7Sq0ICLUAAL29vcalY86le+fe3vf39//OnP//AP///wAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAACH/C05FVFNDQVBFMi4wAwEAAAAh+QQFDwARACwAAAAAQABAAAAF/iAkjmRpnmiqrmzrvnAsz3Rt33iu7zPg/z9eDkgEwnxCUXFpZPkcyB1zGk0BoFiAbjriop7X8JAJ0ZrJpt9iAWBDcd4uutRmr9fvG7VcjpPaDnd4WjVUVYaEXQ6BeHk9e31BkXNli5ZshYh0mkqAlomPXkSdoglAi6AyiKtFZQkMsLCjegZXlre4uAC1f6+xvwypoVO5n7xdvsCycLbFq73Jv1U0np8/1qeXoAAECMrCqmu2AAUiBeNEqIHb3d7SQ26LfRDY11CYf+0ICADBUrfz6j1BRUffvmlDLAVElc5RF4MEwOkpdiubwzL7MvJLUinXgkUKnP3RmFEiHGfN/iqOJIkgIseBFHNFAcCypUkbNFOmVPeJH02WBFwK+QkTSkOGNfcFvUmtXURcgnQZ1Lg0CTeq1YopuFqz6tCp/Xx8xLU1bFehQ1mGDbYkWDCgaHlw1djPrTK7PzN6tVqzrl1psfK2jJsWqF+/f+fufUky6FJZgIMFVUqYb0nKEX1I8+G4M1M4evN6ltYZ82c9oak+DuxY9WmcqdVKO/u6UOx9Dx7wm53bNceVNjX2Rkyzt97KhYPLDkz7d5fbCIbz1n28NjXo0pkbp2y9B/SSs+F2V4VdN/Ho1Lk7V1J+t/b0g8fHEC0YPPOu8o+0P79daf4XfpmSV3ayoNfYfy0gS8aAKcsViN96CjKIm3nTHQghMBIayB91nkG4RGsHhTdYhxCu09mJxJ1IonNXsMEZisRxA+N6XdgBhGMKSrZUEzSW4SIrPwjAI0chAAAh+QQFDwARACwTAAcAGwAeAAAFhGAkjmQpAoAIQWbbok66rm4NxM44s3U53DfdrFdCLRaRRXBHHAGUyGSM2YzcjsnIdFi9OY4Lx5bXaD7FuRM30A2LreuqdS7kyQEJholqzhf5RHh6JACAPYKDalx9eYmFNHJWBAgRBwiPkHeTTouRf3aef6GjpKUjBKapqqusra49B6EhAAA7",
                     Convert.ToBase64String(parts[1].BinaryBody));
    }

    private static string GetTestFilePath(string fileName)
    {
        var testDataDirInfo = new DirectoryInfo(Environment.CurrentDirectory).Parent!.Parent!.Parent!;
        return Path.Combine(testDataDirInfo.FullName, "TestData", fileName);
    }
}