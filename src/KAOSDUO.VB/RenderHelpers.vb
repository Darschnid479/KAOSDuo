Option Strict On
Option Explicit On

Imports System
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Linq

Partial Public Class GameForm
    Private ReadOnly _cyan As Color = Color.FromArgb(105, 245, 255)
    Private ReadOnly _pink As Color = Color.FromArgb(255, 115, 180)
    Private ReadOnly _gold As Color = Color.FromArgb(255, 224, 117)
    Private ReadOnly _muted As Color = Color.FromArgb(186, 185, 222)
    Private ReadOnly _white As Color = Color.FromArgb(249, 248, 255)
    Private ReadOnly _navy As Color = Color.FromArgb(16, 18, 45)

    Private Function RoundPath(rect As RectangleF, corner As Single) As GraphicsPath
        Dim r = Math.Min(corner, Math.Min(rect.Width, rect.Height) / 2.0F)
        Dim d = r * 2.0F
        Dim p As New GraphicsPath()
        p.AddArc(rect.Left, rect.Top, d, d, 180, 90)
        p.AddArc(rect.Right - d, rect.Top, d, d, 270, 90)
        p.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90)
        p.AddArc(rect.Left, rect.Bottom - d, d, d, 90, 90)
        p.CloseFigure()
        Return p
    End Function

    Private Sub FillPanel(g As Graphics, rect As RectangleF, back As Color, outline As Color, Optional radius As Single = 18, Optional thickness As Single = 1.7F)
        Using path = RoundPath(rect, radius), fill As New SolidBrush(back), pen As New Pen(outline, thickness)
            g.FillPath(fill, path)
            g.DrawPath(pen, path)
        End Using
    End Sub

    Private Sub FillGradient(g As Graphics, rect As RectangleF, top As Color, bottom As Color)
        Using brush As New LinearGradientBrush(rect, top, bottom, LinearGradientMode.Vertical)
            g.FillRectangle(brush, rect)
        End Using
    End Sub

    Private Sub Rect(g As Graphics, x As Single, y As Single, w As Single, h As Single, tint As Color)
        Using b As New SolidBrush(tint)
            g.FillRectangle(b, x, y, w, h)
        End Using
    End Sub

    Private Sub Line(g As Graphics, x1 As Single, y1 As Single, x2 As Single, y2 As Single, tint As Color, Optional thick As Single = 2)
        Using pen As New Pen(tint, thick)
            g.DrawLine(pen, x1, y1, x2, y2)
        End Using
    End Sub

    Private Sub Circle(g As Graphics, x As Single, y As Single, r As Single, tint As Color, Optional outline As Color = Nothing, Optional thick As Single = 0)
        Using b As New SolidBrush(tint)
            g.FillEllipse(b, x - r, y - r, r * 2, r * 2)
        End Using
        If thick > 0 Then
            Using pen As New Pen(outline, thick)
                g.DrawEllipse(pen, x - r, y - r, r * 2, r * 2)
            End Using
        End If
    End Sub

    Private Sub Polygon(g As Graphics, tint As Color, ParamArray points As PointF())
        Using b As New SolidBrush(tint)
            g.FillPolygon(b, points)
        End Using
    End Sub

    Private Sub TextAt(g As Graphics, value As String, x As Single, y As Single, sz As Single, Optional tint As Color = Nothing, Optional bold As Boolean = True, Optional width As Single = 1000)
        If String.IsNullOrEmpty(value) Then Return
        Dim chosenStyle As FontStyle = If(bold, FontStyle.Bold, FontStyle.Regular)
        Using f As New Font("Segoe UI", sz, chosenStyle, GraphicsUnit.Pixel), b As New SolidBrush(If(tint = Color.Empty, _white, tint))
            Using format As New StringFormat() With {.Trimming = StringTrimming.EllipsisCharacter, .FormatFlags = StringFormatFlags.NoWrap}
                g.DrawString(value, f, b, New RectangleF(x, y, width, sz * 1.45F), format)
            End Using
        End Using
    End Sub

    Private Sub CenterText(g As Graphics, value As String, centerX As Single, y As Single, sz As Single, Optional tint As Color = Nothing, Optional bold As Boolean = True, Optional width As Single = 1100)
        If String.IsNullOrEmpty(value) Then Return
        Using f As New Font("Segoe UI", sz, If(bold, FontStyle.Bold, FontStyle.Regular), GraphicsUnit.Pixel), b As New SolidBrush(If(tint = Color.Empty, _white, tint)), format As New StringFormat() With {.Alignment = StringAlignment.Center, .Trimming = StringTrimming.EllipsisCharacter}
            g.DrawString(value, f, b, New RectangleF(centerX - width / 2, y, width, sz * 1.6F), format)
        End Using
    End Sub

    Private Sub Button(g As Graphics, x As Single, y As Single, w As Single, h As Single, label As String, action As String, Optional accent As Color = Nothing, Optional selected As Boolean = False, Optional enabled As Boolean = True, Optional fontSize As Single = 17)
        Dim tint = If(accent = Color.Empty, _cyan, accent)
        Dim bg = If(selected, Color.FromArgb(101, 53, 131), If(enabled, Color.FromArgb(39, 40, 82), Color.FromArgb(33, 32, 52)))
        Dim area As New RectangleF(x, y, w, h)
        FillPanel(g, area, bg, If(enabled, tint, Color.FromArgb(90, 90, 125)), 12, If(selected, 3.0F, 1.6F))
        If selected Then Rect(g, x + 9, y + h - 7, w - 18, 3, tint)
        CenterText(g, label, x + w / 2, y + (h - fontSize * 1.55F) / 2, fontSize, If(enabled, _white, _muted), True, w - 12)
        If enabled Then _ui.Add(New HitButton(area, action))
    End Sub

    Private Sub Gauge(g As Graphics, x As Single, y As Single, w As Single, h As Single, percent As Single, colour As Color, Optional title As String = "", Optional background As Color = Nothing)
        Dim bg = If(background = Color.Empty, Color.FromArgb(45, 49, 78), background)
        FillPanel(g, New RectangleF(x, y, w, h), bg, Color.FromArgb(95, 106, 144), h * 0.5F, 1)
        Dim f = Limit(percent, 0, 1)
        If f > 0 Then
            Using path = RoundPath(New RectangleF(x + 3, y + 3, Math.Max(6, (w - 6) * f), h - 6), (h - 6) / 2)
                Using b As New SolidBrush(colour)
                    g.FillPath(b, path)
                End Using
            End Using
        End If
        If title <> "" Then CenterText(g, title, x + w / 2, y - 1, Math.Min(15.0F, h - 2), _white, True, w)
    End Sub

    Private Sub DimBackground(g As Graphics, Optional alpha As Integer = 196)
        Rect(g, 0, 0, 1280, 720, Color.FromArgb(alpha, 6, 7, 23))
    End Sub

    Private Sub DrawNotice(g As Graphics)
        If _noticeTime <= 0 Then Return
        Dim w As Single = 850
        Dim x As Single = (1280 - w) / 2
        FillPanel(g, New RectangleF(x, 151, w, 57), Color.FromArgb(215, 18, 23, 57), Color.FromArgb(218, 172, 255), 16, 2)
        CenterText(g, _notification, 640, 163, 23, _gold, True, 820)
    End Sub

    Private Sub DrawBackdrop(g As Graphics, Optional red As Boolean = False)
        FillGradient(g, New RectangleF(0, 0, 1280, 720), If(red, Color.FromArgb(39, 9, 56), Color.FromArgb(13, 25, 57)), Color.FromArgb(10, 8, 34))
        ' Offset stars have different speeds to give native parallax motion.
        For i = 0 To 88
            Dim dx = CSng(((i * 199.0F + _elapsed * If(i Mod 3 = 0, 8, 2)) Mod 1340) - 30)
            Dim dy = CSng(28 + (i * 73) Mod 490)
            Dim alpha = CInt(70 + 65 * (0.5 + 0.5 * Math.Sin(_elapsed * 1.8 + i)))
            Rect(g, dx, dy, 2 + (i Mod 3), 2 + (i Mod 3), Color.FromArgb(alpha, If(i Mod 4 = 0, 244, 120), If(i Mod 4 = 0, 146, 210), 253))
        Next
        For j = 0 To 3
            Circle(g, 1050, 245, 85 + j * 32 + CSng(Math.Sin(_elapsed * 1.2)) * 4, Color.FromArgb(If(j = 0, 22, 6), 179, 87, 255), _pink, If(j = 0, 4, 1))
        Next
        For i = 0 To 13
            Dim x As Single = i * 108 - CSng((_elapsed * 11) Mod 108)
            Dim h = 105 + (i * 59) Mod 140
            Rect(g, x, GameRules.FloorY - h - 50, 85, h + 50, If(red, Color.FromArgb(43, 18, 63), Color.FromArgb(25, 42, 78)))
            For j = 0 To 4
                Rect(g, x + 11 + (j Mod 2) * 28, GameRules.FloorY - h + 15 + j * 21, 11, 7, Color.FromArgb(65, 96, 223, 255))
            Next
        Next
    End Sub

    Private Sub DrawGround(g As Graphics)
        FillGradient(g, New RectangleF(0, GameRules.FloorY, 1280, 140), Color.FromArgb(32, 42, 91), Color.FromArgb(10, 10, 34))
        Rect(g, 0, GameRules.FloorY - 5, 1280, 5, _cyan)
        Rect(g, 0, GameRules.FloorY + 4, 1280, 2, Color.FromArgb(94, 192, 255))
        For i = 0 To 22
            Dim x = i * 66 - CSng((_elapsed * 56) Mod 66)
            Line(g, x, GameRules.FloorY, x - 40, 720, Color.FromArgb(66, 85, 238, 255), 2)
        Next
        For i = 0 To 3
            Line(g, 0, GameRules.FloorY + 27 + i * 25, 1280, GameRules.FloorY + 27 + i * 25, Color.FromArgb(55, 159, 90, 245), 1)
        Next
    End Sub

    Private Sub DrawSparks(g As Graphics)
        For Each p In _sparks
            Dim alpha As Integer = Math.Max(0, Math.Min(255, CInt(255 * p.Life / Math.Max(0.01F, p.FullLife))))
            Dim tint = Color.FromArgb(alpha, p.Tint.R, p.Tint.G, p.Tint.B)
            Circle(g, p.X, p.Y, p.Size * p.Life / Math.Max(0.1F, p.FullLife), tint)
        Next
    End Sub

    Private Sub DrawBullets(g As Graphics)
        For Each b In _s.Bullets
            Dim tint = If(b.Friendly, GameRules.Weapons(Math.Max(0, Math.Min(7, b.Weapon))).Tint, _pink)
            Dim length = 14.0F
            Dim vLen = CSng(Math.Sqrt(b.Vx * b.Vx + b.Vy * b.Vy))
            If vLen > 0 Then
                Line(g, b.X - b.Vx / vLen * length, b.Y - b.Vy / vLen * length, b.X, b.Y, Color.FromArgb(120, tint), Math.Max(2, b.Radius))
            End If
            Circle(g, b.X, b.Y, b.Radius * 1.7F, Color.FromArgb(44, tint))
            Circle(g, b.X, b.Y, b.Radius, tint, _white, 1)
        Next
    End Sub

    Private Sub DrawHero(g As Graphics, x As Single, footY As Single, moving As Boolean, Optional isPortal As Boolean = False)
        Dim h = _s.Hero
        Dim facing As Single = If(h.Facing >= 0, 1.0F, -1.0F)
        Dim anim = h.MotionTime * If(moving, 13.0F, 2.1F)
        Dim stepAmount As Single = If(moving, CSng(Math.Sin(anim)) * 7.5F, CSng(Math.Sin(anim)) * 1.2F)
        Dim bob = If(moving, Math.Abs(CSng(Math.Sin(anim))) * 2.8F, CSng(Math.Sin(anim)) * 2)
        Dim img = If(_s.Character = HeroKind.Metal, _metal, _blue)
        Dim sourceW = CSng(img.Width), sourceH = CSng(img.Height)
        Dim bodyW As Single = 82, bodyH As Single = 192
        Dim y = footY - bodyH + bob
        Dim trans = g.Save()
        g.TranslateTransform(x, footY)
        g.ScaleTransform(facing, 1)
        g.RotateTransform(If(moving, CSng(Math.Sin(anim * 0.5)) * 2, CSng(Math.Sin(anim)) * 1))
        g.TranslateTransform(-x, -footY)
        ' Split at the hips so the legs actually stride rather than merely bobbing.
        Dim legTop = CSng(Math.Round(sourceH * 0.54F))
        Dim leftHalf = sourceW * 0.51F
        Dim upperDest As New RectangleF(x - bodyW / 2, y, bodyW, bodyH * 0.58F)
        Dim upperSrc As New RectangleF(0, 0, sourceW, legTop + 11)
        Dim leftLeg As New RectangleF(x - bodyW / 2 + stepAmount * 0.55F, y + bodyH * 0.55F, bodyW * 0.51F, bodyH * 0.46F)
        Dim rightLeg As New RectangleF(x - 2 - stepAmount * 0.55F, y + bodyH * 0.55F, bodyW * 0.51F, bodyH * 0.46F)
        Dim leftSrc As New RectangleF(0, legTop, leftHalf, sourceH - legTop)
        Dim rightSrc As New RectangleF(leftHalf - 4, legTop, sourceW - leftHalf + 4, sourceH - legTop)
        Dim oldInterpolation = g.InterpolationMode
        g.InterpolationMode = InterpolationMode.NearestNeighbor
        g.DrawImage(img, leftLeg, leftSrc, GraphicsUnit.Pixel)
        g.DrawImage(img, rightLeg, rightSrc, GraphicsUnit.Pixel)
        g.DrawImage(img, upperDest, upperSrc, GraphicsUnit.Pixel)
        g.InterpolationMode = oldInterpolation
        g.Restore(trans)
        ' The game character really holds the gun: arm, hand, stock, receiver, barrel and muzzle.
        Dim gun = GameRules.Weapons(_s.EquippedWeapon)
        Dim angle = h.Aim
        Dim shoulderX = x + CSng(Math.Cos(angle)) * 11
        Dim shoulderY = footY - 120 + bob
        Dim handX = x + CSng(Math.Cos(angle)) * 27
        Dim handY = footY - 103 + CSng(Math.Sin(angle)) * 14
        Dim cloth = If(_s.Character = HeroKind.Metal, Color.FromArgb(32, 34, 44), Color.FromArgb(52, 69, 111))
        Line(g, shoulderX, shoulderY, handX, handY, Color.FromArgb(12, 12, 26), 17)
        Line(g, shoulderX, shoulderY, handX, handY, cloth, 12)
        DrawWeapon(g, x, footY, angle, gun, h.MuzzleFlash, h.Recoil)
        Circle(g, handX, handY, 7, Color.FromArgb(244, 182, 138), Color.FromArgb(33, 21, 32), 2)
        If h.Invincible > 0 Then
            Dim pulse = Math.Abs(Math.Sin(_elapsed * 17))
            Circle(g, x, footY - 102, 107, Color.FromArgb(CInt(11 + pulse * 20), _cyan), _cyan, 1)
        End If
        If _s.FeverTime > 0 Then
            For i = 0 To 3
                Dim a = _elapsed * 3.5F + i * CSng(Math.PI / 2)
                Circle(g, x + CSng(Math.Cos(a)) * 56, footY - 103 + CSng(Math.Sin(a)) * 58, 5, _pink)
            Next
        End If
    End Sub

    Private Sub DrawWeapon(g As Graphics, x As Single, footY As Single, angle As Single, spec As WeaponSpec, flash As Single, recoil As Single)
        Dim originX = x + CSng(Math.Cos(angle)) * 13
        Dim originY = footY - 102
        Dim state = g.Save()
        g.TranslateTransform(originX, originY)
        g.RotateTransform(angle * CSng(180 / Math.PI))
        g.TranslateTransform(-recoil * 9, 0)
        Rect(g, -15, -6, 26, 13, Color.FromArgb(40, 38, 58))
        Rect(g, -25, 0, 16, 15, Color.FromArgb(54, 48, 61))
        Polygon(g, Color.FromArgb(29, 30, 48), New PointF(-8, -13), New PointF(35, -13), New PointF(46, -6), New PointF(46, 11), New PointF(-8, 11))
        Rect(g, 0, -10, 42, 18, spec.Tint)
        Rect(g, 5, -8, 26, 4, Color.FromArgb(215, 240, 250))
        Rect(g, 15, 9, 13, 13, Color.FromArgb(35, 36, 54))
        Rect(g, 41, -6, 22, 11, Color.FromArgb(26, 30, 50))
        Rect(g, 45, -3, 17, 5, Color.FromArgb(216, 235, 245))
        If spec.Id = 2 OrElse spec.Id = 7 Then
            Rect(g, -4, -17, 24, 7, spec.Tint)
            Rect(g, 20, 13, 20, 4, spec.Tint)
        ElseIf spec.Id = 4 OrElse spec.Id = 1 Then
            Rect(g, 8, -16, 28, 6, Color.FromArgb(255, 207, 125))
            Rect(g, 34, 5, 23, 7, Color.FromArgb(38, 30, 40))
        ElseIf spec.Id = 6 Then
            For i = 0 To 2
                Rect(g, 40, -11 + i * 7, 23, 4, Color.FromArgb(224, 225, 235))
            Next
        End If
        If flash > 0 Then
            Dim strength = flash / 0.11F
            Polygon(g, Color.FromArgb(CInt(225 * strength), spec.Tint),
                New PointF(63, -12), New PointF(93, -3), New PointF(71, 0), New PointF(91, 15), New PointF(63, 8))
        End If
        g.Restore(state)
    End Sub

    Private Sub DrawEnemy(g As Graphics, enemy As EnemyState)
        Dim x = enemy.X, y = enemy.Y
        Dim pulse = CSng(Math.Sin(enemy.Anim * 3))
        Dim tint As Color
        Select Case enemy.Kind
            Case 0 : tint = Color.FromArgb(126, 255, 174)
            Case 1 : tint = Color.FromArgb(219, 147, 255)
            Case 2 : tint = Color.FromArgb(249, 178, 105)
            Case 3 : tint = Color.FromArgb(119, 219, 255)
            Case 4 : tint = Color.FromArgb(255, 132, 161)
            Case Else : tint = Color.FromArgb(245, 105, 227)
        End Select
        If enemy.Boss Then
            Circle(g, x, y, 100 + pulse * 4, Color.FromArgb(27, tint), tint, 3)
            For i = 0 To 7
                Dim a As Single = enemy.Anim * 0.7F + i * CSng(Math.PI / 4)
                Dim xa = x + CSng(Math.Cos(a)) * 77
                Dim ya = y + CSng(Math.Sin(a)) * 77
                Circle(g, xa, ya, 11, tint)
            Next
            Polygon(g, Color.FromArgb(35, 27, 78), New PointF(x, y - 69), New PointF(x + 70, y), New PointF(x, y + 69), New PointF(x - 70, y))
            Circle(g, x, y, 46 + pulse * 4, tint, _white, 3)
            Circle(g, x, y, 23, Color.FromArgb(28, 17, 53))
            Circle(g, x - 10, y - 8, 6, _white) : Circle(g, x + 10, y - 8, 6, _white)
        Else
            Dim radius As Single = enemy.Radius + pulse * 1.7F
            Circle(g, x, y, radius + 9, Color.FromArgb(28, tint))
            Circle(g, x, y, radius, tint, Color.FromArgb(18, 29, 46), 3)
            If enemy.Kind = 1 OrElse enemy.Kind = 3 Then
                Polygon(g, tint, New PointF(x - 14, y), New PointF(x - 53, y - 10 - pulse * 5), New PointF(x - 36, y + 18))
                Polygon(g, tint, New PointF(x + 14, y), New PointF(x + 53, y - 10 + pulse * 5), New PointF(x + 36, y + 18))
            ElseIf enemy.Kind = 2 OrElse enemy.Kind = 4 Then
                Rect(g, x - 23, y - 24, 46, 47, Color.FromArgb(45, 37, 57))
                Rect(g, x - 18, y - 18, 36, 34, tint)
            End If
            Circle(g, x - 10, y - 7, 5, Color.FromArgb(17, 19, 33))
            Circle(g, x + 10, y - 7, 5, Color.FromArgb(17, 19, 33))
            Line(g, x - 8, y + 10, x + 9, y + 9, Color.FromArgb(17, 19, 33), 3)
        End If
        If enemy.Freeze > 0 Then Circle(g, x, y, enemy.Radius + 8, Color.FromArgb(30, _cyan), _cyan, 2)
        If enemy.Hp < enemy.MaxHp Then Gauge(g, x - 45, y - enemy.Radius - 26, 90, 8, enemy.Hp / Math.Max(1, enemy.MaxHp), Color.FromArgb(255, 130, 172))
    End Sub
End Class
