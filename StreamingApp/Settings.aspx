<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Settings.aspx.cs" Inherits="StreamingApp.Settings" MasterPageFile="~/Site.Master" %>

<asp:Content ID="HeaderContent" runat="server" ContentPlaceHolderID="HeadContent">
</asp:Content>
<asp:Content ID="BodyContent" runat="server" ContentPlaceHolderID="MainContent">
    <h2>
        Streaming settings</h2>
    <p>
    Settings apply to <strong>next</strong> streaming job.</p>

<table style="width:100%;">
        <tr>
            <td>
                Video bitrate (kbps)</td>
            <td>
                <asp:TextBox ID="textVideoBitrate" runat="server"></asp:TextBox>
                <asp:RangeValidator ID="valVideoBitrate" runat="server" 
                    ControlToValidate="textVideoBitrate" 
                    ErrorMessage="Must be between 64 and 8192." MaximumValue="8192" 
                    MinimumValue="64" Type="Integer"></asp:RangeValidator>
            </td>
        </tr>
        <tr>
            <td>
                Maximum video bitrate (kbps)</td>
            <td>
                <asp:TextBox ID="textMaxVideoBitrate" runat="server"></asp:TextBox>
                <asp:RangeValidator ID="valMaxVideoBitrate" runat="server" 
                    ControlToValidate="textMaxVideoBitrate" 
                    ErrorMessage="Must be between 64 and 8192." MaximumValue="8192" 
                    MinimumValue="64" Type="Integer"></asp:RangeValidator>
            </td>
        </tr>
        <tr>
            <td>
                Audio bitrate (kbps)</td>
            <td>
                <asp:TextBox ID="textAudioBitrate" runat="server"></asp:TextBox>
                <asp:RangeValidator ID="valAudioBitrate" runat="server" 
                    ControlToValidate="textAudioBitrate" ErrorMessage="Must be between 16 and 320." 
                    MaximumValue="320" MinimumValue="16" Type="Integer"></asp:RangeValidator>
            </td>
        </tr>
        <tr>
            <td>
                Video size (width x height)</td>
            <td>
                <asp:DropDownList ID="listVideoSize" runat="server">
                    <asp:ListItem>720x576</asp:ListItem>
                    <asp:ListItem>360x288</asp:ListItem>
                    <asp:ListItem>320x256</asp:ListItem>
                    <asp:ListItem>180x144</asp:ListItem>
                </asp:DropDownList>
            </td>
        </tr>
        <tr>
            <td>
                FTP concurrent jobs</td>
            <td>
                <asp:TextBox ID="textFtpJobs" runat="server"></asp:TextBox>
                <asp:RangeValidator ID="valFtpJobs" runat="server" 
                    ControlToValidate="textFtpJobs" ErrorMessage="Must be between 1 and 16." 
                    MaximumValue="16" MinimumValue="1" Type="Integer"></asp:RangeValidator>
            </td>
        </tr>
        <tr>
            <td>
                Segment length (seconds)</td>
            <td>
                <asp:TextBox ID="textSegmentLength" runat="server"></asp:TextBox>
                <asp:RangeValidator ID="valSegLen" runat="server" 
                    ControlToValidate="textSegmentLength" ErrorMessage="Must be between 1 and 30." 
                    MaximumValue="30" MinimumValue="1" Type="Integer"></asp:RangeValidator>
            </td>
        </tr>
        <tr>
            <td>
                Number of segments</td>
            <td>
                <asp:TextBox ID="textSegmentCount" runat="server"></asp:TextBox>
                <asp:RangeValidator ID="valSegCount" runat="server" 
                    ControlToValidate="textSegmentCount" 
                    ErrorMessage="Must be between 10 and 1000." MaximumValue="1000" 
                    MinimumValue="10" Type="Integer"></asp:RangeValidator>
            </td>
        </tr>
        <tr><td colspan="2"></td></tr>
        <tr><td>Stream to FTP?</td><td>
            <asp:CheckBox ID="cbStreamToFtp" runat="server" />
            </td></tr>
    </table>
    
    <p><asp:Label ID="lbSaved" runat="server" Text="Settings saved" Visible="false"></asp:Label><br />
    <asp:Button ID="butSave" runat="server" onclick="butSave_Click" 
    Text="Save settings" />
    </p>
<br />
</asp:Content>
