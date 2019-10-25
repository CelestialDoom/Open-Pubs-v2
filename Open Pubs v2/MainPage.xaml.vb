' The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409
Imports System.Net.Http
Imports System.Threading
Imports Csv
Imports MyIP
Imports Windows.Devices.Geolocation
Imports Windows.Phone.UI.Input
Imports Windows.Services.Maps
Imports Windows.Storage.Streams
Imports Windows.UI.Popups
Imports Windows.UI.Xaml.Controls.Maps

''' <summary>
''' An empty page that can be used on its own or navigated to within a Frame.
''' </summary>
Public NotInheritable Class MainPage
    Inherits Page

    Public ReadOnly Property IsDarkTheme As Boolean
        Get
            Return CBool(Application.Current.Resources("IsDarkTheme"))
        End Get
    End Property

    Async Sub BackPressed(sender As Object, e As BackPressedEventArgs)
        'Handles any Back button presses.
        e.Handled = True
        If _pivot.SelectedIndex <> 0 Then
            _pivot.SelectedIndex = 0
        Else
            Await displayMessageAsync(AppName, "Are you sure you want to quit?", "")
        End If
    End Sub

    Public Async Function displayMessageAsync(ByVal title As String, ByVal content As String, ByVal dialogType As String) As Task
        Dim messageDialog = New MessageDialog(content, title)
        If dialogType = "notification" Then
        Else
            messageDialog.Commands.Add(New UICommand("Yes", Nothing))
            messageDialog.Commands.Add(New UICommand("No", Nothing))
            messageDialog.DefaultCommandIndex = 0
        End If
        messageDialog.CancelCommandIndex = 1
        Dim cmdResult = Await messageDialog.ShowAsync()
        If cmdResult.Label = "Yes" Then
            Application.Current.Exit()
        End If
    End Function

    Private Sub MainPage_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        Dim platformFamily = Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily

        'This checks what sort of platform the app is running on
        If platformFamily = "Windows.Desktop" Then
        Else
            DisplayInformation.AutoRotationPreferences = DisplayOrientations.Portrait
            AddHandler HardwareButtons.BackPressed, AddressOf BackPressed
        End If

        If IsDarkTheme Then
            imgLogo.Source = CType(Resources("DarkMode"), BitmapImage)
        Else
            imgLogo.Source = CType(Resources("LightMode"), BitmapImage)
        End If

        _grid_info.Visibility = Visibility.Collapsed

        devnote.Visibility = Visibility.Collapsed

        _warning.Visibility = Visibility.Collapsed

        CloseExpanders(0)

        txtAboutText.Text = AboutApp(1)

        txtInfoText.Text = Licence(1)

        txtVersionApp.Text = "App Name: " & AppName & "     Version: " & appVersion

        Dim url As String = "https://docs.google.com/feeds/download/spreadsheets/Export?key=1HgYFPmbSWAbiMJ7NWUEk5pEuMJHgfF0ZV2_6N69mJEE&exportFormat=csv"
        Dim client As HttpClient = New HttpClient()
        Dim response As HttpResponseMessage = client.GetAsync(url).Result
        Dim content As HttpContent = response.Content
        Dim result As String = content.ReadAsStringAsync().Result

        Dim csv = result

        PubName.Clear()
        PubAddress.Clear()
        PubPostcode.Clear()
        PubLatitude.Clear()
        PubLongitude.Clear()
        PubLocalAuthority.Clear()

        For Each line In CsvReader.ReadFromText(csv)
            PubName.Add(line(0))
            PubAddress.Add(line(1))
            PubPostcode.Add(line(2))
            PubLatitude.Add(line(3))
            PubLongitude.Add(line(4))
            PubLocalAuthority.Add(line(5))
        Next

        'GetCity()
        GetLocation()

        cmbLocalAuthority.ItemsSource = LoadLocalAuthorityCmb()

    End Sub

    Private Async Sub ReverseGeocode(ByVal x As Double, ByVal y As Double)
        Dim location As BasicGeoposition = New BasicGeoposition()
        location.Latitude = x
        location.Longitude = y
        Dim pointToReverseGeocode As Geopoint = New Geopoint(location)
        Dim result As MapLocationFinderResult = Await MapLocationFinder.FindLocationsAtAsync(pointToReverseGeocode)

        If result.Status = MapLocationFinderStatus.Success Then
            CityName = result.Locations(0).Address.District
            btnLocation.Visibility = Visibility.Visible
            p_ring.IsActive = False
        End If
    End Sub

    Private Async Sub GetLocation()
        Try

            ' Create And Get Cancellation Token
            ctsCancel = New CancellationTokenSource()
            Dim canToken As CancellationToken = ctsCancel.Token

            ' Find Position
            Dim gpPos As Geoposition = Await glGeo.GetGeopositionAsync().AsTask(canToken)

            'Display Coordinates
            Latitude_Value = gpPos.Coordinate.Point.Position.Latitude
            Longitude_Value = gpPos.Coordinate.Point.Position.Longitude

            ReverseGeocode(Latitude_Value, Longitude_Value)

            'GetSunriseSunsetTimes(Latitude_Value.ToString, Longitude_Value.ToString)

            'Unauthorized
        Catch eu As System.UnauthorizedAccessException

            '_warning.Visibility = Visibility.Visible

            'Cancelled
        Catch et As TaskCanceledException

            'Any Other Error,  Such As Not Being Connected
        Catch err As Exception
        Finally

            'Clean Up
            ctsCancel = Nothing

        End Try
    End Sub

    Private Async Sub GetCity()

        Dim client = New MyIPClient()
        Dim client_response = Await client.GetAsync()

        Dim http = New HttpClient()
        Dim url = String.Format("https://api.ipgeolocation.io/ipgeo?apiKey=df376b0001cb449198b77d3a086fa648&ip=" & client_response.IPv4Address.ToString)
        Dim response = Await http.GetAsync(url)
        Dim result = Await response.Content.ReadAsStringAsync()
        Dim TestObject As IPGeo = Global.Newtonsoft.Json.JsonConvert.DeserializeObject(Of IPGeo)(result.ToString)
        CityName = TestObject.city
        Country = TestObject.country_code2

    End Sub

    Private Sub BtnLocation_Click(sender As Object, e As RoutedEventArgs) Handles btnLocation.Click
        Dim SearchStr As String

        txtSearchBox.Text = CityName

        SearchStr = ", " & txtSearchBox.Text.ToLower

        FoundCount = 0
        For a = 0 To PubName.Count - 1
            If PubAddress(a).ToLower.Contains(SearchStr) Then
                FoundCount += 1
            End If
        Next

        ReDim P_Name(FoundCount - 1)
        ReDim P_Address(FoundCount - 1)
        ReDim P_Postcode(FoundCount - 1)
        ReDim P_Latitude(FoundCount - 1)
        ReDim P_Longitude(FoundCount - 1)
        FoundCount = 0
        For a = 0 To PubName.Count - 1
            If PubAddress(a).ToLower.Contains(SearchStr) Then
                P_Name(FoundCount) = PubName(a)
                P_Address(FoundCount) = PubAddress(a)
                P_Postcode(FoundCount) = PubPostcode(a)
                P_Latitude(FoundCount) = PubLatitude(a)
                P_Longitude(FoundCount) = PubLongitude(a)
                FoundCount += 1
            End If
        Next
        lstPubs.ItemsSource = LoadLstView()

    End Sub

    Private Sub cmbLocalAuthority_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles cmbLocalAuthority.SelectionChanged
        Dim s_i As Integer
        s_i = cmbLocalAuthority.SelectedIndex
        If s_i > -1 Then
            txtSearchBox.Text = ""
            FoundCount = 0
            For a = 0 To PubName.Count - 1
                If PubLocalAuthority(a) = LocalAuthority(s_i) Then
                    FoundCount += 1
                End If
            Next
            ReDim P_Name(FoundCount - 1)
            ReDim P_Address(FoundCount - 1)
            ReDim P_Postcode(FoundCount - 1)
            ReDim P_Latitude(FoundCount - 1)
            ReDim P_Longitude(FoundCount - 1)
            FoundCount = 0
            For a = 0 To PubName.Count - 1
                If PubLocalAuthority(a) = LocalAuthority(s_i) Then
                    P_Name(FoundCount) = PubName(a)
                    P_Address(FoundCount) = PubAddress(a)
                    P_Postcode(FoundCount) = PubPostcode(a)
                    P_Latitude(FoundCount) = PubLatitude(a)
                    P_Longitude(FoundCount) = PubLongitude(a)
                    FoundCount += 1
                End If
            Next
            lstPubs.ItemsSource = LoadLstView()
        End If
    End Sub

    Async Sub map(ByVal lat As Double, ByVal lon As Double, ByVal pubname As String)
        MyMap.Style = MapStyle.Terrain
        MyMap.MapServiceToken = "3yqdYaTlL5TmJZyd5wXg~x_rpxxJjlMrfyPIg4wTj-g~Av41X-LDybWRQ90ZhZA7fteGzOSOhbEjTb1KFAXcKmyN7zx8eLFhyN8ageSvole9"
        Dim geoPosition As BasicGeoposition = New BasicGeoposition()
        geoPosition.Latitude = lat
        geoPosition.Longitude = lon
        Dim myPoint As Geopoint = New Geopoint(geoPosition)
        Dim myPOI As MapIcon = New MapIcon With {.Location = myPoint, .Title = pubname, .NormalizedAnchorPoint = New Point(0.5, 1.0), .ZIndex = 0}
        myPOI.Image = RandomAccessStreamReference.CreateFromUri(New Uri("ms-appx:///pint_glass_64px.png"))
        MyMap.MapElements.Add(myPOI)
        MyMap.Center = myPoint
        MyMap.ZoomLevel = 10
        Dim mapScene As MapScene = MapScene.CreateFromLocationAndRadius(New Geopoint(geoPosition), 100, 0, 45)
        Await MyMap.TrySetSceneAsync(mapScene)
    End Sub

    Private Sub lstPubs_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles lstPubs.SelectionChanged
        Dim s_i As Integer
        s_i = lstPubs.SelectedIndex
        If s_i > -1 Then
            txtPubName.Text = P_Name(s_i)
            txtPubAddress.Text = P_Address(s_i)
            Dim LatitudeToDouble = Convert.ToDouble(P_Latitude(s_i))
            Dim LongitudeToDouble = Convert.ToDouble(P_Longitude(s_i))
            map(LatitudeToDouble, LongitudeToDouble, P_Name(s_i))
            _pivot.SelectedIndex = 1
            _info.SelectedIndex = 0
            _grid_info.Visibility = Visibility.Visible
            GetNearBy(LatitudeToDouble, LongitudeToDouble)
        End If
    End Sub

    Private Async Sub GetNearBy(ByVal latitude As Double, ByVal longitude As Double)
        Dim TotalStops As Integer
        Dim http = New HttpClient()
        Dim url = String.Format("https://transportapi.com/v3/uk/places.json?app_id=76b64c09&app_key=ede8b69b882c7db5c2da9ef43f2138e8&lat=" & latitude & "&lon=" & longitude & "&type=bus_stop")
        Dim response = Await http.GetAsync(url)
        Dim result = Await response.Content.ReadAsStringAsync()
        Dim TestObject As NearBy = Global.Newtonsoft.Json.JsonConvert.DeserializeObject(Of NearBy)(result.ToString)
        TotalStops = TestObject.member.Length - 1
        ReDim BusStops(TotalStops)
        For a = 0 To TotalStops
            BusStops(a) = TestObject.member.ElementAt(a).name
        Next
        lstStops.ItemsSource = LoadBusStops()
    End Sub

    Private Sub BtnMenu_Click(sender As Object, e As RoutedEventArgs) Handles btnMenu.Click
        SplitMenu.IsPaneOpen = Not SplitMenu.IsPaneOpen
    End Sub

    Private Sub BtnSearch_Click(sender As Object, e As RoutedEventArgs) Handles btnSearch.Click
        Dim SearchStr As String
        SearchStr = ", " & txtSearchBox.Text.ToLower

        FoundCount = 0
        For a = 0 To PubName.Count - 1
            If PubAddress(a).ToLower.Contains(SearchStr) Then
                FoundCount += 1
            End If
        Next

        ReDim P_Name(FoundCount - 1)
        ReDim P_Address(FoundCount - 1)
        ReDim P_Postcode(FoundCount - 1)
        ReDim P_Latitude(FoundCount - 1)
        ReDim P_Longitude(FoundCount - 1)
        FoundCount = 0
        For a = 0 To PubName.Count - 1
            If PubAddress(a).ToLower.Contains(SearchStr) Then
                P_Name(FoundCount) = PubName(a)
                P_Address(FoundCount) = PubAddress(a)
                P_Postcode(FoundCount) = PubPostcode(a)
                P_Latitude(FoundCount) = PubLatitude(a)
                P_Longitude(FoundCount) = PubLongitude(a)
                FoundCount += 1
            End If
        Next
        lstPubs.ItemsSource = LoadLstView()
    End Sub

    Private Sub FindThem(ByVal x As String)
        Dim SearchStr As String
        SearchStr = ", " & x.ToLower

        FoundCount = 0
        For a = 0 To PubName.Count - 1
            If PubAddress(a).ToLower.Contains(SearchStr) Then
                FoundCount += 1
            End If
        Next

        ReDim P_Name(FoundCount - 1)
        ReDim P_Address(FoundCount - 1)
        ReDim P_Postcode(FoundCount - 1)
        ReDim P_Latitude(FoundCount - 1)
        ReDim P_Longitude(FoundCount - 1)
        FoundCount = 0
        For a = 0 To PubName.Count - 1
            If PubAddress(a).ToLower.Contains(SearchStr) Then
                P_Name(FoundCount) = PubName(a)
                P_Address(FoundCount) = PubAddress(a)
                P_Postcode(FoundCount) = PubPostcode(a)
                P_Latitude(FoundCount) = PubLatitude(a)
                P_Longitude(FoundCount) = PubLongitude(a)
                FoundCount += 1
            End If
        Next
        lstPubs.ItemsSource = LoadLstView()
    End Sub

    Private Sub CloseExpanders(ByVal x As Integer)
        Select Case x
            Case 0
                _otherstuff.Visibility = Visibility.Collapsed
            Case 1
                _otherstuff.Visibility = Visibility.Visible
        End Select
    End Sub

    Private Sub btnExpandOther_Click(sender As Object, e As RoutedEventArgs) Handles btnExpandOther.Click
        If _otherstuff.Visibility = Visibility.Collapsed Then
            CloseExpanders(1)
        Else
            CloseExpanders(0)
        End If
    End Sub

    Private Sub BtnAboutMenu_Click(sender As Object, e As RoutedEventArgs) Handles btnAboutMenu.Click
        CloseExpanders(0)
        SplitMenu.IsPaneOpen = Not SplitMenu.IsPaneOpen
        myScrollView_1.ChangeView(Nothing, 0, Nothing, True)
        _pivot.SelectedIndex = 2
    End Sub

    Private Sub BtnLibraryMenu_Click(sender As Object, e As RoutedEventArgs) Handles btnLibraryMenu.Click
        CloseExpanders(0)
        SplitMenu.IsPaneOpen = Not SplitMenu.IsPaneOpen
        myScrollView_2.ChangeView(Nothing, 0, Nothing, True)
        _pivot.SelectedIndex = 3
    End Sub

    Private Sub BtnDevMenu_Click(sender As Object, e As RoutedEventArgs) Handles btnDevMenu.Click
        CloseExpanders(0)
        SplitMenu.IsPaneOpen = Not SplitMenu.IsPaneOpen
        myScrollView_3.ChangeView(Nothing, 0, Nothing, True)
        devnote.Visibility = Visibility.Visible
        txtDevText.Text = "Initial release." & vbCrLf & "App uses Google Spreadsheet as a database. I will (hopefully at sometime) change it to a MySQL database."
    End Sub

    Private Sub btnCloseDev_Click(sender As Object, e As RoutedEventArgs) Handles btnCloseDev.Click
        devnote.Visibility = Visibility.Collapsed
    End Sub

    Private Sub BtnQuitApp_Click(sender As Object, e As RoutedEventArgs) Handles btnQuitApp.Click
        Application.Current.Exit()
    End Sub

    Private Sub _pivot_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles _pivot.SelectionChanged
        myScrollView_1.ChangeView(Nothing, 0, Nothing, True)
        myScrollView_2.ChangeView(Nothing, 0, Nothing, True)
        myScrollView_3.ChangeView(Nothing, 0, Nothing, True)
        txtSearchBox.IsEnabled = True
    End Sub

    Private Sub txtSearchBox_KeyDown(sender As Object, e As KeyRoutedEventArgs) Handles txtSearchBox.KeyDown

        If e.Key = Windows.System.VirtualKey.Enter Then
            Dim SearchStr As String
            SearchStr = ", " & txtSearchBox.Text.ToLower
            txtSearchBox.IsEnabled = False
            FoundCount = 0
            For a = 0 To PubName.Count - 1
                If PubAddress(a).ToLower.Contains(SearchStr) Then
                    FoundCount += 1
                End If
            Next

            ReDim P_Name(FoundCount - 1)
            ReDim P_Address(FoundCount - 1)
            ReDim P_Postcode(FoundCount - 1)
            ReDim P_Latitude(FoundCount - 1)
            ReDim P_Longitude(FoundCount - 1)
            FoundCount = 0
            For a = 0 To PubName.Count - 1
                If PubAddress(a).ToLower.Contains(SearchStr) Then
                    P_Name(FoundCount) = PubName(a)
                    P_Address(FoundCount) = PubAddress(a)
                    P_Postcode(FoundCount) = PubPostcode(a)
                    P_Latitude(FoundCount) = PubLatitude(a)
                    P_Longitude(FoundCount) = PubLongitude(a)
                    FoundCount += 1
                End If
            Next
            lstPubs.ItemsSource = LoadLstView()
        End If
    End Sub

    Private Sub txtSearchBox_LostFocus(sender As Object, e As RoutedEventArgs) Handles txtSearchBox.LostFocus
        Dim currentInputPane As InputPane = InputPane.GetForCurrentView()
        currentInputPane.Visible = False
    End Sub

    Private Sub btnCloseWarning_Click(sender As Object, e As RoutedEventArgs) Handles btnCloseWarning.Click
        _warning.Visibility = Visibility.Collapsed
    End Sub

End Class