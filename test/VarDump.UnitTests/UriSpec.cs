using System;
using Xunit;

namespace VarDump.UnitTests;

public class UriSpec
{
    [Fact]
    public void DumpAbsoluteUriCsharp()
    {
        var uri = new Uri("https://user:password@www.contoso.com:80/Home/Index.htm?q1=v1&q2=v2#FragmentName");
        var dumper = new CSharpDumper();

        var result = dumper.Dump(uri);

        Assert.Equal(
            """
            var uri = new Uri("https://user:password@www.contoso.com:80/Home/Index.htm?q1=v1&q2=v2#FragmentName");
            
            """, result);
    }

    [Fact]
    public void DumpAbsoluteUriVb()
    {
        var uri = new Uri("https://user:password@www.contoso.com:80/Home/Index.htm?q1=v1&q2=v2#FragmentName");

        var dumper = new VisualBasicDumper();

        var result = dumper.Dump(uri);

        Assert.Equal(
            """
            Dim uriValue = New Uri("https://user:password@www.contoso.com:80/Home/Index.htm?q1=v1&q2=v2#FragmentName")
            
            """, result);
    }

    [Fact]
    public void DumpRelativeUriCsharp()
    {
        var address1 = new Uri("http://www.contoso.com/");
        var address2 = new Uri("http://www.contoso.com/index.htm?date=today");
        var relativeUri = address1.MakeRelativeUri(address2);

        var dumper = new CSharpDumper();

        var result = dumper.Dump(relativeUri);

        Assert.Equal(
            """
            var uri = new Uri("index.htm?date=today", UriKind.Relative);
            
            """, result);
    }

    [Fact]
    public void DumpRelativeUriVb()
    {
        var address1 = new Uri("http://www.contoso.com/");
        var address2 = new Uri("http://www.contoso.com/index.htm?date=today");
        var relativeUri = address1.MakeRelativeUri(address2);

        var dumper = new VisualBasicDumper();

        var result = dumper.Dump(relativeUri);

        Assert.Equal(
            """
            Dim uriValue = New Uri("index.htm?date=today", UriKind.Relative)

            """, result);
    }
}