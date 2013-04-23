<%@ Page Title="Termors.net Streaming" Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="true"
    CodeBehind="Default.aspx.cs" Inherits="StreamingApp._Default" %>

<asp:Content ID="HeaderContent" runat="server" ContentPlaceHolderID="HeadContent">
</asp:Content>
<asp:Content ID="BodyContent" runat="server" ContentPlaceHolderID="MainContent">
    <h2>
        Streaming Application
    </h2>
<p>
        Go to Channels page to stream (new) channel.</p>
<p>
        <asp:Table ID="tbCurrent" runat="server">
            <asp:TableRow runat="server" ID="tbCurrentTopRow"><asp:TableHeaderCell>Currently streaming:</asp:TableHeaderCell>
            </asp:TableRow>
            <asp:TableRow runat="server"><asp:TableCell ID="tbCurrentChannelRow"></asp:TableCell>
            </asp:TableRow>
            <asp:TableRow runat="server"><asp:TableCell>Local Url</asp:TableCell><asp:TableCell><a id="localUrlLink" runat="server"></a></asp:TableCell>
            </asp:TableRow>
            <asp:TableRow runat="server"><asp:TableCell>Remote Url</asp:TableCell><asp:TableCell><a id="remoteUrlLink" runat="server"></a></asp:TableCell>
            </asp:TableRow>
            <asp:TableRow runat="server" ID="TableRow1"><asp:TableCell><asp:Button ID="butStop" runat="server" Text="Stop"  onclick="OnStopClicked" /></asp:TableCell>
            </asp:TableRow>
        </asp:Table>
    </p>
    </asp:Content>
