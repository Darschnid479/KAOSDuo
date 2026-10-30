Option Strict On
Option Explicit On

Imports System
Imports System.Collections.Generic
Imports System.Diagnostics
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.IO
Imports System.Linq
Imports System.Reflection
Imports System.Text.Json
Imports System.Windows.Forms

Public Partial Class GameForm
    Inherits Form

    Private ReadOnly _frame As New System.Windows.Forms.Timer() With {.Interval = 16}
    Private ReadOnly _clock As New Stopwatch()
    Private ReadOnly _rng As New Random()
    Private ReadOnly _keys As New HashSet(Of Keys)()
    Private ReadOnly _ui As New List(Of HitButton)()
    Private ReadOnly _sparks As New List(Of Spark)()
    Private ReadOnly _music As New MusicPlayer()
    Private _blue As Bitmap
    Private _metal As Bitmap
    Private _s As New GameSave()
    Private _choiceHero As HeroKind = HeroKind.Blue
    Private _choiceDifficulty As GameDifficulty = GameDifficulty.Medium
    Private _mouseWorld As New PointF(900, 365)
    Private _mouseHeld As Boolean = False
    Private _screenScale As Single = 1
    Private _screenOffsetX As Single = 0
    Private _screenOffsetY As Single = 0
    Private _elapsed As Single = 0
    Private _waveDelay As Single = -1
    Private _saveTimer As Single = 0
    Private _cameraShake As Single = 0
    Private _cameraKick As Single = 0
    Private _notification As String = ""
    Private _noticeTime As Single = 0
    Private _returnToShopOffer As Boolean = False
    Private _screenshotNotice As String = ""
    Private _screenshotTimer As Single = 0
    Private _musicCheckCharacter As HeroKind = HeroKind.Blue
    Private _fullscreen As Boolean = False
    Private _loggedFirstFrame As Boolean = False
    Private _oldBounds As Rectangle
    Private _oldBorder As FormBorderStyle
    Private _oldState As FormWindowState
    Private ReadOnly _savePath As String = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "KAOSDUO", "save_v6.json")

    Public Sub New()
        Me.Text = "KAOSDUO: REFORGED v6.0.1 | Visual Basic .NET"
        Me.BackColor = Color.FromArgb(10, 10, 26)
        Me.ClientSize = New Size(1280, 720)
        Me.MinimumSize = New Size(990, 570)
        Me.StartPosition = FormStartPosition.CenterScreen
        Me.DoubleBuffered = True
        Me.KeyPreview = True
        ' An icon lookup can fail on restricted Windows machines; it must not block the game.
        Try
            Dim appIcon = Icon.ExtractAssociatedIcon(Application.ExecutablePath)
            If appIcon IsNot Nothing Then Me.Icon = appIcon
        Catch ex As Exception
            Program.LogIssue("Optional icon could not be loaded", ex)
        End Try
        Me.SetStyle(ControlStyles.AllPaintingInWmPaint Or ControlStyles.UserPaint Or ControlStyles.OptimizedDoubleBuffer Or ControlStyles.ResizeRedraw, True)
        _blue = LoadSprite("blue.png")
        _metal = LoadSprite("metal.png")
        AddHandler _frame.Tick, AddressOf OnTick
        _clock.Start()
        _frame.Start()
    End Sub

    Private Function LoadSprite(name As String) As Bitmap
        ' Find the actual manifest identifier instead of assuming a resource namespace.
        Dim resourceName = Program.ResolveResourceName(name)
        Using source = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName)
            If source Is Nothing Then Throw New IOException("Missing character sprite: " & resourceName)
            Using temporary As New Bitmap(source)
                Return CType(temporary.Clone(), Bitmap)
            End Using
        End Using
    End Function

    ' Automated Windows startup/render tests. This is also exercised after publishing the single-file EXE.
    ' No game simulation is replaced with HTML or C#; all rendering remains native VB.NET.
    Public Sub RunSmokeTests(outputFolder As String)
        _frame.Stop()
        Directory.CreateDirectory(outputFolder)
        Me.CreateControl()
        Me.Show()
        Application.DoEvents()
        RenderSmokeFrame(outputFolder, "01-main-menu.png")
        BeginNewGame()
        RenderSmokeFrame(outputFolder, "02-combat.png")
        OfferShop()
        RenderSmokeFrame(outputFolder, "03-shop-offer.png")
        StartTravel()
        RenderSmokeFrame(outputFolder, "04-travel.png")
        _s.Scene = SceneKind.PortalBoss
        _s.Portal.BossMaxHp = 300
        _s.Portal.BossHp = 300
        _s.Portal.BossShield = 3
        _s.Portal.BossPlayerX = 260
        _s.Portal.BossPlayerY = 410
        RenderSmokeFrame(outputFolder, "05-portal-boss.png")
        _s.Scene = SceneKind.Highway
        _s.Portal.Type = "HIGHWAY"
        RenderSmokeFrame(outputFolder, "06-highway.png")
        _s.Scene = SceneKind.Portal
        _s.Portal.Type = "GEOMETRY"
        RenderSmokeFrame(outputFolder, "07-geometry.png")
        If Not SaveGame(False) Then Throw New IOException("Native save test failed")
        LoadGame()
        If _s.Portal Is Nothing Then Throw New InvalidDataException("Native load failed")
        File.WriteAllText(Path.Combine(outputFolder, "SMOKE_TEST_PASSED.txt"),
                          "KAOSDUO REFORGED v6.0.1: native WinForms window, embedded sprites, GDI+ scenes, save/load and startup succeeded." & Environment.NewLine &
                          "Windows: " & Environment.OSVersion.ToString() & Environment.NewLine &
                          ".NET: " & Environment.Version.ToString() & Environment.NewLine)
        Me.Hide()
    End Sub

    Private Sub RenderSmokeFrame(folder As String, name As String)
        Using frame As New Bitmap(1280, 720)
            Using g As Graphics = Graphics.FromImage(frame)
                g.Clear(Color.Black)
                g.SmoothingMode = SmoothingMode.AntiAlias
                DrawScene(g)
            End Using
            frame.Save(Path.Combine(folder, name), Imaging.ImageFormat.Png)
        End Using
    End Sub

    Private Function IsHeld(ParamArray keys As Keys()) As Boolean
        For Each k In keys
            If _keys.Contains(k) Then Return True
        Next
        Return False
    End Function

    Private Sub OnTick(sender As Object, args As EventArgs)
        Dim dt = CSng(Math.Min(0.034, _clock.Elapsed.TotalSeconds))
        _clock.Restart()
        _elapsed += dt
        If _noticeTime > 0 Then _noticeTime = Math.Max(0.0F, _noticeTime - dt)
        If _screenshotTimer > 0 Then _screenshotTimer = Math.Max(0.0F, _screenshotTimer - dt)
        _cameraShake = Math.Max(0.0F, _cameraShake - dt * 24)
        _cameraKick = Math.Max(0.0F, _cameraKick - dt * 10)
        If _s.Scene <> SceneKind.Paused AndAlso _s.Scene <> SceneKind.MainMenu AndAlso _s.Scene <> SceneKind.Shop AndAlso _s.Scene <> SceneKind.ShopOffer AndAlso _s.Scene <> SceneKind.Upgrade AndAlso _s.Scene <> SceneKind.PortalFailed AndAlso _s.Scene <> SceneKind.GameOver AndAlso _s.Scene <> SceneKind.Victory Then
            _s.SecondsPlayed += dt
            UpdateEffects(dt)
            Select Case _s.Scene
                Case SceneKind.Combat : UpdateCombat(dt)
                Case SceneKind.Portal : UpdatePortal(dt)
                Case SceneKind.PortalBoss : UpdatePortalBoss(dt)
                Case SceneKind.Highway : UpdateHighway(dt)
            End Select
            _saveTimer += dt
            If _saveTimer >= 22.0F Then
                _saveTimer = 0
                SaveGame(False)
            End If
        Else
            UpdateEffects(Math.Min(dt, 0.008F))
        End If
        Invalidate()
    End Sub

    Private Sub Notify(text As String, Optional seconds As Single = 2.7F)
        _notification = text
        _noticeTime = seconds
    End Sub

    Private Sub BeginNewGame()
        _s = New GameSave() With {.Character = _choiceHero, .Difficulty = _choiceDifficulty}
        _s.Hero.Hp = GameRules.StartingHealth(_s.Difficulty)
        _s.Hero.MaxHp = _s.Hero.Hp
        _s.Hero.X = 210 : _s.Hero.Y = GameRules.FloorY
        _s.MusicEnabled = Not _music.Muted
        _saveTimer = 0
        _sparks.Clear()
        _waveDelay = -1
        BeginWave(1)
        _music.Play(_s.Character)
        Notify("EN HELT NY VIRKELIGHET!  OVERLEV 10 BOLGER!", 3.5F)
    End Sub

    Private Sub BeginWave(waveIndex As Integer)
        _s.Wave = waveIndex
        _s.Scene = SceneKind.Combat
        _s.Enemies.Clear()
        _s.Bullets.Clear()
        _s.SpawnRemaining = 8 + waveIndex * 3 + CInt(_s.Difficulty) * 2
        _s.SpawnCooldown = 0.85F
        _s.WaveBossSpawned = False
        _waveDelay = -1
        _s.Hero.X = 180
        _s.Hero.Y = GameRules.FloorY
        _s.Hero.Vy = 0
        _s.Hero.JumpCount = 0
        _s.Hero.Invincible = Math.Max(_s.Hero.Invincible, 1.0F)
        _s.Hero.Shield = Math.Max(0, _s.Hero.Shield)
        Notify("BOLGE " & waveIndex.ToString() & " / " & GameRules.TotalWaves.ToString(), 2.3F)
        SaveGame(False)
    End Sub

    Private Sub OfferShop()
        If _s.Wave >= GameRules.TotalWaves Then
            _s.Scene = SceneKind.Victory
            _music.Play(_s.Character)
            SaveGame(False)
            Return
        End If
        _s.Scene = SceneKind.ShopOffer
        _s.Bullets.Clear()
        SaveGame(False)
        Notify("BOLGE FULLFORT!  DU HAR " & _s.Coins.ToString() & " MYNTER", 2.8F)
    End Sub

    Private Sub OpenShop()
        _s.PreviousScene = _s.Scene
        _s.Scene = SceneKind.Shop
        _returnToShopOffer = (_s.PreviousScene = SceneKind.ShopOffer)
    End Sub

    Private Sub ExitShop()
        If _s.PreviousScene = SceneKind.ShopOffer OrElse _returnToShopOffer Then
            _returnToShopOffer = False
            StartTravel()
        Else
            _s.Scene = _s.PreviousScene
        End If
        SaveGame(False)
    End Sub

    Private Sub Purchase(id As Integer)
        If id < 0 OrElse id >= GameRules.Weapons.Length Then Return
        If _s.OwnedWeapons.Contains(id) Then
            _s.EquippedWeapon = id
            Notify("UTSTYRT: " & GameRules.Weapons(id).Name, 1.6F)
            Return
        End If
        If Not GameRules.CanAffordWeapon(_s.Coins, _s.OwnedWeapons, id) Then
            Notify("IKKE NOK MYNTER!", 2.0F)
            Return
        End If
        _s.Coins -= GameRules.WeaponCost(id)
        _s.OwnedWeapons.Add(id)
        _s.Magazine(id) = GameRules.Weapons(id).Magazine
        _s.EquippedWeapon = id
        _s.Hero.ReloadRemaining = 0
        Notify("KJOPT: " & GameRules.Weapons(id).Name & "!", 2.2F)
        Burst(640, 340, Color.FromArgb(255, 220, 106), 36, 310)
        SaveGame(False)
    End Sub

    Private Sub Equip(id As Integer)
        If id >= 0 AndAlso id < GameRules.Weapons.Length AndAlso _s.OwnedWeapons.Contains(id) Then
            _s.EquippedWeapon = id
            _s.Hero.ReloadRemaining = 0
            Notify(GameRules.Weapons(id).Name, 1.2F)
        End If
    End Sub

    Private Sub CycleWeapon(delta As Integer)
        If _s.OwnedWeapons.Count = 0 Then Return
        Dim sorted = _s.OwnedWeapons.OrderBy(Function(x) x).ToList()
        Dim ix = sorted.IndexOf(_s.EquippedWeapon)
        Dim nextIx = (ix + delta + sorted.Count) Mod sorted.Count
        Equip(sorted(nextIx))
    End Sub

    Private Function SaveGame(Optional showMessage As Boolean = True) As Boolean
        Try
            Dim dir = Path.GetDirectoryName(_savePath)
            If Not String.IsNullOrEmpty(dir) Then Directory.CreateDirectory(dir)
            Dim json = JsonSerializer.Serialize(_s, New JsonSerializerOptions With {.WriteIndented = True})
            Dim tmp = _savePath & ".tmp"
            File.WriteAllText(tmp, json)
            File.Move(tmp, _savePath, True)
            If showMessage Then Notify("SPILL LAGRET!  F5 = LAGRE, F9 = LAST INN", 2.7F)
            Return True
        Catch ex As Exception
            If showMessage Then Notify("KUNNE IKKE LAGRE: " & ex.Message, 3.8F)
            Return False
        End Try
    End Function

    Private Sub LoadGame()
        If Not File.Exists(_savePath) Then
            Notify("INGEN LAGRING FUNNET ENNA", 2.8F)
            Return
        End If
        Try
            Dim data = JsonSerializer.Deserialize(Of GameSave)(File.ReadAllText(_savePath))
            If data Is Nothing OrElse data.SaveVersion <> 6 Then Throw New InvalidDataException("Ukjent lagringsformat")
            If data.Hero Is Nothing OrElse data.Portal Is Nothing OrElse data.Enemies Is Nothing OrElse data.OwnedWeapons Is Nothing OrElse data.OwnedWeapons.Count = 0 Then Throw New InvalidDataException("Mangler spilldata")
            If data.Wave < 0 OrElse data.Wave > GameRules.TotalWaves Then Throw New InvalidDataException("Ugyldig bolge")
            _s = data
            If _s.Magazine Is Nothing Then _s.Magazine = New Dictionary(Of Integer, Integer)()
            If _s.Bullets Is Nothing Then _s.Bullets = New List(Of ProjectileState)()
            If _s.Portal.Obstacles Is Nothing Then _s.Portal.Obstacles = New List(Of PortalObstacle)()
            If _s.Portal.RoadObstacles Is Nothing Then _s.Portal.RoadObstacles = New List(Of RoadObstacle)()
            If _s.Scene = SceneKind.Paused Then _s.Scene = _s.PreviousScene
            If _s.Scene = SceneKind.MainMenu Then _s.Scene = SceneKind.Combat
            _choiceHero = _s.Character
            _choiceDifficulty = _s.Difficulty
            _music.Muted = Not _s.MusicEnabled
            _music.Play(_s.Character)
            _sparks.Clear()
            _returnToShopOffer = (_s.Scene = SceneKind.Shop AndAlso _s.PreviousScene = SceneKind.ShopOffer)
            _waveDelay = -1
            Notify("LAGRING LASTET INN: BOLGE " & _s.Wave.ToString(), 2.9F)
        Catch ex As Exception
            Notify("LASTING FEILET: " & ex.Message, 3.5F)
        End Try
    End Sub

    Private Sub ToggleMusic()
        _music.Muted = Not _music.Muted
        _s.MusicEnabled = Not _music.Muted
        If Not _music.Muted Then _music.Play(If(_s.Scene = SceneKind.MainMenu, _choiceHero, _s.Character))
        Notify(If(_music.Muted, "MUSIKK AV", If(_music.Available, "MUSIKK PA", "LYD KUNNE IKKE STARTES")), 2.2F)
    End Sub

    Private Sub TogglePause()
        If _s.Scene = SceneKind.Paused Then
            _s.Scene = _s.PreviousScene
        ElseIf _s.Scene = SceneKind.Combat OrElse _s.Scene = SceneKind.Portal OrElse _s.Scene = SceneKind.Highway OrElse _s.Scene = SceneKind.PortalBoss Then
            _s.PreviousScene = _s.Scene
            _s.Scene = SceneKind.Paused
        End If
    End Sub

    Private Sub ToggleFullscreen()
        If Not _fullscreen Then
            _oldBounds = Me.Bounds
            _oldBorder = Me.FormBorderStyle
            _oldState = Me.WindowState
            Me.WindowState = FormWindowState.Normal
            Me.FormBorderStyle = FormBorderStyle.None
            Me.Bounds = Screen.FromControl(Me).Bounds
        Else
            Me.FormBorderStyle = _oldBorder
            Me.WindowState = _oldState
            Me.Bounds = _oldBounds
        End If
        _fullscreen = Not _fullscreen
    End Sub

    Private Sub CaptureScreenshot()
        Try
            Dim folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "KAOSDUO")
            Directory.CreateDirectory(folder)
            Dim filename = Path.Combine(folder, "KAOSDUO_" & DateTime.Now.ToString("yyyyMMdd_HHmmss") & ".png")
            Using image As New Bitmap(1280, 720)
                Using g = Graphics.FromImage(image)
                    DrawScene(g)
                End Using
                image.Save(filename, Imaging.ImageFormat.Png)
            End Using
            Notify("SKJERMBILDE LAGRET I BILDER / KAOSDUO!", 3)
        Catch ex As Exception
            Notify("SKJERMBILDE FEILET: " & ex.Message, 3)
        End Try
    End Sub

    Private Sub Dispatch(actionName As String)
        If actionName.StartsWith("character:", StringComparison.Ordinal) Then
            _choiceHero = If(actionName.EndsWith("metal", StringComparison.Ordinal), HeroKind.Metal, HeroKind.Blue)
            _music.Play(_choiceHero)
            Return
        End If
        If actionName.StartsWith("difficulty:", StringComparison.Ordinal) Then
            _choiceDifficulty = CType(Integer.Parse(actionName.Substring(11)), GameDifficulty)
            Return
        End If
        If actionName.StartsWith("buy:", StringComparison.Ordinal) Then
            Purchase(Integer.Parse(actionName.Substring(4)))
            Return
        End If
        Select Case actionName
            Case "start" : BeginNewGame()
            Case "load" : LoadGame()
            Case "save" : SaveGame(True)
            Case "shop" : OpenShop()
            Case "shop_no" : StartTravel()
            Case "shop_done" : ExitShop()
            Case "retry" : RetryPortal()
            Case "pause" : TogglePause()
            Case "music" : ToggleMusic()
            Case "preview" : _music.StopMusic() : _music.Play(_choiceHero) : Notify("SPILLER " & If(_choiceHero = HeroKind.Metal, "METAL", "BLA") & " SOUNDTRACK", 2)
            Case "restart" : BeginNewGame()
            Case "menu" : _s.Scene = SceneKind.MainMenu : _choiceHero = _s.Character : _choiceDifficulty = _s.Difficulty
            Case "up_damage" : ApplyUpgrade("damage")
            Case "up_speed" : ApplyUpgrade("speed")
            Case "up_armor" : ApplyUpgrade("armor")
            Case "up_rate" : ApplyUpgrade("rate")
            Case "up_coins" : ApplyUpgrade("coins")
        End Select
    End Sub

    Private Sub ApplyUpgrade(which As String)
        If _s.Scene <> SceneKind.Upgrade Then Return
        Select Case which
            Case "damage" : _s.DamageLevel += 1
            Case "speed" : _s.SpeedLevel += 1
            Case "armor" : _s.ArmorLevel += 1 : _s.Hero.MaxHp += 17 : _s.Hero.Hp = Math.Min(_s.Hero.MaxHp, _s.Hero.Hp + 30)
            Case "rate" : _s.FireRateLevel += 1
            Case "coins" : _s.MoneyLevel += 1
        End Select
        _s.UpgradesPending = Math.Max(0, _s.UpgradesPending - 1)
        If _s.UpgradesPending <= 0 Then _s.Scene = _s.PreviousScene
        Burst(_s.Hero.X, _s.Hero.Y - 95, Color.FromArgb(240, 205, 96), 30, 250)
        SaveGame(False)
    End Sub

    Protected Overrides Sub OnKeyDown(e As KeyEventArgs)
        MyBase.OnKeyDown(e)
        Dim newlyPressed = _keys.Add(e.KeyCode)
        If Not newlyPressed Then Return
        Select Case e.KeyCode
            Case Keys.F11 : ToggleFullscreen() : Return
            Case Keys.M : ToggleMusic() : Return
            Case Keys.F5 : If _s.Scene <> SceneKind.MainMenu Then SaveGame(True) : Return
            Case Keys.F9 : LoadGame() : Return
            Case Keys.F12 : CaptureScreenshot() : Return
            Case Keys.Escape, Keys.P
                If _s.Scene = SceneKind.Shop Then
                    ExitShop()
                Else
                    TogglePause()
                End If
                Return
        End Select
        If _s.Scene = SceneKind.MainMenu Then
            If e.KeyCode = Keys.Enter Then BeginNewGame()
            If e.KeyCode = Keys.D1 Then Dispatch("character:blue")
            If e.KeyCode = Keys.D2 Then Dispatch("character:metal")
            Return
        End If
        If _s.Scene = SceneKind.ShopOffer Then
            If e.KeyCode = Keys.Y OrElse e.KeyCode = Keys.B Then OpenShop()
            If e.KeyCode = Keys.N OrElse e.KeyCode = Keys.Enter Then StartTravel()
            Return
        End If
        If _s.Scene = SceneKind.PortalFailed Then
            If e.KeyCode = Keys.Enter OrElse e.KeyCode = Keys.Space Then RetryPortal()
            Return
        End If
        If _s.Scene = SceneKind.Shop Then
            If e.KeyCode >= Keys.D1 AndAlso e.KeyCode <= Keys.D8 Then Purchase(CInt(e.KeyCode) - CInt(Keys.D1))
            If e.KeyCode = Keys.Enter OrElse e.KeyCode = Keys.B Then ExitShop()
            Return
        End If
        If _s.Scene = SceneKind.Upgrade Then
            Select Case e.KeyCode
                Case Keys.D1 : ApplyUpgrade("damage")
                Case Keys.D2 : ApplyUpgrade("speed")
                Case Keys.D3 : ApplyUpgrade("armor")
                Case Keys.D4 : ApplyUpgrade("rate")
                Case Keys.D5 : ApplyUpgrade("coins")
            End Select
            Return
        End If
        If _s.Scene = SceneKind.Paused Then Return
        If _s.Scene = SceneKind.Combat OrElse _s.Scene = SceneKind.PortalBoss Then
            If e.KeyCode >= Keys.D1 AndAlso e.KeyCode <= Keys.D8 Then Equip(CInt(e.KeyCode) - CInt(Keys.D1))
            If e.KeyCode = Keys.B AndAlso _s.Scene = SceneKind.Combat Then OpenShop()
            If e.KeyCode = Keys.Q Then TriggerFever()
            If e.KeyCode = Keys.G Then TriggerSuper()
            If e.KeyCode = Keys.ShiftKey OrElse e.KeyCode = Keys.LShiftKey OrElse e.KeyCode = Keys.RShiftKey Then BeginDash()
            If e.KeyCode = Keys.R Then Reload()
            If e.KeyCode = Keys.Tab Then CycleWeapon(1)
            If _s.Scene = SceneKind.Combat AndAlso (e.KeyCode = Keys.Space OrElse e.KeyCode = Keys.W OrElse e.KeyCode = Keys.Up) Then BeginJump()
        ElseIf _s.Scene = SceneKind.Portal Then
            If e.KeyCode = Keys.W OrElse e.KeyCode = Keys.Up OrElse e.KeyCode = Keys.Space Then TapPortal()
        ElseIf _s.Scene = SceneKind.Highway Then
            If e.KeyCode = Keys.A OrElse e.KeyCode = Keys.Left Then ChangeLane(-1)
            If e.KeyCode = Keys.D OrElse e.KeyCode = Keys.Right Then ChangeLane(1)
        End If
    End Sub

    Protected Overrides Sub OnKeyUp(e As KeyEventArgs)
        MyBase.OnKeyUp(e)
        _keys.Remove(e.KeyCode)
    End Sub

    Protected Overrides Sub OnDeactivate(e As EventArgs)
        MyBase.OnDeactivate(e)
        _keys.Clear()
        _mouseHeld = False
        If _s.Scene = SceneKind.Combat OrElse _s.Scene = SceneKind.Portal OrElse _s.Scene = SceneKind.PortalBoss OrElse _s.Scene = SceneKind.Highway Then TogglePause()
    End Sub

    Protected Overrides Sub OnMouseMove(e As MouseEventArgs)
        MyBase.OnMouseMove(e)
        _mouseWorld = New PointF((e.X - _screenOffsetX) / Math.Max(0.0001F, _screenScale), (e.Y - _screenOffsetY) / Math.Max(0.0001F, _screenScale))
    End Sub

    Protected Overrides Sub OnMouseDown(e As MouseEventArgs)
        MyBase.OnMouseDown(e)
        _mouseWorld = New PointF((e.X - _screenOffsetX) / Math.Max(0.0001F, _screenScale), (e.Y - _screenOffsetY) / Math.Max(0.0001F, _screenScale))
        If e.Button <> MouseButtons.Left Then Return
        For i = _ui.Count - 1 To 0 Step -1
            If _ui(i).Bounds.Contains(_mouseWorld) Then
                Dispatch(_ui(i).Action)
                Return
            End If
        Next
        _mouseHeld = True
        If _s.Scene = SceneKind.Portal Then TapPortal()
    End Sub

    Protected Overrides Sub OnMouseUp(e As MouseEventArgs)
        MyBase.OnMouseUp(e)
        _mouseHeld = False
    End Sub

    Protected Overrides Sub OnMouseWheel(e As MouseEventArgs)
        MyBase.OnMouseWheel(e)
        If _s.Scene = SceneKind.Combat OrElse _s.Scene = SceneKind.PortalBoss Then CycleWeapon(If(e.Delta < 0, 1, -1))
    End Sub

    Protected Overrides Sub OnPaint(e As PaintEventArgs)
        MyBase.OnPaint(e)
        Dim sx = Me.ClientSize.Width / GameRules.WorldWidth
        Dim sy = Me.ClientSize.Height / GameRules.WorldHeight
        _screenScale = Math.Max(0.0001F, Math.Min(sx, sy))
        _screenOffsetX = (Me.ClientSize.Width - GameRules.WorldWidth * _screenScale) * 0.5F
        _screenOffsetY = (Me.ClientSize.Height - GameRules.WorldHeight * _screenScale) * 0.5F
        e.Graphics.Clear(Color.FromArgb(5, 7, 21))
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias
        e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit
        e.Graphics.TranslateTransform(_screenOffsetX, _screenOffsetY)
        e.Graphics.ScaleTransform(_screenScale, _screenScale)
        DrawScene(e.Graphics)
        If Not _loggedFirstFrame Then
            _loggedFirstFrame = True
            Program.LogStatus("First native WinForms frame rendered")
        End If
    End Sub

    Protected Overrides Sub Dispose(disposing As Boolean)
        If disposing Then
            _frame.Stop()
            _frame.Dispose()
            _music.Dispose()
            If _blue IsNot Nothing Then _blue.Dispose()
            If _metal IsNot Nothing Then _metal.Dispose()
        End If
        MyBase.Dispose(disposing)
    End Sub
End Class
