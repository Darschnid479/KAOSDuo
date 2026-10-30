Option Strict On
Option Explicit On

Imports System
Imports System.Linq
Imports System.Text.Json
Imports System.Windows.Forms

Public Module Program
    <STAThread>
    Public Sub Main()
        If Environment.GetCommandLineArgs().Any(Function(arg) String.Equals(arg, "--self-test", StringComparison.OrdinalIgnoreCase)) Then
            Environment.Exit(If(SelfTest(), 0, 2))
            Return
        End If
        Application.EnableVisualStyles()
        Application.SetCompatibleTextRenderingDefault(False)
        Application.Run(New GameForm())
    End Sub

    Private Function SelfTest() As Boolean
        Try
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
            Try
                Dim path = IO.Path.Combine(IO.Path.GetTempPath(), "kaosduo_selftest_error.txt")
                IO.File.WriteAllText(path, ex.ToString())
            Catch
            End Try
            Return False
        End Try
    End Function
End Module
