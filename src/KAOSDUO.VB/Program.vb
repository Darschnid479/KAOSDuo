Option Strict On
Option Explicit On

Imports System
Imports System.Drawing
Imports System.IO
Imports System.Linq
Imports System.Reflection
Imports System.Text
Imports System.Text.Json
Imports System.Threading
Imports System.Windows.Forms

Public Module Program
    Private ReadOnly LogFolder As String = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "KAOSDUO", "logs")

    Public ReadOnly Property StartupLogPath As String
        Get
            Return Path.Combine(LogFolder, "startup.log")
        End Get
    End Property

    Public Sub LogIssue(context As String, ex As Exception)
        Try
            Directory.CreateDirectory(LogFolder)
            Dim report = DateTimeOffset.Now.ToString("yyyy-MM-dd HH:mm:ss zzz") & " | " & context & Environment.NewLine &
                         "OS: " & Environment.OSVersion.ToString() & " | Runtime: " & Environment.Version.ToString() & Environment.NewLine &
                         "EXE: " & Application.ExecutablePath & Environment.NewLine &
                         ex.ToString() & Environment.NewLine & Environment.NewLine
            File.AppendAllText(StartupLogPath, report, Encoding.UTF8)
        Catch
            ' Reporting must never crash the game a second time.
        End Try
    End Sub

    Public Sub LogStatus(message As String)
        Try
            Directory.CreateDirectory(LogFolder)
            File.AppendAllText(StartupLogPath, DateTimeOffset.Now.ToString("yyyy-MM-dd HH:mm:ss zzz") & " | " & message & Environment.NewLine, Encoding.UTF8)
        Catch
        End Try
    End Sub

    Private Sub OnUIThreadException(sender As Object, e As ThreadExceptionEventArgs)
        LogIssue("Unhandled WinForms UI exception", e.Exception)
        Try
            MessageBox.Show("KAOSDUO encountered an error. The diagnostic log is saved here:" & Environment.NewLine & StartupLogPath & Environment.NewLine & Environment.NewLine & e.Exception.Message,
                            "KAOSDUO - Error report", MessageBoxButtons.OK, MessageBoxIcon.Error)
        Catch
        End Try
        Application.Exit()
    End Sub

    Private Sub OnDomainUnhandledException(sender As Object, e As UnhandledExceptionEventArgs)
        Dim ex = TryCast(e.ExceptionObject, Exception)
        LogIssue("Unhandled .NET exception", If(ex, New Exception(Convert.ToString(e.ExceptionObject))))
    End Sub

    Public Function ResolveResourceName(leafName As String) As String
        Dim names = Assembly.GetExecutingAssembly().GetManifestResourceNames()
        Dim found = names.FirstOrDefault(Function(n) n.EndsWith("." & leafName, StringComparison.OrdinalIgnoreCase) OrElse String.Equals(n, leafName, StringComparison.OrdinalIgnoreCase))
        If String.IsNullOrWhiteSpace(found) Then
            Throw New FileNotFoundException("Missing embedded resource " & leafName & ". Available: " & String.Join(", ", names))
        End If
        Return found
    End Function

    <STAThread>
    Public Sub Main()
        AddHandler AppDomain.CurrentDomain.UnhandledException, AddressOf OnDomainUnhandledException
        AddHandler Application.ThreadException, AddressOf OnUIThreadException
        Try
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException)
            Dim commandLine = Environment.GetCommandLineArgs()
            If commandLine.Any(Function(a) String.Equals(a, "--self-test", StringComparison.OrdinalIgnoreCase)) Then
                Environment.ExitCode = If(SelfTest(), 0, 2)
                Return
            End If
            Application.EnableVisualStyles()
            Application.SetCompatibleTextRenderingDefault(False)
            If commandLine.Any(Function(a) String.Equals(a, "--gui-smoke-test", StringComparison.OrdinalIgnoreCase)) Then
                Dim folder As String = Path.Combine(Path.GetTempPath(), "KAOSDUO-smoke")
                Dim i = Array.FindIndex(commandLine, Function(a) String.Equals(a, "--gui-smoke-test", StringComparison.OrdinalIgnoreCase))
                If i >= 0 AndAlso i + 1 < commandLine.Length Then folder = commandLine(i + 1)
                Using game As New GameForm()
                    game.RunSmokeTests(folder)
                End Using
                Environment.ExitCode = 0
                Return
            End If
            LogStatus("Starting KAOSDUO REFORGED v6.0.1")
            Using game As New GameForm()
                LogStatus("GameForm constructed; entering Windows Forms message loop")
                Application.Run(game)
                LogStatus("Window closed normally")
            End Using
        Catch ex As Exception
            LogIssue("Startup failed", ex)
            Environment.ExitCode = 1
            If Not Environment.GetCommandLineArgs().Any(Function(a) a.StartsWith("--", StringComparison.Ordinal)) Then
                Try
                    MessageBox.Show("The game could not start." & Environment.NewLine & Environment.NewLine & ex.Message & Environment.NewLine & Environment.NewLine &
                                    "Please send the log to the developer:" & Environment.NewLine & StartupLogPath,
                                    "KAOSDUO - Startup error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                Catch
                End Try
            End If
        End Try
    End Sub

    Private Function SelfTest() As Boolean
        Try
            Dim resourceNames = Assembly.GetExecutingAssembly().GetManifestResourceNames()
            For Each fileName As String In {"blue.png", "metal.png", "blue_theme.mp3", "metal_theme.mp3"}
                Dim resourceName = ResolveResourceName(fileName)
                Using stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName)
                    If stream Is Nothing OrElse stream.Length < 1000 Then Throw New IOException("Resource empty: " & resourceName)
                    If fileName.EndsWith(".png", StringComparison.OrdinalIgnoreCase) Then
                        Using bitmap As New Bitmap(stream)
                            If bitmap.Width < 16 OrElse bitmap.Height < 16 Then Throw New IOException("Sprite invalid: " & resourceName)
                        End Using
                    End If
                End Using
            Next
            Dim rng As New Random(2026)
            Dim cars = 0, bosses = 0
            For i = 1 To 12000
                If GameRules.RollHighway(rng) Then cars += 1
                If GameRules.RollPortalBoss(rng) Then bosses += 1
            Next
            If cars < 5500 OrElse cars > 6500 Then Throw New Exception("Highway chance out of bounds")
            If bosses < 2600 OrElse bosses > 3400 Then Throw New Exception("Boss chance out of bounds")
            Dim s As New GameSave() With {.Wave = 8, .Coins = 1250, .Scene = SceneKind.PortalBoss}
            s.Portal.TriggerBoss = True
            s.Portal.BossTriggered = True
            s.Portal.Distance = 1800
            s.OwnedWeapons.Add(3)
            Dim copy = JsonSerializer.Deserialize(Of GameSave)(JsonSerializer.Serialize(s))
            If copy Is Nothing OrElse copy.Wave <> 8 OrElse copy.Coins <> 1250 OrElse Not copy.OwnedWeapons.Contains(3) Then Throw New Exception("Save roundtrip failed")
            If copy.Portal.Distance <> 1800 Then Throw New Exception("Portal progress lost")
            If GameRules.CanAffordWeapon(100, {0}, 4) Then Throw New Exception("Weapon incorrectly affordable")
            If Not GameRules.CanAffordWeapon(1000, {0}, 4) Then Throw New Exception("Weapon incorrectly locked")
            Dim pt = GameRules.MuzzlePosition(225, 586, 0)
            If pt.X <= 225 OrElse pt.Y >= 586 Then Throw New Exception("Gun muzzle missing")
            If GameRules.Weapons.Length <> 8 Then Throw New Exception("Weapon count mismatch")
            Return True
        Catch ex As Exception
            LogIssue("Self test failed", ex)
            Return False
        End Try
    End Function
End Module
