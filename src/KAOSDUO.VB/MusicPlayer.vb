Option Strict On
Option Explicit On

Imports System
Imports System.IO
Imports System.Reflection
Imports System.Runtime.InteropServices
Imports System.Text

Public Class MusicPlayer
    Implements IDisposable

    <DllImport("winmm.dll", EntryPoint:="mciSendStringW", CharSet:=CharSet.Unicode)>
    Private Shared Function MciSendString(command As String, result As StringBuilder, resultCapacity As Integer, hwnd As IntPtr) As Integer
    End Function

    Private _current As String = ""
    Private _available As Boolean = True
    Private _mute As Boolean = False
    Private ReadOnly _mediaFolder As String = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "KAOSDUO", "Media")

    Public ReadOnly Property Available As Boolean
        Get
            Return _available
        End Get
    End Property

    Public Property Muted As Boolean
        Get
            Return _mute
        End Get
        Set(value As Boolean)
            _mute = value
            If _mute Then StopMusic()
        End Set
    End Property

    Public Sub Play(character As HeroKind)
        If _mute Then Return
        Dim track As String = If(character = HeroKind.Metal, "metal_theme.mp3", "blue_theme.mp3")
        If _current = track Then Return
        Try
            Directory.CreateDirectory(_mediaFolder)
            Dim trackPath As String = IO.Path.Combine(_mediaFolder, track)
            If Not File.Exists(trackPath) OrElse New FileInfo(trackPath).Length < 20000 Then
                Using stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(Program.ResolveResourceName(track))
                    If stream Is Nothing Then Throw New IOException("Soundtrack not embedded: " & track)
                    Using output = New FileStream(trackPath, FileMode.Create, FileAccess.Write)
                        stream.CopyTo(output)
                    End Using
                End Using
            End If
            StopMusic()
            Dim openCode = MciSendString("open """ & trackPath & """ type mpegvideo alias kaosduomusic", Nothing, 0, IntPtr.Zero)
            If openCode <> 0 Then Throw New IOException("Windows audio code: " & openCode.ToString())
            Dim playCode = MciSendString("play kaosduomusic repeat", Nothing, 0, IntPtr.Zero)
            If playCode <> 0 Then Throw New IOException("Windows playback code: " & playCode.ToString())
            _available = True
            _current = track
        Catch ex As Exception
            Program.LogIssue("Optional soundtrack playback failed for " & track, ex)
            _available = False
            _current = ""
            StopMusic()
        End Try
    End Sub

    Public Sub StopMusic()
        Try
            MciSendString("stop kaosduomusic", Nothing, 0, IntPtr.Zero)
            MciSendString("close kaosduomusic", Nothing, 0, IntPtr.Zero)
        Catch
        End Try
        _current = ""
    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        StopMusic()
        GC.SuppressFinalize(Me)
    End Sub
End Class
