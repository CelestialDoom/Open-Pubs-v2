Module JSON
    'JSON
    Public Class Currency
        Public Property code As String
        Public Property name As String
        Public Property symbol As String
    End Class

    Public Class TimeZone
        Public Property name As String
        Public Property offset As Integer
        Public Property current_time As String
        Public Property current_time_unix As Double
        Public Property is_dst As Boolean
        Public Property dst_savings As Integer
    End Class

    Public Class IPGeo
        Public Property ip As String
        Public Property continent_code As String
        Public Property continent_name As String
        Public Property country_code2 As String
        Public Property country_code3 As String
        Public Property country_name As String
        Public Property country_capital As String
        Public Property state_prov As String
        Public Property district As String
        Public Property city As String
        Public Property zipcode As String
        Public Property latitude As String
        Public Property longitude As String
        Public Property is_eu As Boolean
        Public Property calling_code As String
        Public Property country_tld As String
        Public Property languages As String
        Public Property country_flag As String
        Public Property geoname_id As String
        Public Property isp As String
        Public Property connection_type As String
        Public Property organization As String
        Public Property currency As Currency
        Public Property time_zone As TimeZone
    End Class

    'json
End Module
