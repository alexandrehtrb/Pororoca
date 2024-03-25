using Pororoca.Domain.Features.Common;
using Xunit;
using static Pororoca.Domain.Features.VariableCapture.PororocaResponseValueCapturer;

namespace Pororoca.Domain.Tests.Features.VariableCapture;

public static partial class PororocaResponseValueCapturerTests
{
    private const string testJsonStr = "\"Alexandre\"";
    private const string testJsonSimpleObj = "{\"myObj\":\"Oi\"}";
    private const string testJsonObj = "{\"myObj\":{\"id\": 1, \"myObj2\": {\"name\":\"Alexandre\", \"key\":123 ,\"arr\":[1,2,3]  }}}";
    private const string testJsonArr = "[" + testJsonObj + ", " + testJsonObj + "]";
    private const string testJsonMatrix = "[" + testJsonArr + ", " + testJsonArr + "]";

    private const string testXmlSimpleObj = @"<SessionInfo>
                                                <SessionID>MSCB2B-UKT3517_f2823910df-5eff81-528aff-11e6f-0d2ed2408332</SessionID>
                                                <Profile>A</Profile>
                                                <Language>ENG</Language>
                                                <Version>1</Version>
                                              </SessionInfo>";
    private const string testXmlComplexObj =
        @"<?xml version=""1.0"" encoding=""UTF-8""?>
          <bookstore>
              <book category=""cooking"">
                  <title lang=""en"">Everyday Italian</title>
                  <author>Giada De Laurentiis</author>
                  <year>2005</year>
                  <price>30.00</price>
              </book>
              <book category=""children"">
                  <title lang=""en"">Harry Potter</title>
                  <author>J K. Rowling</author>
                  <year>2005</year>
                  <price>29.99</price>
              </book>
              <book category=""web"">
                  <title lang=""en"">XQuery Kick Start</title>
                  <author>James McGovern</author>
                  <author>Per Bothner</author>
                  <author>Kurt Cagle</author>
                  <author>James Linn</author>
                  <author>Vaidyanathan Nagarajan</author>
                  <year>2003</year>
                  <price>49.99</price>
              </book>
              <book category=""web"">
                  <title lang=""en"">Learning XML</title>
                  <author>Erik T. Ray</author>
                  <year>2003</year>
                  <price>39.95</price>
              </book>
          </bookstore>";

    private const string testXmlObjWithNamespaces =
        @"<env:Envelope xmlns:env=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:wsa=""http://www.w3.org/2005/08/addressing"">
              <env:Body>
                  <xsi:response xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
                      <wsa:MyVal1>Alexandre</wsa:MyVal1>
                      <xsi:Value>
                          <wsa:MyVal2>123987456</wsa:MyVal2>
                      </xsi:Value>
                  </xsi:response>
              </env:Body>
          </env:Envelope>";

    [Theory]
    [InlineData(null, "$.id", "Some string that is not a JSON")]
    [InlineData("Alexandre", "$", testJsonStr)]
    [InlineData("1", "$.myObj.id", testJsonObj)]
    [InlineData("Alexandre", "$.myObj.myObj2.name", testJsonObj)]
    [InlineData("123", "$.myObj.myObj2.key", testJsonObj)]
    [InlineData("1", "$.myObj.myObj2.arr[0]", testJsonObj)]
    [InlineData("2", "$.myObj.myObj2.arr[1]", testJsonObj)]
    [InlineData("3", "$.myObj.myObj2.arr[2]", testJsonObj)]
    [InlineData("1", "$[0].myObj.id", testJsonArr)]
    [InlineData("Alexandre", "$[0].myObj.myObj2.name", testJsonArr)]
    [InlineData("123", "$[0].myObj.myObj2.key", testJsonArr)]
    [InlineData("1", "$[1].myObj.id", testJsonArr)]
    [InlineData("Alexandre", "$[1].myObj.myObj2.name", testJsonArr)]
    [InlineData("123", "$[1].myObj.myObj2.key", testJsonArr)]
    [InlineData("1", "$[0].myObj.myObj2.arr[0]", testJsonArr)]
    [InlineData("2", "$[0].myObj.myObj2.arr[1]", testJsonArr)]
    [InlineData("3", "$[0].myObj.myObj2.arr[2]", testJsonArr)]
    [InlineData("1", "$[0][1].myObj.id", testJsonMatrix)]
    [InlineData("Alexandre", "$[1][0].myObj.myObj2.name", testJsonMatrix)]
    [InlineData("123", "$[1][1].myObj.myObj2.key", testJsonMatrix)]
    [InlineData("2", "$[1][1].myObj.myObj2.arr[1]", testJsonMatrix)]
    [InlineData("[1,2,3]", "$.myObj.myObj2.arr", testJsonObj)]
    [InlineData(testJsonSimpleObj, "$", testJsonSimpleObj)]
    public static void TestJsonValueCapture(string? expectedCapture, string path, string json) =>
        Assert.Equal(expectedCapture, CaptureJsonValue(path, json));

    [Theory]
    [InlineData("2", "$.count()", testJsonArr)]
    [InlineData("3", "$.myObj.myObj2.arr.count()", testJsonObj)]
    [InlineData(null, "$.myObj.count()", testJsonObj)]
    public static void TestJsonValueCaptureCountFunction(string? expectedCapture, string path, string json) =>
        Assert.Equal(expectedCapture, CaptureJsonValue(path, json));

    [Theory]
    [InlineData("ABC", "/a", "<a>ABC</a>")]
    [InlineData("ENG", "/SessionInfo/Language", testXmlSimpleObj)]
    [InlineData("1", "/SessionInfo/Version", testXmlSimpleObj)]
    [InlineData("Giada De Laurentiis", "/bookstore/book[1]/author", testXmlComplexObj)]
    [InlineData("Alexandre", "/env:Envelope/env:Body/xsi:response/wsa:MyVal1", testXmlObjWithNamespaces)]
    [InlineData("123987456", "/env:Envelope/env:Body/xsi:response/xsi:Value/wsa:MyVal2", testXmlObjWithNamespaces)]
    public static void TestXmlValueCapture(string expectedCapture, string xpath, string xml)
    {
        XmlUtils.LoadXmlDocumentAndNamespaceManager(xml, out var doc, out var nsm);
        Assert.Equal(expectedCapture, CaptureXmlValue(xpath, doc, nsm));
    }
}