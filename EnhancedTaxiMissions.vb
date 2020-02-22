Imports GTA
Imports GTA.Math
Imports System
Imports System.IO
Imports System.IO.File
Imports System.Text
Imports System.Drawing
Imports System.Windows.Forms
Imports System.Threading.Tasks
Imports System.Collections.Generic


Public Class EnhancedTaxiMissions
    Inherits Script
    Public RND As New Random

    Public MissionFlag As Boolean = False

    Public TestBlip(500) As Blip

    Public ShowDebugInfo As Boolean = False
    Public ToggleKey As Keys = Keys.L
    Public UnitsInKM As Boolean = True
    Public DoAutosave As Boolean = False
    Public UseHazardLights As Boolean = True
    Public HeaderTaxi As String = "Downtown Cab Driver"
    Public HeaderLimo As String = "Downtown Limo Driver"
    Public HeaderUber As String = "DowntownX Driver"

    Public IsMinigameActive As Boolean = False
    Public MiniGameStage As MiniGameStages = MiniGameStages.Standby

    Public MaxSeats As Integer = 0

    Public LastTimePlayerWasInCar As Integer = 0
    Public TimePlayerHasBeenOutOfCar As Integer = 0

    Public ScriptStartTime As Integer = 0
    Public AreSettingsLoaded As Boolean = False

    Public IsSpecialMission As Boolean = False

    Public Origin, Destination As Location
    Public PotentialOrigins, PotentialDestinations As New List(Of Location)

    Public OriginBlip, DestinationBlip As Blip
    Public OriginMarker, DestinationMarker As Integer

    Public PedsAroundOrigin, PedsAroundDestination As Ped()

    Public Customer As Person
    Public CustomerPed As Ped
    Public IsThereASecondCustomer As Boolean = False
    Public IsThereAThirdCustomer As Boolean = False
    Public Customer2Ped As Ped
    Public Customer3Ped As Ped
    Public Ped1Blip, Ped2Blip, Ped3Blip As Blip

    Public ClosestPedDistance As Single

    Public MissionStartTime As Integer = 0
    Public OriginArrivalTime As Integer = 0
    Public PickupTime As Integer = 0
    Public DestinationArrivalTime As Integer = 0

    Public VehicleStartHealth As Integer = 0
    Public VehicleStartHealthPercent As Single = 0
    Public VehicleEndHealth As Integer = 0
    Public VehicleEndHealthPercent As Single = 0

    Public IdealTripTime As Integer = 0
    Public IdealArrivalTime As Integer = 0
    Public ArrivalWindowStart As Integer = 0
    Public ArrivalWindowEnd As Integer = 0
    Public AverageSpeed As Integer = 65

    Public HasCustomerPedHailedPlayer As Boolean = False
    Public IsCustomerPedSpawned As Boolean = False
    Public IsDestinationCleared As Boolean = False
    Public IsCustomerNudged1 As Boolean = False
    Public IsCustomerNudged2 As Boolean = False
    Public NudgeResetTime As Integer = 0

    Const FareBase As Integer = 6     'In Los Angeles: $2.90 base fare
    Public FarePerMile As Single = 20 'In Los Angeles: $0.30/mile
    Public FareDistance As Single = 0
    Public FareTotal As Integer = 0
    Public FareTip As Integer = 0
    Public FareTipPercent As Single = 0
    Public MaximumTip As Single = 0.4

    Public IngameMinute As Integer = 0
    Public IngameHour As Integer = 0
    Public IngameDay As Integer = 0

    Public NextMissionStartTime As Integer = 0

    Public UI As New UIContainer(New Point(30, 40), New Size(240, 80), Color.FromArgb(0, 0, 0, 0))
    Public UI_DispatchStatus As String = "DISPATCH-TEXT-INIT"
    Public UI_Origin As String = "ORIG-INIT"
    Public UI_Destination As String = "DEST-INIT"
    Public UI_Dist1 As String = "999"
    Public UI_Dist2 As String = "999"

    Public UIcolor_Header As Color = Color.FromArgb(140, 60, 140, 230)
    Public UIcolor_Status As Color = Color.FromArgb(140, 110, 190, 240)
    Public UIcolor_BG As Color = Color.FromArgb(160, 0, 0, 0)
    Public UItext_White As Color = Color.White
    Public UItext_Dark As Color = Color.FromArgb(250, 120, 120, 120)

    Public UI_Debug As New UIContainer(New Point(30, 140), New Size(190, 190), Color.FromArgb(0, 0, 0, 0))

    Public UpdateDist1 As Boolean = False
    Public UpdateDist2 As Boolean = False

    Public Enum MiniGameStages
        Standby                 '0
        DrivingToOrigin         '1
        StoppingAtOrigin        '2
        PedWalkingToCar         '3
        PedGettingInCar         '4
        DrivingToDestination    '5
        StoppingAtDestination   '6
        PedGettingOut           '7
        PedWalkingAway          '8
        SearchingForFare        '9
        RoamingForFare          '10
        BrokenVehicle           '11
        NonPayingCustomer       '12
    End Enum

    Public Sub PRINT(msg As String)
        If ShowDebugInfo = True Then
            GTA.UI.Notify(World.CurrentDayTime.Hours.ToString("D2") & ":" & World.CurrentDayTime.Minutes.ToString("D2") & ": " & msg)
        End If
    End Sub

    ' Check if cars is ok
    Public Function IsCarOK(maxSeats As Short, vehicleClass As VehicleClass)
        If IsThereASecondCustomer And maxSeats < 2 Then
            Return False
        End If

        If IsThereAThirdCustomer And maxSeats < 3 Then
            Return False
        End If

        Return maxSeats >= 1 And
        vehicleClass <> VehicleClass.Motorcycles And
        vehicleClass <> VehicleClass.Boats And
        vehicleClass <> VehicleClass.Cycles And
        vehicleClass <> VehicleClass.Helicopters And
        vehicleClass <> VehicleClass.Planes And
        vehicleClass <> VehicleClass.Trains
    End Function


    ' ============================ INITIALIZATION ===================================
    Public Sub New()
        InitPlaceLists()
        AreSettingsLoaded = False
        ListOfPeople.Remove(NonCeleb)
    End Sub

    Public Sub LoadSettings()
        Dim value As String = Settings.GetValue("SETTINGS", "TOGGLE", Keys.L)
        Select Case value
            Case "A"
                ToggleKey = Keys.A
            Case "B"
                ToggleKey = Keys.B
            Case "C"
                ToggleKey = Keys.C
            Case "D"
                ToggleKey = Keys.D
            Case "E"
                ToggleKey = Keys.E
            Case "F"
                ToggleKey = Keys.F
            Case "G"
                ToggleKey = Keys.G
            Case "H"
                ToggleKey = Keys.H
            Case "I"
                ToggleKey = Keys.I
            Case "J"
                ToggleKey = Keys.J
            Case "K"
                ToggleKey = Keys.K
            Case "L"
                ToggleKey = Keys.L
            Case "M"
                ToggleKey = Keys.M
            Case "N"
                ToggleKey = Keys.N
            Case "O"
                ToggleKey = Keys.O
            Case "P"
                ToggleKey = Keys.P
            Case "Q"
                ToggleKey = Keys.Q
            Case "R"
                ToggleKey = Keys.R
            Case "S"
                ToggleKey = Keys.S
            Case "T"
                ToggleKey = Keys.T
            Case "U"
                ToggleKey = Keys.U
            Case "V"
                ToggleKey = Keys.V
            Case "W"
                ToggleKey = Keys.W
            Case "X"
                ToggleKey = Keys.X
            Case "Y"
                ToggleKey = Keys.Y
            Case "Z"
                ToggleKey = Keys.Z
            Case "F1"
                ToggleKey = Keys.F1
            Case "F2"
                ToggleKey = Keys.F2
            Case "F3"
                ToggleKey = Keys.F3
            Case "F4"
                ToggleKey = Keys.F4
            Case "F5"
                ToggleKey = Keys.F5
            Case "F6"
                ToggleKey = Keys.F6
            Case "F7"
                ToggleKey = Keys.F7
            Case "F8"
                ToggleKey = Keys.F8
            Case "F9"
                ToggleKey = Keys.F9
            Case "F10"
                ToggleKey = Keys.F10
            Case "F11"
                ToggleKey = Keys.F11
            Case "F12"
                ToggleKey = Keys.F12
            Case "1"
                ToggleKey = Keys.D1
            Case "2"
                ToggleKey = Keys.D2
            Case "3"
                ToggleKey = Keys.D3
            Case "4"
                ToggleKey = Keys.D4
            Case "5"
                ToggleKey = Keys.D5
            Case "6"
                ToggleKey = Keys.D6
            Case "7"
                ToggleKey = Keys.D7
            Case "8"
                ToggleKey = Keys.D8
            Case "9"
                ToggleKey = Keys.D9
            Case "0"
                ToggleKey = Keys.D0
            Case Else
                ToggleKey = Keys.L
        End Select


        value = Settings.GetValue("SETTINGS", "UNITS", "KM")
        Select Case value
            Case "KM"
                UnitsInKM = True
            Case "MI"
                UnitsInKM = False
            Case Else
                UnitsInKM = True
        End Select

        value = Settings.GetValue("SETTINGS", "FAREPERMILE", 20)
        FarePerMile = CInt(value)

        value = Settings.GetValue("SETTINGS", "AVERAGESPEED", 35)
        If value = 0 Then value = 1
        AverageSpeed = CInt(value)

        value = Settings.GetValue("DEBUG", "SHOW", 0)
        If value = 1 Then
            ShowDebugInfo = True
        Else
            ShowDebugInfo = False
        End If

        value = Settings.GetValue("SETTINGS", "HEADERTAXI", "Downtown Cab Driver")
        If value <> "" Then
            HeaderTaxi = value
        End If

        value = Settings.GetValue("SETTINGS", "HEADERLIMO", "Downtown Limo Driver")
        If value <> "" Then
            HeaderLimo = value
        End If

        value = Settings.GetValue("SETTINGS", "HEADERUBER", "DowntownX Driver")
        If value <> "" Then
            HeaderUber = value
        End If

        value = Settings.GetValue("SETTINGS", "HAZARDLIGHTS", 1)
        If value = 1 Then
            UseHazardLights = True
        Else
            UseHazardLights = 0
        End If

        AreSettingsLoaded = True

    End Sub

    ' ============================ USER INTERFACE ===================================

    Public Sub RefreshUI()
        UI.Items.Clear()

        '========== TITLE
        UI.Items.Add(New UIRectangle(New Point(0, 0), New Size(240, 25), UIcolor_Header))

        Dim headerText As String
        If Game.Player.Character.IsInVehicle = True Then
            If Game.Player.Character.CurrentVehicle.DisplayName = "TAXI" Then
                headerText = HeaderTaxi
            ElseIf Game.Player.Character.CurrentVehicle.DisplayName = "STRETCH" Or
                   Game.Player.Character.CurrentVehicle.DisplayName = "LIMO2" Then
                headerText = HeaderLimo
            Else
                headerText = HeaderUber
            End If
        Else
            headerText = "Enhanced Taxi Missions"
        End If
        UI.Items.Add(New UIText(headerText, New Point(3, 1), 0.5, UItext_White, GTA.Font.HouseScript, False))


        '========== COUNTDOWN TIMER / CLOCK
        If MiniGameStage = MiniGameStages.DrivingToDestination Then
            Dim remainder As Integer = ArrivalWindowEnd - Game.GameTime
            If remainder <= 0 Then
                UI.Items.Add(New UIText(IngameHour.ToString("D2") & ":" & IngameMinute.ToString("D2"), New Point(208, 0), 0.5, UItext_White, 4, False))
            Else
                Dim s As Integer
                Dim col As Color
                s = CInt(remainder / 1000)
                If s < CInt((ArrivalWindowEnd - ArrivalWindowStart) / 1000) Then
                    col = Color.Yellow
                ElseIf s < 10 Then
                    col = Color.Red
                Else
                    col = Color.Green
                End If
                UI.Items.Add(New UIText(s.ToString, New Point(224, 0), 0.5, col, 4, True))
            End If
        Else
            UI.Items.Add(New UIText(IngameHour.ToString("D2") & ":" & IngameMinute.ToString("D2"), New Point(203, 0), 0.5, UItext_White, 4, False))
        End If


        '========== DISPATCH STATUS
        Dim statusMessageModification As String
        If Game.Player.Character.IsInVehicle = True Then
            If IsCarOK(MaxSeats, Game.Player.Character.CurrentVehicle.ClassType) Then
                UI.Items.Add(New UIRectangle(New Point(0, 27), New Size(240, 20), UIcolor_Status))
                statusMessageModification = UI_DispatchStatus
            Else
                UI.Items.Add(New UIRectangle(New Point(0, 27), New Size(240, 20), Color.IndianRed))
                statusMessageModification = "You cannot use this vehicle!"
            End If
        Else
            UI.Items.Add(New UIRectangle(New Point(0, 27), New Size(240, 20), Color.IndianRed))
            statusMessageModification = "Please return to your vehicle!"
        End If
        UI.Items.Add(New UIText(statusMessageModification, New Point(3, 28), 0.35F, UItext_White, 4, False))



        '========== ORIGIN/DESTINATION INFORMATION
        UI.Items.Add(New UIRectangle(New Point(0, 47), New Size(240, 40), UIcolor_BG))

        If MiniGameStage = MiniGameStages.DrivingToOrigin Or MiniGameStage = MiniGameStages.StoppingAtOrigin Then
            UI.Items.Add(New UIText(UI_Origin & UI_Dist1, New Point(3, 48), 0.35F, UItext_White, 4, False))
        Else
            UI.Items.Add(New UIText(UI_Origin & UI_Dist1, New Point(3, 48), 0.35F, UItext_Dark, 4, False))
        End If

        If MiniGameStage = MiniGameStages.DrivingToDestination Or MiniGameStage = MiniGameStages.StoppingAtDestination Then
            UI.Items.Add(New UIText(UI_Destination & UI_Dist2, New Point(3, 68), 0.35F, UItext_White, 4, False))
        Else
            UI.Items.Add(New UIText(UI_Destination & UI_Dist2, New Point(3, 68), 0.35F, UItext_Dark, 4, False))
        End If

        UI.Draw()


        '========== ORIGIN/DESTINATION MISSION MARKERS WITH FADE-OUT EFFECT UPON APPROACH
        If MiniGameStage = MiniGameStages.DrivingToOrigin Or MiniGameStage = MiniGameStages.StoppingAtOrigin Then

            Dim distTo = World.GetDistance(Game.Player.Character.Position, Origin.Coords)
            Dim alpha As Integer

            If distTo > 10 Then
                alpha = 180
            Else
                alpha = CInt(((distTo - 5) / 5) * 180)
                If alpha < 0 Then alpha = 0
            End If

            Dim markerGroundHeight = World.GetGroundHeight(Origin.Coords) + 0.5
            Dim markerCoordinates As Vector3 = New Vector3(Origin.Coords.X, Origin.Coords.Y, markerGroundHeight)

            World.DrawMarker(MarkerType.ChevronUpx1, markerCoordinates, New Vector3(0, 0, 0), New Vector3(0, 180, 0), New Vector3(1, 1, 1), Color.FromArgb(alpha, Color.CadetBlue), True, True, 0, False, "", "", False)
        End If

        If MiniGameStage = MiniGameStages.DrivingToDestination Or MiniGameStage = MiniGameStages.StoppingAtDestination Then

            Dim distTo = World.GetDistance(Game.Player.Character.Position, Destination.Coords)
            Dim alpha As Integer

            If distTo > 10 Then
                alpha = 180
            Else
                alpha = CInt(((distTo - 5) / 5) * 180)
                If alpha < 0 Then alpha = 0
            End If

            Dim markerGroundHeight = World.GetGroundHeight(Destination.Coords) + 0.5
            Dim markerCoordinates As Vector3 = New Vector3(Destination.Coords.X, Destination.Coords.Y, markerGroundHeight)

            World.DrawMarker(MarkerType.ChevronUpx1, markerCoordinates, New Vector3(0, 0, 0), New Vector3(0, 180, 0), New Vector3(1, 1, 1), Color.FromArgb(alpha, Color.CadetBlue), True, True, 0, False, "", "", False)
        End If





        ' ================== DEBUG ==================

        If ShowDebugInfo = True Then

            Dim DebugTextFields As New List(Of String) From {
                "-- DEBUG INFORMATION --",
                "Game Stage: " & MiniGameStage.ToString,
                "Current Day Time: " & World.CurrentDayTime.TotalSeconds & " (" & Math.Round(100 * (World.CurrentDayTime.TotalSeconds / 86400)) & "%)",
                "GameTime: " & (Game.GameTime / 1000).ToString("#,###,###.###") & " seconds"
            }

            If MiniGameStage <> MiniGameStages.RoamingForFare Or MiniGameStage <> MiniGameStages.SearchingForFare Or MiniGameStage <> MiniGameStages.Standby Then
                Dim numOfCustomers = 1
                If IsThereAThirdCustomer = True Then
                    numOfCustomers = 3
                ElseIf IsThereASecondCustomer = True Then
                    numOfCustomers = 2
                End If
                DebugTextFields.Add("No. of Customers: " & numOfCustomers)

                If CustomerPed IsNot Nothing Then
                    If CustomerPed.Exists Then
                        If CustomerPed.IsAlive Then
                            DebugTextFields.Add("Customer 1: Alive")
                        Else
                            DebugTextFields.Add("Customer 1: DEAD")
                        End If
                    End If
                Else
                    DebugTextFields.Add("Customer 1: Not Yet Spawned")
                End If

                If IsThereASecondCustomer Then
                    If Customer2Ped IsNot Nothing Then
                        If Customer2Ped.Exists Then
                            If Customer2Ped.IsAlive Then
                                DebugTextFields.Add("Customer 2: Alive")
                            Else
                                DebugTextFields.Add("Customer 2: DEAD")
                            End If
                        End If
                    Else
                        DebugTextFields.Add("Customer 2: Not Yet Spawned")
                    End If
                End If

                If IsThereAThirdCustomer Then
                    If Customer3Ped IsNot Nothing Then
                        If Customer3Ped.Exists Then
                            If Customer3Ped.IsAlive Then
                                DebugTextFields.Add("Customer 3: Alive")
                            Else
                                DebugTextFields.Add("Customer 3: DEAD")
                            End If
                        End If
                    Else
                        DebugTextFields.Add("Customer 3: Not Yet Spawned")
                    End If
                End If
            End If

            If MiniGameStage = MiniGameStages.PedWalkingToCar Or MiniGameStage = MiniGameStages.PedGettingInCar Then
                DebugTextFields.Add("Closest Customer Distance: " & Math.Round(ClosestPedDistance, 1) & " m")
            End If

            DebugTextFields.Add("Mission start time: " & Math.Round(MissionStartTime / 1000).ToString("#,###,###,###") & " s")
            DebugTextFields.Add("Origin Arrival Time: " & Math.Round(OriginArrivalTime / 1000).ToString("#,###,###,###") & " s")
            DebugTextFields.Add("Pickup Time: " & Math.Round(PickupTime / 1000).ToString("#,###,###,###") & " s")
            DebugTextFields.Add("Destination Arrival Time: " & Math.Round(DestinationArrivalTime / 1000).ToString("#,###,###,###") & " s")

            DebugTextFields.Add("Car Starting Health: " & VehicleStartHealth & " (" & Math.Round(VehicleStartHealthPercent * 100, 1) & "%)")

            If Game.Player.Character.IsInVehicle Then
                Dim vehHealth = Game.Player.Character.CurrentVehicle.Health
                Dim vehHealthPercent = Math.Round(100 * (vehHealth / Game.Player.Character.CurrentVehicle.MaxHealth), 1)
                DebugTextFields.Add("Car Current Health: " & vehHealth & " (" & vehHealthPercent & "%)")

                If MiniGameStage = MiniGameStages.DrivingToDestination Then
                    Dim damageAmount = VehicleStartHealth - vehHealth
                    DebugTextFields.Add("Car Damage Incurred: " & damageAmount)
                End If

                Dim vehDirt = Math.Round(Game.Player.Character.CurrentVehicle.DirtLevel, 4)
                DebugTextFields.Add("Dirtiness: " & vehDirt)

                Dim vehSpeed = Game.Player.Character.CurrentVehicle.Speed
                DebugTextFields.Add("Speed: " & Math.Round(vehSpeed, 1) & " m/s, " & Math.Round(vehSpeed * 3.6) & " kph")

                Dim vehAcc = Game.Player.Character.CurrentVehicle.Acceleration
                DebugTextFields.Add("Acceleration: " & vehAcc)
            End If

            If MiniGameStage = MiniGameStages.DrivingToDestination Then
                DebugTextFields.Add("Fare Distance: " & (FareDistance * 1000).ToString("#,###") & " meters")
            End If

            If Origin IsNot Nothing Then
                DebugTextFields.Add("Distance To Origin " & Math.Round(World.GetDistance(Game.Player.Character.Position, Origin.Coords), 1))
            End If

            If Destination IsNot Nothing Then
                DebugTextFields.Add("Distance To Destination " & Math.Round(World.GetDistance(Game.Player.Character.Position, Destination.Coords), 1))
            End If

            DebugTextFields.Add("Last time player was in car: " & Math.Round(LastTimePlayerWasInCar / 1000).ToString("#,###,###"))
            If TimePlayerHasBeenOutOfCar > 1000 Then
                DebugTextFields.Add("Time out of car: " & Math.Round(TimePlayerHasBeenOutOfCar / 1000).ToString("#,###,###"))
            End If


            UI_Debug.Items.Clear()
            UI_Debug.Items.Add(New UIRectangle(New Point(0, 0), New Size(190, (DebugTextFields.Count * 11) + 3), UIcolor_BG))

            For Each textField As String In DebugTextFields
                Dim index As Integer = DebugTextFields.IndexOf(textField)
                UI_Debug.Items.Add(New UIText(textField, New Point(3, index * 11), 0.21, UItext_White, 0, False))
            Next

            UI_Debug.Draw()

        End If
    End Sub



    ' ============================= UPDATE LOOPS ====================================
    Public Sub Update(ByVal sender As Object, ByVal e As EventArgs) Handles MyBase.Tick

        If AreSettingsLoaded = False Then LoadSettings()

        UI.Enabled = IsMinigameActive
        UI_Debug.Enabled = IsMinigameActive

        MissionFlag = Native.Function.Call(Of Boolean)(Native.Hash.GET_MISSION_FLAG)

        IngameHour = World.CurrentDayTime.Hours
        IngameMinute = World.CurrentDayTime.Minutes
        IngameDay = Native.Function.Call(Of Integer)(Native.Hash.GET_CLOCK_DAY_OF_WEEK)

        If IsMinigameActive = True Then

            'updateUIDistances()
            UpdateTaxiLight()

            If Game.Player.WantedLevel > 0 Then
                EndMinigame(True)
                Exit Sub
            End If

            If Game.Player.IsDead Then
                EndMinigame(True)
                Exit Sub
            End If


            '===== CHECK IF THE PLAYER HAS LEFT THE CAR FOR TOO LONG
            If Game.Player.Character.IsInVehicle = False Then
                TimePlayerHasBeenOutOfCar = Game.GameTime - LastTimePlayerWasInCar
            Else
                MaxSeats = Native.Function.Call(Of Integer)(Native.Hash.GET_VEHICLE_MAX_NUMBER_OF_PASSENGERS, Game.Player.Character.CurrentVehicle)
                If IsCarOK(MaxSeats, Game.Player.Character.CurrentVehicle.ClassType) Then
                    LastTimePlayerWasInCar = Game.GameTime
                End If
                TimePlayerHasBeenOutOfCar = Game.GameTime - LastTimePlayerWasInCar
            End If

            If TimePlayerHasBeenOutOfCar > 30 * 1000 Then

                Dim paxCount As String
                If IsThereASecondCustomer = True Then
                    paxCount = "passengers"
                Else
                    paxCount = "passenger"
                End If

                GTA.UI.ShowSubtitle("~r~Your " & paxCount & " decided to look for another ride.", 6000)
                EndMinigame()
                Exit Sub
            End If


            '===== CHECK IF THE CAR IS TOO DAMAGED TO CONTINUE
            If Game.Player.Character.IsInVehicle = True Then
                If MiniGameStage = MiniGameStages.DrivingToDestination Then

                    Dim curHealth = Game.Player.Character.CurrentVehicle.Health / Game.Player.Character.CurrentVehicle.MaxHealth
                    If curHealth < 0.7 Then
                        EndMinigame(True)
                        Exit Sub
                    End If
                End If
            End If



            ' ===== CHECK IF THE CUSTOMERS ARE PANICKING =====
            If MiniGameStage = MiniGameStages.StoppingAtOrigin Or MiniGameStage = MiniGameStages.PedWalkingToCar Then
                If CustomerPed IsNot Nothing Then
                    If CustomerPed.Exists Then
                        If CustomerPed.IsFleeing Then
                            GTA.UI.Notify("Your customer seems to be fleeing...")
                            EndMinigame()
                        End If
                    End If
                End If
            End If


            CheckIfPedsAreAlive()


            '===== CHECK IF THE CONDITIONS FOR SPECIFIC MISSION STAGES HAVE BEEN REACHED
            CheckIfPlayerIsRoaming()
            CheckIfItsTimeToStartANewMission()
            CheckIfCloseEnoughToHail()
            CheckIfPlayerHasAbandonedHail()
            CheckIfCloseEnoughToSpawnPed()
            CheckIfPlayerHasArrivedAtOrigin()
            CheckIfPlayerHasStoppedAtOrigin()
            CheckVehicleBoardingCondition()

            ResetNudgeFlags()
            CheckIfPassengerNeedsToBeNudged()

            CheckIfPedHasReachedCar()
            CheckIfPedHasEnteredCar()
            CheckIfCloseEnoughToClearDestination()
            CheckIfPlayerHasArrivedAtDestination()
            CheckIfPlayerHasStoppedAtDestination()

            RefreshUI()
        End If

    End Sub

    Public Sub UpdateRouteDisplay(ByVal sender As Object, ByVal e As EventArgs) Handles MyBase.Tick

        If IsMinigameActive = True Then
            If DateTime.Now.Millisecond Mod 100 = 0 Then
                If MiniGameStage = MiniGameStages.DrivingToOrigin Then
                    If OriginBlip IsNot Nothing Then
                        OriginBlip.ShowRoute = False
                        OriginBlip.ShowRoute = True
                    End If
                ElseIf MiniGameStage = MiniGameStages.DrivingToDestination Then
                    If DestinationBlip IsNot Nothing Then
                        DestinationBlip.ShowRoute = False
                        DestinationBlip.ShowRoute = True
                    End If
                End If
            End If
        End If

    End Sub


    'MINOR UPDATES
    Public Sub UpdateUIDistances()

        If Origin IsNot Nothing Then
            If UpdateDist1 = True Then

                Dim dist As Single = World.CalculateTravelDistance(Game.Player.Character.Position, Origin.Coords)

                If dist < 500 Then
                    If UnitsInKM = True Then
                        UI_Dist1 = "  (" & dist & " m)"
                    Else
                        UI_Dist1 = "  (" & Math.Round(dist * 3.28084) & " ft)"
                    End If
                ElseIf dist > 3000 And UnitsInKM = True Then
                    UI_Dist2 = " (3+ km)"
                ElseIf dist > 3218 And UnitsInKM = False Then
                    UI_Dist2 = " (2+ mi)"
                Else
                    If UnitsInKM = True Then
                        UI_Dist1 = "  (" & Math.Round(dist / 1000, 2) & " km)"
                    Else
                        UI_Dist1 = "  (" & Math.Round((dist / 1000) * 0.621371, 2) & " mi)"
                    End If
                End If
            Else
                UI_Dist1 = ""
            End If
        End If

        If Destination IsNot Nothing Then
            If UpdateDist2 = True Then

                Dim dist As Single = World.CalculateTravelDistance(Game.Player.Character.Position, Destination.Coords)

                If dist < 500 Then
                    If UnitsInKM = True Then
                        UI_Dist2 = "  (" & dist & " m)"
                    Else
                        UI_Dist2 = "  (" & Math.Round(dist * 3.28084) & " ft)"
                    End If
                ElseIf dist > 3000 And UnitsInKM = True Then
                    UI_Dist2 = " (3+ km)"
                ElseIf dist > 3218 And UnitsInKM = False Then
                    UI_Dist2 = " (2+ mi)"
                Else
                    If UnitsInKM = True Then
                        UI_Dist2 = "  (" & Math.Round(dist / 1000, 2) & " km)"
                    Else
                        UI_Dist2 = "  (" & Math.Round((dist / 1000) * 0.621371, 2) & " mi)"
                    End If
                End If
            Else
                UI_Dist2 = ""
            End If
        End If

    End Sub

    Public Sub UpdateTaxiLight()
        If Game.Player.Character.IsInVehicle Then
            If Game.Player.Character.CurrentVehicle.DisplayName = "TAXI" Then
                If MiniGameStage = MiniGameStages.DrivingToDestination Then
                    Game.Player.Character.CurrentVehicle.TaxiLightOn = False
                Else
                    Game.Player.Character.CurrentVehicle.TaxiLightOn = True
                End If
            End If
        End If
    End Sub

    Public Sub ResetNudgeFlags()
        If IsCustomerNudged1 = True Then
            If Game.GameTime > NudgeResetTime Then
                IsCustomerNudged1 = False
            End If
        End If

        If IsCustomerNudged2 = True Then
            If Game.GameTime > NudgeResetTime Then
                IsCustomerNudged2 = False
            End If
        End If
    End Sub


    'CONDITION CHECKERS
    Public Sub CheckIfItsTimeToStartANewMission()
        If MiniGameStage = MiniGameStages.SearchingForFare Then

            If Game.GameTime > NextMissionStartTime Then
                NextMissionStartTime = 0
                StartMinigame()
            End If
        End If
    End Sub

    Public Sub CheckIfPlayerIsRoaming()
        If MiniGameStage <> MiniGameStages.RoamingForFare Then Exit Sub

        Dim p As Ped = World.GetClosestPed(Game.Player.Character.CurrentVehicle.Position + (Game.Player.Character.CurrentVehicle.ForwardVector * 40) + New Vector3(RND.Next(-10, 10), RND.Next(-10, 10), RND.Next(-10, 10)), 35.0F)
        If p = Nothing Then Exit Sub
        If p.IsInVehicle Then
            p.MarkAsNoLongerNeeded()
            Exit Sub
        End If
        If p.IsHuman = False Then
            p.MarkAsNoLongerNeeded()
            Exit Sub
        End If

        Customer = NonCeleb
        CustomerPed = p
        IsCustomerPedSpawned = True

        StreetSidePickup.Coords = p.Position
        StreetSidePickup.PedStart = p.Position
        Origin = StreetSidePickup

        Dim ZoneName = World.GetZoneName(Origin.Coords)
        If ZoneName <> "" Then
            UI_Origin = Origin.Name & ", " & ZoneName
        Else
            UI_Origin = Origin.Name
        End If

        Dim ts As New TaskSequence
        ts.AddTask.TurnTo(Game.Player.Character, 1000)
        ts.AddTask.PlayAnimation("taxi_hail", "hail_taxi", 8.0F, 3500, True, 8.0F)
        ts.AddTask.TurnTo(Game.Player.Character)

        CustomerPed.Task.PerformSequence(ts)
        CustomerPed.IsPersistent = True
        ts.Dispose()

        UI_DispatchStatus = "You've been hailed."

        StartRoamingMission()

    End Sub

    Public Sub CheckIfCloseEnoughToSpawnPed()
        If MiniGameStage = MiniGameStages.DrivingToOrigin Then
            Dim ppos As Vector3 = Game.Player.Character.Position
            Dim opos As Vector3 = Origin.Coords
            Dim distance As Single = World.GetDistance(ppos, opos)

            If distance < 130 Then
                Dim pos As Vector3 = Origin.PedStart
                If IsCustomerPedSpawned = False Then
                    IsCustomerPedSpawned = True

                    Native.Function.Call(Native.Hash.CLEAR_AREA_OF_PEDS, pos.X, pos.Y, pos.Z, 30)

                    If Customer.isCeleb = True Then
                        CustomerPed = World.CreatePed(New Model(Customer.Model), Origin.PedStart, Origin.PedStartHDG)
                    Else
                        CustomerPed = World.CreateRandomPed(pos)
                        If CustomerPed.Exists Then
                            CustomerPed.Heading = Origin.PedStartHDG
                            CustomerPed.Money = RND.Next(5, 200)
                            CustomerPed.AlwaysKeepTask = True
                        End If
                    End If
                    CustomerPed.RelationshipGroup = Game.Player.Character.RelationshipGroup

                    If IsThereASecondCustomer = True Then
                        Dim around As Vector3 = pos.Around(0.6)
                        Customer2Ped = World.CreateRandomPed(around)
                        Customer2Ped.Money = RND.Next(5, 200)
                        Customer2Ped.RelationshipGroup = Game.Player.Character.RelationshipGroup
                        Customer2Ped.AlwaysKeepTask = True
                    End If

                    If IsThereAThirdCustomer = True Then
                        Dim around As Vector3 = pos.Around(0.6)
                        Customer3Ped = World.CreateRandomPed(around)
                        Customer3Ped.Money = RND.Next(5, 200)
                        Customer3Ped.RelationshipGroup = Game.Player.Character.RelationshipGroup
                        Customer3Ped.AlwaysKeepTask = True
                    End If


                    'MAKE SURROUNDING PEDS NON-AGGRESSIVE
                    PedsAroundOrigin = World.GetNearbyPeds(Origin.Coords, 40)

                    For Each p As Ped In PedsAroundOrigin
                        If p.IsHuman Then
                            p.RelationshipGroup = Game.Player.Character.RelationshipGroup
                        End If
                    Next

                End If
            End If
        End If
    End Sub

    Public Sub CheckIfCloseEnoughToHail()
        If MiniGameStage <> MiniGameStages.StoppingAtOrigin Then Exit Sub
        If HasCustomerPedHailedPlayer = True Then Exit Sub
        If Destination.Name <> StreetSidePickup.Name Then Exit Sub
        If World.GetDistance(Game.Player.Character.Position, CustomerPed.Position) > 10 Then Exit Sub

        Dim ts As New TaskSequence
        ts.AddTask.ClearAll()
        PRINT("Second Hail")
        ts.AddTask.PlayAnimation("taxi_hail", "hail_taxi", 8.0F, 2500, True, 8.0F)
        ts.AddTask.TurnTo(Game.Player.Character, 200)
        CustomerPed.Task.PerformSequence(ts)
        ts.Dispose()
        HasCustomerPedHailedPlayer = True

    End Sub

    Public Sub CheckIfPlayerHasAbandonedHail()
        If MiniGameStage <> MiniGameStages.DrivingToOrigin Then Exit Sub
        If Destination.Name <> StreetSidePickup.Name Then Exit Sub
        If World.GetDistance(Game.Player.Character.Position, CustomerPed.Position) < 80 Then Exit Sub

        PRINT("Hail Abandoned")
        CustomerPed.Task.ClearAll()
        CustomerPed.MarkAsNoLongerNeeded()
        CustomerPed = Nothing

        MiniGameStage = MiniGameStages.SearchingForFare
        SetStandbySpecs()
    End Sub

    Public Sub CheckIfPedsAreAlive()
        Dim isPed1Alive As Boolean = True
        Dim isPed2Alive As Boolean = True
        Dim isPed3Alive As Boolean = True

        If CustomerPed IsNot Nothing Then
            If CustomerPed.Exists Then
                If CustomerPed.IsDead Then
                    isPed1Alive = False
                End If
            End If
        End If

        If IsThereASecondCustomer = True Then
            If Customer2Ped IsNot Nothing Then
                If Customer2Ped.Exists Then
                    If Customer2Ped.IsDead Then
                        isPed2Alive = False
                    End If
                End If
            End If
        End If

        If IsThereAThirdCustomer = True Then
            If Customer3Ped IsNot Nothing Then
                If Customer3Ped.Exists Then
                    If Customer3Ped.IsDead Then
                        isPed3Alive = False
                    End If
                End If
            End If
        End If

        If isPed1Alive = False Or isPed2Alive = False Or isPed3Alive = False Then
            CustomerHasDied()
        End If

    End Sub

    Public Sub CheckIfPlayerHasArrivedAtOrigin()
        If MiniGameStage = MiniGameStages.DrivingToOrigin Then

            Dim distance As Single = World.GetDistance(Game.Player.Character.Position, Origin.Coords)

            If Origin.Name = StreetSidePickup.Name Then
                If distance <= 25 Then
                    PlayerHasArrivedAtOrigin()
                End If
            Else
                If distance < 30 Then
                    PlayerHasArrivedAtOrigin()
                End If
            End If
        End If
    End Sub

    Public Sub CheckIfPlayerHasStoppedAtOrigin()
        If MiniGameStage = MiniGameStages.StoppingAtOrigin Then
            If Game.Player.Character.IsInVehicle = True Then
                If Game.Player.Character.CurrentVehicle.Speed = 0 Then
                    If World.GetDistance(Game.Player.Character.Position, Origin.Coords) < 70 Then
                        PlayerHasStoppedAtOrigin()
                    End If
                End If
            End If
        End If
    End Sub

    Public Sub CheckVehicleBoardingCondition()

        If MiniGameStage <> MiniGameStages.PedWalkingToCar And MiniGameStage <> MiniGameStages.PedGettingInCar Then Exit Sub

        ' FIND THE DISTANCE OF THE CLOSEST PED TO THE PLAYER
        Dim ped1Distance, ped2Distance, ped3Distance As Single

        ped1Distance = World.GetDistance(Game.Player.Character.Position, CustomerPed.Position)
        ClosestPedDistance = ped1Distance

        If IsThereASecondCustomer Then
            ped2Distance = World.GetDistance(Game.Player.Character.Position, Customer2Ped.Position)
            If ped2Distance < ClosestPedDistance Then
                ClosestPedDistance = ped2Distance
            End If
        End If

        If IsThereAThirdCustomer Then
            ped3Distance = World.GetDistance(Game.Player.Character.Position, Customer3Ped.Position)
            If ped3Distance < ClosestPedDistance Then
                ClosestPedDistance = ped3Distance
            End If
        End If




        If Game.Player.Character.IsInVehicle Then


            ' CHECK DIRT LEVEL
            If Game.Player.Character.CurrentVehicle.DirtLevel > 20 And ClosestPedDistance < 3 Then

                Dim message = RND.Next(4)
                Dim messageText As String

                Select Case message
                    Case 1
                        messageText = "Your car is filthy! Go get it washed."
                    Case 2
                        messageText = "When's the last time you washed this thing?"
                    Case 3
                        messageText = "Ew! I'm not getting in this dirty car!"
                    Case Else
                        messageText = "Wash your car, it's disgusting!"
                End Select
                GTA.UI.ShowSubtitle("~r~" & messageText, 8000)
                EndMinigame(False)
                Exit Sub

            End If


            ' CHECK DAMAGE LEVEL
            VehicleStartHealth = Game.Player.Character.CurrentVehicle.Health
            VehicleStartHealthPercent = VehicleStartHealth / Game.Player.Character.CurrentVehicle.MaxHealth

            If VehicleStartHealthPercent < 0.75 And ClosestPedDistance < 4 Then
                Dim messageText As String

                If Customer2Ped IsNot Nothing Then
                    Dim message = RND.Next(6)

                    Select Case message
                        Case 1
                            messageText = "What a piece of junk! We're not getting in that."
                        Case 2
                            messageText = "This car is broken! You need to get it fixed!"
                        Case 3
                            messageText = "Are you trying to get us killed? That car's unsafe!"
                        Case 4
                            messageText = "We'd rather walk than get in your broken-ass car!"
                        Case Else
                            messageText = "We're taking another cab. Your car is too damaged."
                    End Select

                Else
                    Dim message = RND.Next(6)

                    Select Case message
                        Case 1
                            messageText = "This car's making some funny noises. I think I'll take a different cab."
                        Case 2
                            messageText = "What the hell is wrong with your car? Forget it, I'm not riding with you."
                        Case 3
                            messageText = "Nu-uh! You're driving a dangerous pile of junk!"
                        Case 4
                            messageText = "I'd rather walk than get in your broken-ass car!"
                        Case Else
                            messageText = "I'm taking another cab. Your car is too damaged."
                    End Select

                End If

                'TODO: ADD PED ANIMATIONS AND NEGATIVE REACTIONS

                GTA.UI.ShowSubtitle("~r~" & messageText, 8000)
                EndMinigame(False)
                Exit Sub

            End If


            ' CHECK IF THERE'S A DEAD PERSON
            If ClosestPedDistance < 3 Then
                Dim occupants = Game.Player.Character.CurrentVehicle.Passengers
                For Each p As Ped In occupants
                    If p.IsDead Then
                        Dim message = RND.Next(5)

                        Dim messageText As String
                        Select Case message
                            Case 1
                                messageText = "There's a dead body in your car!"
                            Case 2
                                messageText = "Aagh! You're driving around with a corpse!"
                            Case 3
                                messageText = "Why is there a dead person in your car?"
                            Case Else
                                messageText = "Help! This guy's driving around with a corpse!"
                        End Select

                        GTA.UI.ShowSubtitle("~r~" & messageText, 8000)
                        GTA.UI.Notify("You may want to do something about that dead body in your car...")
                        EndMinigame(True)
                    End If
                Next
            End If
        End If
    End Sub

    Public Sub CheckIfPassengerNeedsToBeNudged()
        Dim isHonking As Boolean = Game.Player.IsPressingHorn

        If MiniGameStage = MiniGameStages.PedWalkingToCar Then
            If isHonking = True Then
                If IsCustomerNudged1 = False Then
                    If CustomerPed.Exists Then
                        CustomerPed.Position += CustomerPed.ForwardVector * 2
                        CustomerPed.Task.GoTo(Game.Player.Character.Position, False)

                        If IsThereASecondCustomer = True Then
                            If Customer2Ped IsNot Nothing Then
                                If Customer2Ped.Exists = True Then
                                    Customer2Ped.Position += Customer2Ped.ForwardVector * 2
                                    Customer2Ped.Task.GoTo(Game.Player.Character.Position, False)
                                End If
                            End If
                        End If

                        If IsThereAThirdCustomer = True Then
                            If Customer3Ped IsNot Nothing Then
                                If Customer3Ped.Exists = True Then
                                    Customer3Ped.Position += Customer3Ped.ForwardVector * 2
                                    Customer3Ped.Task.GoTo(Game.Player.Character.Position, False)
                                End If
                            End If
                        End If

                        IsCustomerNudged1 = True
                        NudgeResetTime = Game.GameTime + 750
                    End If
                End If
            End If
        End If



        If MiniGameStage = MiniGameStages.PedGettingInCar Then
            If Game.Player.Character.IsInVehicle And isHonking = True Then
                If IsCustomerNudged2 = False Then
                    If CustomerPed.Exists Then
                        If MaxSeats >= 3 Then
                            CustomerPed.Task.EnterVehicle(Game.Player.Character.CurrentVehicle, VehicleSeat.RightRear, 8000)
                        Else
                            CustomerPed.Task.EnterVehicle(Game.Player.Character.CurrentVehicle, VehicleSeat.Passenger, 8000)
                        End If
                    End If
                    If IsThereASecondCustomer = True Then
                        If Customer2Ped.Exists Then
                            Customer2Ped.Task.EnterVehicle(Game.Player.Character.CurrentVehicle, VehicleSeat.LeftRear, 8000)
                        End If
                    End If
                    If IsThereAThirdCustomer = True Then
                        If Customer3Ped.Exists Then
                            Customer3Ped.Task.EnterVehicle(Game.Player.Character.CurrentVehicle, VehicleSeat.Passenger, 8000)
                        End If
                    End If
                    IsCustomerNudged2 = True
                    NudgeResetTime = Game.GameTime + 750
                End If
            End If
        End If
    End Sub

    Public Sub CheckIfPedHasReachedCar()
        If MiniGameStage = MiniGameStages.PedWalkingToCar Then
            If CustomerPed IsNot Nothing Then
                If CustomerPed.Exists = True Then
                    Dim tgt As Vector3 = Game.Player.Character.Position
                    Dim ppo As Vector3 = CustomerPed.Position
                    Dim distance As Single = World.GetDistance(tgt, ppo)
                    If distance < 9 Then
                        PedHasReachedCar()
                    End If
                End If
            End If
        End If
    End Sub

    Public Sub CheckIfPedHasEnteredCar()

        If MiniGameStage = MiniGameStages.PedGettingInCar Then
            If CustomerPed.Exists = True Then
                If Game.Player.Character.IsInVehicle Then
                    Dim isPed1Sitting As Boolean = Native.Function.Call(Of Boolean)(Native.Hash.IS_PED_IN_VEHICLE, CustomerPed, Game.Player.Character.CurrentVehicle, False)
                    Dim isPed2Sitting As Boolean
                    Dim isPed3Sitting As Boolean

                    If IsThereASecondCustomer = True Then
                        isPed2Sitting = Native.Function.Call(Of Boolean)(Native.Hash.IS_PED_IN_VEHICLE, Customer2Ped, Game.Player.Character.CurrentVehicle, False)
                    Else
                        isPed2Sitting = True
                    End If

                    If IsThereAThirdCustomer = True Then
                        isPed3Sitting = Native.Function.Call(Of Boolean)(Native.Hash.IS_PED_IN_VEHICLE, Customer3Ped, Game.Player.Character.CurrentVehicle, False)
                    Else
                        isPed3Sitting = True
                    End If

                    Dim areAllPedsSitting As Boolean = isPed1Sitting And isPed2Sitting And isPed3Sitting
                    If areAllPedsSitting = True Then
                        PedHasEnteredCar()
                    End If
                End If
            End If
        End If


        'BOOL IS_PED_IN_VEHICLE(Ped pedHandle, Vehicle vehicleHandle, BOOL atGetIn) // 0x7DA6BC83
        'Gets a value indicating whether the specified ped is in the specified vehicle.
        'If 'atGetIn' is false, the function will not return true until the ped is sitting in the vehicle and is about to close the door. If it's true, the function returns true 
        'the moment the ped starts to get onto the seat (after opening the door). Eg. if false, and the ped is getting into a submersible, the function will not return true until             
        'the ped has descended down into the submersible and gotten into the seat, while if it's true, it'll return true the moment the hatch has been opened and the ped is about 
        'to descend into the submersible.

    End Sub

    Public Sub CheckIfCloseEnoughToClearDestination()
        If MiniGameStage = MiniGameStages.DrivingToDestination Then
            Dim distance As Single = World.GetDistance(Game.Player.Character.Position, Destination.Coords)

            If distance < 90 Then
                Dim pos As Vector3 = Destination.PedStart
                If IsDestinationCleared = False Then
                    IsDestinationCleared = True

                    PedsAroundDestination = World.GetNearbyPeds(Destination.PedStart, 30)

                    For Each p As Ped In PedsAroundDestination
                        If p.IsHuman Then
                            p.RelationshipGroup = Game.Player.Character.RelationshipGroup
                        End If
                    Next

                End If
            End If

        End If
    End Sub

    Public Sub CheckIfPlayerHasArrivedAtDestination()
        If MiniGameStage = MiniGameStages.DrivingToDestination Then
            Dim distance As Single = World.GetDistance(Game.Player.Character.Position, Destination.Coords)

            If distance < 11 Then
                PlayerHasArrivedAtDestination()
            End If
        End If
    End Sub

    Public Sub CheckIfPlayerHasStoppedAtDestination()
        If MiniGameStage = MiniGameStages.StoppingAtDestination Then
            If Game.Player.Character.IsInVehicle = True Then
                If Game.Player.Character.CurrentVehicle.Speed = 0 Then
                    If World.GetDistance(Game.Player.Character.Position, Destination.Coords) < 75 Then
                        PlayerHasStoppedAtDestination()
                    End If
                End If
            End If
        End If
    End Sub








    'TOGGLES
    Public Sub ToggleMinigame(ByVal sender As Object, ByVal k As KeyEventArgs) Handles MyBase.KeyUp

        If Game.Player.CanStartMission = False Then Exit Sub

        If k.KeyCode = ToggleKey Then

            If IsMinigameActive = True Then
                IsMinigameActive = False
                EndMinigame()
            Else

                If Game.Player.Character.IsInVehicle Then
                    MaxSeats = Native.Function.Call(Of Integer)(Native.Hash.GET_VEHICLE_MAX_NUMBER_OF_PASSENGERS, Game.Player.Character.CurrentVehicle)
                    If IsCarOK(MaxSeats, Game.Player.Character.CurrentVehicle.ClassType) Then
                        StartMinigame()
                        IsMinigameActive = True
                    Else
                        GTA.UI.Notify("Taxi missions can only be started in a car with at least 2 seats.")
                    End If

                End If

            End If
        End If


        ' FOR DEBUGGING PURPOSES: KILLS ONE OF THE CUSTOMERS BY PRESSING THE "Y" KEY
        If ShowDebugInfo = True Then
            If k.KeyCode = Keys.Y Then

                Dim customerToKill As Integer

                If IsThereAThirdCustomer Then
                    customerToKill = RND.Next(1, 4)
                    If customerToKill > 3 Then customerToKill = 3
                ElseIf IsThereASecondCustomer Then
                    customerToKill = RND.Next(1, 3)
                    If customerToKill > 2 Then customerToKill = 2
                Else
                    customerToKill = 1
                End If

                Dim doomedPed As Ped
                Select Case customerToKill
                    Case 1
                        doomedPed = CustomerPed
                    Case 2
                        doomedPed = Customer2Ped
                    Case 3
                        doomedPed = Customer3Ped
                    Case Else
                        doomedPed = CustomerPed
                End Select

                If doomedPed IsNot Nothing Then
                    If doomedPed.Exists Then
                        doomedPed.MarkAsNoLongerNeeded()
                        doomedPed.IsPersistent = False
                        doomedPed.AlwaysDiesOnLowHealth = True
                        doomedPed.ApplyDamage(1000)
                    End If
                End If
            End If
        End If

    End Sub

    Public Sub ResetDoors(ByVal sender As Object, ByVal k As KeyEventArgs) Handles MyBase.KeyUp

        If k.KeyCode = Keys.Divide Then

            If Game.Player.Character.IsInVehicle Then
                GTA.UI.ShowSubtitle("Opening doors", 400)
                Game.Player.Character.CurrentVehicle.OpenDoor(VehicleDoor.FrontRightDoor, False, False)
                Game.Player.Character.CurrentVehicle.OpenDoor(VehicleDoor.BackLeftDoor, False, False)
                Game.Player.Character.CurrentVehicle.OpenDoor(VehicleDoor.BackRightDoor, False, False)
                Wait(400)
                GTA.UI.ShowSubtitle("Closing doors", 400)
                Game.Player.Character.CurrentVehicle.CloseDoor(VehicleDoor.FrontRightDoor, False)
                Game.Player.Character.CurrentVehicle.CloseDoor(VehicleDoor.BackLeftDoor, False)
                Game.Player.Character.CurrentVehicle.CloseDoor(VehicleDoor.BackRightDoor, False)
            End If
        End If
    End Sub

    Public Sub StartMinigame()

        If MissionFlag = True Then Exit Sub
        If Game.Player.Character.IsInVehicle = False Then Exit Sub

        IsMinigameActive = True

        Origin = Nothing
        Destination = Nothing

        PedsAroundOrigin = Nothing
        PedsAroundDestination = Nothing

        UpdateDist1 = False
        UpdateDist2 = False

        IsSpecialMission = False
        HasCustomerPedHailedPlayer = False
        IsCustomerPedSpawned = False
        IsDestinationCleared = False
        IsCustomerNudged1 = False
        IsCustomerNudged2 = False
        IsThereASecondCustomer = False
        IsThereAThirdCustomer = False

        CustomerPed = Nothing
        Customer2Ped = Nothing
        Customer3Ped = Nothing

        MissionStartTime = 0
        OriginArrivalTime = 0
        DestinationArrivalTime = 0
        PickupTime = 0
        IdealTripTime = 0

        VehicleStartHealth = 0
        VehicleEndHealth = 0
        VehicleStartHealthPercent = 0
        VehicleEndHealthPercent = 0

        FareDistance = 0
        FareTotal = 0
        FareTip = 0

        UI_DispatchStatus = "Standby..."
        UI_Destination = ""
        UI_Origin = ""
        UI_Dist1 = ""
        UI_Dist2 = ""

        MiniGameStage = MiniGameStages.Standby

        If Game.Player.Character.CurrentVehicle.DisplayName = "TAXI" Then
            Dim r As Integer = RND.Next(0, 2)

            If r = 0 Then
                StartMission()
            Else
                SetUpRoamingMission()
            End If
        Else
            StartMission()
        End If

    End Sub

    Public Sub EndMinigame(Optional panic As Boolean = False)

        If OriginBlip IsNot Nothing Then
            If OriginBlip.Exists Then
                OriginBlip.ShowRoute = False
                OriginBlip.Remove()
            End If
        End If

        If DestinationBlip IsNot Nothing Then
            If DestinationBlip.Exists Then
                DestinationBlip.ShowRoute = False
                DestinationBlip.Remove()
            End If
        End If

        If CustomerPed IsNot Nothing Then
            If CustomerPed.Exists Then
                If CustomerPed.IsAlive Then
                    If panic = True Then
                        CustomerPed.Task.ReactAndFlee(Game.Player.Character)
                    Else
                        CustomerPed.Task.WanderAround()
                        CustomerPed.AlwaysKeepTask = True
                    End If
                End If

                If Ped1Blip IsNot Nothing Then
                    If Ped1Blip.Exists Then
                        Ped1Blip.Remove()
                        Ped1Blip = Nothing
                    End If
                End If
            End If
            CustomerPed.MarkAsNoLongerNeeded()
            CustomerPed.IsPersistent = False
        End If

        If Customer2Ped IsNot Nothing Then
            If Customer2Ped.Exists = True Then
                If Customer2Ped.IsAlive Then
                    If panic = True Then
                        Customer2Ped.Task.ReactAndFlee(Game.Player.Character)
                    Else
                        Customer2Ped.Task.WanderAround()
                        Customer2Ped.AlwaysKeepTask = True
                    End If
                End If

                If Ped2Blip IsNot Nothing Then
                    If Ped2Blip.Exists Then
                        Ped2Blip.Remove()
                        Ped2Blip = Nothing
                    End If
                End If
            End If
            Customer2Ped.MarkAsNoLongerNeeded()
            Customer2Ped.IsPersistent = False
        End If

        If Customer3Ped IsNot Nothing Then
            If Customer3Ped.Exists = True Then
                If Customer3Ped.IsAlive Then
                    If panic = True Then
                        Customer3Ped.Task.ReactAndFlee(Game.Player.Character)
                    Else
                        Customer3Ped.Task.WanderAround()
                        Customer3Ped.AlwaysKeepTask = True
                    End If
                End If

                If Ped3Blip IsNot Nothing Then
                    If Ped3Blip.Exists Then
                        Ped3Blip.Remove()
                        Ped3Blip = Nothing
                    End If
                End If
            End If
            Customer3Ped.MarkAsNoLongerNeeded()
            Customer3Ped.IsPersistent = False
        End If

        If Game.Player.Character.IsInVehicle Then
            Game.Player.Character.CurrentVehicle.LeftIndicatorLightOn = False
            Game.Player.Character.CurrentVehicle.RightIndicatorLightOn = False
        End If

        IsMinigameActive = False

    End Sub



    'CALCULATIONS
    Public Sub PayPlayer(amount As Integer)
        Dim currentMoney = Game.Player.Money
        Game.Player.Money = currentMoney + amount

        Native.Function.Call(Native.Hash.DISPLAY_CASH, True)
    End Sub

    Public Sub CalculateFare(StartPoint As Vector3, EndPoint As Vector3)

        FareDistance = World.CalculateTravelDistance(StartPoint, EndPoint) / 1000
        If FareDistance > 20 Then
            FareDistance = World.GetDistance(StartPoint, EndPoint) / 1000
        End If

        IdealTripTime = (FareDistance / AverageSpeed) * 60 * 60 * 1000

        FareDistance *= 0.621371
        FareTotal = CInt(Math.Round(FareBase + (FareDistance * FarePerMile)))
    End Sub

    Public Sub CalculateTipParameters()
        ArrivalWindowEnd = PickupTime + IdealTripTime - (IdealTripTime * 0.05)
        PRINT("Ideal Trip Time: " & Math.Round(IdealTripTime / 1000) & " seconds")

        ArrivalWindowStart = Math.Round(ArrivalWindowEnd - (IdealTripTime * 0.5))
    End Sub

    Public Sub CalculateTip()

        If DestinationArrivalTime < ArrivalWindowStart Then
            PRINT("Arrived early")
            FareTipPercent = MaximumTip
        ElseIf DestinationArrivalTime > ArrivalWindowEnd Then
            PRINT("Arrived too late")
            FareTipPercent = 0
        Else

            Dim span As Integer = ArrivalWindowEnd - ArrivalWindowStart
            Dim arr As Integer = DestinationArrivalTime - ArrivalWindowStart
            Dim pct As Single = arr / span
            FareTipPercent = MaximumTip - (MaximumTip * pct)
        End If
        PRINT("Tip Percent " & Math.Round(FareTipPercent, 3) & " ARR: " & DestinationArrivalTime - ArrivalWindowStart)

        FareTip = FareTotal * FareTipPercent
    End Sub




    'CONDITIONS MET
    Private Sub SetUpRoamingMission()
        UI_DispatchStatus = "Drive around to find a fare."
        MiniGameStage = MiniGameStages.RoamingForFare
    End Sub

    Private Sub StartMission()

        MissionStartTime = Game.GameTime

        CustomerPed = Nothing
        Customer2Ped = Nothing
        Customer3Ped = Nothing

        Dim r As Integer = RND.Next(0, 10)
        If r < 1 Then
            IsSpecialMission = True
            GenerateSpecialMissionLocations()
        Else
            IsSpecialMission = False
            GenerateGenericMissionLocations()
        End If

        SelectValidOrigin(PotentialOrigins)

        If Origin.Type = LocationType.AirportArrive Then
            Dim l As New List(Of Location)
            l.AddRange(lResidential)
            l.AddRange(lHotelLS)
            l.AddRange(lMotelLS)
            l.AddRange(lOffice)
            SelectValidDestination(l)
        Else
            SelectValidDestination(PotentialDestinations)
        End If

        If IsSpecialMission = True Then
            SelectSpecialCustomers()
        Else
            SelectGenericCustomers()
        End If

        OriginBlip = World.CreateBlip(Origin.Coords)
        'OriginBlip.Sprite = 280
        OriginBlip.Color = BlipColor.Blue
        OriginBlip.ShowRoute = True

        Native.Function.Call(Native.Hash.FLASH_MINIMAP_DISPLAY)

        Game.Player.Character.CurrentVehicle.LeftIndicatorLightOn = False
        Game.Player.Character.CurrentVehicle.RightIndicatorLightOn = False

        UpdateDist1 = True

        MiniGameStage = MiniGameStages.DrivingToOrigin
    End Sub

    Private Sub StartRoamingMission()

        MissionStartTime = Game.GameTime

        GenerateGenericMissionLocations()

        SelectValidDestination(PotentialDestinations)

        OriginBlip = World.CreateBlip(CustomerPed.Position)
        OriginBlip.Sprite = 280
        OriginBlip.Color = BlipColor.Blue
        OriginBlip.ShowRoute = True

        Native.Function.Call(Native.Hash.FLASH_MINIMAP_DISPLAY)

        Game.Player.Character.CurrentVehicle.LeftIndicatorLightOn = False
        Game.Player.Character.CurrentVehicle.RightIndicatorLightOn = False

        UpdateDist1 = True
        MiniGameStage = MiniGameStages.DrivingToOrigin
    End Sub

    Private Sub GenerateGenericMissionLocations()

        PotentialOrigins.Clear()
        PotentialDestinations.Clear()

        Select Case IngameHour
            Case 0 To 5
                With PotentialOrigins
                    .AddRange(lAirportA)
                    .AddRange(lBar)
                    .AddRange(lMotelLS)
                    .AddRange(lStripClub)
                    .AddRange(lTheater)
                    .AddRange(lFactory)
                End With
                With PotentialDestinations
                    .AddRange(lHotelLS)
                    .AddRange(lResidential)
                    .AddRange(lBar)
                    .AddRange(lMotelLS)
                    .AddRange(lStripClub)
                End With

            Case 6 To 10
                With PotentialOrigins
                    .AddRange(lResidential)
                    .AddRange(lHotelLS)
                    .AddRange(lMotelLS)
                    .AddRange(lSport)
                    .AddRange(lFastFood)
                    .AddRange(lAirportA)
                    .AddRange(lShopping)
                    .AddRange(lReligious)
                    .AddRange(lStripClub)
                    .AddRange(lFactory)
                End With
                With PotentialDestinations
                    .AddRange(lAirportD)
                    .AddRange(lHotelLS)
                    .AddRange(lMotelLS)
                    .AddRange(lSport)
                    .AddRange(lEntertainment)
                    .AddRange(lFastFood)
                    .AddRange(lShopping)
                    .AddRange(lReligious)
                    .AddRange(lOffice)
                    .AddRange(lSchool)
                    .AddRange(lFactory)
                End With

            Case 11 To 15
                With PotentialOrigins
                    .AddRange(lAirportA)
                    .AddRange(lHotelLS)
                    .AddRange(lMotelLS)
                    .AddRange(lSport)
                    .AddRange(lEntertainment)
                    .AddRange(lFastFood)
                    .AddRange(lResidential)
                    .AddRange(lRestaurant)
                    .AddRange(lReligious)
                    .AddRange(lShopping)
                    .AddRange(lOffice)
                    .AddRange(lSchool)
                End With
                With PotentialDestinations
                    .AddRange(lAirportD)
                    .AddRange(lHotelLS)
                    .AddRange(lMotelLS)
                    .AddRange(lSport)
                    .AddRange(lEntertainment)
                    .AddRange(lFastFood)
                    .AddRange(lResidential)
                    .AddRange(lRestaurant)
                    .AddRange(lReligious)
                    .AddRange(lShopping)
                    .AddRange(lOffice)
                    .AddRange(lTheater)
                    .AddRange(lSchool)
                End With

            Case 16 To 19
                With PotentialOrigins
                    .AddRange(lAirportA)
                    .AddRange(lHotelLS)
                    .AddRange(lMotelLS)
                    .AddRange(lSport)
                    .AddRange(lEntertainment)
                    .AddRange(lFastFood)
                    .AddRange(lRestaurant)
                    .AddRange(lShopping)
                    .AddRange(lReligious)
                    .AddRange(lOffice)
                    .AddRange(lTheater)
                    .AddRange(lSchool)
                    .AddRange(lFactory)
                End With
                With PotentialDestinations
                    .AddRange(lAirportD)
                    .AddRange(lHotelLS)
                    .AddRange(lMotelLS)
                    .AddRange(lEntertainment)
                    .AddRange(lFastFood)
                    .AddRange(lResidential)
                    .AddRange(lRestaurant)
                    .AddRange(lShopping)
                    .AddRange(lBar)
                    .AddRange(lTheater)
                    .AddRange(lSchool)
                    .AddRange(lFactory)
                End With

            Case 20 To 23
                With PotentialOrigins
                    .AddRange(lAirportA)
                    .AddRange(lHotelLS)
                    .AddRange(lMotelLS)
                    .AddRange(lEntertainment)
                    .AddRange(lFastFood)
                    .AddRange(lResidential)
                    .AddRange(lRestaurant)
                    .AddRange(lShopping)
                    .AddRange(lBar)
                    .AddRange(lStripClub)
                    .AddRange(lTheater)
                    .AddRange(lSchool)
                End With
                With PotentialDestinations
                    .AddRange(lResidential)
                    .AddRange(lHotelLS)
                    .AddRange(lMotelLS)
                    .AddRange(lEntertainment)
                    .AddRange(lFastFood)
                    .AddRange(lResidential)
                    .AddRange(lShopping)
                    .AddRange(lBar)
                    .AddRange(lStripClub)
                    .AddRange(lTheater)
                End With

            Case Else
                PotentialOrigins = ListOfPlaces
                PotentialDestinations = ListOfPlaces

        End Select
    End Sub

    Private Sub GenerateSpecialMissionLocations()

        '\/  \/  \/  TEMPORARY!  \/  \/  \/
        GenerateGenericMissionLocations()
        '/\  /\  /\  TEMPORARY!  /\  /\  /\

        'EPSILON
        'CELEB
        'HURRY
        'FOLLOWTHATCAR

    End Sub

    Private Sub SelectValidOrigin(Places As List(Of Location))

        If Places.Count = 0 Then Places.AddRange(ListOfPlaces)

        Dim LocationsAll, LocationsFar, LocationsMedium, LocationsNear As New List(Of Location)

        For Each loc As Location In Places

            Dim playerPosition As Vector3 = Game.Player.Character.Position
            Dim locationDistance As Single
            locationDistance = World.GetDistance(loc.Coords, playerPosition)

            If locationDistance > 1200 Then
                LocationsAll.Add(loc)
            ElseIf locationDistance > 800 Then
                LocationsFar.Add(loc)
            ElseIf locationDistance > 300 Then
                LocationsMedium.Add(loc)
            Else
                LocationsNear.Add(loc)
            End If

        Next


        Dim SelectedOrigins As List(Of Location)
        If LocationsNear.Count < 2 Then
            If LocationsMedium.Count < 2 Then
                If LocationsFar.Count < 2 Then
                    SelectedOrigins = LocationsAll
                Else
                    SelectedOrigins = LocationsFar
                End If
            Else
                SelectedOrigins = LocationsMedium
            End If
        Else
            SelectedOrigins = LocationsNear
        End If


        Dim r As Integer = RND.Next(0, SelectedOrigins.Count)
        Origin = SelectedOrigins(r)

        Dim ZoneName = World.GetZoneName(Origin.Coords)
        If ZoneName <> "" Then
            UI_Origin = Origin.Name & ", " & ZoneName
        Else
            UI_Origin = Origin.Name
        End If
    End Sub

    Private Sub SelectValidDestination(Places As List(Of Location))
        PRINT("Valid Destinations: " & Places.Count)
        If Places.Count = 0 Then Places.AddRange(ListOfPlaces)

        Dim r As Integer
        Dim distance As Single
        Dim c As Integer = 0
        Do
            r = RND.Next(0, Places.Count)
            Destination = Places(r)
            distance = World.GetDistance(Origin.Coords, Destination.Coords)
            c += 1
            If c > 10 Then
                r = RND.Next(0, ListOfPlaces.Count)
                Destination = ListOfPlaces(r)
                Exit Do
            End If
        Loop While Origin.Name = Destination.Name Or distance < 450 Or Origin.isValidDestination = False

        Dim ZoneName = World.GetZoneName(Destination.Coords)
        If ZoneName <> "" Then
            UI_Destination = Destination.Name & ", " & ZoneName
        Else
            UI_Destination = Destination.Name
        End If

    End Sub

    Private Sub SelectGenericCustomers()
        Dim r As Integer

        r = RND.Next(0, 500)
        If r <= 4 Then
            Customer = ListOfPeople(r)
        Else
            Customer = NonCeleb
        End If


        'GENERATE ADDITIONAL CUSTOMERS / PASSENGERS
        If MaxSeats >= 2 Then
            r = RND.Next(0, 5)
            If r = 0 Or r = 1 Then
                IsThereASecondCustomer = True
                If MaxSeats >= 3 Then
                    If r = 1 Then
                        IsThereAThirdCustomer = True
                    End If
                End If
            End If
        End If




        If Customer.isCeleb = True Then
            UI_DispatchStatus = Customer.Name & " waiting for pickup"
        Else
            UI_DispatchStatus = "Customer waiting for pickup"
            If IsThereASecondCustomer = True Then
                UI_DispatchStatus = "Customers waiting for pickup"
            End If
        End If
    End Sub

    Private Sub SelectSpecialCustomers()
        '\/  \/  \/  TEMPORARY!  \/  \/  \/
        SelectGenericCustomers()
        '/\  /\  /\  TEMPORARY!  /\  /\  /\
    End Sub

    Private Sub PlayerHasArrivedAtOrigin()
        UpdateDist1 = False
        UpdateDist2 = True
        OriginArrivalTime = Game.GameTime
        UI_DispatchStatus = "Please stop at the marker"
        PRINT("Orig Arr Time: " & OriginArrivalTime & " / Time taken: " & Math.Round((OriginArrivalTime - MissionStartTime) / 1000, 1))
        MiniGameStage = MiniGameStages.StoppingAtOrigin
    End Sub

    Private Sub PlayerHasStoppedAtOrigin()

        Dim playerPosition As Vector3 = Game.Player.Character.Position
        If Game.Player.Character.IsInVehicle Then
            If UseHazardLights = True Then
                Game.Player.Character.CurrentVehicle.LeftIndicatorLightOn = True
                Game.Player.Character.CurrentVehicle.RightIndicatorLightOn = True
            End If
        End If

        If CustomerPed IsNot Nothing Then
            If CustomerPed.Exists Then
                CustomerPed.Task.ClearAll()
                CustomerPed.Task.GoTo(playerPosition, False)
                CustomerPed.AlwaysKeepTask = True
                Ped1Blip = CustomerPed.AddBlip
                Ped1Blip.Sprite = 480
                Ped1Blip.Color = BlipColor.Green
                Ped1Blip.Scale = 0.8
            End If
        End If

        If IsThereASecondCustomer = True Then
            If Customer2Ped IsNot Nothing Then
                If Customer2Ped.Exists = True Then
                    Customer2Ped.Task.ClearAll()
                    Customer2Ped.Task.GoTo(playerPosition, False)
                    CustomerPed.AlwaysKeepTask = True
                    Ped2Blip = Customer2Ped.AddBlip
                    Ped2Blip.Sprite = 480
                    Ped2Blip.Color = BlipColor.Green
                    Ped2Blip.Scale = 0.8
                End If
            End If
        End If

        If IsThereAThirdCustomer = True Then
            If Customer3Ped IsNot Nothing Then
                If Customer3Ped.Exists = True Then
                    Customer3Ped.Task.ClearAll()
                    Customer3Ped.Task.GoTo(playerPosition, False)
                    CustomerPed.AlwaysKeepTask = True
                    Ped3Blip = Customer3Ped.AddBlip
                    Ped3Blip.Sprite = 480
                    Ped3Blip.Color = BlipColor.Green
                    Ped3Blip.Scale = 0.8
                End If
            End If
        End If

        If OriginBlip IsNot Nothing Then
            If OriginBlip.Exists Then
                OriginBlip.ShowRoute = False
                OriginBlip.Remove()
            End If
        End If

        CalculateFare(Origin.Coords, Destination.Coords)

        If IsThereASecondCustomer = True Then
            UI_DispatchStatus = "Customers have been notified of your arrival"
        Else
            If Customer.isCeleb Then
                UI_DispatchStatus = Customer.Name & " has been notified of your arrival"
            Else
                UI_DispatchStatus = "Customer has been notified of your arrival"
            End If
        End If

        MiniGameStage = MiniGameStages.PedWalkingToCar

    End Sub

    Private Sub PedHasReachedCar()
        If Game.Player.Character.IsInVehicle Then
            If CustomerPed.Exists Then
                If MaxSeats >= 3 Then
                    CustomerPed.Task.EnterVehicle(Game.Player.Character.CurrentVehicle, VehicleSeat.RightRear, 20000)
                Else
                    CustomerPed.Task.EnterVehicle(Game.Player.Character.CurrentVehicle, VehicleSeat.Passenger, 20000)
                End If
            End If

            If IsThereASecondCustomer = True Then
                If Customer2Ped.Exists = True Then
                    Customer2Ped.Task.EnterVehicle(Game.Player.Character.CurrentVehicle, VehicleSeat.LeftRear, 20000)
                End If
            End If

            If IsThereAThirdCustomer = True Then
                If Customer3Ped.Exists = True Then
                    Customer3Ped.Task.EnterVehicle(Game.Player.Character.CurrentVehicle, VehicleSeat.Passenger, 20000)
                End If
            End If
        End If

        MiniGameStage = MiniGameStages.PedGettingInCar
    End Sub

    Private Sub PedHasEnteredCar()
        PickupTime = Game.GameTime

        If Game.Player.Character.IsInVehicle = True Then
            Game.Player.Character.CurrentVehicle.LeftIndicatorLightOn = False
            Game.Player.Character.CurrentVehicle.RightIndicatorLightOn = False
        End If

        CalculateTipParameters()

        DestinationBlip = World.CreateBlip(Destination.Coords)
        DestinationBlip.Color = BlipColor.Blue
        DestinationBlip.ShowRoute = True

        If CustomerPed IsNot Nothing Then
            If CustomerPed.Exists Then
                Ped1Blip.Remove()
            End If
        End If

        If IsThereASecondCustomer = True Then
            If Customer2Ped IsNot Nothing Then
                If Customer2Ped.Exists Then
                    Ped2Blip.Remove()
                End If
            End If
        End If

        If IsThereAThirdCustomer = True Then
            If Customer3Ped IsNot Nothing Then
                If Customer3Ped.Exists Then
                    Ped3Blip.Remove()
                End If
            End If
        End If


        UI_DispatchStatus = "Please drive the customer to the destination"
        If IsThereASecondCustomer = True Then
            UI_DispatchStatus = "Please drive the customers to their destination"
        End If
        MiniGameStage = MiniGameStages.DrivingToDestination
    End Sub

    Private Sub PlayerHasArrivedAtDestination()
        UpdateDist2 = False
        DestinationArrivalTime = Game.GameTime
        CalculateTip()
        UI_DispatchStatus = "Please stop at the marker"
        MiniGameStage = MiniGameStages.StoppingAtDestination
    End Sub

    Private Sub PlayerHasStoppedAtDestination()


        If Game.Player.Character.IsInVehicle = True Then
            If UseHazardLights = True Then
                Game.Player.Character.CurrentVehicle.LeftIndicatorLightOn = True
                Game.Player.Character.CurrentVehicle.RightIndicatorLightOn = True
            End If
        End If

        MiniGameStage = MiniGameStages.PedGettingOut

        DestinationBlip.ShowRoute = False
        DestinationBlip.Remove()


        Dim isDestinationSet As Boolean
        If Destination.PedEnd.X = 0 And Destination.PedEnd.Y = 0 And Destination.PedEnd.Z = 0 Then
            isDestinationSet = False
        Else
            isDestinationSet = True
        End If


        Dim TargetPoint As Vector3
        If isDestinationSet = False Then
            TargetPoint = Destination.PedStart
        Else
            TargetPoint = Destination.PedEnd
        End If


        If CustomerPed IsNot Nothing Then
            If CustomerPed.Exists = True Then
                Dim LeaveSequence As New TaskSequence
                LeaveSequence.AddTask.Wait(RND.Next(200, 900))
                LeaveSequence.AddTask.LeaveVehicle()
                LeaveSequence.AddTask.Wait(250)
                LeaveSequence.AddTask.GoTo(TargetPoint, False)
                LeaveSequence.AddTask.Wait(10000)
                LeaveSequence.Close(False)
                CustomerPed.Task.PerformSequence(LeaveSequence)
                LeaveSequence.Dispose()
                CustomerPed.AlwaysKeepTask = True
                CustomerPed.MarkAsNoLongerNeeded()
                CustomerPed.IsPersistent = False
            End If
        End If

        If IsThereASecondCustomer = True Then
            If Customer2Ped IsNot Nothing Then
                If Customer2Ped.Exists Then
                    Dim LeaveSequence As New TaskSequence
                    LeaveSequence.AddTask.Wait(RND.Next(200, 1200))
                    LeaveSequence.AddTask.LeaveVehicle()
                    LeaveSequence.AddTask.Wait(250)
                    LeaveSequence.AddTask.GoTo(TargetPoint, False)
                    LeaveSequence.AddTask.Wait(10000)
                    Customer2Ped.Task.PerformSequence(LeaveSequence)
                    LeaveSequence.Dispose()
                    Customer2Ped.AlwaysKeepTask = True
                    Customer2Ped.MarkAsNoLongerNeeded()
                    Customer2Ped.IsPersistent = False
                End If
            End If
        End If


        If IsThereAThirdCustomer = True Then
            If Customer3Ped IsNot Nothing Then
                If Customer3Ped.Exists Then
                    Dim LeaveSequence As New TaskSequence
                    LeaveSequence.AddTask.Wait(RND.Next(1000, 1400))
                    LeaveSequence.AddTask.LeaveVehicle(Game.Player.Character.CurrentVehicle, True)
                    LeaveSequence.AddTask.Wait(250)
                    LeaveSequence.AddTask.GoTo(TargetPoint, False)
                    LeaveSequence.AddTask.Wait(10000)
                    Customer3Ped.Task.PerformSequence(LeaveSequence)
                    LeaveSequence.Dispose()
                    Customer3Ped.AlwaysKeepTask = True
                    Customer3Ped.MarkAsNoLongerNeeded()
                    Customer2Ped.IsPersistent = False
                End If
            End If
        End If

        Game.Player.Character.CurrentVehicle.CloseDoor(VehicleDoor.FrontRightDoor, False)
        Game.Player.Character.CurrentVehicle.CloseDoor(VehicleDoor.BackRightDoor, False)
        Game.Player.Character.CurrentVehicle.CloseDoor(VehicleDoor.BackLeftDoor, False)


        VehicleEndHealth = Game.Player.Character.CurrentVehicle.Health
        VehicleEndHealthPercent = VehicleEndHealth / Game.Player.Character.CurrentVehicle.MaxHealth

        If VehicleEndHealthPercent < 0.75 Then
            'PLAYER BANGED UP THE CAR A LOT
            GTA.UI.ShowSubtitle("Your car is in terrible condition! No way am I paying the full fare!", 6000)
            FareTotal *= CInt(VehicleEndHealthPercent)
            PayPlayer(FareTotal)
            FareTip = 0

        ElseIf VehicleEndHealth < VehicleStartHealth - 15 Then
            Dim i = RND.Next(1, 6)
            'PLAYER HAD A MINOR ACCIDENT ALONG THE WAY

            Dim msg As String
            Select Case i
                Case 1
                    msg = "You need to learn how to drive better!"
                Case 2
                    msg = "Where the hell did you get your license? You're lucky I'm paying anything at all."
                Case 3
                    msg = "You could've killed me with your reckless driving!"
                Case 4
                    msg = "How the hell did you get a job driving like that?"
                Case 4
                    msg = "I can't believe they hired such a dangerous driver!"
                Case Else
                    msg = "You're a terrible driver. I'm not paying the full fare."
            End Select

            GTA.UI.ShowSubtitle("~r~" & msg, 6000)

            FareTotal *= CInt(VehicleEndHealthPercent)
            PayPlayer(FareTotal)
            FareTip = 0

        Else
            PayPlayer(FareTotal)
            If FareTip > 0 Then
                PayPlayer(FareTip)
            End If

        End If


        Dim MessageText, SubjectText As String
        If FareTip > 0 Then
            SubjectText = "Payment Received: $" & FareTotal + FareTip
            MessageText = "Fare: $" & FareTotal & ", Tip: $" & FareTip
        Else
            SubjectText = "Payment Received: $" & FareTotal
            MessageText = ""
        End If

        If Game.Player.Character.CurrentVehicle.DisplayName = "TAXI" Then
            DisplayFancyMessage("Downtown Cab Co", SubjectText, "CHAR_TAXI", MessageText, 6000)
        Else
            Dim pID = Native.Function.Call(Of Integer)(Native.Hash.GET_PED_TYPE, Game.Player.Character)
            If pID = 0 Then
                DisplayFancyMessage("Maze Bank", SubjectText, "CHAR_BANK_MAZE", MessageText, 6000)
            ElseIf pID = 1 Then
                DisplayFancyMessage("Fleeca", SubjectText, "CHAR_BANK_FLEECA", MessageText, 6000)
            Else
                DisplayFancyMessage("Bank of Liberty", SubjectText, "CHAR_BANK_BOL", MessageText, 6000)
            End If
        End If


        If DoAutosave = True Then
            Native.Function.Call(Native.Hash.DO_AUTO_SAVE)
        End If

        If PedsAroundOrigin IsNot Nothing Then
            For Each p As Ped In PedsAroundOrigin
                If p.Gender = Gender.Male Then p.RelationshipGroup = 1
                If p.Gender = Gender.Female Then p.RelationshipGroup = 2
                p.MarkAsNoLongerNeeded()
                p.IsPersistent = False
            Next
        End If
        PedsAroundOrigin = Nothing

        If PedsAroundDestination IsNot Nothing Then
            For Each p As Ped In PedsAroundDestination
                If p.Gender = Gender.Male Then p.RelationshipGroup = 1
                If p.Gender = Gender.Female Then p.RelationshipGroup = 2
                p.MarkAsNoLongerNeeded()
                p.IsPersistent = False
            Next
        End If
        PedsAroundDestination = Nothing

        MiniGameStage = MiniGameStages.SearchingForFare
        SetStandbySpecs()
    End Sub

    Public Sub SetStandbySpecs()
        UI_DispatchStatus = "Standby, looking for fares..."
        UI_Destination = ""
        UI_Origin = ""


        Dim maxWaitTime As Integer = 3
        Select Case IngameDay
            Case Is > 4 'FRI + SAT + SUN
                Select Case IngameHour
                    Case 0 To 3
                        maxWaitTime = 4
                    Case 4 To 5
                        maxWaitTime = 30
                    Case 6 To 10
                        maxWaitTime = 12
                    Case 11 To 13
                        maxWaitTime = 10
                    Case 14 To 17
                        maxWaitTime = 4
                    Case 18 To 23
                        maxWaitTime = 3
                End Select
            Case Else 'MON - THU
                Select Case IngameHour
                    Case 0
                        maxWaitTime = 9
                    Case 1 To 5
                        maxWaitTime = 40
                    Case 6 To 10
                        maxWaitTime = 5
                    Case 11 To 14
                        maxWaitTime = 11
                    Case 15 To 18
                        maxWaitTime = 5
                    Case 19 To 22
                        maxWaitTime = 4
                    Case 23
                        maxWaitTime = 8
                End Select
        End Select


        Dim r As Integer = RND.Next(500, maxWaitTime * 1000)
        NextMissionStartTime = Game.GameTime + r

        If Game.Player.Character.IsInVehicle = True Then
            Game.Player.Character.CurrentVehicle.LeftIndicatorLightOn = False
            Game.Player.Character.CurrentVehicle.RightIndicatorLightOn = False
        End If

    End Sub

    Private Sub CustomerHasDied()

        Dim isPed1Alive As Boolean = True
        Dim isPed2Alive As Boolean = True
        Dim isPed3Alive As Boolean = True

        If CustomerPed.Exists Then
            If CustomerPed.IsDead Then
                isPed1Alive = False
            End If
        End If

        If IsThereASecondCustomer Then
            If Customer2Ped.Exists Then
                If Customer2Ped.IsDead Then
                    isPed2Alive = False
                End If
            End If
        End If

        If IsThereAThirdCustomer Then
            If Customer3Ped.Exists Then
                If Customer3Ped.IsDead Then
                    isPed3Alive = False
                End If
            End If
        End If

        Dim TotalPeds As Integer
        If IsThereAThirdCustomer = True Then
            TotalPeds = 3
        ElseIf IsThereASecondCustomer = True Then
            TotalPeds = 2
        Else
            TotalPeds = 1
        End If


        Dim PedsAlive As Integer
        If TotalPeds = 3 Then
            PedsAlive = CInt(isPed1Alive) + CInt(isPed2Alive) + CInt(isPed3Alive)
        ElseIf TotalPeds = 2 Then
            PedsAlive = CInt(isPed1Alive) + CInt(isPed2Alive)
        Else
            PedsAlive = CInt(isPed1Alive)
        End If

        Dim DeathMessage As String

        If TotalPeds = 3 Then

            If PedsAlive = 1 Then
                DeathMessage = "Two of your customers have died!"
            ElseIf PedsAlive = 2 Then
                DeathMessage = "One of your customers has died!"
            Else
                DeathMessage = "Your customers have died!"
            End If

        ElseIf TotalPeds = 2 Then

            If PedsAlive = 1 Then
                DeathMessage = "One of your customers has died!"
            Else
                DeathMessage = "Your customers have died!"
            End If

        Else
            DeathMessage = "Your customer has died!"
        End If


        GTA.UI.Notify(DeathMessage & " You cannot complete this fare.")
        EndMinigame(True)
    End Sub


    Private Sub DisplayFancyMessage(msgSender As String, msgSubject As String, clanTag As String, msgText As String, duration As Integer)
        Native.Function.Call(Native.Hash._SET_NOTIFICATION_TEXT_ENTRY, "STRING")
        If msgText <> "" Then Native.Function.Call(Native.Hash._0x6C188BE134E074AA, msgText)
        Native.Function.Call(Native.Hash._SET_NOTIFICATION_MESSAGE_CLAN_TAG_2, clanTag, clanTag, False, 4, msgSender, msgSubject, 1.0F, "")
        Native.Function.Call(Native.Hash._DRAW_NOTIFICATION, duration, False)
    End Sub

End Class



'PEDS

Public Class Person
    Public Name As String = ""
    Public Model As String = ""
    Public isCeleb As Boolean = False

    Public Sub New(n As String, mdl As String, celeb As Boolean)
        Name = n
        Model = mdl
        isCeleb = celeb
        ListOfPeople.Add(Me)
    End Sub
End Class

Public Module People
    Public ListOfPeople As New List(Of Person)

    Public PoppyMitchell As New Person("Poppy Mitchell", "u_f_y_poppymich", True)
    Public PamelaDrake As New Person("Pamela Drake", "u_f_o_moviestar", True)
    Public MirandaCowan As New Person("Miranda Cowan", "u_f_m_miranda", True)
    Public AlDiNapoli As New Person("Al Di Napoli", "u_m_m_aldinapoli", True)
    Public MarkFostenburg As New Person("Mark Fostenburg", "u_m_m_markfost", True)
    Public WillyMcTavish As New Person("Willy McTavish", "u_m_m_willyfist", True)
    Public TylerDixon As New Person("Tyler Dixon", "ig_tylerdix", True)
    Public LazlowJones As New Person("Lazlow Jones", "ig_lazlow", True)
    Public IsiahFriedlander As New Person("Isiah Friedlander", "ig_drfriedlander", True)
    Public FabienLaRouche As New Person("Fabien LaRouche", "ig_fabien", True)
    Public PeterDreyfuss As New Person("Peter Dreyfuss", "ig_dreyfuss", True)
    Public KerryMcintosh As New Person("Kerry McIntosh", "ig_kerrymcintosh", True)
    Public JimmyBoston As New Person("Jimmy Boston", "ig_jimmyboston", True)
    Public MiltonMcIlroy As New Person("Milton McIlroy", "cs_milton", True)
    Public AnitaMendoza As New Person("Anita Mendoza", "csb_anita", True)
    Public HughHarrison As New Person("Hugh Harrison", "csb_hugh", True)
    Public ImranShinowa As New Person("Imran Shinowa", "csb_imran", True)
    Public SolomonRichards As New Person("Solomon Richards", "ig_solomon", True)
    Public AntonBeaudelaire As New Person("Anton Beaudelaire", "u_m_y_antonb", True)
    Public AbigailMathers As New Person("Abigail Mathers", "c_s_b_abigail", True)
    Public ChrisFormage As New Person("Cris Formage", "cs_chrisformage", True)
    'ADD EPSILON MEMBERS

    Public NonCeleb As New Person("", "", False)

End Module



'LOCATIONS

Public Enum LocationType
    Residential
    Entertainment
    Shopping
    Restaurant
    FastFood
    Bar
    Theater
    StripClub
    Sport
    HotelLS
    MotelLS
    MotelBC
    AirportDepart
    AirportArrive
    Religious
    Media
    School
    Office
    Factory
    StreetSidePickup
End Enum

Public Class Location
    Public Name As String = ""
    Public Type As LocationType
    Public Coords As New Vector3(0, 0, 0)
    Public PedStart As New Vector3(0, 0, 0)
    Public PedStartHDG As Integer
    Public PedEnd As New Vector3(0, 0, 0)
    Public isValidDestination As Boolean = False

    Public Sub New(n As String, coord As Vector3, t As LocationType, StartPos As Vector3, StartHeading As Integer, Optional ValidAsDestination As Boolean = True)
        Name = n
        Coords = coord
        Type = t
        PedStart = StartPos
        PedStartHDG = StartHeading
        isValidDestination = ValidAsDestination
        ListOfPlaces.Add(Me)
    End Sub
End Class

Public Module Places

    Public ListOfPlaces As New List(Of Location)

    Public lAirportD As New List(Of Location)
    Public lAirportA As New List(Of Location)
    Public lHotelLS As New List(Of Location)
    Public lMotelLS As New List(Of Location)
    Public lMotelBC As New List(Of Location)
    Public lResidential As New List(Of Location)
    Public lEntertainment As New List(Of Location)
    Public lBar As New List(Of Location)
    Public lShopping As New List(Of Location)
    Public lRestaurant As New List(Of Location)
    Public lFastFood As New List(Of Location)
    Public lReligious As New List(Of Location)
    Public lSport As New List(Of Location)
    Public lOffice As New List(Of Location)
    Public lFactory As New List(Of Location)
    Public lStripClub As New List(Of Location)
    Public lEpsilon As New List(Of Location)
    Public lTheater As New List(Of Location)
    Public lSchool As New List(Of Location)

    'NULLS
    Public StreetSidePickup As New Location("Streetside pickup", Vector3.Zero, LocationType.StreetSidePickup, Vector3.Zero, 0)

    'SCHOOL
    Public ULSA1 As New Location("ULSA Campus", New Vector3(-1572.412, 175.073, 57.622), LocationType.School, New Vector3(-1577.04, 183.68, 58.88), 219)
    Public ULSA2 As New Location("ULSA Campus", New Vector3(-1644.79, 141.821, 61.468), LocationType.School, New Vector3(-1649.18, 150.28, 62.17), 216)
    Public VineInst As New Location("Vinewood Institute", New Vector3(172.5, -34.9, 67.3), LocationType.School, New Vector3(-173.1, -26.6, 68.3), 159)
    Public ULSA3 As New Location("ULSA Annexe", New Vector3(-1209.7, -413.7, 33.3), LocationType.School, New Vector3(-1212.6, -407.6, 33.8), 210)

    'RELIGIOUS
    Public EpsilonHQ As New Location("Epsilon HQ", New Vector3(-695.732, 39.476, 42.895), LocationType.Religious, New Vector3(-696.74, 44.1, 43.32), 179)
    Public HillValleyChurch As New Location("Hill Valley Church", New Vector3(-1688.557, -297.007, 51.34), LocationType.Religious, New Vector3(-1685.52, -292.62, 51.89), 190)
    Public RockfordHillsChurch As New Location("Rockford Hills Church", New Vector3(-761.49, -37.93, 36.97), LocationType.Religious, New Vector3(-766.56, -23.58, 41.08), 210)
    Public LittleSeoulChurch As New Location("Little Seoul Church", New Vector3(-768.87, -667.37, 29.15), LocationType.Religious, New Vector3(-765.65, -684.76, 30.09), 1)
    Public StBrigidBaptist As New Location("St Brigid Baptist Church", New Vector3(-340.22, 6160.46, 31.01), LocationType.Religious, New Vector3(-331.65, 6150.4, 32.31), 85)
    Public ParsonsRehab As New Location("Parsons Rehab Center", New Vector3(-1528.9, 857.2, 180.9), LocationType.Religious, New Vector3(-1521, 853.4, 181.6), 44)
    Public Friedlander As New Location("Dr. Friedlander's Office", New Vector3(-1897.4, -557.1, 11.3), LocationType.Religious, New Vector3(-1896.1, -570.3, 11.8), 322)
    Public Pagoda As New Location("The Pagoda", New Vector3(-862.4, -844, 19), LocationType.Religious, New Vector3(-879.2, -859.3, 19.1), 285)
    Public Serenity As New Location("Serenity Wellness", New Vector3(-513.7, 20.2, 43.8), LocationType.Religious, New Vector3(-502.1, 32.2, 44.7), 171)
    Public StraMort As New Location("Strawberry Mortuary", New Vector3(405.7, -1477.1, 28.9), LocationType.Religious, New Vector3(411.9, -1488, 30.1), 30)
    Public RanchoChurch As New Location("Rancho Church", New Vector3(508.1, -1730.3, 28.7), LocationType.Religious, New Vector3(518.9, -1733.8, 30.7), 109)


    'SPORT
    Public Golfcourse As New Location("Los Santos Country Club", New Vector3(-1378.862, 45.125, 53.367), LocationType.Sport, New Vector3(-1367.97, 56.55, 53.83), 92)
    Public TennisRichman As New Location("Richman Hotel Tennis Courts", New Vector3(-1256.139, 396.519, 74.882), LocationType.Sport, New Vector3(-1255.72, 371.67, 75.87), 51) With {.PedEnd = New Vector3(-1230.9, 365.97, 79.98)}
    Public ULSATennis As New Location("ULSA Training Center Tennis Courts", New Vector3(-1654.964, 291.968, 59.93), LocationType.Sport, New Vector3(-1639.79, 275.91, 59.55), 244)
    Public ULSATrainCtr As New Location("ULSA Training Center", New Vector3(-1654.9, 291.9, 60), LocationType.Sport, New Vector3(-1650.3, 274, 59.6), 296)
    Public ULSAStadium As New Location("ULSA Stadium", New Vector3(-1680.3, 282.2, 60.9), LocationType.Sport, New Vector3(-1675.3, 265.6, 62.4), 346)
    Public ULSAAnnexe As New Location("ULSA Annexe", New Vector3(-1209.6, -413.6, 33.2), LocationType.Sport, New Vector3(-1213.2, -407, 34.1), 201)
    Public PumpRun As New Location("Pump & Run Gym, Rockford Hills", New Vector3(-1266.5, -342.4, 36.2), LocationType.Sport, New Vector3(-1261.1, -348.3, 36.8), 29)
    Public DeckerPark As New Location("Decker Park", New Vector3(-864.56, -679.59, 27), LocationType.Sport, New Vector3(-878.9, -679.3, 27.8), 337)
    Public KoreanPavilion As New Location("Korean Pavilion", New Vector3(-874.5, -836, 18.6), LocationType.Sport, New Vector3(-891.3, -852.9, 20.6), 113)
    Public PBCC As New Location("Pacific Bluffs Country Club", New Vector3(-3016.76, 85.64, 11.2), LocationType.Sport, New Vector3(-3023.85, 80.96, 11.61), 317)
    Public RatonCanyonTrails As New Location("Raton Canyon Trails", New Vector3(-1511.41, 4971.09, 61.95), LocationType.Sport, New Vector3(-1492.77, 4968.99, 63.93), 75) With {.PedEnd = New Vector3(-1573.99, 4848.2, 60.58)}
    Public sDPBeachS As New Location("South Del Perro Beach", New Vector3(-1457.92, -963.99, 6.75), LocationType.Sport, New Vector3(-1463.03, -979.96, 6.91), 181)
    Public sDPBeachC As New Location("Central Del Perro Beach", New Vector3(-1725.3, -733.1, 9.9), LocationType.Sport, New Vector3(-1730.3, -739.3, 9.95), 230)
    Public sVBeachN1 As New Location("North Vespucci Beach", New Vector3(-1400.62, -1028.97, 3.88), LocationType.Sport, New Vector3(-1413.8, -1046.67, 4.62), 135)
    Public sVBeachN2 As New Location("North Vespucci Beach", New Vector3(-1345.2, -1205.1, 4.2), LocationType.Sport, New Vector3(-1367.04, -1196.21, 4.45), 212)
    Public sDPBeachN As New Location("North Del Perro Beach", New Vector3(-1862.15, -616.73, 10.76), LocationType.Sport, New Vector3(-1868.61, -631.09, 11.09), 130)
    Public BrokerPark1 As New Location("North Broker Park", New Vector3(766.1, -170, 74.3), LocationType.Sport, New Vector3(756.6, -179.4, 73.7), 348) With {.PedEnd = New Vector3(712.2, -238, 65.8)}
    Public BrokerPark2 As New Location("East Broker Park", New Vector3(932.7, -272.5, 66.9), LocationType.Sport, New Vector3(925.5, -277.4, 67.6), 300) With {.PedEnd = New Vector3(877.8, -289.8, 65.3)}
    Public MirrorPark1 As New Location("Northwest Mirror Park", New Vector3(1040.04, -531.29, 60.78), LocationType.Sport, New Vector3(1046.1, -535.54, 61.03), 207) With {.PedEnd = New Vector3(1057.2, -714, 56.4)}
    Public MirrorPark2 As New Location("Northeast Mirror Park", New Vector3(1162.2, -520, 64.5), LocationType.Sport, New Vector3(1156.5, -527.8, 64.5), 315) With {.PedEnd = New Vector3(1135.2, -545.8, 62.1)}
    Public MirrorPark3 As New Location("East Mirror Park", New Vector3(1173, -642.7, 62), LocationType.Sport, New Vector3(1165.9, -646.6, 62), 297) With {.PedEnd = New Vector3(1119.7, -624.8, 56.4)}
    Public MirrorPark4 As New Location("Southeast Mirror Park", New Vector3(1175.4, -754.7, 57.5), LocationType.Sport, New Vector3(1172.3, -749.1, 57.8), 223) With {.PedEnd = New Vector3(1119.7, -624.8, 56.4)}
    Public PuertaDelSol As New Location("Puerta Del Sol Yacht Club", New Vector3(-816.06, -133.44, 4.62), LocationType.Sport, New Vector3(-816.41, -1346.46, 5.15), 48)
    Public KoiSpa As New Location("Koi Retreat & Spa", New Vector3(-1047.08, -1467.7, 4.6), LocationType.Sport, New Vector3(-1040.63, -1474.9, 5.6), 53)
    Public ParkSheld As New Location("N Sheldon Ave Park", New Vector3(-807.1, 826.7, 202.6), LocationType.Sport, New Vector3(-805.7, 840.9, 203.5), 359)
    Public GWCGolf As New Location("GWC and Golfing Society", New Vector3(-1125.4, 380.2, 70), LocationType.Sport, New Vector3(-1139.8, 376.2, 71.3), 313)
    Public InnocencePark As New Location("Innocence Blvd Park", New Vector3(385.2, -1552.2, 28.8), LocationType.Sport, New Vector3(387.1, -1541, 29.3), 146) With {.PedEnd = New Vector3(397.3, -1523.3, 29.3)}
    Public BJSmith As New Location("B.J. Smith Rec Center & Park", New Vector3(-212.8, -1495.5, 30.9), LocationType.Sport, New Vector3(-223.7, -1499.6, 32), 301) With {.PedEnd = New Vector3(-239.1, -1515.4, 33.4)}
    Public MuscGym As New Location("Muscle Gymnasium", New Vector3(124.6, 348.7, 111.7), LocationType.Sport, New Vector3(127.9, 342.3, 111.9), 29)
    Public SalvMount As New Location("Salvation Mountain", New Vector3(2478.1, 3819.9, 39.8), LocationType.Sport, New Vector3(2481.7, 3808.6, 40.3), 26)
    Public LagoZanWet As New Location("Lago Zancudo Wetlands Trail", New Vector3(-2208.3, 2317.8, 32.4), LocationType.Sport, New Vector3(-2211.8, 2322.4, 32.5), 11)
    Public CottagePark As New Location("Cottage Park", New Vector3(-940, 299.9, 70.5), LocationType.Sport, New Vector3(-949.6, 302.7, 70.7), 265)
    Public Whitewater As New Location("Whitewater Activity Center", New Vector3(-1502.9, 1495, 115.2), LocationType.Sport, New Vector3(-1505.5, 1511.7, 115.3), 250)
    Public IAAPlaza1 As New Location("IAA Plaza", New Vector3(81.1, -679.7, 43.7), LocationType.Sport, New Vector3(93, -683.9, 44.2), 65) With {.PedEnd = New Vector3(182.8, -710.1, 39.4)}
    Public PuertoTennis As New Location("Puerto Del Sol Tennis Courts", New Vector3(-936.9, -1229.3, 4.8), LocationType.Sport, New Vector3(-934.5, -1237.2, 5.9), 27) With {.PedEnd = New Vector3(-926.8, -1249.8, 8)}
    Public ArthursPass As New Location("Arthur's Pass Trails", New Vector3(-389.8, 1230.3, 325.3), LocationType.Sport, New Vector3(-378.3, 1239.5, 326.8), 270) With {.PedEnd = New Vector3(-340, 1300.1, 336.4)}
    Public GalileoOverlook As New Location("Galileo Observatory Overlook", New Vector3(-351.3, 1165.8, 325.1), LocationType.Sport, New Vector3(-342.5, 1145.7, 325.7), 198)
    Public FruityDance As New Location("Fruity Dance Studio", New Vector3(-1251.2, -1176.6, 6.5), LocationType.Sport, New Vector3(-1246.8, -1182.2, 7.7), 28)

    'FAST FOOD
    Public DPPUnA As New Location("Up-n-Atom, Del Perro Plaza", New Vector3(-1529.8, -444.44, 34.73), LocationType.FastFood, New Vector3(-1552.48, -440, 40.52), 229)
    Public DPPTaco As New Location("Taco Bomb, Del Perro Plaza", New Vector3(-1529.8, -444.44, 34.73), LocationType.FastFood, New Vector3(-1552.48, -440, 40.52), 229)
    Public DPPBean As New Location("Bean Machine, Del Perro Plaza", New Vector3(-1529.8, -444.44, 34.73), LocationType.FastFood, New Vector3(-1548.81, -435.8, 35.89), 240)
    Public DPPChihu As New Location("Chihuahuha Hotdogs, Del Perro Plaza", New Vector3(-1529.8, -444.44, 34.73), LocationType.FastFood, New Vector3(-1534.29, -421.96, 35.59), 211)
    Public DPPBite As New Location("Bite, Del Perro Plaza", New Vector3(-1529.8, -444.44, 34.73), LocationType.FastFood, New Vector3(-1540.61, -428.96, 35.59), 254)
    Public DPPWig As New Location("Wigwam Burger, Del Perro Plaza", New Vector3(-1529.8, -444.44, 34.73), LocationType.FastFood, New Vector3(-1535.02, -451.73, 35.88), 308)
    Public TacoLibre As New Location("Taco Libre", New Vector3(-1181.24, -1270.67, 5.57), LocationType.FastFood, New Vector3(-1169.86, -1264.5, 6.6), 152)
    Public UnAVine As New Location("Up-n-Atom", New Vector3(70.47, 258.4, 108.45), LocationType.FastFood, New Vector3(78.34, 273.76, 110.21), 198)
    Public WigVesp As New Location("Wigwam Burger", New Vector3(-850.75, -1149.78, 5.6), LocationType.FastFood, New Vector3(-861.85, -1142.63, 6.99), 234)
    Public BeanLitSeo As New Location("Bean Machine", New Vector3(-826.09, -641.22, 26.91), LocationType.FastFood, New Vector3(-839, -609.28, 29.03), 146)
    Public BeanRockHill As New Location("Bean Machine", New Vector3(-826, -357.1, 37.1), LocationType.FastFood, New Vector3(-843.4, -352, 38.7), 301)
    Public BeanArirang As New Location("Bean Machine, Arirang Plaza", New Vector3(-654.5, -820.3, 24.3), LocationType.FastFood, New Vector3(-659.2, -814.3, 24.5), 228)
    Public BeanQuikHouse As New Location("Bean Machine, Quik House", New Vector3(-320.6, -837.3, 31.2), LocationType.FastFood, New Vector3(-310.2, -826.3, 32.4), 93)
    Public BeanMW As New Location("Bean Machine, Morningwood Blvd", New Vector3(-1374.5, -212.1, 43.9), LocationType.FastFood, New Vector3(-1367.6, -209, 44.4), 125)
    Public SHoLitSeo As New Location("S. Ho Korean Noodle House", New Vector3(-826.09, -641.22, 26.91), LocationType.FastFood, New Vector3(-798.27, -635.43, 29.03), 100)
    Public NoodlLS As New Location("Noodle Exchange", New Vector3(260.26, -970.32, 28.7), LocationType.FastFood, New Vector3(272.12, -964.79, 29.3), 42)
    Public NoodlRH As New Location("Noodle Exchange", New Vector3(-1228, -293.2, 37.1), LocationType.FastFood, New Vector3(-1229.6, -286.2, 37.7), 201)
    Public CoolBeansLS As New Location("Cool Beans", New Vector3(260.26, -970.32, 28.7), LocationType.FastFood, New Vector3(263.08, -981.74, 29.36), 86)
    Public CoolBeansVSP As New Location("Cool Beans", New Vector3(-1264.1, -893, 11.2), LocationType.FastFood, New Vector3(-1267.9, -878.4, 11.9), 209)
    Public CoolBeansMP As New Location("Cool Beans", New Vector3(1195.04, -403.86, 67.56), LocationType.FastFood, New Vector3(1181.57, -393.69, 68.02), 227)
    Public Hornys As New Location("Horny's", New Vector3(1239.12, -376.58, 68.6), LocationType.FastFood, New Vector3(1241.25, -367.15, 69.08), 176)
    Public BSDP As New Location("Burger Shot", New Vector3(-1205, -878.4, 12.8), LocationType.FastFood, New Vector3(-1198, -883.9, 13.8), 33)
    Public CocoCafe As New Location("Coconut Cafe", New Vector3(-1104.8, -1451.3, 4.6), LocationType.FastFood, New Vector3(-1110.8, -1453.1, 5.1), 252)
    Public IceMaiden As New Location("Icemaiden", New Vector3(-1173.7, -1428.4, 4), LocationType.FastFood, New Vector3(-1171.9, -1434.6, 4.4), 28)
    Public MusclePeach As New Location("Muscle Peach Cafe", New Vector3(-1187.5, -1528.6, 4), LocationType.FastFood, New Vector3(-1186.9, -1533.7, 4.4), 5)
    Public HeartyTacoMP As New Location("Hearty Taco", New Vector3(1096.5, -370.1, 66.7), LocationType.FastFood, New Vector3(1093.2, -363.9, 67), 172)
    Public HeartyTacoRancho As New Location("Hearty Taco", New Vector3(438, -1457.7, 28.5), LocationType.FastFood, New Vector3(438.7, -1466, 29.3), 75)
    Public CaseysDiner As New Location("Casey's Diner", New Vector3(781.8, -750.6, 26.8), LocationType.FastFood, New Vector3(792.3, -737.9, 27.5), 119)
    Public BeanVine As New Location("Bean Machine on Eclipse", New Vector3(-628.9, 254.4, 81.1), LocationType.FastFood, New Vector3(-628.2, 239, 81.9), 90)
    Public RingFire As New Location("Ring of Fire Chili House", New Vector3(185.7, -1426.9, 28.8), LocationType.FastFood, New Vector3(177, -1437.5, 29.2), 327)
    Public LuckyPluck As New Location("Lucky Plucker Chicken", New Vector3(119.2, -1455.7, 28.8), LocationType.FastFood, New Vector3(132.6, -1462.2, 29.4), 46)
    Public VacaLoco As New Location("La Vaca Loco Burgers", New Vector3(128.6, -1549.8, 28.9), LocationType.FastFood, New Vector3(138.9, -1542.3, 29.1), 225)
    Public TacoFarmer As New Location("The Taco Farmer", New Vector3(-2.1, -1600.4, 28.8), LocationType.FastFood, New Vector3(4.5, -1606, 29.3), 286)
    Public BishopsChicken As New Location("Bishop's Chicken", New Vector3(161.3, -1618.2, 28.8), LocationType.FastFood, New Vector3(169.2, -1633.7, 29.3), 32)
    Public SanAnTaq As New Location("San An Taqueria", New Vector3(988.3, -1411.2, 30.9), LocationType.FastFood, New Vector3(980.8, -1398, 31.5), 206)
    Public SenorBuns As New Location("Señor Buns", New Vector3(-522, -668.6, 32.5), LocationType.FastFood, New Vector3(-528, -678.8, 33.7), 41)
    Public Dickies As New Location("Dickie's Bagels", New Vector3(-1211.4, -1148.4, 7), LocationType.FastFood, New Vector3(-1205, -1146.5, 7.7), 112)
    Public CluckinRockPlaz As New Location("Cluckin' Bell, Rockford Plaza", New Vector3(-132, -261, 42.7), LocationType.FastFood, New Vector3(-138.3, -257, 43.6), 296)
    Public GuidosTakeout As New Location("Guido's Takeout", New Vector3(-437.8, 124, 99.5), LocationType.FastFood, New Vector3(443.8, 134.2, 100), 156)
    Public AguilaBurrito As New Location("Aguila Burrito, Chamberlain Mall", New Vector3(110.5, -1424.8, 28.8), LocationType.FastFood, New Vector3(98.9, -1419.1, 29.4), 322)
    Public Rockpool As New Location("The Rockpool", New Vector3(-1131.3, -1340.5, 4.6), LocationType.FastFood, New Vector3(-1130, -1347.5, 5), 23)
    Public LimeysRH As New Location("Limey's Juice & Smoothies", New Vector3(-1234.8, -296.9, 37), LocationType.FastFood, New Vector3(-1236.3, -288.6, 37.6), 206)
    Public BiteRH As New Location("Bite", New Vector3(-1247, -303.2, 36.8), LocationType.FastFood, New Vector3(-1250.5, -295.1, 37.3), 213)
    Public JavaRH As New Location("java.update", New Vector3(-1250.8, -305.2, 36.7), LocationType.FastFood, New Vector3(-1253.2, -296.4, 37.3), 209)
    Public DickiesMW As New Location("Dickie's Bagels", New Vector3(-1326.2, -287, 39.3), LocationType.FastFood, New Vector3(-1319.2, -282.5, 39.9), 120)
    Public CherryPopper As New Location("CherryPopper", New Vector3(-1350.6, -246.9, 42.1), LocationType.FastFood, New Vector3(-1345.2, -241.7, 42.7), 124)


    'RESTAURANT
    Public LaSpada As New Location("La Spada", New Vector3(-1046.724, -1398.146, 4.949), LocationType.Restaurant, New Vector3(-1038.01, -1396.84, 5.55), 84)
    Public CafeRedemptionPortola As New Location("Cafe Redemption, Portola Dr", New Vector3(-641.08, -308.14, 34.21), LocationType.Restaurant, New Vector3(-634.26, -302.17, 35.06), 131)
    Public LastTrain As New Location("Last Train In Los Santos", New Vector3(-364.02, 251.64, 83.9), LocationType.Restaurant, New Vector3(-369.1, 267.09, 84.84), 186)
    Public AlDentV As New Location("Al Dente's", New Vector3(-1184.04, -1419.9, 3.98), LocationType.Restaurant, New Vector3(-1186.5, -1413.4, 4.4), 199)
    Public PrawnViv As New Location("Prawn Vivant", New Vector3(-1227.5, -1096.9, 7.6), LocationType.Restaurant, New Vector3(-1221.8, -1096.2, 8.1), 107)
    Public Hedera As New Location("Hedera", New Vector3(-1362.3, -800.6, 19), LocationType.Restaurant, New Vector3(-1357.2, -792, 20.2), 135)
    Public PescadoRojo As New Location("Pescado Rojo", New Vector3(-1424.1, -724.1, 23.2), LocationType.Restaurant, New Vector3(-1426.1, -719.1, 23.5), 191)
    Public Sumac As New Location("Sumac Restaurant", New Vector3(-1463.7, -715.8, 24.9), LocationType.Restaurant, New Vector3(-1463.4, -704.9, 26.8), 151)
    Public ChineseSS As New Location("Chinese Restaurant", New Vector3(1887.2, 3708.8, 32.6), LocationType.Restaurant, New Vector3(1893.9, 3714.2, 32.8), 132)
    Public ParkJung As New Location("Park Jung Restaurant", New Vector3(-684.3, -885.7, 24.4), LocationType.Restaurant, New Vector3(-654, -885.1, 24.7), 258)
    Public HitNRun As New Location("Hit'n'Run Coffee", New Vector3(-553.2, -679.2, 32.9), LocationType.Restaurant, New Vector3(-566.6, -679.8, 32.4), 331)
    Public LettuceBe As New Location("Lettuce Be Restaurant", New Vector3(-17.2, -116.7, 56.6), LocationType.Restaurant, New Vector3(-14.5, -110.4, 56.8), 171)
    Public Periscope As New Location("Periscope Restaurant", New Vector3(-481.4, -9, 44.5), LocationType.Restaurant, New Vector3(-482.6, -17.3, 45.1), 350)
    Public Spitroasters As New Location("Spitroasters Meathouse", New Vector3(-242.9, 272.8, 91.2), LocationType.Restaurant, New Vector3(-242.1, 279.3, 92), 189)
    Public LasCuadras As New Location("Las Cuadras Deli", New Vector3(-1448.9, -330.2, 43.7), LocationType.Restaurant, New Vector3(-1471.7, -329.9, 44.8), 308)
    Public GroundPound As New Location("Ground & Pound Cafe", New Vector3(368.5, -1037, 28.5), LocationType.Restaurant, New Vector3(370.6, -1028.1, 29.3), 176)
    Public GroundPoundMW As New Location("Ground & Pound Cafe", New Vector3(-1390.9, -223.3, 44), LocationType.Restaurant, New Vector3(-1395, -229.7, 44.3), 321)
    Public LesBianc1 As New Location("Les Bianco", New Vector3(-717.1, 250.2, 79.3), LocationType.Restaurant, New Vector3(-718.7, 256.5, 79.9), 207)
    Public LesBiancRockPlaz As New Location("Les Bianco, Rockford Plaza", New Vector3(-152.8, -97.2, 54.3), LocationType.Restaurant, New Vector3(-150.6, -106.8, 55), 343)
    Public Haute As New Location("Haute Restaurant", New Vector3(17.4, 234.4, 109), LocationType.Restaurant, New Vector3(4.5, 242, 109.6), 284)
    Public PinkSand As New Location("The Pink Sandwich", New Vector3(104.8, 219.9, 107.4), LocationType.Restaurant, New Vector3(101, 210.1, 107.8), 334)
    Public CafeVespucci As New Location("Cafe Vespucci", New Vector3(-1409.3, -154.5, 47.2), LocationType.Restaurant, New Vector3(-1413, -139.7, 48.8), 195)
    Public WookNoodleHouse As New Location("Wook Noodle House", New Vector3(-648.5, -878.3, 24.2), LocationType.Restaurant, New Vector3(-655, 880.2, 24.6), 288)
    Public GardenRestaurant As New Location("Garden Restaurant", New Vector3(-648.9, -900.4, 24.3), LocationType.Restaurant, New Vector3(-661, -900.7, 24.6), 270)
    Public ViendemorteRockPlaz As New Location("Viendemorte, Rockford Plaza", New Vector3(-245.7, -373, 29.6), LocationType.Restaurant, New Vector3(-236.3, -372.3, 30), 137)
    Public ViendemorteMW As New Location("Viendemorte, Morningwood", New Vector3(-1346.5, -253, 42.1), LocationType.Restaurant, New Vector3(-1338.5, -249.8, 42.7), 127)
    Public VinewoodCafeRockPlaz As New Location("Vinewood Cafe, Rockford Plaza", New Vector3(-227.7, -71.4, 49.3), LocationType.Restaurant, New Vector3(-229, -79.4, 49.8), 358)
    Public CafeAustere As New Location("Café Austère, Rockford Plaza", New Vector3(-127.3, -247, 44), LocationType.Restaurant, New Vector3(-131.4, -239.6, 44.7), 263)
    Public Giovannis As New Location("Giovanni's Italian Restaurant", New Vector3(-1335.8, -859.2, 16.3), LocationType.Restaurant, New Vector3(-1342.6, -871.4, 16.8), 303)
    Public JazzDesserts As New Location("Jazz Desserts", New Vector3(494.9, 103.1, 96), LocationType.Restaurant, New Vector3(502.3, 112.9, 96.6), 164)
    Public PotHeads As New Location("Pot-Heads Seafood Restaurant", New Vector3(-1225.5, -1167.6, 7.2), LocationType.Restaurant, New Vector3(-1229.8, -1174.4, 7.7), 330)
    Public Hookies As New Location("Hookies Seafood Diner", New Vector3(-2203.3, 4265.9, 47.5), LocationType.Restaurant, New Vector3(-2194.4, 4279, 49.2), 134)
    Public ParkView As New Location("Park View Diner", New Vector3(2688.3, 4326.9, 45.4), LocationType.Restaurant, New Vector3(2697.5, 4325, 45.9), 35)
    Public Rex As New Location("Rex's Diner", New Vector3(2557, 2616.2, 37.5), LocationType.Restaurant, New Vector3(2558.1, 2608.3, 38.1), 19)
    Public Chido As New Location("Chido Taqueria", New Vector3(-1506.2, -228.7, 50.6), LocationType.Restaurant, New Vector3(-1497.4, -225.5, 51.3), 151)
    Public BigPuffa As New Location("Big Puffa Seafood Restaurant", New Vector3(-1614.7, -1004.5, 13), LocationType.Restaurant, New Vector3(-1609.7, -999.7, 13), 139)
    Public Surfries As New Location("Surfries Diner", New Vector3(-1253.6, -1073, 7.8), LocationType.Restaurant, New Vector3(-1256.6, -1079.5, 8.4), 341)
    Public SplitKipper As New Location("Split Kipper Fish Restaurant", New Vector3(-1241, -1104.6, 7.5), LocationType.Restaurant, New Vector3(-1246.6, -1105.6, 8.1), 281)
    Public Frenchies As New Location("Frenchie's Restaurant", New Vector3(103.9, -968.2, 28.7), LocationType.Restaurant, New Vector3(97, -964.9, 29.5), 245)

    'BAR
    Public PipelineInn As New Location("Pipeline Inn", New Vector3(-2182.395, -391.984, 12.83), LocationType.Bar, New Vector3(-2192.54, -389.54, 13.47), 249)
    Public EclipseLounge As New Location("Eclipse Lounge", New Vector3(-83, 246.52, 99.77), LocationType.Bar, New Vector3(-84.96, 235.74, 100.56), 2)
    Public MojitoInn As New Location("Mojito Inn", New Vector3(-130.08, 6396.05, 30.88), LocationType.Bar, New Vector3(-121.04, 6394.28, 31.49), 82)
    Public Henhouse As New Location("The Hen House", New Vector3(-295.27, 6248.5, 30.82), LocationType.Bar, New Vector3(-295.15, 6259.08, 31.49), 174)
    Public BayviewLodge As New Location("Bayview Lodge", New Vector3(-700, 5816.39, 16.68), LocationType.Bar, New Vector3(-697.98, 5802.34, 17.33), 54)
    Public Sightings As New Location("Sightings Bar & Restaurant", New Vector3(-865.79, -2543.2, 13.33), LocationType.Bar, New Vector3(-886.79, -2536.09, 14.55), 240)
    Public Tequi As New Location("Tequi-La-La", New Vector3(-564.86, 267.92, 82.43), LocationType.Bar, New Vector3(-567.94, 274.83, 83.02), 194)
    Public DungeonCrawler As New Location("Dungeon Crawler", New Vector3(-259.71, 252.11, 90.59), LocationType.Bar, New Vector3(-264.45, 245.73, 90.77), 344)
    Public Cockatoos As New Location("Cockatoos Nightclub", New Vector3(-421.99, -34.9, 45.75), LocationType.Bar, New Vector3(-430.12, -24.4, 46.23), 274)
    Public PenalCol As New Location("Penal Colony Nightclub", New Vector3(-536.57, -64.83, 40.7), LocationType.Bar, New Vector3(-531.01, -62.84, 41.02), 139)
    Public RobsChum As New Location("Rob's Liquor", New Vector3(-2981.17, 389.71, 14.14), LocationType.Bar, New Vector3(-2968.54, 390.26, 15.04), 292)
    Public RobsVes As New Location("Rob's Liquor", New Vector3(-1230.02, -896.64, 11.43), LocationType.Bar, New Vector3(-1223.81, -906.32, 12.33), 229)
    Public RobsDP As New Location("Rob's Liquor", New Vector3(-1499.01, -394.84, 38.71), LocationType.Bar, New Vector3(-1487.55, -379.76, 40.16), 330)
    Public Bahama As New Location("Bahama Mama's West", New Vector3(-1394.2, -581.62, 29.47), LocationType.Bar, New Vector3(-1389.47, -586.25, 30.26), 10)
    Public Chaps As New Location("Chaps Nightclub", New Vector3(-475.51, -101.19, 38.35), LocationType.Bar, New Vector3(-473.49, -94.59, 39.28), 164)
    Public Singletons As New Location("Singletons Bar", New Vector3(233.21, 301.46, 105.17), LocationType.Bar, New Vector3(221.6, 307.5, 105.57), 194)
    Public Clappers As New Location("Clappers", New Vector3(405.86, 131.85, 101.36), LocationType.Bar, New Vector3(412.27, 150.72, 103.21), 161)
    Public MirrParTav As New Location("Mirror Park Tavern", New Vector3(1209.94, -415.29, 67.26), LocationType.Bar, New Vector3(1217.94, -416.8, 67.78), 78)
    Public RobsMurr As New Location("Rob's Liquor", New Vector3(1149.2, -980.4, 45.7), LocationType.Bar, New Vector3(1136.3, -979.4, 46.4), 25)
    Public HiMen As New Location("Hi-Men Bar", New Vector3(500, -1544.3, 28.8), LocationType.Bar, New Vector3(494.2, -1542.4, 29.3), 257)
    Public BrewersDrop As New Location("Brewer's Drop Liquor Store", New Vector3(89.6, -1315.4, 28.8), LocationType.Bar, New Vector3(98.9, -1309.5, 29.3), 122)
    Public BrougeLiqu As New Location("Brouge Liquor Store", New Vector3(222, -1739.9, 28.4), LocationType.Bar, New Vector3(225.6, -1749.9, 29.3), 33)
    Public Pitchers As New Location("Pitchers On Vinewood", New Vector3(229.1, 347.1, 105.1), LocationType.Bar, New Vector3(225, 337.6, 105.6), 343)
    Public LiquorJr As New Location("Liquor Jr", New Vector3(576.3, 2684.5, 41.4), LocationType.Bar, New Vector3(579.5, 2678.2, 41.8), 29)
    Public Scoops As New Location("Scoops Liquor Barn", New Vector3(1166.1, 2692.2, 37.5), LocationType.Bar, New Vector3(1166.2, 2708.5, 38.2), 3)
    Public Vault As New Location("The Vault", New Vector3(243.9, -1062.2, 28.5), LocationType.Bar, New Vector3(243.3, -1073.7, 29.3), 346)
    Public Shenanigan As New Location("Shenanigan's Bar", New Vector3(246.6, -1009.4, 28.4), LocationType.Bar, New Vector3(254.9, -1013.4, 29.3), 69)
    Public BeerWine As New Location("Strawberry Liquor Store", New Vector3(178.8, -1343.4, 28.8), LocationType.Bar, New Vector3(170.3, -1337.1, 29.3), 274)
    Public YellowJackInn As New Location("Yellow Jack Inn", New Vector3(1959.3, 3846, 31.5), LocationType.Bar, New Vector3(1953, 3842.5, 32.2), 303)
    Public GrapeLiquor As New Location("Grapeseed Liquor Market", New Vector3(2464, 4056, 37.2), LocationType.Bar, New Vector3(2455.9, 4058.3, 38.1), 248)
    Public GrapeLiquor2 As New Location("Grapeseed Liquor Store", New Vector3(2486.5, 4098.7, 37.5), LocationType.Bar, New Vector3(2481.9, 4100.1, 38.1), 239)
    Public LasCuadrasBar As New Location("Las Cuadras Bar", New Vector3(-1446.4, -333.9, 44.2), LocationType.Bar, New Vector3(-1459.8, -339.9, 44.7), 154)

    'SHOPPING
    Public BobMulet As New Location("Bob Mulet Hair & Beauty", New Vector3(-830.47, -190.49, 36.74), LocationType.Shopping, New Vector3(-812.96, -184.69, 37.57), 36)
    Public PonsonPD As New Location("Ponsonbys Portola Drive", New Vector3(-722.99, -162.11, 36.22), LocationType.Shopping, New Vector3(-711.15, -152.5, 37.42), 280)
    Public ChumSU As New Location("SubUrban, Chumash Plaza", New Vector3(-3153.66, 1062.2, 20.25), LocationType.Shopping, New Vector3(-3177.78, 1037.39, 20.86), 341)
    Public ChumInk As New Location("Ink Inc, Chumash Plaza", New Vector3(-3153.66, 1062.2, 20.25), LocationType.Shopping, New Vector3(-3169.05, 1076.69, 20.83), 166)
    Public ChumAmmu As New Location("Ammu-Nation, Chumash Plaza", New Vector3(-3153.66, 1062.2, 20.25), LocationType.Shopping, New Vector3(-3171.64, 1087.57, 20.84), 44)
    Public ChumNelsons As New Location("Nelson's General Store, Chumash Plaza", New Vector3(-3153.66, 1062.2, 20.25), LocationType.Shopping, New Vector3(-3152.25, 1110.59, 20.87), 232)
    Public PonsonRP As New Location("Ponsonbys Rockford Plaza", New Vector3(-148.33, -308.77, 37.83), LocationType.Shopping, New Vector3(-168.03, -299.35, 39.73), 306)
    Public RockfordPlazaWest As New Location("Rockford Plaza West Entrance", New Vector3(-255, -338, 29.5), LocationType.Shopping, New Vector3(-235, -330.8, 30), 306)
    Public JonnyTungPortola As New Location("Jonny Tung, Portola Drive", New Vector3(-611.62, -316.21, 34), LocationType.Shopping, New Vector3(-620.14, -309.45, 34.82), 86)
    Public HelgaKrepp As New Location("Helga Kreppsohle", New Vector3(-647.68, -297.63, 34.51), LocationType.Shopping, New Vector3(-638.52, -293.33, 35.3), 109)
    Public Dalique As New Location("Dalique", New Vector3(-647.68, -297.63, 34.51), LocationType.Shopping, New Vector3(-643.21, -285.69, 35.5), 117)
    Public WinfreyCasti As New Location("Winfrey Castiglione", New Vector3(-659.29, -276.9, 35.02), LocationType.Shopping, New Vector3(-649.25, -276.35, 35.73), 95)
    Public LittlePortola As New Location("Little Portola", New Vector3(-676.69, -215.97, 36.31), LocationType.Shopping, New Vector3(-662.16, -227.05, 37.47), 53)
    Public ArirangPlaza As New Location("Arirang Plaza", New Vector3(-688.49, -826.51, 23.15), LocationType.Shopping, New Vector3(-690.75, -813.6, 23.93), 179)
    Public SimmetAlley As New Location("Simmet Alley", New Vector3(455.45, -820.15, 27.07), LocationType.Shopping, New Vector3(460.21, -794.62, 27.36), 89)
    Public Krapea As New Location("Krapea", New Vector3(330.75, -772.94, 28.68), LocationType.Shopping, New Vector3(337.8, -777.29, 29.27), 69)
    Public Chu247 As New Location("24-7, Chumash Family Pier", New Vector3(-3235.04, 1005.12, 11.85), LocationType.Shopping, New Vector3(-3243.35, 1002.05, 12.83), 173)
    Public ChuHang As New Location("Hang Ten", New Vector3(-2977.1, 433.89, 14.33), LocationType.Shopping, New Vector3(-2965.07, 432.85, 15.28), 94)
    Public ChuTide As New Location("Tidemarks", New Vector3(-2976.36, 457.16, 14.43), LocationType.Shopping, New Vector3(-2963.84, 454.82, 15.32), 91)
    Public PonsonMW As New Location("Ponsonbys", New Vector3(-1456.74, -225.29, 48.34), LocationType.Shopping, New Vector3(-1451.61, -241.21, 49.81), 321)
    Public MexMark As New Location("Mexican Market", New Vector3(402.8, -382.98, 46.06), LocationType.Shopping, New Vector3(392.42, -368.65, 46.81), 217)
    Public DidierPH As New Location("Didier Sachs", New Vector3(-226.51, -962.32, 28.45), LocationType.Shopping, New Vector3(-248.98, -954.6, 31.22), 260)
    Public HawSn As New Location("Hawaiian Snow", New Vector3(278.99, -228.33, 53.27), LocationType.Shopping, New Vector3(281.56, -220.32, 53.98), 147)
    Public WhWid As New Location("White Widow", New Vector3(211.96, -230.93, 53.13), LocationType.Shopping, New Vector3(202.15, -239.65, 53.97), 308)
    Public PillRH As New Location("PillPharm", New Vector3(-382.72, -400.09, 30.95), LocationType.Shopping, New Vector3(-389.93, -421.66, 31.62), 343)
    Public VPAmmu As New Location("Ammu-Nation", New Vector3(237.52, -44.36, 69.28), LocationType.Shopping, New Vector3(251.34, -49.2, 69.94), 45)
    Public Freds As New Location("Fred's Store", New Vector3(337.57, 132.14, 102.6), LocationType.Shopping, New Vector3(333.23, 119.31, 104.31), 310)
    Public BlazingTat As New Location("Blazing Tattoo, Vinewood", New Vector3(316.39, 165.29, 103.28), LocationType.Shopping, New Vector3(320.9, 183.07, 103.59), 221)
    Public DavisMM As New Location("Davis Mega Mall", New Vector3(68.09, -1707.57, 28.67), LocationType.Shopping, New Vector3(61.38, -1728.26, 29.53), 46)
    Public VBSidewMark As New Location("Vespucci Beach Sidewalk Market", New Vector3(-1208.53, -1444.11, 3.9), LocationType.Shopping, New Vector3(-1237.16, -1468.65, 4.29), 126)
    Public ThePit As New Location("The Pit", New Vector3(-1163.91, -1415.34, 4.38), LocationType.Shopping, New Vector3(-1155.54, -1426.46, 4.95), 319)
    Public VespMall As New Location("Vespucci Mall", New Vector3(-803.1, -1095.8, 10.4), LocationType.Shopping, New Vector3(-824, -1084.3, 11.1), 256)
    Public Harmony247 As New Location("24-7 Supermarket", New Vector3(542.8, 2680, 42), LocationType.Shopping, New Vector3(547.7, 2669.5, 42.2), 273)
    Public Gabrielas As New Location("Gabriela's Market", New Vector3(1175, -280.3, 68.5), LocationType.Shopping, New Vector3(1168.8, -290.9, 69), 329)
    Public Leopolds As New Location("Leopolds Rockford Hills", New Vector3(-692.9, -372.1, 33.7), LocationType.Shopping, New Vector3(-697.8, -379.8, 34.5), 334)
    Public EchorockPl As New Location("Echorock Shopping Plaza", New Vector3(94.3, -185.8, 54.3), LocationType.Shopping, New Vector3(106.5, -206, 54.6), 37)
    Public LuxuryAutos As New Location("Luxury Autos", New Vector3(-809.7, -227.5, 36.7), LocationType.Shopping, New Vector3(-803.4, -224.2, 37.2), 113)
    Public Covgari As New Location("Covgari", New Vector3(-778.6, -287.2, 36.5), LocationType.Shopping, New Vector3(-770.7, -287.2, 37.1), 90)
    Public Straw247 As New Location("24-7", New Vector3(25.2, -1357.1, 28.8), LocationType.Shopping, New Vector3(26.6, -1345.5, 29.5), 96)
    Public DiscJew As New Location("Discount Jewels", New Vector3(136.4, -1761.5, 28.5), LocationType.Shopping, New Vector3(130.9, -1771.8, 29.7), 320)
    Public AtomicSvc As New Location("Atomic Service Center", New Vector3(494.8, -1877.9, 25.8), LocationType.Shopping, New Vector3(487, -1878.3, 26.2), 272)
    Public AmmuCypress As New Location("Ammu-Nation", New Vector3(813.7, -2133, 28.9), LocationType.Shopping, New Vector3(810.6, -2156.6, 29.6), 171)
    Public ChambMall As New Location("Chamberlain Mall", New Vector3(96.1, -1379.5, 28.8), LocationType.Shopping, New Vector3(76, -1393.2, 29.4), 100)
    Public EclipsePharm As New Location("Eclipse Pharmacy", New Vector3(1217, -389.1, 68), LocationType.Shopping, New Vector3(1224.4, -390.9, 68.7), 54)
    Public ChicosHyper As New Location("Chico's Hypermarket", New Vector3(1088.5, -764.5, 57.3), LocationType.Shopping, New Vector3(1088.6, -775.7, 58.3), 1)
    Public LeroysElec As New Location("Leroy's Electricals", New Vector3(1128.5, -354.3, 66.8), LocationType.Shopping, New Vector3(1124.6, -345.5, 67.1), 206)
    Public ARMarket As New Location("A&R Market", New Vector3(-1370, -970.6, 8.5), LocationType.Shopping, New Vector3(-1359.9, -963.8, 9.7), 133)
    Public SwallowDP As New Location("Swallow", New Vector3(-1442.5, -606.2, 30.5), LocationType.Shopping, New Vector3(-1433.6, -612.9, 30.8), 58)
    Public DPPlaza As New Location("Del Perro Plaza", New Vector3(-1408.5, -732.9, 23.3), LocationType.Shopping, New Vector3(-1392.4, -747.2, 24.6), 108)
    Public DolPilHarmony As New Location("Dollar Pills", New Vector3(590.4, 2730.5, 41.7), LocationType.Shopping, New Vector3(591.3, 2743.6, 42), 186)
    Public Route68Store As New Location("Route 68 Store", New Vector3(1198.4, 267.2, 37.4), LocationType.Shopping, New Vector3(1201.9, 2655.4, 37.9), 325)
    Public LarrysRV As New Location("Larry's RV Sales", New Vector3(1234.3, 2714.7, 37.6), LocationType.Shopping, New Vector3(1224.4, 2726.8, 38), 204)
    Public GrapeSuper As New Location("Grapeseed Supermarket", New Vector3(1702, 4801.4, 41.4), LocationType.Shopping, New Vector3(1706.2, 4792.6, 42), 87)
    Public AlamoFruit As New Location("Alamo Fruit Market", New Vector3(1800.7, 4586, 36.6), LocationType.Shopping, New Vector3(1792.6, 4593.4, 37.7), 189)
    Public DonsPaleto As New Location("Don's Country Store", New Vector3(158.6, 6620.2, 31.5), LocationType.Shopping, New Vector3(161.8, 6635.9, 31.6), 132)
    Public BincoVesp As New Location("Binco, Vespucci Mall", New Vector3(-812.9, -1083.1, 10.8), LocationType.Shopping, New Vector3(-819.4, -1073.6, 11.3), 193)
    Public AmmuLittleSeoul As New Location("AmmuNation", New Vector3(-663.2, -952.3, 21), LocationType.Shopping, New Vector3(-661, -940.8, 21.8), 128)
    Public SaveACent As New Location("Save-A-Cent", New Vector3(-552.9, -847, 27.7), LocationType.Shopping, New Vector3(-551.9, -854.7, 28.2), 6)
    Public LookSee As New Location("Look-See Opticians", New Vector3(-543.3, -847.3, 28.5), LocationType.Shopping, New Vector3(-544.2, -853.7, 28.8), 3)
    Public KoreanPlaza As New Location("Korean Plaza", New Vector3(-588.7, -1103.5, 21.9), LocationType.Shopping, New Vector3(-600.9, -1111.2, 22.3), 277)
    Public GingerMall As New Location("Ginger Mall", New Vector3(-615.4, -1019.1, 21.6), LocationType.Shopping, New Vector3(-604.2, -1018.2, 22.3), 359)
    Public TimVapid As New Location("Tim Vapid", New Vector3(-549.7, -358.3, 35), LocationType.Shopping, New Vector3(-549.5, -345.9, 35.2), 233)
    Public TSLC As New Location("TSLC", New Vector3(-615, -351.1, 34.5), LocationType.Shopping, New Vector3(-601.9, -347.8, 35.2), 109)
    Public Pendulus As New Location("Pendulus", New Vector3(-572.6, -316.7, 34.7), LocationType.Shopping, New Vector3(-570.2, -323.8, 35.1), 28)
    Public RockfordPlaza As New Location("Rockford Plaza Underground Entrance", New Vector3(-158.2, -162.9, 43.4), LocationType.Shopping, New Vector3(-150.7, -164.8, 43.6), 78)
    Public HairHawick As New Location("Hair on Hawick", New Vector3(-32.1, -136.1, 56.8), LocationType.Shopping, New Vector3(-33.9, -155.2, 57.1), 346)
    Public Kushy As New Location("Kushy Farmacy", New Vector3(-87, -90.5, 57.5), LocationType.Shopping, New Vector3(-87.3, -84.1, 57.8), 212)
    Public ThorsToys As New Location("Thor's Toys", New Vector3(-129, -77.8, 55), LocationType.Shopping, New Vector3(-124.8, -71.6, 56), 152)
    Public GuidoZen As New Location("Guido Zenitalia", New Vector3(-635.4, -174.6, 36.9), LocationType.Shopping, New Vector3(-645.9, -177.5, 37.8), 280)
    Public VineSouv As New Location("Vinewood Souvenir", New Vector3(177, 189.7, 105.2), LocationType.Shopping, New Vector3(172.7, 183.3, 105.7), 341)
    Public MissTVine As New Location("Miss T", New Vector3(222.5, 173.7, 104.9), LocationType.Shopping, New Vector3(227, 164.3, 105.3), 350)
    Public Vankhov As New Location("Vankhov Jewelry", New Vector3(-1381.8, -281.5, 42.6), LocationType.Shopping, New Vector3(-1377.3, -284.7, 43.1), 33)
    Public AmmuMW As New Location("Ammu-Nation", New Vector3(-1323.1, -392.5, 36.1), LocationType.Shopping, New Vector3(-1311.6, -393.9, 36.7), 27)
    Public LongPig As New Location("Long Pig Mini Market", New Vector3(-290.5, -1330.2, 30.8), LocationType.Shopping, New Vector3(-297.6, -1332.6, 31.3), 316)
    Public YogarishimaLittleSeoul As New Location("Yogarishima", New Vector3(-510.4, -860.4, 29.4), LocationType.Shopping, New Vector3(-520.3, -855.8, 30.3), 347)
    Public PopsPillsVinewood As New Location("Pops Pills", New Vector3(109.9, -13.9, 67.1), LocationType.Shopping, New Vector3(114.7, -5.3, 67.8), 189)
    Public EuroLuxCarDealer As New Location("Luxury Car Dealer", New Vector3(-73.4, 56.1, 71.2), LocationType.Shopping, New Vector3(-69.3, 62.9, 71.9), 141)
    Public DesignerSlaveVinewood As New Location("Designer Slave", New Vector3(171.2, 220.3, 105.4), LocationType.Shopping, New Vector3(177.3, 228.3, 106), 157)
    Public JudithMantoyani As New Location("Judith Mantoyani", New Vector3(-743.6, -178.5, 36.4), LocationType.Shopping, New Vector3(-750.2, -183.1, 37.4), 349)
    Public Percii As New Location("Percii", New Vector3(-757.9, -157.5, 36.5), LocationType.Shopping, New Vector3(-765.9, -158.1, 37.6), 269)
    Public CrocAHoop As New Location("Croq-A-Hoop, Rockford Plaza", New Vector3(-127.6, -107.2, 56), LocationType.Shopping, New Vector3(-133, -118.4, 56.6), 333)
    Public Tophes As New Location("Tophes, Rockford Plaza", New Vector3(-156, -330, 36), LocationType.Shopping, New Vector3(-163.6, -330.2, 36.4), 252)
    Public LeChien As New Location("Le Chien, Rockford Plaza", New Vector3(-164, -350, 33.7), LocationType.Shopping, New Vector3(-171, -346.7, 34.6), 241)
    Public DigDenTC As New Location("Digital Den", New Vector3(389.5, -841.3, 28.3), LocationType.Shopping, New Vector3(392.8, -831.9, 29.3), 226)
    Public GutterBloodPill As New Location("Gutter & Blood", New Vector3(302.2, -784.6, 28.5), LocationType.Shopping, New Vector3(289.8, -778.6, 29.3), 255)
    Public GutterBloodRock As New Location("Gutter & Blood", New Vector3(-665.6, -129.2, 37.3), LocationType.Shopping, New Vector3(-670.5, -137.8, 37.9), 306)
    Public GutterBloodMW As New Location("Gutter & Blood", New Vector3(-1338.9, -265, 41.7), LocationType.Shopping, New Vector3(-1331.8, -260.4, 42.3), 118)
    Public RanchFactory As New Location("Ranch Factory Outlet", New Vector3(411.9, -767.6, 28.8), LocationType.Shopping, New Vector3(418.6, -767.6, 29.4), 88)
    Public CentCarpet As New Location("CentCarpet", New Vector3(459.4, -1666.7, 28.7), LocationType.Shopping, New Vector3(465.3, -1673, 29.3), 53)
    Public Glorias As New Location("Gloria's Fashion Boutique", New Vector3(208, -1267.2, 28.8), LocationType.Shopping, New Vector3(199.6, -1268.7, 29.2), 266)
    Public NewDo As New Location("New Do Barber Shop", New Vector3(194.5, -1313.8, 28.8), LocationType.Shopping, New Vector3(186.7, -1310.4, 29.3), 245)
    Public DiscoCloth As New Location("Discount Clothes, Chamberlain Mall", New Vector3(88.4, -1392.4, 28.8), LocationType.Shopping, New Vector3(75.6, -1393.1, 29.4), 90) With {.PedEnd = New Vector3(69.9, -1399.1, 29.4)}
    Public VespSports As New Location("Vespucci Sports", New Vector3(-944.8, -1202.1, 4.7), LocationType.Shopping, New Vector3(-945.7, -1191.6, 5), 169)
    Public FarmerGiles As New Location("Farmer Giles Organic Food Store", New Vector3(-1111.6, -1374.3, 4.6), LocationType.Shopping, New Vector3(-1113.4, -1366.3, 5.1), 208)
    Public BeachBlonde As New Location("Beach Blonde", New Vector3(-1139.7, -1344.4, 4.5), LocationType.Shopping, New Vector3(-1136, -1350.6, 5), 30)
    Public HeroinChic As New Location("Heroin Chic", New Vector3(-1170.7, -1327.5, 4.5), LocationType.Shopping, New Vector3(-1175.7, -1331.9, 5), 256)
    Public FuqueRockHill As New Location("Fuque", New Vector3(-672.4, -115.1, 37.4), LocationType.Shopping, New Vector3(-677.5, -117.1, 37.9), 288)
    Public FuqueMW As New Location("Fuque, Morningwood", New Vector3(-1334, -273.6, 41), LocationType.Shopping, New Vector3(-1326.6, -268.5, 41.6), 123)
    Public CoutureCriminalli As New Location("Couture Criminalli", New Vector3(-671.7, -202, 36.9), LocationType.Shopping, New Vector3(-676.4, -194.9, 37.4), 216)
    Public Ushero As New Location("Ushero", New Vector3(-713.6, -173.5, 36.4), LocationType.Shopping, New Vector3(-707.3, -171.3, 36.9), 114)
    Public DidierRock As New Location("Didier Sachs", New Vector3(-847.3, -158.4, 37.3), LocationType.Shopping, New Vector3(-838.3, -161.5, 37.8), 70)
    Public Sandy247 As New Location("24-7", New Vector3(1968.3, 3735.5, 31.8), LocationType.Shopping, New Vector3(1960.7, 3741.5, 32.3), 125) With {.PedEnd = New Vector3(1965.5, 3747.8, 32.3)}
    Public OShea As New Location("O'Shea's Barber Shop", New Vector3(1937.6, 3719.1, 31.9), LocationType.Shopping, New Vector3(1935, 3728.1, 32.8), 120)
    Public AmmuSandy As New Location("Ammu-Nation", New Vector3(1703.1, 3742.8, 33.2), LocationType.Shopping, New Vector3(1698.8, 3756.5, 34.7), 95)
    Public GrapeFarmersMarket As New Location("Grapeseed Farmers Market", New Vector3(1672.9, 4887.9, 41.5), LocationType.Shopping, New Vector3(1677.7, 4881.5, 42), 39)
    Public GrapeFeed As New Location("Grapeseed Feed Store", New Vector3(1700.6, 4724.9, 41.7), LocationType.Shopping, New Vector3(1710.2, 4728.3, 42.1), 109)
    Public FruitVineRH As New Location("Fruit of the Vine", New Vector3(-1265.7, -312.7, 36.4), LocationType.Shopping, New Vector3(-1270, -305.7, 37), 241)
    Public VitaPharm As New Location("Vitamins Pharmacy", New Vector3(-1296.6, -325.8, 36.1), LocationType.Shopping, New Vector3(-1296.9, -319.6, 36.8), 168)
    Public LSOSC As New Location("LS Office Supply Company", New Vector3(-1328.7, -345.6, 36.1), LocationType.Shopping, New Vector3(-1334.8, -338.1, 36.7), 209)
    Public LoyDisCar As New Location("Loyal Discount Carpets", New Vector3(-1359.8, -361.3, 36.1), LocationType.Shopping, New Vector3(-1360.2, -354.3, 36.7), 203)
    Public WoodysAutos As New Location("Woody's Autos", New Vector3(-1375.7, -370.7, 36.1), LocationType.Shopping, New Vector3(-1377.4, -361.4, 36.6), 167)
    Public PanacheRH As New Location("Panache Cleaners", New Vector3(-1406.4, -391, 36), LocationType.Shopping, New Vector3(-1411.4, -385.9, 36.6), 263)
    Public VineBeautySalon As New Location("Vinewood Beauty Salon", New Vector3(-1434.7, -408.7, 35.4), LocationType.Shopping, New Vector3(-1438.6, -403.5, 36), 207)
    Public VinePawnJewl As New Location("Vinewood Pawn & Jewelry", New Vector3(-1459.5, -421.3, 35.3), LocationType.Shopping, New Vector3(-1459.5, -414, 35.7), 166)
    Public GoTMW As New Location("Grain of Truth", New Vector3(-1422.8, -323.2, 44.1), LocationType.Shopping, New Vector3(-1413, -320.2, 44.2), 85)
    Public Radge As New Location("Radge", New Vector3(-1342.3, -259.7, 42), LocationType.Shopping, New Vector3(-1335.6, -254.8, 42.7), 126)
    Public ForeignAttire As New Location("Foreign Attire, Morningwood Blvd", New Vector3(-1354.6, -241.2, 42.2), LocationType.Shopping, New Vector3(-1347.7, -237.1, 42.8), 124)
    Public GussetMW As New Location("Gussét, Morningwood Blvd", New Vector3(-1361, -231.8, 42.8), LocationType.Shopping, New Vector3(-1354.6, -228.5, 43.4), 117)
    Public GussetPortola As New Location("Gussét, Portola Dr", New Vector3(-719.3, -219.9, 36.5), LocationType.Shopping, New Vector3(-728.6, -223.5, 37.2), 291)
    Public XYZ As New Location("XYZ Store, Morningwood Blvd", New Vector3(-1429.6, -173.3, 46.9), LocationType.Shopping, New Vector3(-1439.8, -174.2, 47.7), 271)
    Public HommeGinaMW As New Location("Homme Gina, Perth St", New Vector3(-1373.6, -257.4, 42.1), LocationType.Shopping, New Vector3(-1381.6, -256.2, 43.1), 255)
    Public HommeGinaCou As New Location("Homme Gina, Cougar Ave", New Vector3(-1474.1, -240.5, 49.4), LocationType.Shopping, New Vector3(-1467.4, -245.8, 50), 39)
    Public HommeGinaPortola As New Location("Homme Gina", New Vector3(-679.3, -241.7, 36.1), LocationType.Shopping, New Vector3(-668.5, -237.6, 36.8), 62)
    Public OldieGoodie As New Location("Oldie but Goodie Antiquities", New Vector3(-1561.4, -200.3, 54.9), LocationType.Shopping, New Vector3(-1561.2, -209.8, 55.5), 0)
    Public WupEtDux As New Location("Wup et Dux, Portola Dr", New Vector3(-732.7, -200.8, 36.6), LocationType.Shopping, New Vector3(-741.8, -206.2, 37.3), 297)
    Public Sessanta As New Location("Sessanta Nove, Portola Dr", New Vector3(-712.9, -239.3, 36.4), LocationType.Shopping, New Vector3(-714.3, -248.4, 37), 351)
    Public Maracas As New Location("Maracas, Portola Dr", New Vector3(-769.6, -133.4, 37.2), LocationType.Shopping, New Vector3(-776.9, -138.7, 37.8), 304)
    Public Farshtunken As New Location("Farshtunken International", New Vector3(-752.1, -165.5, 36.5), LocationType.Shopping, New Vector3(-758.6, -170.3, 37.5), 303)
    Public QuincyBiro As New Location("Quincy Biro, Rockford Dr", New Vector3(-599, -237.6, 35.9), LocationType.Shopping, New Vector3(-605.7, -241.4, 36.6), 301)
    Public Pfister As New Location("Pfister Design, Rockford Dr", New Vector3(-587.8, -258.1, 35.3), LocationType.Shopping, New Vector3(-591.6, -264.1, 35.9), 299)
    Public SantoCapra As New Location("Santo Capra, Rockford Dr", New Vector3(-579.8, -271.2, 34.9), LocationType.Shopping, New Vector3(-586.2, -273.6, 35.6), 299)
    Public Perseus As New Location("Perseus, Rockford Dr", New Vector3(-661.7, -323.1, 34.4), LocationType.Shopping, New Vector3(-666.2, -329.5, 35.2), 295)
    Public Cougari As New Location("Cougari", New Vector3(-778.4, -287.7, 36.5), LocationType.Shopping, New Vector3(-770.6, -287.1, 37.1), 109)
    Public ChebsEaterie As New Location("Chebs Eaterie, Dorset Dr", New Vector3(-730.21, -330.45, 35), LocationType.Shopping, New Vector3(-735.26, -319.63, 36.22), 187)
    Public TotallyWirelessLS As New Location("Totally Wireless Internet Cafe", New Vector3(-766.6, -586.2, 29.9), LocationType.Shopping, New Vector3(-762.6, -601.2, 30.3), 320)
    Public VespBook As New Location("Vespucci Bookstore", New Vector3(-1244.3, -1093.7, 7.6), LocationType.Shopping, New Vector3(-1250.2, -1094.8, 8.2), 284)
    Public Escapism As New Location("Escapism Travel", New Vector3(136.8, -877.8, 29.9), LocationType.Shopping, New Vector3(122, -878, 31.1), 248)
    Public Zorse As New Location("Zorse Leatherware", New Vector3(108.8, -953.3, 29), LocationType.Shopping, New Vector3(101.9, -948.2, 29.6), 247)
    Public BourgeoisBi As New Location("Bourgeois Bicycles", New Vector3(-501.1, -8.3, 44.3), LocationType.Shopping, New Vector3(-500.7, -19.1, 45.1), 35)
    Public Trinculo As New Location("Trinculo Clothing", New Vector3(-1232.6, -1080.5, 7.8), LocationType.Shopping, New Vector3(-1227.4, -1078.1, 8.3), 119)
    Public UUDelPerr As New Location("Universal Uniform", New Vector3(-1168.7, -824.8, 13.8), LocationType.Shopping, New Vector3(-1177, -814, 14.5), 174)

    'ENTERTAINMENT
    Public DelPerroPier As New Location("Del Perro Pier", New Vector3(-1624.56, -1008.23, 12.4), LocationType.Entertainment, New Vector3(-1638, -1012.97, 13.12), 346) With {.PedEnd = New Vector3(-1841.98, -1213.19, 13.02)}
    Public Tramway As New Location("Pala Springs Aerial Tramway", New Vector3(-771.53, 5582.98, 33.01), LocationType.Entertainment, New Vector3(-755.66, 5583.63, 36.71), 91) With {.PedEnd = New Vector3(-745.23, 5594.77, 41.65)}
    Public LSGC As New Location("Los Santos Gun Club", New Vector3(16.86, -1125.85, 29.3), LocationType.Entertainment, New Vector3(20.24, -1107.24, 29.8), 173)
    Public MazeBankArena As New Location("Maze Bank Arena", New Vector3(-235.91, -1863.7, 28.03), LocationType.Entertainment, New Vector3(-260.4, -1897.91, 27.76), 8)
    Public DPBeachS As New Location("South Del Perro Beach", New Vector3(-1457.92, -963.99, 6.75), LocationType.Entertainment, New Vector3(-1463.03, -979.96, 6.91), 181)
    Public VBeachN1 As New Location("North Vespucci Beach", New Vector3(-1400.62, -1028.97, 3.88), LocationType.Entertainment, New Vector3(-1413.8, -1046.67, 4.62), 135)
    Public VBeachN2 As New Location("North Vespucci Beach", New Vector3(-1345.2, -1205.1, 4.2), LocationType.Entertainment, New Vector3(-1367.04, -1196.21, 4.45), 212)
    Public SplitSides As New Location("Split Sides West", New Vector3(-429.43, 252.64, 82.51), LocationType.Entertainment, New Vector3(-423.71, 259.76, 83.1), 167)
    Public ChuFamPier As New Location("Chumash Family Pier", New Vector3(-3235.25, 968.84, 12.59), LocationType.Entertainment, New Vector3(-3239.96, 971.7, 12.7), 90) With {.PedEnd = New Vector3(-3426.4, 967.81, 8.35)}
    Public Kortz As New Location("Kortz Center", New Vector3(-2296.4, 376.32, 173.75), LocationType.Entertainment, New Vector3(-2288.4, 353.93, 174.6), 3)
    Public Galileo As New Location("Galileo Observatory", New Vector3(-411.51, 1174.21, 324.92), LocationType.Entertainment, New Vector3(-415.25, 1166.59, 325.85), 340)
    Public BetsyPav As New Location("Betsy O'Neil Pavilion", New Vector3(-548.26, -648.62, 32.42), LocationType.Entertainment, New Vector3(-555.76, -620.95, 34.68), 183)
    Public SAGOMA As New Location("S.A. Gallery of Modern Art", New Vector3(-424.08, 13.09, 45.75), LocationType.Entertainment, New Vector3(-424.55, 22.97, 46.26), 178)
    Public Casino As New Location("Be Lucky Casino", New Vector3(919.58, 48.24, 90.39), LocationType.Entertainment, New Vector3(929.17, 42.99, 81.09), 59)
    Public DPPierNorth As New Location("Del Perro Pier, North Entrance", New Vector3(-1650.6, -951.3, 7.4), LocationType.Entertainment, New Vector3(-1664.32, -967.68, 7.63), 321) With {.PedEnd = New Vector3(-1673.9, -997.3, 7.4)}
    Public Bishops As New Location("Bishop's WTF", New Vector3(59.8, 233.7, 108.8), LocationType.Entertainment, New Vector3(58.5, 224.6, 109.3), 345)
    Public RanchoTowers As New Location("Rancho Towers", New Vector3(386.9, -2148.7, 15.9), LocationType.Entertainment, New Vector3(378.9, -2139.5, 16.3), 200) With {.PedEnd = New Vector3(358.8, -2118.8, 16)}
    Public DavisLib As New Location("Davis Public Library", New Vector3(238.5, -1573, 28.8), LocationType.Entertainment, New Vector3(252.7, -1594.1, 31.5), 328)
    Public VineBowl As New Location("Vinewood Bowl", New Vector3(818.2, 559, 125.4), LocationType.Entertainment, New Vector3(815.1, 544.5, 125.9), 322)
    Public DPBLVDG As New Location("Del Perro Blvd Gallery", New Vector3(-453.6, -11, 45), LocationType.Entertainment, New Vector3(-456.2, -19.1, 46.1), 345)
    Public HardcoreComicStore As New Location("Hardcore Comic Store", New Vector3(-143.7, 243.8, 94.2), LocationType.Entertainment, New Vector3(-143.5, 229.7, 94.9), 2)
    Public LSPubLibrary As New Location("Los Santos Public Library", New Vector3(-579, -72.5, 41.3), LocationType.Entertainment, New Vector3(-587.5, -89.5, 42.3), 343) With {.PedEnd = New Vector3(-566.1, -118.7, 40)}
    Public Oeuvre As New Location("Oeuvre Gallery", New Vector3(208.1, -198.4, 53.3), LocationType.Entertainment, New Vector3(207.8, -191.3, 54.2), 172)
    Public WellHung As New Location("Well Hung Gallery", New Vector3(-1192.6, -1238.4, 6.3), LocationType.Entertainment, New Vector3(-1187.6, -1235.9, 7), 119)
    Public Blarneys As New Location("Blarney's World of Awesome", New Vector3(125.8, 209.8, 106.8), LocationType.Entertainment, New Vector3(121.2, 202.8, 107.2), 333)

    'THEATER
    Public LosSantosTheater As New Location("Los Santos Theater", New Vector3(345.33, -867.2, 28.72), LocationType.Theater, New Vector3(353.7, -874.09, 29.29), 8)
    Public TenCentTheater As New Location("Ten Cent Theater", New Vector3(401.16, -711.92, 28.7), LocationType.Theater, New Vector3(394.68, -710.04, 29.28), 254)
    Public TivoliTheater As New Location("Tivoli Theater", New Vector3(-1430.66, -193.99, 46.59), LocationType.Theater, New Vector3(-1423.98, -213.35, 46.5), 359)
    Public MorningwoodTheater As New Location("Morningwood Theater", New Vector3(-1389.53, -190.44, 46.12), LocationType.Theater, New Vector3(-1372.27, -173.32, 47.47), 84)
    Public Whirly As New Location("Whirlygig Theater", New Vector3(306.1, 145.38, 103.31), LocationType.Theater, New Vector3(303.21, 136.62, 103.81), 337)
    Public Oriental As New Location("Oriental Theater", New Vector3(292.32, 176, 103.7), LocationType.Theater, New Vector3(292.3, 192.13, 104.37), 195)
    Public Doppler As New Location("Doppler Theater", New Vector3(330.98, 161.02, 102.94), LocationType.Theater, New Vector3(337.21, 177.19, 103.12), 344)
    Public Beacon As New Location("Beacon Theatre", New Vector3(461.9, -1448.7, 28.8), LocationType.Theater, New Vector3(461.3, -1456.8, 29.3), 12)
    Public Sisy As New Location("Sisyphus Theater", New Vector3(232.7, 1173.1, 225.1), LocationType.Theater, New Vector3(225.8, 1173.3, 225.5), 285)
    Public Valdez As New Location("Valdez Theater", New Vector3(-722.6, -667.4, 29.9), LocationType.Theater, New Vector3(-725.5, -686.8, 30.3), 5)
    Public DorsetTh As New Location("Weasel Dorset Theater", New Vector3(-482.1, -388, 33.8), LocationType.Theater, New Vector3(-483.4, -400.9, 34.5), 344)
    Public FullMoon As New Location("Full Moon Theater", New Vector3(-576.4, 269.3, 82.2), LocationType.Theater, New Vector3(-576.1, 276.7, 82.8), 170)

    'S CLUB
    Public StripHornbills As New Location("Hornbill's", New Vector3(-380.469, 230.008, 83.622), LocationType.StripClub, New Vector3(-386.78, 220.33, 83.79), 6)
    Public StripVanUni As New Location("Vanilla Unicorn", New Vector3(133.93, -1307.91, 28.28), LocationType.StripClub, New Vector3(127.26, -1289.09, 29.28), 206)
    Public TBs As New Location("Tits n Bobs", New Vector3(311.6, -1294.7, 30.6), LocationType.StripClub, New Vector3(314.5, -1289.1, 30.9), 155)

    'OFFICE
    Public LombankLittleSeoul As New Location("Lombank", New Vector3(-688.22, -648.09, 30.37), LocationType.Office, New Vector3(-687.1, -617.62, 31.56), 157)
    Public LombankRockHill As New Location("Lombank", New Vector3(-856.5, -184.1, 37.1), LocationType.Office, New Vector3(-864.5, -191, 37.8), 297)
    Public IntlOnlineUnlimited As New Location("International Online Unlimited Office", New Vector3(-828.6, -317.2, 37.1), LocationType.Office, New Vector3(-841.2, -334.8, 38.7), 290)
    Public AuguryIns As New Location("Augury Insurance", New Vector3(-289.45, -412.25, 29.25), LocationType.Office, New Vector3(-296.18, -424.89, 30.24), 325)
    Public IAA As New Location("International Affairs Agency", New Vector3(106.08, -611.18, 43.63), LocationType.Office, New Vector3(117.54, -622.52, 44.24), 53)
    Public FIB As New Location("Federal Investigation Bureau", New Vector3(63.71, -727.4, 43.63), LocationType.Office, New Vector3(102.02, -742.66, 45.75), 103)
    Public UD As New Location("Union Depository", New Vector3(-8.13, -741.36, 43.74), LocationType.Office, New Vector3(5.28, -709.38, 45.97), 184)
    Public MB As New Location("Maze Bank Tower", New Vector3(-50.06, -785.19, 43.75), LocationType.Office, New Vector3(-66.16, -800.06, 44.23), 334)
    Public DG As New Location("Daily Globe International", New Vector3(-300.64, -620.04, 33), LocationType.Office, New Vector3(-317.41, -610.01, 33.56), 250)
    Public Weazel As New Location("Weazel News Studio", New Vector3(-621.96, -930.59, 21.84), LocationType.Office, New Vector3(-600.64, -929.9, 23.86), 95)
    Public NooseLSIA As New Location("N.O.O.S.E. LSIA", New Vector3(-880.37, -2419.36, 13.36), LocationType.Office, New Vector3(-894.21, -2401.18, 14.02), 191)
    Public BilgecoLSIA As New Location("Bilgeco Shipping Services", New Vector3(-1006.02, -2113.84, 11.37), LocationType.Office, New Vector3(-1024.47, -2127.24, 13.16), 307)
    Public LSCustLSIA As New Location("Los Santos Customs", New Vector3(-1132.49, -1989.56, 12.67), LocationType.Office, New Vector3(-1141.05, -1991.34, 13.16), 276)
    Public BurtonHealth As New Location("Burton Health Center", New Vector3(-423.94, -67.88, 42.29), LocationType.Office, New Vector3(-431.83, -60.78, 43), 274)
    Public FleecaChumash As New Location("Fleeca Bank", New Vector3(-2974.37, 483.18, 14.55), LocationType.Office, New Vector3(-2967.19, 482.39, 15.69), 99)
    Public FleecaBurton As New Location("Fleeca Bank", New Vector3(-342.3, -30.8, 46.9), LocationType.Office, New Vector3(-355.4, -48.2, 49), 328)
    Public FleecaRockHill As New Location("Fleeca Bank", New Vector3(-1220.2, -318.2, 37.1), LocationType.Office, New Vector3(-1213, -330.6, 37.8), 208)
    Public FleecaHawick As New Location("Fleeca Bank, Hawick Ave", New Vector3(318.9, -268.4, 53.5), LocationType.Office, New Vector3(314.1, -278.9, 54.2), 160)
    Public CityHallDP As New Location("Del Perro City Hall", New Vector3(-1272.96, -560.89, 29.14), LocationType.Office, New Vector3(-1285.19, -566.24, 31.71), 307)
    Public MazeOfficeDP As New Location("Maze Bank Office", New Vector3(-1401.03, -514.78, 31.03), LocationType.Office, New Vector3(-1382.44, -502.77, 33.16), 179)
    Public LiveInv As New Location("Live Invader HQ", New Vector3(-1076.87, -265.67, 36.96), LocationType.Office, New Vector3(-1084.55, -262.85, 37.76), 238)
    Public PenrisDT1 As New Location("Penris Tower", New Vector3(148.62, -583.3, 43.21), LocationType.Office, New Vector3(155.67, -566.55, 43.89), 122)
    Public PenrisDT2 As New Location("Penris Tower", New Vector3(252.96, -569.05, 42.45), LocationType.Office, New Vector3(217.36, -564.97, 43.87), 297)
    Public CityHallLS As New Location("Los Santos City Hall", New Vector3(257.4, -377.35, 43.84), LocationType.Office, New Vector3(251.39, -389.63, 45.4), 331) With {.PedEnd = New Vector3(235.62, -411.8, 48.11)}
    Public LombankDT As New Location("Lombank Tower", New Vector3(0.03, -947.8, 28.53), LocationType.Office, New Vector3(6.34, -934.49, 29.91), 120)
    Public CityHallRH As New Location("Rockford Hills City Hall", New Vector3(-515.6, -265, 34.9), LocationType.Office, New Vector3(-519.76, -255.26, 35.65), 228) With {.PedEnd = New Vector3(-544.91, -205.23, 38.22)}
    Public Slaughter3 As New Location("Slaughter, Slaughter & Slaughter", New Vector3(-243.19, -708.09, 33.06), LocationType.Office, New Vector3(-271.49, -703.8, 38.28), 272)
    Public Schlongberg As New Location("Schlongberg Sachs", New Vector3(-232.97, -722.25, 33.06), LocationType.Office, New Vector3(-213.97, -728.8, 33.55), 82)
    Public Arcadius As New Location("Arcadius Business Center", New Vector3(-108.01, -613.95, 35.66), LocationType.Office, New Vector3(-116.6, -605, 36.28), 251)
    Public GalileoHouse As New Location("Galileo House", New Vector3(389.24, -82.62, 67.32), LocationType.Office, New Vector3(389.43, -75.65, 68.18), 164)
    Public BadgerB As New Location("Badger Building", New Vector3(460.49, -138.47, 61.42), LocationType.Office, New Vector3(478.25, -107.44, 63.16), 144)
    Public PacStanBank As New Location("Pacific Standard Bank", New Vector3(225.61, 200.76, 104.96), LocationType.Office, New Vector3(239.99, 219.95, 106.29), 308)
    Public Wenger1 As New Location("Wenger Institute", New Vector3(-294.94, -279.51, 30.61), LocationType.Office, New Vector3(-309.69, -279.12, 31.72), 265)
    Public Wenger2 As New Location("Wenger Institute", New Vector3(-383.61, -237.61, 35.17), LocationType.Office, New Vector3(-369.15, -240.52, 36.08), 61)
    Public Vesp707 As New Location("707 Vespucci Blvd", New Vector3(-274.7, -834.1, 31.2), LocationType.Office, New Vector3(-262.4, -837.6, 31.5), 129)
    Public Vesp7072 As New Location("707 Vespucci Blvd Offices", New Vector3(-239.3, -860.4, 30), LocationType.Office, New Vector3(-230, -852.4, 30.7), 161)
    Public RebelRad As New Location("Rebel Radio Studio", New Vector3(741.6, 2523.4, 72.8), LocationType.Office, New Vector3(733, 2523.7, 73.2), 255)
    Public WeazelPlaza As New Location("Weazel Plaza", New Vector3(-874.8, -395.3, 38.7), LocationType.Office, New Vector3(-867.8, -410.9, 36.6), 229)
    Public MorsMutual As New Location("Mors Mutual Insurance", New Vector3(-816.4, -256.1, 36.4), LocationType.Office, New Vector3(-825.5, -260.8, 38), 296)
    Public LSCoCoroner As New Location("LS County Coroner", New Vector3(231.7, -1391.6, 30.1), LocationType.Office, New Vector3(239.5, -1381, 33.7), 146)
    Public HospCentralLSMed As New Location("Central LS Medical Center", New Vector3(309.4, -1378.1, 31.4), LocationType.Office, New Vector3(323.2, -1386.3, 31.9), 67) With {.PedEnd = New Vector3(343.2, -1398.6, 32.5)}
    Public PillboxMedical As New Location("Pillbox Hill Medical Center", New Vector3(356.8, -604.8, 27.8), LocationType.Office, New Vector3(352.4, -604.5, 28.8), 290)
    Public PortolaTrinity As New Location("Portola Trinity Medical Center", New Vector3(-873.6, -296.9, 39.3), LocationType.Office, New Vector3(-874.7, -309.2, 39.5), 347D)
    Public DavisCourts As New Location("Davis Courts Building", New Vector3(254.9, -1641.5, 28.7), LocationType.Office, New Vector3(261.2, -1632.3, 30), 153)
    Public LSMeteor As New Location("Los Santos Meteor", New Vector3(-113.6, -1312.9, 28.8), LocationType.Office, New Vector3(-121.1, -1314, 29.3), 272)
    Public VinewoodPD As New Location("Vinewood P.D.", New Vector3(662.9, -16.3, 83), LocationType.Office, New Vector3(639.3, 1.7, 82.8), 244)
    Public BacklotCity As New Location("Backlot City", New Vector3(-1059.1, -466.3, 36.2), LocationType.Office, New Vector3(-1063.9, -477, 36.7), 306)
    Public CelltowaBldg As New Location("Celltowa Building", New Vector3(-1045.4, -748.2, 18.9), LocationType.Office, New Vector3(-1039.5, -756.7, 19.8), 79)
    Public TotalBankersLS As New Location("Total Bankers", New Vector3(-589.2, -669, 32), LocationType.Office, New Vector3(-590.7, -681.2, 32.3), 6) With {.PedEnd = New Vector3(-589.3, -707.6, 36.3)}
    Public SAAvenue302 As New Location("302 San Andreas Ave", New Vector3(-466, -668.7, 32), LocationType.Office, New Vector3(-469.6, -678.3, 32.7), 354)
    Public Coffield As New Location("Coffield Center for Spinal Research", New Vector3(-445.2, -366.1, 33.2), LocationType.Office, New Vector3(-444.5, -358.7, 34.3), 178)
    Public Bullhead As New Location("Bullhead Legal", New Vector3(-234.8, 155.3, 71), LocationType.Office, New Vector3(-245.1, 156.2, 74.1), 256)
    Public WeazelPlazaRotunda As New Location("Weazel Plaza Rotunda", New Vector3(-849, -439, 35.8), LocationType.Office, New Vector3(-883.3, -435.3, 39.6), 244)
    Public CNT As New Location("CNT Headquarters", New Vector3(738.1, 191.9, 84), LocationType.Office, New Vector3(751.4, 222.7, 87.4), 152)
    Public VinePlaz As New Location("Vinewood Plaza", New Vector3(546.4, 154.7, 98.4), LocationType.Office, New Vector3(554.2, 151.3, 99.2), 74)
    Public Wolfs As New Location("Wolfs International Realty", New Vector3(81, -1095.9, 28.5), LocationType.Office, New Vector3(89.4, -1099.6, 29.3), 65)
    Public Society As New Location("Society Building", New Vector3(-740, 240.3, 76.1), LocationType.Office, New Vector3(-742.8, 246.4, 77.3), 202)
    Public SchlongVine As New Location("Schlongberg Sachs", New Vector3(-690, 262.9, 80.6), LocationType.Office, New Vector3(-698.3, 271.3, 83.1), 298)
    Public BettaVine As New Location("Betta Life", New Vector3(-640, 281, 81), LocationType.Office, New Vector3(-639.4, 295.8, 82.5), 178)
    Public EclipseMedical As New Location("Eclipse Medical Tower", New Vector3(-678.2, 293.1, 81.7), LocationType.Office, New Vector3(-675.5, 312, 83.1), 174)
    Public VineIndia As New Location("Vinewood India Building", New Vector3(382.2, 116.7, 102.2), LocationType.Office, New Vector3(378.6, 106.8, 102.8), 337)
    Public KaytonBankingGroup As New Location("Kayton Banking Group Office", New Vector3(-728.5, -825.9, 22.7), LocationType.Office, New Vector3(-731.1, -312.4, 23.7), 184)
    Public BigDanVision As New Location("Big Dan Vision Optometry", New Vector3(-712.4, -867.7, 23), LocationType.Office, New Vector3(-697.7, -869.3, 23.7), 90)
    Public SanAn7302 As New Location("Offices at 7302 San Andreas Avenue", New Vector3(-472.5, -668.7, 31.8), LocationType.Office, New Vector3(-467.2, -678.9, 32.7), 352)
    Public DavisSheriff As New Location("Davis Sheriff's Station", New Vector3(372.7, -1574.2, 28.7), LocationType.Office, New Vector3(360.4, -1584.8, 29.3), 46)
    Public DavisCityHall As New Location("Davis City Hall", New Vector3(290.7, -1545.5, 28.5), LocationType.Office, New Vector3(316.2, -1557.2, 29.7), 54)
    Public PostOpCouriers As New Location("Post OP Couriers Building", New Vector3(-210.7, -918, 28.5), LocationType.Office, New Vector3(-231.7, -914.8, 32.3), 339)
    Public GoPostalPill As New Location("GoPostal", New Vector3(-260, -853.3, 30.7), LocationType.Office, New Vector3(-259, -841.7, 31.4), 114)
    Public QuickHouse As New Location("LS Quick HQ", New Vector3(-291.7, -841.4, 31.1), LocationType.Office, New Vector3(-259.3, -829, 32.4), 204)
    Public RobertDazzler As New Location("Robert Dazzler International Jewelry Exchange", New Vector3(-293.3, -881.1, 28.3), LocationType.Office, New Vector3(301.6, -883.4, 29.3), 76)
    Public BluffTower As New Location("Bluff Tower Offices", New Vector3(-1557.5, -638.9, 28.9), LocationType.Office, New Vector3(-1539.1, -607.4, 31.3), 173)
    Public AkanRecords As New Location("Akan Records", New Vector3(-1033.5, -254.6, 37.2), LocationType.Office, New Vector3(-1016.9, -265.6, 39), 65)
    Public PenrisRockHills As New Location("Penris Office", New Vector3(-1048, -391.5, 37.2), LocationType.Office, New Vector3(-1030.4, -420.1, 39.6), 72)
    Public ShlongRock As New Location("Shlongberg & Sachs Office", New Vector3(-1048, -391.5, 37.2), LocationType.Office, New Vector3(-1030.4, -420.1, 39.6), 72)
    Public BawRock As New Location("Bawsaq Office", New Vector3(-1048, -391.5, 37.2), LocationType.Office, New Vector3(-1030.4, -420.1, 39.6), 72)
    Public LifeInv As New Location("LifeInvader Office", New Vector3(-1077.8, -265.9, 37.3), LocationType.Office, New Vector3(-1081.6, -260.3, 37.8), 206)
    Public AltaSt3 As New Location("3 Alta Street Office Tower", New Vector3(-238, -992.7, 28.6), LocationType.Office, New Vector3(-261.6, -970.9, 31.2), 202)
    Public Gruppe6PH As New Location("Gruppe Sechs Office", New Vector3(-179.4, -834.9, 29.9), LocationType.Office, New Vector3(-196.3, -831, 30.8), 291)
    Public BettaRock As New Location("Betta Life Office", New Vector3(-1235.6, -326.2, 36.8), LocationType.Office, New Vector3(-1230.8, -336.6, 37.6), 28)
    Public Wiwang As New Location("Wiwang Tower", New Vector3(-834.6, -667.6, 27.5), LocationType.Office, New Vector3(-828.1, -688.5, 28.1), 92)
    Public EugenicsLS As New Location("Eugenics", New Vector3(-779.2, -587.3, 29.9), LocationType.Office, New Vector3(-778.5, -599.6, 30.3), 29)

    'FACTORY
    Public PisswasserFac As New Location("Pisswasser Factory", New Vector3(949.7, -1808.2, 30.7), LocationType.Factory, New Vector3(930.4, -1807.9, 31.3), 269)
    Public Fridgit As New Location("Fridgit Cold Storage", New Vector3(852.6, -1641.7, 29.7), LocationType.Factory, New Vector3(867.8, -1639.7, 30.2), 103)
    Public BagCo As New Location("Los Santos Bag Co", New Vector3(785.5, -1351.1, 25.9), LocationType.Factory, New Vector3(764.8, -1355.1, 26.4), 103)
    Public LeanPork As New Location("Larry's Lean Pork", New Vector3(-291.7, -1294.8, 30.7), LocationType.Factory, New Vector3(-295.1, -1295.2, 31.3), 265)
    Public VinePrint As New Location("Vine Print", New Vector3(-131, -1387.4, 29.1), LocationType.Factory, New Vector3(-128.3, -1393.8, 29.6), 23)
    Public LSBagCo As New Location("Los Santos Bag Co", New Vector3(762.8, -1352.2, 26), LocationType.Factory, New Vector3(764.9, -1358.4, 27.9), 13)
    Public LSPDLaMesa As New Location("LSPD La Mesa", New Vector3(808.7, -1289.6, 25.8), LocationType.Factory, New Vector3(826.3, -1290, 28.2), 87)
    Public NatlXferStorageCo As New Location("National Storage", New Vector3(795.5, -998, 25.6), LocationType.Factory, New Vector3(804, -989.7, 26.1), 138)
    Public BigGoods As New Location("Big Goods", New Vector3(812.2, -916.1, 25.2), LocationType.Factory, New Vector3(812.6, -912, 25.5), 165)
    Public OttosAutoParts As New Location("Otto's Auto Parts", New Vector3(787.6, -813.3, 25.8), LocationType.Factory, New Vector3(806.5, -810.1, 26.2), 98)
    Public Leyer As New Location("Léyer Cosmetics", New Vector3(871.5, -1007, 30.5), LocationType.Factory, New Vector3(871, -1015.7, 31.2), 4)
    Public AutoExotic As New Location("Auto Exotic", New Vector3(526.5, -192.7, 52.6), LocationType.Factory, New Vector3(542.5, -189.5, 54.5), 94)
    Public JetAbr As New Location("Jet Abrasives", New Vector3(1035.6, -2091.7, 30.7), LocationType.Factory, New Vector3(1036, -2110.4, 32.5), 5)
    Public FloodControl As New Location("Storm Drain Flood Control Gates", New Vector3(1219.8, -1075.6, 38.3), LocationType.Factory, New Vector3(1229.8, -1083.4, 38.5), 72)
    Public LTWELD As New Location("L. T. Weld Supply Co", New Vector3(1170.6, -1347.9, 34.1), LocationType.Factory, New Vector3(1165.7, -1347, 35.7), 272)
    Public LTWELD2 As New Location("L. T. Weld Supply Co", New Vector3(1146.3, -1409.2, 33.9), LocationType.Factory, New Vector3(1146, -1403, 34.8), 174)
    Public LSFD7 As New Location("LSFD Station 7", New Vector3(1190.6, -1446.7, 34.3), LocationType.Factory, New Vector3(1195.2, -1473.8, 34.9), 340)
    Public JetsamPort As New Location("Jetsam Terminal", New Vector3(777, -2983.4, 5.1), LocationType.Factory, New Vector3(795.5, -2979.6, 6), 105)
    Public MPRail As New Location("Mirror Park Railyard", New Vector3(508.4, -650.5, 24.3), LocationType.Factory, New Vector3(501.6, -651.3, 24.8), 267)
    Public BertsTool As New Location("Berts Tool Supply Co", New Vector3(337.5, -1307.8, 31.4), LocationType.Factory, New Vector3(342.7, -1298.7, 32.5), 157)
    Public RimmPaint As New Location("Rimm Paint", New Vector3(-115.5, -1293.4, 28.9), LocationType.Factory, New Vector3(-120.5, -1290.4, 29.3), 249)
    Public SouthTile As New Location("Southern Tile", New Vector3(-231.8, -1306.8, 30.9), LocationType.Factory, New Vector3(-232.4, -1311.5, 31.3), 357)
    Public SLSR As New Location("South L.S. Recycling", New Vector3(-315.7, -1532.9, 27.2), LocationType.Factory, New Vector3(-321.8, -1546.1, 31), 353)
    Public AluVinWholesale As New Location("Aluminum & Vinyl Wholesales", New Vector3(-518.4, -736.8, 31.9), LocationType.Factory, New Vector3(-513.8, -734.3, 32.7), 135)
    Public AlphaMailLSIA As New Location("Alpha Mail Couriers", New Vector3(-736.3, -2464.5, 13.2), LocationType.Factory, New Vector3(-734, -2466.3, 13.9), 50)
    Public JetsamLSIA As New Location("Jetsam Distribution Center", New Vector3(-786.3, -2551, 13.2), LocationType.Factory, New Vector3(-783.5, -2553.3, 13.9), 49)
    Public BigHouseLSIA As New Location("Big House Storage", New Vector3(-540.3, -2195, 5.4), LocationType.Factory, New Vector3(-534, -2201, 6.3), 57)
    Public WaterPower As New Location("LS Department of Water and Power", New Vector3(545.1, -1663, 28.3), LocationType.Factory, New Vector3(542.6, -1652.9, 28.6), 218)
    Public AGLRefrigerated As New Location("AGL Refrigerated Storage", New Vector3(518.3, -1421, 28.5), LocationType.Factory, New Vector3(518.3, -1408.4, 29.3), 259)
    Public PostOPTerminal As New Location("Post OP Terminal", New Vector3(1175, -3253, 5.2), LocationType.Factory, New Vector3(1197, -3253.5, 7.1), 103)
    Public PostOPDepository As New Location("Post OP Depository", New Vector3(-425.7, -2779.7, 5.4), LocationType.Factory, New Vector3(-424.1, -2789.3, 6.3), 334)
    Public GoPostalBuilding As New Location("GoPostal Building", New Vector3(55.4, 103.7, 78.3), LocationType.Factory, New Vector3(57.2, 114.6, 79.1), 162)
    Public SimmetTraders As New Location("Simmet Alley Traders Co-Op", New Vector3(432.5, -820, 28.3), LocationType.Factory, New Vector3(432.4, -813.9, 28.8), 185)
    Public TGSA As New Location("Textile Gallery San Andreas", New Vector3(495.8, -875.8, 24.9), LocationType.Factory, New Vector3(488.1, -873.2, 25.4), 275)
    Public VitreousGlass As New Location("Vitreous Glass Masters", New Vector3(937.6, -1964, 29.8), LocationType.Factory, New Vector3(931.4, -1964.1, 30.4), 263)
    Public StraightDirect As New Location("Straight & Direct Piping Components", New Vector3(-324.9, -1406.1, 30.8), LocationType.Factory, New Vector3(-322.3, -1389.6, 36.5), 169)
    Public SandyFire As New Location("Sandy Shores Fire Station", New Vector3(1699.8, 3581.5, 35), LocationType.Factory, New Vector3(1690.1, 3580.9, 35.6), 200)
    Public ShadyTree As New Location("Shady Tree Farm", New Vector3(2552.1, 4687.3, 33.3), LocationType.Factory, New Vector3(2564.3, 4680.3, 34.1), 45)
    Public UnionGrain As New Location("Union Grain Inc", New Vector3(2038.1, 4984.1, 40), LocationType.Factory, New Vector3(2029.8, 4980.5, 42.1), 283)
    Public PaintShop As New Location("The Paint Shop", New Vector3(1665.1, 4837.1, 41.5), LocationType.Factory, New Vector3(1658.1, 4839.2, 42), 283)
    Public LuckyJims As New Location("Lucky Jim's Ranch", New Vector3(2234.6, 5149.7, 55.8), LocationType.Factory, New Vector3(2242.9, 5153.7, 57.7), 134)
    Public HJSilos As New Location("H.J. Silos, Grapeseed", New Vector3(2907.1, 4423.5, 47.9), LocationType.Factory, New Vector3(2899.3, 4422.4, 48.6), 288)
    Public DavisQuartz As New Location("Davis Quartz Quarry", New Vector3(2565.5, 2709.6, 41.7), LocationType.Factory, New Vector3(2569.7, 2719.7, 42.9), 218)
    Public CapeCatfish As New Location("Cape Catfish", New Vector3(3792.6, 4469.7, 5.2), LocationType.Factory, New Vector3(3805.5, 4477.3, 6), 219)
    Public StationTen As New Location("Station Ten, LS Power & Water", New Vector3(294.6, -24.8, 74.3), LocationType.Factory, New Vector3(287.9, -23.1, 74.5), 248)
    Public FernsFD As New Location("Fern's Foreign & Domestic", New Vector3(43.9, -1298.5, 28.8), LocationType.Factory, New Vector3(26.5, -1300.5, 29.2), 174)


    'HOTEL
    Public HotelRichman As New Location("Richman Hotel", New Vector3(-1285.498, 294.565, 64.368), LocationType.HotelLS, New Vector3(-1274.5, 313.97, 65.51), 151)
    Public HotelVenetian As New Location("The Venetian Hotel", New Vector3(-1330.82, -1095.89, 6.37), LocationType.HotelLS, New Vector3(-1342.88, -1080.84, 6.94), 255)
    Public HotelViceroy As New Location("The Viceroy Hotel", New Vector3(-828.04, -1218.07, 6.46), LocationType.HotelLS, New Vector3(-821.72, -1221.23, 7.33), 60)
    Public HotelRockDors As New Location("The Rockford Dorset Hotel", New Vector3(-569.89, -384.24, 34.19), LocationType.HotelLS, New Vector3(-570.79, -394.13, 35.07), 350)
    Public HotelVCPacificBluffs As New Location("Von Crastenburg Hotel", New Vector3(-1862.575, -352.879, 48.752), LocationType.HotelLS, New Vector3(-1859.5, -347.92, 49.84), 157)
    Public HotelBannerDP As New Location("Banner Hotel, Del Perro", New Vector3(-1668.53, -542.07, 34.31), LocationType.HotelLS, New Vector3(-1662.99, -535.5, 35.33), 152)
    Public HotelVCRock1 As New Location("Von Crastenburg Hotel", New Vector3(-1208.148, -128.829, 40.71), LocationType.HotelLS, New Vector3(-1239.6, -156.26, 40.41), 62)
    Public HotelVCRock2 As New Location("Von Crastenburg Hotel", New Vector3(-1228.485, -193.734, 38.8), LocationType.HotelLS, New Vector3(-1239.6, -156.26, 40.41), 62)
    Public HotelEmissary As New Location("Emissary Hotel", New Vector3(116.09, -935.88, 28.94), LocationType.HotelLS, New Vector3(106.31, -933.52, 29.79), 254)
    Public HotelAlesandro As New Location("Alesandro Hotel", New Vector3(318.07, -732.85, 28.73), LocationType.HotelLS, New Vector3(309.89, -728.62, 29.32), 251)
    Public HotelVCLSIA As New Location("Von Crastenburg Hotel", New Vector3(-887.55, -2187.61, 7.81), LocationType.HotelLS, New Vector3(-878.67, -2179.05, 9.81), 134)
    Public HotelVCLSIA2 As New Location("Von Crastenburg Hotel", New Vector3(-882.35, -2107.39, 8.14), LocationType.HotelLS, New Vector3(-878.67, -2179.05, 9.81), 134)
    Public HotelOpium As New Location("Opium Nights Hotel", New Vector3(-689.92, -2287.82, 12.87), LocationType.HotelLS, New Vector3(-702.13, -2276.6, 13.46), 229)
    Public HotelOpium2 As New Location("Opium Nights Hotel", New Vector3(-754.43, -2292.86, 12.14), LocationType.HotelLS, New Vector3(-737.53, -2277.44, 13.44), 133)
    Public HotelBannerPH As New Location("Banner Hotel", New Vector3(-278.28, -1065.08, 25.04), LocationType.HotelLS, New Vector3(-286.06, -1061.29, 27.21), 253)
    Public HotelGeneric As New Location("The Generic Hotel", New Vector3(-479.44, 225.87, 82.63), LocationType.HotelLS, New Vector3(-482.92, 219.25, 83.7), 341)
    Public HotelPegasusConc As New Location("Pegasus Concierge Hotel", New Vector3(-310.2, 226.61, 87.43), LocationType.HotelLS, New Vector3(-310.84, 222.29, 87.93), 12)
    Public HotelPegasusRH As New Location("Pegasus Concierge", New Vector3(-822.7, -206.8, 36.9), LocationType.HotelLS, New Vector3(-815, -202.1, 37.5), 122)
    Public HotelGentry As New Location("Gentry Manor Hotel", New Vector3(-62.57, 329.18, 110.3), LocationType.HotelLS, New Vector3(-53.93, 356.92, 113.06), 181)
    Public HotelVineGar As New Location("Vinewood Gardens Hotel", New Vector3(322.17, -87.74, 68.19), LocationType.HotelLS, New Vector3(328.71, -70.77, 72.25), 161)
    Public HotelVCVine As New Location("Von Crastenburg Hotel", New Vector3(437.14, 221.31, 102.77), LocationType.HotelLS, New Vector3(435.53, 215.57, 103.17), 340)
    Public HotelHookah As New Location("Hookah Palace Hotel", New Vector3(10.5, -973.6, 28.8), LocationType.HotelLS, New Vector3(5.7, -985.6, 29.4), 341)
    Public HotelElkridge As New Location("Elkridge Hotel", New Vector3(275.7, -931.4, 28.4), LocationType.HotelLS, New Vector3(285.1, -938, 29.4), 128)
    Public Fannypackers As New Location("Vinewood Fannypackers Hostel & Motel", New Vector3(458.7, 116.6, 98.3), LocationType.HotelLS, New Vector3(464.3, 128.4, 99.2), 156)

    'MOTEL
    Public PerreraBeach As New Location("Perrera Beach Motel", New Vector3(-1480.4, -669.76, 28.23), LocationType.MotelLS, New Vector3(-1478.68, -649.89, 29.58), 162) With {.PedEnd = New Vector3(-1479.64, -674.43, 29.04)}
    Public DreamView As New Location("Dream View Motel", New Vector3(-94.06, 6310.33, 31.02), LocationType.MotelBC, New Vector3(-106.33, 6315.21, 31.49), 212)
    Public CrownJewels As New Location("Crown Jewels Motel", New Vector3(-1300.2, -922.46, 10.55), LocationType.MotelLS, New Vector3(-1308.91, -930.84, 13.36), 313)
    Public PinkCage As New Location("Pink Cage Motel", New Vector3(314.31, -244.63, 53.22), LocationType.MotelLS, New Vector3(313.91, -227.21, 54.02), 229)
    Public AltaMotel As New Location("Alta Motel", New Vector3(66.07, -283.71, 46.68), LocationType.MotelLS, New Vector3(62.95, -255.06, 48.19), 84)
    Public EasternMotel As New Location("Eastern Motel", New Vector3(324.1, 2626.7, 44.2), LocationType.MotelBC, New Vector3(318.6, 2623, 44.5), 312)
    Public BilingsgateMotel As New Location("Bilingsgate Motel", New Vector3(571.8, -1732.7, 28.8), LocationType.MotelLS, New Vector3(571.4, -1741.2, 29.3), 335) With {.PedEnd = New Vector3(553, -1765.7, 33.4)}
    Public MotorHotel As New Location("Motor Hotel", New Vector3(1137.4, 2665.7, 37.6), LocationType.MotelBC, New Vector3(1141, 2664.3, 38.2), 68)

    'AIRPORT
    Public LSIA1Depart = New Location("Terminal 1 Departures", New Vector3(-1016.8, -2477.9, 19.6), LocationType.AirportDepart, New Vector3(-1029.35, -2486.58, 20.17), 253)
    Public LSIA1DepartCaipira = New Location("Terminal 1, Caipira", New Vector3(-1013.3, -2470.7, 19.4), LocationType.AirportDepart, New Vector3(-1029.35, -2486.58, 20.17), 253)
    Public LSIA1DepartHerler = New Location("Terminal 1, Air Herler", New Vector3(-1020.7, -2484.5, 19.4), LocationType.AirportDepart, New Vector3(-1029.35, -2486.58, 20.17), 253)
    Public LSIA2DepartSFAir = New Location("Terminal 2, San Fierro Air", New Vector3(-1036.6, -2511.6, 19.4), LocationType.AirportDepart, New Vector3(-1029.35, -2486.58, 20.17), 253)
    Public LSIA3DepartEmu = New Location("Terminal 3, Air Emu", New Vector3(-1053.7, -2541.1, 19.4), LocationType.AirportDepart, New Vector3(-1059.9, -2539.3, 20.2), 238)
    Public LSIA3DepartMyfly = New Location("Terminal 3, Myfly", New Vector3(-1068.6, -2567, 19.4), LocationType.AirportDepart, New Vector3(-1073, -2563.5, 20.2), 240)
    Public LSIA3DepartAdios = New Location("Terminal 3, Adios Air", New Vector3(-1085.8, -2597.1, 19.4), LocationType.AirportDepart, New Vector3(-1091.4, -2595.4, 20.2), 244)
    Public LSIA4Depart As New Location("Terminal 4 Departures", New Vector3(-1033.549, -2730.294, 19.583), LocationType.AirportDepart, New Vector3(-1037.78, -2748.22, 21.36), 8)
    Public LSIA1Arrive = New Location("Terminal 1 Arrivals", New Vector3(-1016, -2482.257, 13.155), LocationType.AirportArrive, New Vector3(-1023.33, -2479.52, 13.94), 238, False)
    Public LSIA2Arrive = New Location("Terminal 2 Arrivals", New Vector3(-1047.546, -2536.564, 13.148), LocationType.AirportArrive, New Vector3(-1061.04, -2544.39, 13.94), 356, False)
    Public LSIA3Arrive = New Location("Terminal 3 Arrivals", New Vector3(-1082.547, -2598.047, 13.169), LocationType.AirportArrive, New Vector3(-1087.61, -2588.75, 13.88), 281, False)
    Public LSIA4Arrive As New Location("Terminal 4 Arrivals", New Vector3(-1018.835, -2731.926, 13.17), LocationType.AirportArrive, New Vector3(-1043.18, -2737.67, 13.86), 328, False)

    'RESIDENTIAL
    Public NRD1018 As New Location("1018 North Rockford Dr", New Vector3(-1962.174, 617.408, 120.52), LocationType.Residential, New Vector3(-1974.471, 630.919, 122.54), 250)
    Public NRD1012 As New Location("1012 North Rockford Dr", New Vector3(-2004.645, 482.538, 105.446), LocationType.Residential, New Vector3(-2014.892, 499.58, 107.17), 253)
    Public NRD1016 As New Location("1016 North Rockford Dr", New Vector3(-1981.123, 599.796, 117.889), LocationType.Residential, New Vector3(-1995.21, 590.85, 117.9), 256)
    Public NRD1010 As New Location("1010 North Rockford Dr", New Vector3(-1998.424, 456.349, 101.974), LocationType.Residential, New Vector3(-2010.84, 445.13, 103.02), 288)
    Public NRD1008 As New Location("1008 North Rockford Dr", New Vector3(-2000.783, 366.728, 94.01), LocationType.Residential, New Vector3(-2009.12, 367.43, 94.81), 270)
    Public NRD1006 As New Location("1006 North Rockford Dr", New Vector3(-1993.148, 287.433, 90.97), LocationType.Residential, New Vector3(-1995.33, 300.61, 91.96), 196)
    Public NRD1004 As New Location("1004 North Rockford Dr", New Vector3(-1953.658, 252.294, 84.56), LocationType.Residential, New Vector3(-1970.29, 246.03, 87.81), 289)
    Public NRD1002 As New Location("1002 North Rockford Dr", New Vector3(-1940.945, 205.206, 84.71), LocationType.Residential, New Vector3(-1960.92, 211.98, 86.8), 295)
    Public NRD1001 As New Location("1001 North Rockford Dr", New Vector3(-1878.937, 190.919, 83.594), LocationType.Residential, New Vector3(-1877.27, 215.63, 84.44), 127)
    Public NRD1003 As New Location("1003 North Rockford Dr", New Vector3(-1910.118, 249.629, 85.78), LocationType.Residential, New Vector3(-1887.2, 240.12, 86.45), 211)
    Public NRD1005 As New Location("1005 North Rockford Dr", New Vector3(-1926.447, 292.492, 88.6), LocationType.Residential, New Vector3(-1922.37, 298.18, 89.29), 102)
    Public NRD1007 As New Location("1007 North Rockford Dr", New Vector3(-1944.09, 350.647, 91.82), LocationType.Residential, New Vector3(-1929.54, 369.41, 93.78), 100)
    Public NRD1009 As New Location("1009 North Rockford Dr", New Vector3(-1960.21, 383.541, 93.767), LocationType.Residential, New Vector3(-1942.2, 380.89, 96.12), 27)
    Public NRD1011 As New Location("1011 North Rockford Dr", New Vector3(-1954.286, 448.664, 100.563), LocationType.Residential, New Vector3(-1944.51, 449.53, 102.7), 94)
    Public NRD1015 As New Location("1015 North Rockford Dr", New Vector3(-1941.942, 554.111, 114.35), LocationType.Residential, New Vector3(-1937.57, 551.05, 115.02), 69)
    Public NRD1017 As New Location("1017 North Rockford Dr", New Vector3(-1953.778, 589.897, 118.277), LocationType.Residential, New Vector3(-1928.958, 595.436, 122.28), 65)
    Public NRD1019 As New Location("1019 North Rockford Dr", New Vector3(-1898.356, 619.346, 127.93), LocationType.Residential, New Vector3(-1896.82, 642.375, 130.21), 180)
    Public NRD1022 As New Location("1022 North Rockford Dr", New Vector3(-1859.05, 334.48, 87.88), LocationType.Residential, New Vector3(-1841.58, 313.81, 90.92), 13)
    Public NRD1024 As New Location("1024 North Rockford Dr", New Vector3(-1814.82, 346.16, 87.91), LocationType.Residential, New Vector3(-1808.41, 333.8, 89.37), 33)
    Public NRD1026 As New Location("1026 North Rockford Dr", New Vector3(-1738.51, 388.53, 88.17), LocationType.Residential, New Vector3(-1733.3, 379.93, 89.73), 30)
    Public NRD1028 As New Location("1028 North Rockford Dr", New Vector3(-1674.76, 398.75, 88.28), LocationType.Residential, New Vector3(-1673.37, 386.67, 89.35), 349)
    Public AW1 As New Location("1 Americano Way", New Vector3(-1466.627, 40.889, 53.436), LocationType.Residential, New Vector3(-1467.26, 35.88, 54.54), 351)
    Public AW2 As New Location("2 Americano Way", New Vector3(-1515.466, 30.088, 55.67), LocationType.Residential, New Vector3(-1515.17, 25.24, 56.82), 353)
    Public AW3 As New Location("3 Americano Way", New Vector3(-1568.744, 32.989, 58.65), LocationType.Residential, New Vector3(-1570.5, 23.53, 59.55), 352)
    Public AW4 As New Location("4 Americano Way", New Vector3(-1616.825, 62.221, 60.85), LocationType.Residential, New Vector3(-1629.7, 37.29, 62.94), 335)
    Public SW1 As New Location("1 Steele Way", New Vector3(-910.013, 188.821, 68.969), LocationType.Residential, New Vector3(-903.26, 191.59, 69.45), 120)
    Public SW2 As New Location("1 Steele Way", New Vector3(-954.629, 174.792, 64.76), LocationType.Residential, New Vector3(-949.64, 195.94, 67.39), 166)
    Public PD1 As New Location("1001 Portola Dr", New Vector3(-856.87, 103.36, 52.02), LocationType.Residential, New Vector3(-831.82, 114.7, 55.42), 119)
    Public CaesarsPlace1 As New Location("1 Caesars Pl", New Vector3(-926.348, 16.409, 47.31), LocationType.Residential, New Vector3(-930.26, 19.01, 48.33), 244)
    Public CaesarsPlace2 As New Location("2 Caesars Pl", New Vector3(-891.514, -2.175, 43.04), LocationType.Residential, New Vector3(-895.69, -4.52, 43.8), 304)
    Public CaesarsPlace3 As New Location("3 Caesars Pl", New Vector3(-882.7, 16.31, 43.86), LocationType.Residential, New Vector3(-886.89, 41.79, 48.76), 234)
    Public WMD3673 As New Location("3673 Whispymound Dr", New Vector3(25.86, 560.716, 177.833), LocationType.Residential, New Vector3(45.58, 556.26, 180.08), 16)
    Public WMD3675 As New Location("3675 Whispymound Dr", New Vector3(84.983, 568.64, 181.422), LocationType.Residential, New Vector3(84.89, 562.37, 182.57), 4)
    Public WMD3677 As New Location("3677 Whispymound Dr", New Vector3(119.326, 569.391, 182.554), LocationType.Residential, New Vector3(119.2, 564.51, 183.96), 359)
    Public WMD3679 As New Location("3679 Whispymound Dr", New Vector3(138.758, 570.422, 183.113), LocationType.Residential, New Vector3(163.31, 551.59, 182.34), 187)
    Public WMD3681 As New Location("3681 Whispymound Dr", New Vector3(210.967, 619.599, 186.797), LocationType.Residential, New Vector3(216.06, 620.78, 187.64), 78)
    Public WMD3683 As New Location("3683 Whispymound Dr", New Vector3(217.176, 667.946, 188.55), LocationType.Residential, New Vector3(232, 672.51, 189.95), 39)
    Public NCA2041 As New Location("2041 North Conker Ave", New Vector3(317.672, 569.013, 153.955), LocationType.Residential, New Vector3(317.7, 563.3, 154.45), 4)
    Public NCA2042 As New Location("2042 North Conker Ave", New Vector3(328.543, 497.446, 151.29), LocationType.Residential, New Vector3(316.27, 500.62, 153.18), 226)
    Public NCA2043 As New Location("2043 North Conker Ave", New Vector3(333.456, 474.255, 149.49), LocationType.Residential, New Vector3(331.36, 465.98, 151.19), 7)
    Public NCA2044 As New Location("2044 North Conker Ave", New Vector3(359.103, 441.625, 144.766), LocationType.Residential, New Vector3(346.95, 441.28, 147.7), 306)
    Public NCA2045 As New Location("2045 North Conker Ave", New Vector3(374.333, 436.442, 143.675), LocationType.Residential, New Vector3(372.92, 427.9, 145.68), 26)
    Public DD3550 As New Location("3550 Didion Dr", New Vector3(-470.738, 357.95, 102.73), LocationType.Residential, New Vector3(-468.52, 329.14, 104.15), 319)
    Public DD3552 As New Location("3552 Didion Dr", New Vector3(-445.537, 347.804, 104.17), LocationType.Residential, New Vector3(-444.84, 343.8, 105.36), 11)
    Public DD3554 As New Location("3554 Didion Dr", New Vector3(-400.909, 350.3, 107.781), LocationType.Residential, New Vector3(-408.97, 341.66, 108.91), 312)
    Public DD3556 As New Location("3556 Didion Dr", New Vector3(-368.68, 352.18, 108.927), LocationType.Residential, New Vector3(-359.29, 348.17, 109.39), 62)
    Public DD3558 As New Location("3558 Didion Dr", New Vector3(-350.631, 372.532, 109.507), LocationType.Residential, New Vector3(-328.29, 370.21, 110.02), 34)
    Public DD3560 As New Location("3560 Didion Dr", New Vector3(-307.178, 387.202, 109.705), LocationType.Residential, New Vector3(-297.29, 381.21, 112.05), 32)
    Public DD3562 As New Location("3562 Didion Dr", New Vector3(-261.608, 400.212, 109.444), LocationType.Residential, New Vector3(-239.82, 381.82, 112.43), 83)
    Public DD3564 As New Location("3564 Didion Dr", New Vector3(-202.011, 415.544, 109.273), LocationType.Residential, New Vector3(-214.12, 400.48, 111.11), 16)
    Public DD3566 As New Location("3566 Didion Dr", New Vector3(-184.768, 424.017, 109.846), LocationType.Residential, New Vector3(-178.01, 423.29, 110.88), 101)
    Public DD3567 As New Location("3567 Didion Dr", New Vector3(-91.562, 424.167, 112.621), LocationType.Residential, New Vector3(-72.36, 427.22, 113.04), 106)
    Public DD3569 As New Location("3569 Didion Dr", New Vector3(19.967, 369.569, 111.884), LocationType.Residential, New Vector3(-8.47, 409.27, 120.13), 92)
    Public DD3571 As New Location("3571 Didion Dr", New Vector3(19.967, 369.569, 111.884), LocationType.Residential, New Vector3(41.31, 360.4, 116.04), 238)
    Public DD3651 As New Location("3651 Didion Dr", New Vector3(-318.022, 461.247, 108.009), LocationType.Residential, New Vector3(-312.51, 475.06, 111.82), 129)
    Public DD3581 As New Location("3581 Didion Dr", New Vector3(-347.302, 478.578, 111.87), LocationType.Residential, New Vector3(-355.32, 459.06, 116.47), 6)
    Public DD3589 As New Location("3589 Didion Dr", New Vector3(-480.869, 552.292, 119.27), LocationType.Residential, New Vector3(-500.82, 552.7, 120.43), 297)
    Public DD3587 As New Location("3587 Didion Dr", New Vector3(-468.259, 547.306, 119.666), LocationType.Residential, New Vector3(-458.82, 537.47, 121.46), 352)
    Public DD3585 As New Location("3585 Didion Dr", New Vector3(-437.985, 545.711, 121.246), LocationType.Residential, New Vector3(-437.14, 540.93, 122.13), 352)
    Public DD3583 As New Location("3583 Didion Dr", New Vector3(-379.92, 512.65, 119.83), LocationType.Residential, New Vector3(-386.4, 505.07, 120.41), 330)

    Public MR6085 As New Location("6085 Milton Rd", New Vector3(-656.424, 909.115, 227.743), LocationType.Residential, New Vector3(-659.34, 888.76, 229.25), 13)
    Public MR4589 As New Location("4589 Milton Rd", New Vector3(-597.672, 863.667, 210.149), LocationType.Residential, New Vector3(-599, 852.85, 211.25), 6)
    Public MR4588 As New Location("4588 Milton Rd", New Vector3(-549.421, 836.533, 197.679), LocationType.Residential, New Vector3(-548.63, 827.78, 197.51), 27)
    Public MR4587 As New Location("4587 Milton Rd", New Vector3(-479.269, 800.352, 180.562), LocationType.Residential, New Vector3(-496.07, 799.09, 184.19), 257)
    Public MR4586 As New Location("4586 Milton Rd", New Vector3(-481.381, 742.255, 163.363), LocationType.Residential, New Vector3(-492.34, 737.86, 162.83), 314)
    Public MR4585 As New Location("4585 Milton Rd", New Vector3(-529.918, 700.58, 149.313), LocationType.Residential, New Vector3(-533.23, 708.61, 152.91), 211)
    Public MR2850 As New Location("2850 Milton Rd", New Vector3(-509.213, 625.94, 131.747), LocationType.Residential, New Vector3(-521.85, 627.93, 137.97), 275)
    Public MR2848 As New Location("2848 Milton Rd", New Vector3(-506.436, 576.881, 119.951), LocationType.Residential, New Vector3(-519.14, 594.95, 120.84), 207)
    Public MR3545 As New Location("3545 Milton Rd", New Vector3(-532.605, 536.938, 110.266), LocationType.Residential, New Vector3(-527.28, 518.16, 112.94), 46)
    Public MR2846 As New Location("2846 Milton Rd", New Vector3(-540.44, 542.03, 109.99), LocationType.Residential, New Vector3(-552.33, 540, 110.33), 229)
    Public MR3543 As New Location("3543 Milton Rd", New Vector3(-545.46, 490.96, 103.51), LocationType.Residential, New Vector3(-538.21, 477.76, 103.18), 22)
    Public MR3548 As New Location("3548 Milton Rd", New Vector3(-522.28, 396.8, 93.29), LocationType.Residential, New Vector3(-502.37, 399.91, 97.41), 62)
    Public MR3842 As New Location("3842 Milton Rd", New Vector3(-483.19, 598.37, 126.51), LocationType.Residential, New Vector3(-475.24, 585.9, 128.68), 44)
    Public AJD2103 As New Location("2103 Ace Jones Dr", New Vector3(-1531.82, 438.83, 107.83), LocationType.Residential, New Vector3(-1540.07, 421.57, 110.01), 5)
    Public AJD2105 As New Location("2105 Ace Jones Dr", New Vector3(-1513.27, 433.71, 109.95), LocationType.Residential, New Vector3(-1495.75, 437.85, 112.5), 68)
    Public AJD2107 As New Location("2107 Ace Jones Dr", New Vector3(-1473.67, 518.31, 117.19), LocationType.Residential, New Vector3(-1454.26, 512.88, 117.63), 102)
    Public NSA1102 As New Location("1102 North Sheldon Ave", New Vector3(-1493.39, 511.81, 116.73), LocationType.Residential, New Vector3(-1499.76, 522.91, 118.27), 209)
    Public NSA1107 As New Location("1107 North Sheldon Ave", New Vector3(-1292.56, 631.21, 137.32), LocationType.Residential, New Vector3(-1278.97, 628.8, 142.31), 126)
    Public NSA1109 As New Location("1109 North Sheldon Ave", New Vector3(-1237.13, 655.33, 141.49), LocationType.Residential, New Vector3(-1247.92, 643.67, 142.62), 305)
    Public NSA1111 As New Location("1111 North Sheldon Ave", New Vector3(-1225.09, 665.88, 142.96), LocationType.Residential, New Vector3(-1218.73, 666.08, 144.53), 85)
    Public NSA1113 As New Location("1113 North Sheldon Ave", New Vector3(-1202.93, 691.86, 146.28), LocationType.Residential, New Vector3(-1197.29, 693.49, 147.42), 88)
    Public NSA1115 As New Location("1115 North Sheldon Ave", New Vector3(-1163.07, 747.48, 153.66), LocationType.Residential, New Vector3(-1165.25, 728.7, 155.61), 53)
    Public NSA1117 As New Location("1117 North Sheldon Ave", New Vector3(-1118.39, 775.59, 161.44), LocationType.Residential, New Vector3(-1118.37, 762.3, 164.29), 35)
    Public NSA1112 As New Location("1112 North Sheldon Ave", New Vector3(-1040.12, 792.68, 166.92), LocationType.Residential, New Vector3(-1051.57, 794.85, 167.01), 207)
    Public NSA1110 As New Location("1110 North Sheldon Ave", New Vector3(-1095.98, 786.03, 163.44), LocationType.Residential, New Vector3(-1100.32, 796.2, 166.99), 200)
    Public NSA1108 As New Location("1108 North Sheldon Ave", New Vector3(-1118.1, 781.31, 166.62), LocationType.Residential, New Vector3(-1129.95, 783.95, 163.89), 261)
    Public NSA11152 As New Location("1115 North Sheldon Ave", New Vector3(-995, 789.6, 171.8), LocationType.Residential, New Vector3(-997.1, 768.5, 171.5), 22)
    Public NSA11172 As New Location("1117 North Sheldon Ave", New Vector3(-970.3, 766.9, 174.8), LocationType.Residential, New Vector3(-972.7, 753, 176.4), 344)
    Public NSA1119 As New Location("1119 North Sheldon Ave", New Vector3(-906.4, 789, 185.1), LocationType.Residential, New Vector3(-911.8, 778.4, 187), 9)
    Public NSA1206 As New Location("1206 North Sheldon Ave", New Vector3(-870.1, 797, 190.4), LocationType.Residential, New Vector3(-867.5, 785.2, 191.9), 15)
    Public NSA1121 As New Location("1121 North Sheldon Ave", New Vector3(-827.6, 815.4, 198.5), LocationType.Residential, New Vector3(-824.6, 807.6, 202.6), 23)
    Public NSA1118 As New Location("1118 North Sheldon Ave", New Vector3(-939.7, 792.7, 180.8), LocationType.Residential, New Vector3(-931.1, 807.4, 184.8), 180)
    Public NSA1116 As New Location("1116 North Sheldon Ave", New Vector3(-957.8, 796.5, 177.7), LocationType.Residential, New Vector3(-962.7, 813.1, 177.6), 185)
    Public NSA1114 As New Location("1114 North Sheldon Ave", New Vector3(-993.2, 804, 172.1), LocationType.Residential, New Vector3(-998.7, 816.1, 173), 230)
    Public NDY1201 As New Location("1201 Normandy Dr", New Vector3(-743.5, 817, 213.1), LocationType.Residential, New Vector3(-746.8, 808.1, 215), 340)
    Public NDY1203 As New Location("1203 Normandy Dr", New Vector3(-661.1, 813.9, 199.3), LocationType.Residential, New Vector3(-655.3, 803.4, 199), 6)
    Public NDY1205 As New Location("1205 Normandy Dr", New Vector3(-588.1, 787.4, 188.2), LocationType.Residential, New Vector3(-594.9, 780.8, 189.1), 312)
    Public NDY1207 As New Location("1207 Normandy Dr", New Vector3(-698, 711.6, 157.3), LocationType.Residential, New Vector3(-707.3, 709.9, 162), 286)
    Public NDY2856 As New Location("2856 Normandy Dr", New Vector3(-614.7, 683.4, 149.1), LocationType.Residential, New Vector3(-606.3, 673, 151.6), 350)
    Public NDY2117 As New Location("2117 Normandy Dr", New Vector3(-552.8, 669, 144.1), LocationType.Residential, New Vector3(-559, 664.4, 145.5), 317)
    Public NDY4136 As New Location("4136 Normandy Dr", New Vector3(-561.8, 677.6, 145.2), LocationType.Residential, New Vector3(-565, 683.7, 146.2), 198)
    Public NDY1202 As New Location("1202 Normandy Dr", New Vector3(-666.8, 760.4, 174.2), LocationType.Residential, New Vector3(-663.2, 741.4, 174.3), 341)
    Public NDY1200 As New Location("1200 Normandy Dr", New Vector3(-582.9, 740.8, 183.2), LocationType.Residential, New Vector3(-579.7, 733.8, 184.2), 26)
    Public NDY1198 As New Location("1198 Normandy Dr", New Vector3(-601.2, 801.8, 190.6), LocationType.Residential, New Vector3(-599.8, 806.7, 191.1), 180)

    Public HA1105 As New Location("1105 Hangman Ave", New Vector3(-1358.67, 611.61, 133.36), LocationType.Residential, New Vector3(-1366.6, 611.25, 133.92), 272)
    Public HA2106 As New Location("2106 Hangman Ave", New Vector3(-1354.85, 608.47, 133.3), LocationType.Residential, New Vector3(-1338.54, 605.89, 134.38), 89)
    Public HA1103 As New Location("1103 Hangman Ave", New Vector3(-1353.4, 576.59, 130.56), LocationType.Residential, New Vector3(-1365.62, 567.2, 134.97), 288)
    Public HA2108 As New Location("2108 Hangman Ave", New Vector3(-1363.52, 556.62, 127.66), LocationType.Residential, New Vector3(-1346.59, 560.57, 130.53), 51)
    Public HA1101 As New Location("1101 Hangman Ave", New Vector3(-1412.23, 556.19, 123.1), LocationType.Residential, New Vector3(-1404.22, 561.31, 125.41), 162)
    Public HCA2888 As New Location("2888 Hillcrest Ave", New Vector3(-1047.57, 769.86, 166.75), LocationType.Residential, New Vector3(-1055.87, 761.43, 167.32), 335)
    Public HCA2886 As New Location("2886 Hillcrest Ave", New Vector3(-1043.81, 743.97, 166.23), LocationType.Residential, New Vector3(-1066.26, 727.89, 165.74), 330)
    Public HCA2884 As New Location("2884 Hillcrest Ave", New Vector3(-1017.98, 703.58, 161.5), LocationType.Residential, New Vector3(-1034.18, 686.01, 161.3), 79)
    Public HCA2882 As New Location("2882 Hillcrest Ave", New Vector3(-988.74, 694.61, 157.26), LocationType.Residential, New Vector3(-972.4, 685.69, 158.03), 22)
    Public HCA2880 As New Location("2880 Hillcrest Ave", New Vector3(-932.88, 697.59, 151.78), LocationType.Residential, New Vector3(-931.2, 691.19, 153.47), 19)
    Public HCA2878 As New Location("2878 Hillcrest Ave", New Vector3(-911.61, 699.12, 150.62), LocationType.Residential, New Vector3(-908.08, 694.55, 151.43), 24)
    Public HCA2876 As New Location("2876 Hillcrest Ave", New Vector3(-887.29, 704.76, 149.34), LocationType.Residential, New Vector3(-885.71, 699.43, 151.27), 44)
    Public HCA2874 As New Location("2874 Hillcrest Ave", New Vector3(-860.57, 704.02, 148.31), LocationType.Residential, New Vector3(-853.64, 695.87, 148.78), 42)
    Public HCA2872 As New Location("2872 Hillcrest Ave", New Vector3(-810.79, 712.12, 146.17), LocationType.Residential, New Vector3(-819.78, 697.54, 148.11), 304)
    Public HCA2870 As New Location("2870 Hillcrest Ave", New Vector3(-756.23, 659.58, 142.4), LocationType.Residential, New Vector3(-765.34, 650.81, 145.5), 315)
    Public HCA2868 As New Location("2868 Hillcrest Ave", New Vector3(-750.34, 627.82, 141.7), LocationType.Residential, New Vector3(-752.46, 620.55, 142.41), 333)
    Public HCA2866 As New Location("2866 Hillcrest Ave", New Vector3(-740.26, 603.83, 141.28), LocationType.Residential, New Vector3(-732.92, 593.57, 142.48), 356)
    Public HCA2864 As New Location("2864 Hillcrest Ave", New Vector3(-705.05, 593.98, 141.52), LocationType.Residential, New Vector3(-704.01, 589.44, 141.93), 23)
    Public HCA2862 As New Location("2862 Hillcrest Ave", New Vector3(-691, 600.73, 142.5), LocationType.Residential, New Vector3(-686.51, 596.42, 143.64), 47)
    Public HCA2860 As New Location("2860 Hillcrest Ave", New Vector3(-677.32, 647.44, 148.05), LocationType.Residential, New Vector3(-669.69, 638.66, 149.53), 45)
    Public HCA2858 As New Location("2858 Hillcrest Ave", New Vector3(-677.08, 673.88, 151.18), LocationType.Residential, New Vector3(-661.66, 681.05, 153.92), 164)
    Public HCA2859 As New Location("2859 Hillcrest Ave", New Vector3(-682.87, 669.84, 150.7), LocationType.Residential, New Vector3(-700.48, 648.63, 155.18), 333)
    Public CED2123 As New Location("2123 Cockingend Dr", New Vector3(-1069.6, 438.6, 73.2), LocationType.Residential, New Vector3(-1052, 432.2, 77.1), 183)
    Public CED2124 As New Location("2124 Cockingend Dr", New Vector3(-1019, 501.3, 79), LocationType.Residential, New Vector3(-1041.1, 532.4, 84.6), 234)
    Public CED2125 As New Location("2125 Cockingend Dr", New Vector3(-1014, 492.8, 78.9), LocationType.Residential, New Vector3(-1009, 479.9, 79.4), 332)
    Public CED2126 As New Location("2126 Cockingend Dr", New Vector3(-1007.9, 508.2, 79.2), LocationType.Residential, New Vector3(-1007.5, 513, 79.6), 188)
    Public CED2127 As New Location("2127 Cockingend Dr", New Vector3(-993.5, 497, 80.4), LocationType.Residential, New Vector3(-986.8, 487.9, 82.3), 18)
    Public CED2128 As New Location("2128 Cockingend Dr", New Vector3(-978.3, 515.8, 81.1), LocationType.Residential, New Vector3(-968.1, 509.9, 81.7), 150)
    Public CED2129 As New Location("2129 Cockingend Dr", New Vector3(-960.1, 442.2, 79.3), LocationType.Residential, New Vector3(-968.3, 436.3, 80.6), 254)
    Public CED2130 As New Location("2130 Cockingend Dr", New Vector3(-949.7, 440.7, 79.3), LocationType.Residential, New Vector3(-950.6, 465, 80.8), 108)
    Public MWT2122 As New Location("2122 Mad Wayne Thunder Dr", New Vector3(-1085.4, 453.2, 75.7), LocationType.Residential, New Vector3(-1062.4, 475.7, 81.3), 238)
    Public MWT2120 As New Location("2120 Mad Wayne Thunder Dr", New Vector3(-1114.1, 468.1, 80), LocationType.Residential, New Vector3(-1122.5, 485.3, 82.2), 170)
    Public MWT2118 As New Location("2118 Mad Wayne Thunder Dr", New Vector3(-1152.9, 472, 84), LocationType.Residential, New Vector3(-1158.7, 481.6, 86.1), 189)
    Public MWT2112 As New Location("2112 Mad Wayne Thunder Dr", New Vector3(-1354.8, 463.4, 102.2), LocationType.Residential, New Vector3(-1343.5, 481.3, 102.8), 92)
    Public MWT2110 As New Location("2110 Mad Wayne Thunder Dr", New Vector3(-1415.8, 537.9, 121.5), LocationType.Residential, New Vector3(-1405.9, 527.2, 123.8), 36)
    Public MWT2109 As New Location("2109 Mad Wayne Thunder Dr", New Vector3(-1415.3, 472.1, 108.1), LocationType.Residential, New Vector3(-1413.4, 462.6, 109.2), 55)
    Public MWT2111b As New Location("2111b Mad Wayne Thunder Dr", New Vector3(-1389.1, 461.9, 105.2), LocationType.Residential, New Vector3(-1372.2, 444.1, 105.9), 76)
    Public MWT2111 As New Location("2111 Mad Wayne Thunder Dr", New Vector3(-1315.3, 456.9, 98.6), LocationType.Residential, New Vector3(-1308, 449.5, 101), 12)
    Public MWT2113 As New Location("2113 Mad Wayne Thunder Dr", New Vector3(-1295.9, 457.4, 96.8), LocationType.Residential, New Vector3(-1294.3, 454.4, 97.6), 19)
    Public MWT2114 As New Location("2114 Mad Wayne Thunder Dr", New Vector3(-1251.7, 487.4, 93.6), LocationType.Residential, New Vector3(-1277.3, 497.2, 97.9), 265)
    Public MWT2115 As New Location("2115 Mad Wayne Thunder Dr", New Vector3(-1273.5, 457.9, 95), LocationType.Residential, New Vector3(-1263.8, 455.6, 94.8), 54)
    Public MWT2116 As New Location("2116 Mad Wayne Thunder Dr", New Vector3(-1243.1, 483.3, 92.7), LocationType.Residential, New Vector3(-1218.4, 505.9, 95.7), 171)
    Public MWT2117 As New Location("2117 Mad Wayne Thunder Dr", New Vector3(-1215.5, 466.3, 90.1), LocationType.Residential, New Vector3(-1215.8, 458.5, 91.9), 358)
    Public MWT2119 As New Location("2119 Mad Wayne Thunder Dr", New Vector3(-1180.8, 456.9, 86.4), LocationType.Residential, New Vector3(-1175, 440.2, 86.8), 88)
    Public MWT2121 As New Location("2121 Mad Wayne Thunder Dr", New Vector3(-1085, 436.9, 74.1), LocationType.Residential, New Vector3(-1093, 427.2, 75.7), 274)
    Public SMM1 As New Location("S Mo Milton Dr", New Vector3(-847.8, 304.5, 85.8), LocationType.Residential, New Vector3(-876.2, 305.8, 48.2), 251)
    Public SMM2 As New Location("S Mo Milton Dr", New Vector3(-822, 281, 86), LocationType.Residential, New Vector3(-820, 267.9, 86.4), 73)
    Public SMM3 As New Location("S Mo Milton Dr", New Vector3(-864.4, 386, 87.2), LocationType.Residential, New Vector3(-882, 364.5, 85.4), 38)
    Public SMM2148 As New Location("2148 S Mo Milton Dr", New Vector3(-850.1, 459, 86.6), LocationType.Residential, New Vector3(-843.6, 467.5, 87.6), 183)
    Public SMM2146 As New Location("2146 S Mo Milton Dr", New Vector3(-861.1, 513.7, 88.5), LocationType.Residential, New Vector3(-849.6, 509.1, 90.8), 40)
    Public SMM2144 As New Location("2144 S Mo Milton Dr", New Vector3(-875, 538.8, 91), LocationType.Residential, New Vector3(-873.6, 562.6, 96.6), 129)
    Public SMM2142 As New Location("2142 S Mo Milton Dr", New Vector3(-919.7, 576.7, 98.5), LocationType.Residential, New Vector3(-904.5, 587.1, 101), 148)
    Public SMM2140 As New Location("2140 S Mo Milton Dr", New Vector3(-962.3, 592.6, 100.9), LocationType.Residential, New Vector3(-958.4, 605.3, 105.4), 157)
    Public SMM2134 As New Location("2134 S Mo Milton Dr", New Vector3(-1088.8, 589.4, 102.4), LocationType.Residential, New Vector3(-1107.3, 593.3, 104.5), 222)
    Public SMM2130 As New Location("2130 S Mo Milton Dr", New Vector3(-1193.1, 553, 98.3), LocationType.Residential, New Vector3(-1192.8, 563.7, 100.3), 185)
    Public SMM2131 As New Location("2131 S Mo Milton Dr", New Vector3(-1146.5, 550.6, 100.9), LocationType.Residential, New Vector3(-1146.7, 545.9, 101.7), 10)
    Public SMM2133 As New Location("2133 S Mo Milton Dr", New Vector3(-1130.4, 555.4, 101.5), LocationType.Residential, New Vector3(-1125.5, 548.8, 102.6), 13)
    Public SMM2135 As New Location("2135 S Mo Milton Dr", New Vector3(-1114.3, 560.2, 101.9), LocationType.Residential, New Vector3(-1090.2, 548.7, 103.6), 121)
    Public SMM2137 As New Location("2137 S Mo Milton Dr", New Vector3(-1022.5, 594.4, 102.5), LocationType.Residential, New Vector3(-1022.6, 587.2, 103.2), 359)
    Public SMM2139 As New Location("2139 S Mo Milton Dr", New Vector3(-973.7, 588, 101.2), LocationType.Residential, New Vector3(-974.5, 582.2, 102.8), 348)
    Public SMM2141 As New Location("2141 S Mo Milton Dr", New Vector3(-947.1, 578.8, 100), LocationType.Residential, New Vector3(-948, 568.6, 101.5), 343)
    Public SMM2143 As New Location("2143 S Mo Milton Dr", New Vector3(-925, 568.5, 98.5), LocationType.Residential, New Vector3(-924.7, 561.9, 99.9), 359)
    Public SMM2145 As New Location("2145 S Mo Milton Dr", New Vector3(-906.6, 557.3, 96.2), LocationType.Residential, New Vector3(-907.1, 545.4, 100.2), 323)
    Public SMM2147 As New Location("2147 S Mo Milton Dr", New Vector3(-873.1, 522.1, 89.3), LocationType.Residential, New Vector3(-884, 518.3, 92.4), 288)
    Public SMM2117 As New Location("2117 S Mo Milton Dr", New Vector3(-857.9, 464.1, 86.7), LocationType.Residential, New Vector3(-866.5, 457.1, 88.3), 183)

    Public PPD2835 As New Location("2835 Picture Perfect Dr", New Vector3(-806, 436.7, 90.2), LocationType.Residential, New Vector3(-825.1, 423.8, 91.8), 3)
    Public PPD2837 As New Location("2837 Picture Perfect Dr", New Vector3(-762.9, 446.3, 98.4), LocationType.Residential, New Vector3(-762.3, 431.2, 100.2), 19)
    Public PPD2839 As New Location("2839 Picture Perfect Dr", New Vector3(-732, 468.6, 105.3), LocationType.Residential, New Vector3(-718.2, 449.2, 106.9), 38)
    Public PPD2841 As New Location("2841 Picture Perfect Dr", New Vector3(-655.5, 491.9, 109.3), LocationType.Residential, New Vector3(-667.6, 472.3, 114.1), 16)
    Public PPD2843 As New Location("2843 Picture Perfect Dr", New Vector3(-615.3, 499.5, 106.8), LocationType.Residential, New Vector3(-623.2, 489.4, 108.8), 354)
    Public PPD2845 As New Location("2845 Picture Perfect Dr", New Vector3(-580.8, 504.7, 105.1), LocationType.Residential, New Vector3(-580.3, 492.6, 108.9), 16)
    Public PPD2844 As New Location("2844 Picture Perfect Dr", New Vector3(-579, 516.1, 105.7), LocationType.Residential, New Vector3(-595.3, 529.8, 107.8), 205)
    Public PPD2842 As New Location("2842 Picture Perfect Dr", New Vector3(-643.7, 502.5, 108.2), LocationType.Residential, New Vector3(-640.7, 519.8, 109.7), 183)
    Public PPD2840 As New Location("2840 Picture Perfect Dr", New Vector3(-684.5, 494.7, 109.3), LocationType.Residential, New Vector3(-678.6, 511.2, 113.5), 200)
    Public PPD2838 As New Location("2838 Picture Perfect Dr", New Vector3(-712.2, 486.2, 107.9), LocationType.Residential, New Vector3(-724.2, 487.7, 109.4), 295)
    Public PPD2836 As New Location("2836 Picture Perfect Dr", New Vector3(-778.9, 449.1, 95.5), LocationType.Residential, New Vector3(-782.8, 459.4, 100.2), 200)

    Public KHD1 As New Location("1 Kimble Hill Dr", New Vector3(-470.4, 650.2, 143.9), LocationType.Residential, New Vector3(-476.5, 648, 144.4), 5)
    Public KHD2 As New Location("2 Kimble Hill Dr", New Vector3(-443, 679, 151.9), LocationType.Residential, New Vector3(-446, 685.7, 153), 205)
    Public KHD3 As New Location("3 Kimble Hill Dr", New Vector3(-393.6, 673.9, 162.7), LocationType.Residential, New Vector3(-400, 665.2, 163.8), 3)
    Public KHD4 As New Location("4 Kimble Hill Dr", New Vector3(-345.3, 661.2, 169.4), LocationType.Residential, New Vector3(-340.2, 668.3, 172.8), 259)
    Public KHD5 As New Location("5 Kimble Hill Dr", New Vector3(-345.4, 635.1, 171.9), LocationType.Residential, New Vector3(-339.9, 625.6, 171.4), 60)
    Public KHD6 As New Location("6 Kimble Hill Dr", New Vector3(-320.6, 636.3, 173.2), LocationType.Residential, New Vector3(-307.7, 643.2, 176.1), 119)
    Public KHD7 As New Location("7 Kimble Hill Dr", New Vector3(-274.6, 603.2, 181.4), LocationType.Residential, New Vector3(-293.5, 600.9, 181.6), 354)
    Public KHD8 As New Location("8 Kimble Hill Dr", New Vector3(-271.9, 613.8, 181.5), LocationType.Residential, New Vector3(-257.6, 632.4, 187.8), 78)
    Public KHD9 As New Location("9 Kimble Hill Dr", New Vector3(-223.5, 593.7, 190.1), LocationType.Residential, New Vector3(-232.5, 588.4, 190.5), 359)
    Public KHD10 As New Location("10 Kimble Hill Dr", New Vector3(-187.9, 607.8, 196), LocationType.Residential, New Vector3(-189.2, 618, 199.7), 182)
    Public KHD11 As New Location("11 Kimble Hill Dr", New Vector3(-179.2, 594.3, 197.3), LocationType.Residential, New Vector3(-185.5, 591.2, 197.8), 1)
    Public KHD13 As New Location("13 Kimble Hill Dr", New Vector3(-145.7, 596.4, 203.3), LocationType.Residential, New Vector3(-126.3, 588.4, 204.7), 358)

    Public WOD3653 As New Location("3653 Wild Oats Dr", New Vector3(-231.1, 497.6, 127.4), LocationType.Residential, New Vector3(-230.2, 488.3, 128.8), 2)
    Public WOD3655 As New Location("3655 Wild Oats Dr", New Vector3(-178.2, 506.9, 135.7), LocationType.Residential, New Vector3(-174.7, 502.6, 137.4), 97)
    Public WOD3657 As New Location("3657 Wild Oats Dr", New Vector3(-108.2, 511.5, 143.1), LocationType.Residential, New Vector3(-110, 502.7, 143.3), 352)
    Public WOD3659 As New Location("3659 Wild Oats Dr", New Vector3(-57.7, 495.9, 144.2), LocationType.Residential, New Vector3(-66.6, 490.2, 144.7), 341)
    Public WOD3661 As New Location("3661 Wild Oats Dr", New Vector3(-3, 472.3, 145.4), LocationType.Residential, New Vector3(-7.7, 468.3, 145.9), 338)
    Public WOD3663 As New Location("3663 Wild Oats Dr", New Vector3(64.2, 456, 146.4), LocationType.Residential, New Vector3(57.6, 450, 147), 332)
    Public WOD3664 As New Location("3664 Wild Oats Dr", New Vector3(56, 467.7, 146.4), LocationType.Residential, New Vector3(42.9, 468.5, 148.1), 214)
    Public WOD3666 As New Location("3666 Wild Oats Dr", New Vector3(92.4, 485.6, 147.2), LocationType.Residential, New Vector3(80.2, 486, 148.2), 216)
    Public WOD3661a As New Location("3661a Wild Oats Dr", New Vector3(103.3, 478.7, 146.9), LocationType.Residential, New Vector3(106.9, 466.9, 147.6), 15)
    Public WOD3668 As New Location("3668 Wild Oats Dr", New Vector3(113.8, 494.3, 146.8), LocationType.Residential, New Vector3(119.5, 494, 147.3), 112)
    Public WOD3667 As New Location("3667 Wild Oats Dr", New Vector3(174.4, 484.3, 142), LocationType.Residential, New Vector3(166.9, 473.9, 142.5), 81)
    Public WOD3669 As New Location("3669 Wild Oats Dr", New Vector3(220.1, 516.4, 140.3), LocationType.Residential, New Vector3(223.8, 513.8, 140.8), 51)


    Public Forum1804 As New Location("1804 Forum Dr", New Vector3(-151.3, -1554.9, 34.4), LocationType.Residential, New Vector3(-170.7, -1538.3, 35.1), 325)
    Public Forum1802 As New Location("1802 Forum Dr", New Vector3(-187.7, -1605.6, 33.6), LocationType.Residential, New Vector3(-211.2, -1598.4, 34.9), 194)
    Public Forum1800 As New Location("1800 Forum Dr", New Vector3(-188.6, -1671.9, 33.1), LocationType.Residential, New Vector3(-214.2, -1667.6, 34.5), 189)
    Public Forum1801 As New Location("1801 Forum Dr", New Vector3(-159.7, -1704.1, 30.6), LocationType.Residential, New Vector3(-150.8, -1689.3, 32.9), 134)
    Public Forum1803 As New Location("1803 Forum Dr", New Vector3(-178.7, -1634, 32.8), LocationType.Residential, New Vector3(-160.2, -1637.1, 34), 55)
    Public Forum1805 As New Location("1805 Forum Dr", New Vector3(-158, -1576.5, 34.4), LocationType.Residential, New Vector3(-138.2, -1590.1, 34.2), 120)
    Public Forum1807 As New Location("1807 Forum Dr", New Vector3(-142.3, -1558.8, 34), LocationType.Residential, New Vector3(-152.6, -1576.1, 34.2), 354)
    Public Brouge1 As New Location("Brouge Ave", New Vector3(260.7, -1683.6, 28.8), LocationType.Residential, New Vector3(252.9, -1671.8, 29.7), 219)
    Public Brouge2 As New Location("Brouge Ave", New Vector3(251, -1695.5, 28.7), LocationType.Residential, New Vector3(242, -1688.1, 29.3), 241)
    Public Brouge3 As New Location("Brouge Ave", New Vector3(233.2, -1716, 28.6), LocationType.Residential, New Vector3(223.1, -1703.2, 29.7), 227)
    Public Brouge4 As New Location("Brouge Ave", New Vector3(226, -1725.1, 28.5), LocationType.Residential, New Vector3(217.2, -1717.3, 29.3), 272)


    Public WMD1 As New Location("West Mirror Drive", New Vector3(1073.1, -390.1, 66.9), LocationType.Residential, New Vector3(1061.3, -378.5, 68.2), 226)
    Public WMD2 As New Location("West Mirror Drive", New Vector3(1040.6, -418, 65.9), LocationType.Residential, New Vector3(1029.7, -409.2, 65.9), 214)
    Public WMD3 As New Location("West Mirror Drive", New Vector3(1018.9, -434.9, 64.6), LocationType.Residential, New Vector3(1011.1, -422.8, 65), 249)
    Public WMD4 As New Location("West Mirror Drive", New Vector3(1001, -448.8, 63.6), LocationType.Residential, New Vector3(988, -433.4, 63.9), 226)
    Public WMD5 As New Location("West Mirror Drive", New Vector3(929.3, -494.7, 59.3), LocationType.Residential, New Vector3(921.9, -478.4, 61.1), 201)
    Public WMD6 As New Location("West Mirror Drive", New Vector3(874.2, -521.2, 57), LocationType.Residential, New Vector3(862.4, -509.8, 57.3), 235)
    Public WMD7 As New Location("West Mirror Drive", New Vector3(866.8, -561.6, 57), LocationType.Residential, New Vector3(844.2, -563.1, 57.8), 227)
    Public WMD8 As New Location("West Mirror Drive", New Vector3(873.8, -572.8, 57), LocationType.Residential, New Vector3(861.6, -583.2, 58.2), 359)
    Public WMD9 As New Location("West Mirror Drive", New Vector3(899.9, -591.1, 57), LocationType.Residential, New Vector3(887.4, -607.5, 58.2), 331)
    Public WMD10 As New Location("West Mirror Drive", New Vector3(943.4, -624.3, 57.1), LocationType.Residential, New Vector3(929.4, -639.2, 58.2), 314)
    Public WMD11 As New Location("West Mirror Drive", New Vector3(961.7, -645.4, 57.1), LocationType.Residential, New Vector3(943.7, -653.6, 58.5), 262)
    Public WMD12 As New Location("West Mirror Drive", New Vector3(984.9, -690.1, 57.1), LocationType.Residential, New Vector3(971.2, -700.4, 58.5), 337)
    Public WMD13 As New Location("West Mirror Drive", New Vector3(992.9, -703.8, 57.1), LocationType.Residential, New Vector3(979.8, -715.6, 58), 316)
    Public WMD14 As New Location("West Mirror Drive", New Vector3(1007.8, -721.6, 57.2), LocationType.Residential, New Vector3(997.2, -729.4, 57.8), 312)
    Public WMD15 As New Location("West Mirror Drive", New Vector3(966.5, -635.6, 57.1), LocationType.Residential, New Vector3(979.8, -627.3, 59.2), 119)
    Public WMD16 As New Location("West Mirror Drive", New Vector3(873.7, -547.5, 57), LocationType.Residential, New Vector3(892.7, -540.8, 58.5), 108)
    Public WMD17 As New Location("West Mirror Drive", New Vector3(915.7, -511.5, 58.2), LocationType.Residential, New Vector3(924, -525.4, 59.6), 33)
    Public WMD18 As New Location("West Mirror Drive", New Vector3(938.1, -501.2, 59.6), LocationType.Residential, New Vector3(946.3, -518.7, 60.6), 24)
    Public WMD19 As New Location("West Mirror Drive", New Vector3(958.9, -489.7, 60.9), LocationType.Residential, New Vector3(969.7, -502.2, 62.1), 83)
    Public WMD20 As New Location("West Mirror Drive", New Vector3(1006, -456.5, 63.6), LocationType.Residential, New Vector3(1013.8, -468, 64.3), 40)
    Public EMD1 As New Location("East Mirror Drive", New Vector3(1275.5, -423.6, 68.6), LocationType.Residential, New Vector3(1263.2, -428.9, 69.8), 287)
    Public EMD2 As New Location("East Mirror Drive", New Vector3(1282.1, -456, 68.6), LocationType.Residential, New Vector3(1266.6, -457.6, 70.5), 267)
    Public EMD3 As New Location("East Mirror Drive", New Vector3(1271.4, -498.3, 68.6), LocationType.Residential, New Vector3(1252.1, -494.2, 69.7), 265)
    Public EMD4 As New Location("East Mirror Drive", New Vector3(1264.1, -520.7, 68.6), LocationType.Residential, New Vector3(1251.2, -515.2, 69.3), 252)
    Public EMD5 As New Location("East Mirror Drive", New Vector3(1263, -598.6, 68.5), LocationType.Residential, New Vector3(1241.3, -601.7, 69.4), 274)
    Public EMD6 As New Location("East Mirror Drive", New Vector3(1271.1, -617.5, 68.5), LocationType.Residential, New Vector3(1251.6, -621.7, 69.4), 272)
    Public EMD7 As New Location("East Mirror Drive", New Vector3(1289.3, -682, 65.2), LocationType.Residential, New Vector3(1271, -682.1, 66), 277)
    Public EMD8 As New Location("East Mirror Drive", New Vector3(1276.4, -710.9, 64.2), LocationType.Residential, New Vector3(1265.9, -703.2, 64.6), 244)
    Public BridgeSt1 As New Location("Bridge Street", New Vector3(1077.4, -482.8, 63.6), LocationType.Residential, New Vector3(1089.7, -484.5, 65.7), 81)
    Public BridgeSt2 As New Location("Bridge Street", New Vector3(1081.6, -461.4, 64.7), LocationType.Residential, New Vector3(1098.5, -464.6, 67.3), 105)
    Public BridgeSt3 As New Location("Bridge Street", New Vector3(1084.3, -446.9, 65.5), LocationType.Residential, New Vector3(1099.5, -450.9, 67.6), 85)
    Public BridgeSt4 As New Location("Bridge Street", New Vector3(1088.8, -408.5, 66.8), LocationType.Residential, New Vector3(1100, -411.4, 67.6), 94)
    Public BridgeSt5 As New Location("Bridge Street", New Vector3(1073.6, -450, 65.3), LocationType.Residential, New Vector3(1056.8, -448, 66.3), 304)
    Public BridgeSt6 As New Location("Bridge Street", New Vector3(1068.9, -474.8, 63.9), LocationType.Residential, New Vector3(1052, -470.8, 63.9), 256)
    Public BridgeSt7 As New Location("Bridge Street", New Vector3(1062, -507.7, 62.1), LocationType.Residential, New Vector3(1046.4, -497.5, 64.1), 323)
    Public BridgeSt8 As New Location("Bridge Street", New Vector3(831.1, -194.5, 72.5), LocationType.Residential, New Vector3(840, -182, 74.2), 116)
    Public BridgeSt9 As New Location("Bridge Street", New Vector3(767.8, -158.9, 74.2), LocationType.Residential, New Vector3(773.4, -150.9, 75.4), 152)

    Public JamestownSt1 As New Location("1011 Jamestown St", New Vector3(486, -1811.5, 27.9), LocationType.Residential, New Vector3(495.6, -1823.1, 28.9), 322)
    Public JamestownSt2 As New Location("1012 Jamestown St", New Vector3(501.4, -1789.2, 28), LocationType.Residential, New Vector3(512.2, -1790.7, 28.9), 89)
    Public JamestownSt3 As New Location("1013 Jamestown St", New Vector3(500.2, -1780.4, 28), LocationType.Residential, New Vector3(513.6, -1779.8, 28.9), 101)
    Public JamestownSt4 As New Location("1014 Jamestown St", New Vector3(508.3, -1705.3, 28.8), LocationType.Residential, New Vector3(498, -1700.2, 29.4), 180)
    Public JamestownSt5 As New Location("1015 Jamestown St", New Vector3(502.3, -1720.5, 28.8), LocationType.Residential, New Vector3(490.2, -1714.3, 29.7), 240)
    Public JamestownSt6 As New Location("1016 Jamestown St", New Vector3(491.5, -1749.1, 28.1), LocationType.Residential, New Vector3(479.6, -1737.3, 29.2), 242)
    Public JamestownSt7 As New Location("1017 Jamestown St", New Vector3(487.9, -1761.9, 28), LocationType.Residential, New Vector3(475.7, -1756.9, 28.7), 277)
    Public JamestownSt8 As New Location("1018 Jamestown St", New Vector3(488.4, -1775.5, 28), LocationType.Residential, New Vector3(472.5, -1774.7, 29.1), 269)
    Public JamestownSt9 As New Location("1019 Jamestown St", New Vector3(444.1, -1844.5, 27.4), LocationType.Residential, New Vector3(440.3, -1830, 28.4), 132)
    Public JamestownSt10 As New Location("1010 Jamestown St", New Vector3(436, -1852.3, 27.2), LocationType.Residential, New Vector3(427.5, -1841.8, 28.5), 262)
    Public JamestownSt11 As New Location("1011 Jamestown St", New Vector3(423, -1864.9, 26.5), LocationType.Residential, New Vector3(412.7, -1856.2, 27.3), 318)
    Public JamestownSt12 As New Location("1012 Jamestown St", New Vector3(409.9, -1877.4, 25.7), LocationType.Residential, New Vector3(399.2, -1864.7, 26.7), 220)
    Public JamestownSt13 As New Location("1013 Jamestown St", New Vector3(395.4, -1892.3, 24.8), LocationType.Residential, New Vector3(385.8, -1882.2, 25.9), 232)
    Public JamestownSt14 As New Location("1014 Jamestown St", New Vector3(371.7, -1914.8, 24.1), LocationType.Residential, New Vector3(368.5, -1896.8, 25.2), 182)
    Public JamestownSt15 As New Location("1015 Jamestown St", New Vector3(331.7, -1953.2, 23.8), LocationType.Residential, New Vector3(324.2, -1937.6, 25), 133)
    Public JamestownSt16 As New Location("1016 Jamestown St", New Vector3(322.4, -1964, 23.5), LocationType.Residential, New Vector3(312.4, -1956.3, 24.6), 237)
    Public JamestownSt17 As New Location("1017 Jamestown St", New Vector3(306.2, -1983.1, 21.5), LocationType.Residential, New Vector3(296.6, -1971.9, 22.9), 230)
    Public JamestownSt18 As New Location("1018 Jamestown St", New Vector3(296.4, -1995, 20.2), LocationType.Residential, New Vector3(291.5, -1980.5, 21.6), 150)
    Public JamestownSt19 As New Location("1019 Jamestown St", New Vector3(290.4, -2002.5, 19.7), LocationType.Residential, New Vector3(279.9, -1993.4, 20.8), 228)
    Public JamestownSt20 As New Location("1020 Jamestown St", New Vector3(268.8, -2028.4, 18.1), LocationType.Residential, New Vector3(257, -2023.6, 19.3), 261)
    Public JamestownSt21 As New Location("1021 Jamestown St", New Vector3(249.6, -2050.5, 17.3), LocationType.Residential, New Vector3(236.2, -2046.3, 18.4), 325)

    Public RLB1 As New Location("1120 Roy Lowenstein Blvd", New Vector3(239.2, -1929.2, 23.5), LocationType.Residential, New Vector3(250.4, -1934.8, 24.7), 59)
    Public RLB2 As New Location("1124 Roy Lowenstein Blvd", New Vector3(258.8, -1905.1, 25.5), LocationType.Residential, New Vector3(270.2, -1917.2, 26.2), 69)
    Public RLB3 As New Location("1128 Roy Lowenstein Blvd", New Vector3(274.8, -1886.7, 26.2), LocationType.Residential, New Vector3(282.8, -1898.9, 27.3), 48)
    Public RLB4 As New Location("1132 Roy Lowenstein Blvd", New Vector3(316.2, -1833.3, 26.6), LocationType.Residential, New Vector3(328.8, -1845.4, 27.7), 45)
    Public RLB5 As New Location("1134 Roy Lowenstein Blvd", New Vector3(339.5, -1809, 27.6), LocationType.Residential, New Vector3(348.7, -1820.7, 28.9), 10)
    Public RLB6 As New Location("1144 Roy Lowenstein Blvd", New Vector3(395.9, -1742, 28.7), LocationType.Residential, New Vector3(405.7, -1751.6, 29.7), 58)
    Public RLB7 As New Location("1146 Roy Lowenstein Blvd", New Vector3(406.9, -1728.9, 28.7), LocationType.Residential, New Vector3(418.7, -1735.5, 29.6), 102)
    Public RLB8 As New Location("1148 Roy Lowenstein Blvd", New Vector3(420.1, -1713.5, 28.6), LocationType.Residential, New Vector3(431.1, -1725.6, 29.6), 154)
    Public RLB9 As New Location("1150 Roy Lowenstein Blvd", New Vector3(432.8, -1698.2, 28.7), LocationType.Residential, New Vector3(443.6, -1707.2, 29.7), 53)
    Public RLB10 As New Location("1139 Roy Lowenstein Blvd", New Vector3(342.1, -1754.8, 28.6), LocationType.Residential, New Vector3(332.4, -1741.2, 29.7), 183)
    Public RLB11 As New Location("1137 Roy Lowenstein Blvd", New Vector3(328.7, -1770.4, 28.3), LocationType.Residential, New Vector3(321.3, -1759.8, 29.6), 236)
    Public RLB12 As New Location("1135 Roy Lowenstein Blvd", New Vector3(314.7, -1787.5, 27.6), LocationType.Residential, New Vector3(304.9, -1775.9, 29.1), 224)
    Public RLB13 As New Location("1133 Roy Lowenstein Blvd", New Vector3(304.9, -1798.5, 27.1), LocationType.Residential, New Vector3(300.2, -1784.3, 28.4), 163)
    Public RLB14 As New Location("1131 Roy Lowenstein Blvd", New Vector3(304.9, -1798.5, 27.1), LocationType.Residential, New Vector3(289.3, -1792.8, 28.1), 271)
    Public RLB15 As New Location("1099 Roy Lowenstein Blvd", New Vector3(187.8, -1939.4, 20.5), LocationType.Residential, New Vector3(179, -1924.1, 21.4), 159)
    Public RLB16 As New Location("1097 Roy Lowenstein Blvd", New Vector3(174.4, -1954.6, 19.1), LocationType.Residential, New Vector3(165.5, -1945.3, 20.2), 276)
    Public RLB17 As New Location("1095 Roy Lowenstein Blvd", New Vector3(159.8, -1971.2, 18), LocationType.Residential, New Vector3(148.9, -1960.6, 19.5), 232)
    Public RLB18 As New Location("1093 Roy Lowenstein Blvd", New Vector3(153, -1979.6, 17.8), LocationType.Residential, New Vector3(144.2, -1969.2, 18.9), 149)

    Public Fudge1 As New Location("25 Fudge Lane", New Vector3(1187.9, -1647.7, 40.5), LocationType.Residential, New Vector3(1193.3, -1656.3, 43), 33)
    Public Fudge2 As New Location("27 Fudge Lane", New Vector3(1208, -1634, 45.8), LocationType.Residential, New Vector3(1214.1, -1644.1, 48.6), 23)
    Public Fudge3 As New Location("29 Fudge Lane", New Vector3(1239.9, -1616.4, 51.7), LocationType.Residential, New Vector3(1245.1, -1626.5, 53.3), 24)
    Public Fudge4 As New Location("31 Fudge Lane", New Vector3(1254.2, -1605.7, 52.7), LocationType.Residential, New Vector3(1261.3, -1616.6, 54.7), 32)
    Public Fudge5 As New Location("33 Fudge Lane", New Vector3(1282, -1585.9, 51.4), LocationType.Residential, New Vector3(1286.6, -1604.5, 54.8), 16)
    Public Fudge6 As New Location("35 Fudge Lane", New Vector3(1317.6, -1555.8, 50.1), LocationType.Residential, New Vector3(1321.2, -1561.3, 51), 117)
    Public Fudge7 As New Location("37 Fudge Lane", New Vector3(1359.1, -1544, 54.3), LocationType.Residential, New Vector3(1360.5, -1555.8, 56.3), 12)
    Public Fudge8 As New Location("39 Fudge Lane", New Vector3(1376.4, -1538.5, 55.8), LocationType.Residential, New Vector3(1382.1, -1544.7, 57.1), 17)
    Public Fudge9 As New Location("42 Fudge Lane", New Vector3(1419.2, -1511, 59.9), LocationType.Residential, New Vector3(1411.6, -1491, 60.7), 181)
    Public Fudge10 As New Location("40 Fudge Lane", New Vector3(1419.2, -1511, 59.9), LocationType.Residential, New Vector3(1402.3, -1490.1, 59.8), 265)
    Public Fudge11 As New Location("38 Fudge Lane", New Vector3(1398.6, -1518.3, 57.5), LocationType.Residential, New Vector3(1390.7, -1507.8, 58.4), 13)
    Public Fudge12 As New Location("36 Fudge Lane", New Vector3(1379.4, -1528.2, 56.1), LocationType.Residential, New Vector3(1379.5, -1515.4, 58), 200)
    Public Fudge13 As New Location("34 Fudge Lane", New Vector3(1336.2, -1537.9, 51.8), LocationType.Residential, New Vector3(1338.2, -1525, 54.4), 174)
    Public Fudge14 As New Location("32 Fudge Lane", New Vector3(1317.8, -1542.5, 49.5), LocationType.Residential, New Vector3(1315.9, -1526.8, 51.8), 199)
    Public Fudge15 As New Location("30 Fudge Lane", New Vector3(1240.9, -1606.1, 52.3), LocationType.Residential, New Vector3(1230.7, -1591, 53.8), 217)
    Public Fudge16 As New Location("28 Fudge Lane", New Vector3(1219.1, -1619.7, 48.6), LocationType.Residential, New Vector3(1210.9, -1606.8, 50.7), 216)
    Public Fudge17 As New Location("26 Fudge Lane", New Vector3(1194.1, -1635.3, 43), LocationType.Residential, New Vector3(1193.3, -1622.9, 45.2), 167)


    Public DPHeights As New Location("Del Perro Heights", New Vector3(-1415.9, -575.7, 30.3), LocationType.Residential, New Vector3(-1442.2, -546.1, 34.7), 226)
    Public Alta0601 As New Location("0601 Alta St", New Vector3(148.56, 63.6, 78.25), LocationType.Residential, New Vector3(124.5, 64.8, 79.74), 249)
    Public Alta0602 As New Location("0602 Alta St", New Vector3(138.26, 38.42, 71.89), LocationType.Residential, New Vector3(112.25, 56.62, 73.51), 257)
    Public Alta0706 As New Location("0703 - 0706 Alta St", New Vector3(169.5, 72.4, 82.8), LocationType.Residential, New Vector3(198.5, 64.1, 87.9), 26)
    Public Alta0707 As New Location("0707 Alta St", New Vector3(207.8, 56.2, 83.5), LocationType.Residential, New Vector3(199.7, 36.1, 83.7), 10)
    Public Alta0708 As New Location("0708 Alta St", New Vector3(152.8, 28.1, 71.4), LocationType.Residential, New Vector3(173.8, 10.5, 73.4), 157)
    Public Alta1144 As New Location("1144 Alta St", New Vector3(98.88, -85.93, 61.43), LocationType.Residential, New Vector3(64.09, -81.33, 66.7), 342)
    Public Alta1145 As New Location("1145 Alta St", New Vector3(93.45, -101.94, 58.46), LocationType.Residential, New Vector3(74.94, -107.3, 58.19), 313)
    Public AltaPl2130 As New Location("2130 Alta Place", New Vector3(182.7, -90, 67.2), LocationType.Residential, New Vector3(173.9, -89, 72.8), 274)
    Public AltaPl2154 As New Location("2154 Alta Place", New Vector3(172.4, -116.3, 61.7), LocationType.Residential, New Vector3(156.9, -117, 62.4), 281)
    Public VistaDelMarApts As New Location("Vista Del Mar Apartments", New Vector3(-1037.748, -1530.254, 4.529), LocationType.Residential, New Vector3(-1029.53, -1505.1, 4.9), 211)
    Public SRD122 As New Location("122 South Rockford Dr", New Vector3(-799.3, -991.6, 12.86), LocationType.Residential, New Vector3(-813.13, -981.2, 14.14), 157)
    Public VB2057 As New Location("2057 Vespucci Blvd", New Vector3(-666.35, -846.62, 32.5), LocationType.Residential, New Vector3(-662.52, -854.18, 24.46), 9)
    Public BDP1115 As New Location("1115 Boulevard Del Perro", New Vector3(-1609.42, -411.52, 40.67), LocationType.Residential, New Vector3(-1598.22, -421.69, 41.41), 51)
    Public EclipseTowers As New Location("Eclipse Towers", New Vector3(-774.24, 293.42, 85.15), LocationType.Residential, New Vector3(-773.88, 311.63, 85.7), 191)
    Public IntegrityTower As New Location("Integrity Tower", New Vector3(250.66, -641.62, 39.23), LocationType.Residential, New Vector3(267.18, -642.04, 42.02), 83)
    Public Alta3 As New Location("3 Alta St", New Vector3(-236.11, -988.83, 28.45), LocationType.Residential, New Vector3(-261.18, -973.53, 31.22), 215)
    Public SpanAv0605 As New Location("0605 Spanish Ave", New Vector3(0, 26.1, 70.3), LocationType.Residential, New Vector3(3.6, 36.3, 71.5), 158)
    Public SpanAv1041 As New Location("1041 Spanish Ave", New Vector3(-265.2, 116.1, 68.1), LocationType.Residential, New Vector3(-262.2, 99, 69.3), 273)
    Public SpanAv1043 As New Location("1043 Spanish Ave", New Vector3(-331.4, 118.6, 66.3), LocationType.Residential, New Vector3(-332.6, 98.9, 71.2), 273)
    Public SpanAv1150 As New Location("1150 Spanish Ave", New Vector3(254.99, -81.19, 69.45), LocationType.Residential, New Vector3(235.62, -108.02, 74.35), 7)
    Public SpanAv1161 As New Location("1161 Spanish Ave", New Vector3(323.2, -111.72, 67.83), LocationType.Residential, New Vector3(314.31, -128.14, 69.98), 324)
    Public SpanAv1160 As New Location("1160 Spanish Ave", New Vector3(356.82, -124.23, 65.71), LocationType.Residential, New Vector3(352.91, -141.05, 66.69), 334)
    Public SpanAv1562 As New Location("1562 Spanish Ave", New Vector3(-164.4, 106.9, 69.6), LocationType.Residential, New Vector3(-150.9, 123.4, 70.2), 140)
    Public SanVit1563nr1 As New Location("1563 San Vitus Ave", New Vector3(-201.3, 128.2, 68.8), LocationType.Residential, New Vector3(-198.4, 140.8, 70.2), 177)
    Public SanVit1564 As New Location("1564 San Vitus Ave", New Vector3(-818.3, 184.4, 78), LocationType.Residential, New Vector3(-201.3, 186.6, 80.3), 98)
    Public TheRoyale As New Location("The Royale", New Vector3(-202.64, 114.13, 69.09), LocationType.Residential, New Vector3(-197.46, 86.8, 69.75), 4)
    Public EclipseLodgeApts As New Location("Eclipse Lodge Apartments", New Vector3(-269.15, 26.8, 54.31), LocationType.Residential, New Vector3(-273.24, 28.41, 54.75), 233)
    Public VespCan1 As New Location("Vespucci Canals", New Vector3(-1094, -959.4, 1.9), LocationType.Residential, New Vector3(-1061.2, -944.7, 2.2), 200)
    Public VespCan2 As New Location("Vespucci Canals", New Vector3(-1058.2, -1040.2, 1.6), LocationType.Residential, New Vector3(-1066, -1051.2, 6.4), 306)
    Public Elgin1 As New Location("Elgin House", New Vector3(-7.7, 164.7, 94.9), LocationType.Residential, New Vector3(-36.1, 170.8, 95), 281)
    Public Elgin2 As New Location("Elgin House", New Vector3(-74.6, 146.5, 80.9), LocationType.Residential, New Vector3(-70.6, 141.6, 81.9), 37)
    Public RichMajApt As New Location("Richards Majestic Apartments", New Vector3(-966.5, -400.8, 37.2), LocationType.Residential, New Vector3(-937.5, -380.4, 39), 124)
    Public RLBApt As New Location("Apartment, R. Lowenstein Blvd", New Vector3(482.1, -1569.6, 28.7), LocationType.Residential, New Vector3(470.8, -1568.9, 29.3), 251) With {.PedEnd = New Vector3(446.5, -1571.4, 32.8)}
    Public LasLag0604 As New Location("0604 Las Lagunas Blvd", New Vector3(-16.2, 97.3, 78.1), LocationType.Residential, New Vector3(12, 84.2, 78.4), 65)
    Public LasLag0605 As New Location("0605 Laguna Place", New Vector3(-66.8, -23.6, 66.4), LocationType.Residential, New Vector3(-97.3, -12.9, 66.4), 337)
    Public LasLag922 As New Location("922 Las Lagunas Blvd", New Vector3(-77.2, -47.9, 61.1), LocationType.Residential, New Vector3(-95.9, -35.2, 62.2), 346) With {.PedEnd = New Vector3(-102.3, -31.6, 70.4)}
    Public LasLag924 As New Location("924 Las Lagunas Blvd", New Vector3(-66.8, -23.6, 66.4), LocationType.Residential, New Vector3(-97.3, -12.9, 66.4), 337)
    Public LasLag926 As New Location("926 Las Lagunas Blvd", New Vector3(-60.4, 1, 70.6), LocationType.Residential, New Vector3(-76.4, 2.1, 70.2), 238) With {.PedEnd = New Vector3(-103.1, 2.5, 70.2)}
    Public LasLag2143 As New Location("2143 Las Lagunas Blvd", New Vector3(-52.2, -44.7, 62.6), LocationType.Residential, New Vector3(-42, -58.5, 63.5), 72)
    Public LasLag2142 As New Location("2142 Las Lagunas Blvd", New Vector3(-46.9, -11.6, 69.1), LocationType.Residential, New Vector3(-26.4, -21, 73.2), 62)
    Public LasLag2141 As New Location("2141 Las Lagunas Blvd", New Vector3(-46.9, -11.6, 69.1), LocationType.Residential, New Vector3(-21.6, -10.3, 71.1), 157)

    Public Power1162 As New Location("1162 Power St", New Vector3(264.1, -150.7, 63.2), LocationType.Residential, New Vector3(285.2, -160.3, 64.6), 72)
    Public Power1156 As New Location("1156 Power St", New Vector3(249, -146.3, 63.1), LocationType.Residential, New Vector3(234.1, -145.3, 63.8), 253) With {.PedEnd = New Vector3(231.1, -131.3, 63.8)}
    Public Power1155 As New Location("1155 Power St", New Vector3(242.4, -165, 58.9), LocationType.Residential, New Vector3(225, -161, 59.1), 256)
    Public Castillo As New Location("Castillo Apartments, Power St", New Vector3(335, 42.7, 90.1), LocationType.Residential, New Vector3(354.3, 29.4, 91.3), 59) With {.PedEnd = New Vector3(388.7, -0.8, 91.7)}
    Public Power0702 As New Location("0702 Power St", New Vector3(311.3, 23.8, 84.6), LocationType.Residential, New Vector3(284.2, 30.9, 88.5), 164)
    Public Power0701 As New Location("0701 Power St", New Vector3(261.1, 31.1, 83.8), LocationType.Residential, New Vector3(254.8, 25.8, 84), 333)

    Public Mara1 As New Location("Apartment Building, Marathon Ave", New Vector3(-1158.8, -399.5, 35.4), LocationType.Residential, New Vector3(-1160.5, -388.5, 36.6), 183)

    Public Apt1 As New Location("Apartment Building", New Vector3(-616, -779.3, 24.9), LocationType.Residential, New Vector3(-604.3, -782.7, 25), 13) With {.PedEnd = New Vector3(-581, -778.6, 25)}
    Public Apt2 As New Location("Apartment Building", New Vector3(-551.4, -826.5, 27.8), LocationType.Residential, New Vector3(-551.9, -811.1, 30.7), 189) With {.PedEnd = New Vector3(-567.5, -780, 30.7)}
    Public Apt3 As New Location("Apartment Building", New Vector3(-831.8, -841.5, 19.2), LocationType.Residential, New Vector3(-831, -861.3, 20.7), 10)
    Public Ginger1068 As New Location("1068 Ginger Street", New Vector3(-754.1, -916.8, 18.9), LocationType.Residential, New Vector3(-766.2, -917.1, 21.3), 265)
    Public SunshineApts As New Location("Sunshine Apartments", New Vector3(-739.8, -878.6, 21.4), LocationType.Residential, New Vector3(-729.9, -879.8, 22.7), 98)
    Public SRD325 As New Location("325 South Rockford Drive", New Vector3(-827.2, -841.3, 19.4), LocationType.Residential, New Vector3(-832.1, -861.5, 20.7), 4)
    Public DreamTower As New Location("Dream Tower", New Vector3(-750, -753.7, 26.4), LocationType.Residential, New Vector3(-762.9, -754.3, 27.9), 270)
    Public Lindsay1 As New Location("Apartment, Lindsay Circus", New Vector3(-746.1, -969.1, 16.7), LocationType.Residential, New Vector3(-741.8, -981.5, 17.1), 25)
    Public Lindsay2 As New Location("Apartment, Lindsay Circus", New Vector3(-677.2, -962.2, 20.4), LocationType.Residential, New Vector3(-668, -970.9, 22.3), 46)
    Public Apt4 As New Location("Apartment, Hawick Ave", New Vector3(-359.8, 2.4, 46.2), LocationType.Residential, New Vector3(-353.6, 16.1, 47.9), 183) With {.PedEnd = New Vector3(-339.6, 21.7, 47.9)}
    Public WeazelPlazaApts As New Location("Weazel Plaza Apartments", New Vector3(-933.7, -458, 36.4), LocationType.Residential, New Vector3(-916.1, -452.2, 39.6), 98)
    Public AbeMilt1 As New Location("Apartment, Abe Milton Pkwy", New Vector3(-427.6, -192.4, 35.8), LocationType.Residential, New Vector3(-417, -187.4, 37.5), 121)
    Public AbeMilt2 As New Location("Apartment, Abe Milton Pkwy", New Vector3(-460, -138.2, 37.6), LocationType.Residential, New Vector3(-449.1, -132.7, 39.1), 124)

    Public Coug0069 As New Location("0069 Cougar Ave", New Vector3(-1542.8, -324.6, 46.4), LocationType.Residential, New Vector3(-1534, -326.7, 47.9), 48)
    Public Coug1 As New Location("Apartment, Cougar Ave", New Vector3(-1524.5, -281.8, 48.6), LocationType.Residential, New Vector3(-1532.6, -275.9, 49.7), 234)
    Public Coug2 As New Location("Residence, Cougar Ave", New Vector3(-1609.7, -381.3, 42.5), LocationType.Residential, New Vector3(-1622.4, -380.1, 43.7), 235)
    Public Coug3 As New Location("Residence, Cougar Ave", New Vector3(-1630.6, -421.2, 39), LocationType.Residential, New Vector3(-1642.6, -412, 42.1), 230)
    Public Coug4 As New Location("Residence, Cougar Ave", New Vector3(-1669.7, -452.4, 38.5), LocationType.Residential, New Vector3(-1667.2, -441.5, 40.4), 230)

    Public BayCit1 As New Location("Apartment, Bay City Ave", New Vector3(-1773.1, -438.9, 41.1), LocationType.Residential, New Vector3(-11778, -427.6, 41.4), 192)
    Public PalominoAv1 As New Location("Apartment, Palomino Ave", New Vector3(-691.4, -1054.8, 14.5), LocationType.Residential, New Vector3(-696.5, -1038.1, 16.1), 176)


    Public VCan1 As New Location("Vespucci Canals", New Vector3(-946.9, -893.7, 2), LocationType.Residential, New Vector3(-964.7, -894.3, 2.2), 298)
    Public VCan5 As New Location("Vespucci Canals", New Vector3(-928.4, -930.5, 2), LocationType.Residential, New Vector3(-933.8, -939.3, 2.1), 299)
    Public VCanLast As New Location("Vespucci Canals", New Vector3(-854.1, -1096.4, 2), LocationType.Residential, New Vector3(-868.2, -1103.9, 6.4), 281)

    Public Goma1 As New Location("Apartment, Goma Street", New Vector3(-1133.5, -1477.1, 3.8), LocationType.Residential, New Vector3(-1142.2, -1462.8, 4.6), 200)
    Public Goma2 As New Location("Apartment, Goma Street", New Vector3(-1121, -1480.3, 3.9), LocationType.Residential, New Vector3(-1116.1, -1485.6, 4.7), 35)
    Public Goma3 As New Location("Apartment, Goma Street", New Vector3(-1134.3, -1478, 4), LocationType.Residential, New Vector3(-1145.7, -1465.9, 7.7), 299)
    Public Magellan1 As New Location("Apartment, Magellan Avenue", New Vector3(-1111.4, -1538, 3.6), LocationType.Residential, New Vector3(-1108.8, -1527.4, 6.8), 94)
    Public Magellan2 As New Location("Apartment, Magellan Avenue", New Vector3(-1045.3, -1643.9, 3.8), LocationType.Residential, New Vector3(-1059, -1657.2, 4.7), 331)
    Public Magellan3 As New Location("Apartment, Magellan Avenue", New Vector3(-1070.8, -1612.8, 3.7), LocationType.Residential, New Vector3(-1078.6, -1616.5, 4.4), 273)
    Public Magellan4 As New Location("Apartment, Magellan Avenue", New Vector3(-1081.2, -1597.7, 3.7), LocationType.Residential, New Vector3(-1093.8, -1608.1, 8.5), 302)

    Public Barbareno1 As New Location("1 Barbareno Rd", New Vector3(-3172.41, 1289.02, 13.41), LocationType.Residential, New Vector3(-3190.34, 1297.37, 19.07), 247)
    Public Barbareno2 As New Location("2 Barbareno Rd", New Vector3(-3176.96, 1271.39, 11.98), LocationType.Residential, New Vector3(-3186.28, 1273.3, 12.93), 250)
    Public Barbareno3 As New Location("3 Barbareno Rd", New Vector3(-3184.02, 1223.59, 9.64), LocationType.Residential, New Vector3(-3194.51, 1230.73, 10.05), 292)
    Public Barbareno4 As New Location("4 Barbareno Rd", New Vector3(-3186.07, 1200.83, 9.16), LocationType.Residential, New Vector3(-3205.32, 1198.95, 9.54), 99)
    Public Barbareno5 As New Location("5 Barbareno Rd", New Vector3(-3188.7, 1176.57, 9.05), LocationType.Residential, New Vector3(-3205.7, 1186.27, 9.66), 352)
    Public Barbareno6 As New Location("6 Barbareno Rd", New Vector3(-3193.35, 1156.28, 9.18), LocationType.Residential, New Vector3(-3198.94, 1164.24, 9.65), 231)
    Public Barbareno7 As New Location("7 Barbareno Rd", New Vector3(-3193.35, 1156.28, 9.18), LocationType.Residential, New Vector3(-3204.1, 1151.96, 9.65), 293)
    Public Barbareno8 As New Location("8 Barbareno Rd", New Vector3(-3201.44, 1136.83, 9.46), LocationType.Residential, New Vector3(-3209.43, 1146.02, 9.9), 252)
    Public Barbareno9 As New Location("9 Barbareno Rd", New Vector3(-3215.84, 1104.29, 10.02), LocationType.Residential, New Vector3(-3224.81, 1113.62, 10.58), 245)
    Public Barbareno10 As New Location("10 Barbareno Rd", New Vector3(-3223.05, 1086.69, 10.33), LocationType.Residential, New Vector3(-3231.81, 1079.29, 10.84), 270)
    Public Barbareno11 As New Location("11 Barbareno Rd", New Vector3(-3227.93, 1065.93, 10.71), LocationType.Residential, New Vector3(-3232.38, 1067.4, 11.02), 251)
    Public Barbareno12 As New Location("12 Barbareno Rd", New Vector3(-3232.1, 1036.52, 11.25), LocationType.Residential, New Vector3(-3253.59, 1042.83, 11.76), 264)
    Public Barbareno14 As New Location("14 Barbareno Rd", New Vector3(-3230.46, 951.99, 12.65), LocationType.Residential, New Vector3(-3237.3, 952.97, 13.14), 275)
    Public Barbareno15 As New Location("15 Barbareno Rd", New Vector3(-3226.83, 938.8, 12.94), LocationType.Residential, New Vector3(-3232.34, 934.62, 13.8), 297)
    Public Barbareno16 As New Location("16 Barbareno Rd", New Vector3(-3221.6, 928.33, 13.18), LocationType.Residential, New Vector3(-3228.25, 927.8, 13.97), 293)
    Public Barbareno17 As New Location("17 Barbareno Rd", New Vector3(-3211.66, 916.67, 13.46), LocationType.Residential, New Vector3(-3218.1, 912.81, 13.99), 313)

    Public Procopio4401 As New Location("4401 Procopio Dr", New Vector3(-310.5, 6342.5, 30.3), LocationType.Residential, New Vector3(-302.6, 6327.3, 32.9), 32)
    Public Procopio4484 As New Location("4584 Procopio Dr", New Vector3(-108.2, 6537.5, 29.4), LocationType.Residential, New Vector3(-106.3, 6529.9, 29.9), 30)

    Public Grapeseed1893 As New Location("1893 Grapeseed Avenue", New Vector3(1670, 4769.7, 41.4), LocationType.Residential, New Vector3(1663.1, 4776.2, 42), 271)
    Public Grapeseed1891 As New Location("1891 Grapeseed Avenue", New Vector3(1674.3, 4753.4, 41.4), LocationType.Residential, New Vector3(1664.2, 4739.8, 42), 288)
    Public Grapeseed1889 As New Location("1889 Grapeseed Avenue", New Vector3(1690.8, 4680, 42.6), LocationType.Residential, New Vector3(1683.2, 4689.6, 43.1), 267)
    Public Grapeseed1887 As New Location("1887 Grapeseed Avenue", New Vector3(1689, 4652, 43), LocationType.Residential, New Vector3(1673.8, 4658.4, 43.4), 258)
    Public Grapeseed1886 As New Location("1886 Grapeseed Avenue", New Vector3(1709.4, 4634.6, 42.8), LocationType.Residential, New Vector3(1725.1, 4642.4, 43.9), 119)
    Public Grapeseed1888 As New Location("1888 Grapeseed Avenue", New Vector3(1702.6, 4668.8, 42.8), LocationType.Residential, New Vector3(1718.7, 4677.4, 43.7), 85)

    'Public x As New Location("xxx", New Vector3(), LocationType.x, New Vector3(), 0)

    Public Sub InitPlaceLists()
        For Each l As Location In ListOfPlaces
            Select Case l.Type
                Case LocationType.AirportArrive
                    lAirportA.Add(l)
                Case LocationType.AirportDepart
                    lAirportD.Add(l)
                Case LocationType.HotelLS
                    lHotelLS.Add(l)
                Case LocationType.Residential
                    lResidential.Add(l)
                Case LocationType.Entertainment
                    lEntertainment.Add(l)
                Case LocationType.Bar
                    lBar.Add(l)
                Case LocationType.FastFood
                    lFastFood.Add(l)
                Case LocationType.Restaurant
                    lRestaurant.Add(l)
                Case LocationType.MotelLS
                    lMotelLS.Add(l)
                Case LocationType.Religious
                    lReligious.Add(l)
                Case LocationType.Shopping
                    lShopping.Add(l)
                Case LocationType.Sport
                    lSport.Add(l)
                Case LocationType.Office
                    lOffice.Add(l)
                Case LocationType.Theater
                    lTheater.Add(l)
                Case LocationType.School
                    lSchool.Add(l)
                Case LocationType.Factory
                    lFactory.Add(l)
            End Select
        Next
    End Sub
End Module
