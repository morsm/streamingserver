<%@ Page Title="Streaming channels" Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="true"
    CodeBehind="Channels.aspx.cs" Inherits="StreamingApp.Channels" %>

<asp:Content ID="HeaderContent" runat="server" ContentPlaceHolderID="HeadContent">
</asp:Content>
<asp:Content ID="BodyContent" runat="server" ContentPlaceHolderID="MainContent">
    <h2>
        Channels</h2>
    <p>
        <asp:GridView ID="gviewChannels" runat="server" 
            DataSourceID="ObjectDataSource1" onrowcommand="GridView1_RowCommand" 
            AllowSorting="True">
            <Columns>
                <asp:ButtonField CommandName="Stream" Text="Stream" />
            </Columns>
        </asp:GridView>
        <asp:ObjectDataSource ID="ObjectDataSource1" runat="server" 
            SelectMethod="GetChannels" TypeName="StreamingApp.ServiceConnection">
        </asp:ObjectDataSource>
    </p>
</asp:Content>
