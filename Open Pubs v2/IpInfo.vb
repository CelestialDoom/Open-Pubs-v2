Imports Newtonsoft.Json

Public Class IpInfo
    <JsonProperty("ip")>
    Public Property Ip As String
    <JsonProperty("hostname")>
    Public Property Hostname As String
    <JsonProperty("city")>
    Public Property City As String
    <JsonProperty("region")>
    Public Property Region As String
    <JsonProperty("country")>
    Public Property Country As String
    <JsonProperty("loc")>
    Public Property Loc As String
    <JsonProperty("org")>
    Public Property Org As String
    <JsonProperty("postal")>
    Public Property Postal As String
End Class
