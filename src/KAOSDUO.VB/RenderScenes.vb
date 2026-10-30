Option Strict On
Option Explicit On

Imports System
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Linq

Partial Public Class GameForm
    Private Sub DrawScene(g As Graphics)
        _ui.Clear()
        Dim kind = _s.Scene
        If kind = SceneKind.MainMenu Then
            DrawMainMenu(g)
        ElseIf kind = SceneKind.Portal OrElse (kind = SceneKind.PortalFailed AndAlso _s.Portal.Type = "GEOMETRY") OrElse (kind = SceneKind.Paused AndAlso _s.PreviousScene = SceneKind.Portal) Then
            DrawPortal(g)
        ElseIf kind = SceneKind.Highway OrElse (kind = SceneKind.PortalFailed AndAlso _s.Portal.Type = "HIGHWAY") OrElse (kind = SceneKind.Paused AndAlso _s.PreviousScene = SceneKind.Highway) Then
            DrawHighway(g)
        ElseIf kind = SceneKind.PortalBoss OrElse (kind = SceneKind.Paused AndAlso _s.PreviousScene = SceneKind.PortalBoss) Then
            DrawPortalBoss(g)
        Else
            DrawCombat(g)
        End If
        Select Case kind
            Case SceneKind.ShopOffer : DrawShopOffer(g)
            Case SceneKind.Shop : DrawShop(g)
            Case SceneKind.Upgrade : DrawUpgrade(g)
            Case SceneKind.PortalFailed : DrawPortalFail(g)
            Case SceneKind.Paused : DrawPause(g)
            Case SceneKind.GameOver : DrawEnd(g, False)
            Case SceneKind.Victory : DrawEnd(g, True)
        End Select
        DrawNotice(g)
        If kind <> SceneKind.MainMenu Then
            TextAt(g, "KAOSDUO REFORGED  |  NATIVE VB.NET  |  v6.0", 19, 701, 10, Color.FromArgb(125, 156, 193), False, 500)
        End If
    End Sub

    Private Sub DrawMainMenu(g As Graphics)
        DrawBackdrop(g, True)
        DrawGround(g)
        FillPanel(g, New RectangleF(51, 39, 1178, 628), Color.FromArgb(231, 15, 15, 43), Color.FromArgb(142, 113, 232), 26, 3)
        Rect(g, 78, 74, 7, 86, _cyan)
        TextAt(g, "NATIVE WINDOWS EDITION  /  VERSION 6.0", 99, 71, 18, _cyan, True, 500)
        TextAt(g, "KAOS", 98, 108, 76, _white, True, 450)
        TextAt(g, "DUO", 320, 108, 76, _gold, True, 350)
        TextAt(g, "R E F O R G E D", 104, 194, 27, _pink, True, 540)
        TextAt(g, "TO HELTER. EN SPILLER. EN VIRKELIGHET I KAOS.", 103, 245, 18, _muted, False, 580)
        TextAt(g, "Animerte helter / 8 fysiske våpen / bevegelig portalboss", 102, 276, 15, _muted, False, 600)
        TextAt(g, "10 kampbølger / flyging / 3D-lignende bilreise / våpenbutikk", 102, 300, 15, _muted, False, 625)
        TextAt(g, "VELG HELT", 101, 351, 18, _cyan, True, 270)
        Button(g, 99, 381, 243, 57, "1   BLÅ  /  COZY", "character:blue", _cyan, _choiceHero = HeroKind.Blue, True, 17)
        Button(g, 353, 381, 243, 57, "2   METAL  /  RIFFS", "character:metal", _pink, _choiceHero = HeroKind.Metal, True, 16)
        TextAt(g, "VELG VANSKELIGHET", 101, 454, 17, _cyan, True, 310)
        Button(g, 99, 484, 117, 49, "LETT", "difficulty:0", _cyan, _choiceDifficulty = GameDifficulty.Easy, True, 15)
        Button(g, 225, 484, 117, 49, "MEDIUM", "difficulty:1", _cyan, _choiceDifficulty = GameDifficulty.Medium, True, 14)
        Button(g, 351, 484, 117, 49, "VANSK.", "difficulty:2", _pink, _choiceDifficulty = GameDifficulty.Hard, True, 14)
        Button(g, 477, 484, 119, 49, "DRIT V.", "difficulty:3", _pink, _choiceDifficulty = GameDifficulty.Insane, True, 14)
        Button(g, 99, 557, 276, 68, "START SPILLET", "start", _gold, True, True, 23)
        Button(g, 388, 557, 208, 68, "LAST INN (F9)", "load", _cyan, False, True, 19)
        ' Hero cards use the actual two original pixel-art sprites, not generic stand-ins.
        FillPanel(g, New RectangleF(655, 76, 267, 496), If(_choiceHero = HeroKind.Blue, Color.FromArgb(77, 39, 72), Color.FromArgb(37, 33, 65)), If(_choiceHero = HeroKind.Blue, _cyan, Color.FromArgb(91, 82, 142)), 23)
        FillPanel(g, New RectangleF(937, 76, 255, 496), If(_choiceHero = HeroKind.Metal, Color.FromArgb(78, 40, 73), Color.FromArgb(37, 33, 65)), If(_choiceHero = HeroKind.Metal, _pink, Color.FromArgb(91, 82, 142)), 23)
        Dim blueBob As Single = CSng(Math.Sin(_elapsed * 2.1)) * 6
        Dim metalBob As Single = CSng(Math.Sin(_elapsed * 2.1 + 1.8)) * 7
        g.InterpolationMode = InterpolationMode.NearestNeighbor
        g.DrawImage(_blue, New RectangleF(716, 104 + blueBob, 135, 322))
        g.DrawImage(_metal, New RectangleF(998, 104 + metalBob, 133, 320))
        g.InterpolationMode = InterpolationMode.HighQualityBicubic
        CenterText(g, "BLÅ", 789, 437, 29, _cyan)
        CenterText(g, "METAL", 1066, 437, 29, _pink)
        CenterText(g, "Rolig og koselig", 789, 478, 14, _muted, False, 220)
        CenterText(g, "Tunge gitarer", 1066, 478, 14, _muted, False, 220)
        Button(g, 676, 589, 242, 49, "SPILL VALGT MUSIKK", "preview", _cyan, False, True, 15)
        Button(g, 935, 589, 242, 49, If(_music.Muted, "MUSIKK: AV", "MUSIKK: PÅ"), "music", _pink, False, True, 17)
        TextAt(g, "WASD / PILTASTER: BEVEGELSE     MUS: SIKT + SKYT     F5/F9: LAGRE / LAST INN", 96, 647, 12, _muted, False, 1050)
    End Sub

    Private Sub DrawCombat(g As Graphics)
        DrawBackdrop(g)
        DrawGround(g)
        Dim heroX = _s.Hero.X, heroY = _s.Hero.Y
        For Each enemy In _s.Enemies
            DrawEnemy(g, enemy)
        Next
        DrawBullets(g)
        DrawHero(g, heroX, heroY, IsHeld(System.Windows.Forms.Keys.A, System.Windows.Forms.Keys.D, System.Windows.Forms.Keys.Left, System.Windows.Forms.Keys.Right))
        DrawSparks(g)
        DrawCombatHud(g)
        If _s.Scene = SceneKind.Combat Then
            TextAt(g, "A / D: LØP     W / SPACE: HOPP     MUS / F: SKYT     SHIFT: DASH     G: SUPER     Q: OVERDRIVE", 165, 661, 15, _muted, False, 1030)
            TextAt(g, "1–8: VÅPEN     R: LAD     B: BUTIKK     P: PAUSE     M: MUSIKK     F5/F9: SAVE/LOAD", 165, 686, 12, Color.FromArgb(151, 194, 234), False, 1050)
        End If
    End Sub

    Private Sub DrawCombatHud(g As Graphics)
        FillPanel(g, New RectangleF(24, 20, 550, 116), Color.FromArgb(226, 12, 16, 43), Color.FromArgb(74, 179, 238), 15)
        TextAt(g, If(_s.Character = HeroKind.Metal, "METAL", "BLÅ") & "  |  BØLGE " & _s.Wave.ToString() & "/10", 43, 32, 22, _white)
        TextAt(g, GameRules.GetDifficultyName(_s.Difficulty), 350, 34, 14, _gold)
        TextAt(g, CInt(Math.Max(0, _s.Hero.Hp)).ToString() & "/" & CInt(_s.Hero.MaxHp).ToString() & " HP", 45, 69, 14, _white)
        Gauge(g, 143, 75, 389, 21, _s.Hero.Hp / Math.Max(1, _s.Hero.MaxHp), _pink)
        Gauge(g, 44, 111, 488, 9, CSng(_s.Experience) / Math.Max(1, _s.ExpNeeded), _cyan)
        FillPanel(g, New RectangleF(592, 20, 665, 116), Color.FromArgb(226, 12, 16, 43), Color.FromArgb(92, 89, 187), 15)
        TextAt(g, "MYNTER", 611, 32, 14, _muted)
        TextAt(g, _s.Coins.ToString("N0"), 613, 57, 26, _gold)
        TextAt(g, "SCORE", 843, 32, 14, _muted)
        TextAt(g, _s.Score.ToString("N0"), 842, 57, 26, _white)
        TextAt(g, "KOMBO", 1081, 32, 14, _muted)
        TextAt(g, "x" & _s.Combo.ToString(), 1080, 57, 26, If(_s.Combo >= 8, _pink, _cyan))
        Gauge(g, 613, 112, 607, 9, _s.FeverMeter / 100, Color.FromArgb(221, 111, 249))
        Dim gun = GameRules.Weapons(_s.EquippedWeapon)
        FillPanel(g, New RectangleF(26, 615, 730, 84), Color.FromArgb(224, 14, 18, 47), gun.Tint, 16)
        TextAt(g, gun.Name, 45, 626, 23, gun.Tint)
        Dim ammo = gun.Magazine
        If _s.Magazine.ContainsKey(gun.Id) Then ammo = _s.Magazine(gun.Id)
        TextAt(g, If(_s.Hero.ReloadRemaining > 0, "LADER...", "AMMO " & Math.Max(0, ammo).ToString() & "/" & gun.Magazine.ToString()), 480, 634, 19, _white)
        TextAt(g, "F5 LAGRE  |  F9 LAST INN  |  F12 SKJERMBILDE", 46, 665, 12, _muted, False)
        FillPanel(g, New RectangleF(770, 615, 486, 84), Color.FromArgb(224, 14, 18, 47), Color.FromArgb(107, 82, 160), 16)
        TextAt(g, "SUPER " & If(_s.SuperAvailable, If(_s.Hero.SuperCooldown > 0, CInt(_s.Hero.SuperCooldown).ToString() & "s", "KLAR"), "MISTET") & "   |   DASH " & If(_s.DashAvailable, "JA", "NEI"), 790, 633, 16, _gold)
        TextAt(g, "OVERDRIVE " & If(_s.OverdriveAvailable, CInt(_s.FeverMeter).ToString() & "%", "MISTET") & "   |   LEVEL " & _s.Level.ToString(), 790, 661, 15, _cyan)
    End Sub

    Private Sub DrawPortal(g As Graphics)
        Dim p = _s.Portal
        Dim stage = PortalSectionMode(p, p.Distance)
        Dim a = _elapsed * 3.0F
        FillGradient(g, New RectangleF(0, 0, 1280, 720), Color.FromArgb(51, 16, 100), Color.FromArgb(14, 13, 53))
        For i = 0 To 19
            Dim x = CSng((i * 115 - p.Distance * 0.18F + 25000) Mod 1440) - 100
            Dim y = CSng(120 + (i * 83) Mod 400)
            Rect(g, x, y, 74, 105 + (i * 41) Mod 140, Color.FromArgb(24, 185, 125, 252))
        Next
        For i = 0 To 4
            Circle(g, 990, 334, 95 + i * 37 + CSng(Math.Sin(a)) * 7, Color.FromArgb(If(i = 0, 22, 4), _cyan), If(i Mod 2 = 0, _pink, _cyan), 2)
        Next
        Dim beat = CSng(0.5 + 0.5 * Math.Sin(_elapsed * 9))
        Rect(g, 0, GameRules.FloorY, 1280, 134, Color.FromArgb(16, 15, 48))
        Line(g, 0, GameRules.FloorY, 1280, GameRules.FloorY, Color.FromArgb(130 + CInt(beat * 110), _cyan), 6)
        For i = 0 To 20
            Dim x = CSng((i * 93 - p.Distance * 0.5F + 35000) Mod 1400) - 90
            Line(g, x, GameRules.FloorY, x - 85, 720, Color.FromArgb(80, _cyan), 2)
        Next
        For Each ob In p.Obstacles
            Dim x = ob.X - p.Distance + 260
            If x < -100 OrElse x > 1380 Then Continue For
            Dim accent = If(ob.Kind Mod 2 = 0, _pink, _cyan)
            If ob.Kind = 0 Then
                Polygon(g, Color.FromArgb(39, 25, 75), New PointF(x, ob.Y + ob.H), New PointF(x + ob.W * 0.5F, ob.Y), New PointF(x + ob.W, ob.Y + ob.H))
                Line(g, x, ob.Y + ob.H, x + ob.W / 2, ob.Y, accent, 4)
                Line(g, x + ob.W / 2, ob.Y, x + ob.W, ob.Y + ob.H, accent, 4)
            ElseIf ob.Kind = 1 Then
                FillPanel(g, New RectangleF(x, ob.Y, ob.W, ob.H), Color.FromArgb(27, 33, 76), accent, 6, 4)
                Rect(g, x + 9, ob.Y + 9, ob.W - 18, ob.H - 18, Color.FromArgb(110, accent))
            Else
                Dim r = ob.W / 2
                Circle(g, x + r, ob.Y + ob.H / 2, r, Color.FromArgb(32, accent), accent, 4)
                For blade = 0 To 5
                    Dim theta = a * 1.2F + blade * CSng(Math.PI / 3)
                    Line(g, x + r, ob.Y + ob.H / 2, x + r + CSng(Math.Cos(theta)) * r, ob.Y + ob.H / 2 + CSng(Math.Sin(theta)) * r, accent, 4)
                Next
            End If
        Next
        Dim cubeColor = If(_s.Character = HeroKind.Blue, _cyan, _pink)
        Dim cx As Single = 282, cy As Single = p.Y + 22
        If stage = 1 Then
            Polygon(g, cubeColor, New PointF(cx - 34, cy), New PointF(cx + 21, cy - 24), New PointF(cx + 43, cy), New PointF(cx + 21, cy + 24))
            Polygon(g, Color.FromArgb(19, 31, 57), New PointF(cx - 14, cy), New PointF(cx + 15, cy - 13), New PointF(cx + 17, cy + 12))
            Polygon(g, _gold, New PointF(cx - 34, cy - 9), New PointF(cx - 65 - beat * 12, cy), New PointF(cx - 34, cy + 9))
        Else
            Dim old = g.Save()
            g.TranslateTransform(cx, cy)
            g.RotateTransform(If(stage = 0 AndAlso Not p.Grounded, _elapsed * 175.0F, If(stage = 2, CSng(Math.Sin(a)) * 14, 0.0F)))
            FillPanel(g, New RectangleF(-22, -22, 44, 44), cubeColor, Color.White, 5, 3)
            Rect(g, -10, -7, 6, 7, Color.FromArgb(25, 20, 57))
            Rect(g, 6, -7, 6, 7, Color.FromArgb(25, 20, 57))
            Line(g, -9, 9, 8, 9, Color.FromArgb(25, 20, 57), 3)
            g.Restore(old)
        End If
        For i = 0 To 14
            Dim tx = 240 - i * 13
            Dim ty = cy + CSng(Math.Sin(_elapsed * 12 + i)) * 4
            Circle(g, tx, ty, Math.Max(1, 7 - i * 0.4F), Color.FromArgb(Math.Max(12, 150 - i * 9), cubeColor))
        Next
        If p.TriggerBoss AndAlso Not p.BossDefeated AndAlso Not p.BossTriggered Then
            ' The 25% ambush is intentionally not announced until it actually happens.
        End If
        DrawTravelHud(g, "PORTAL RUN", If(stage = 0, "KUBE / HOPP", If(stage = 1, "JET / HOLD SPACE", "GRAVITASJON / BYTT")))
        CenterText(g, "SPACE / W: " & If(stage = 0, "HOPP", If(stage = 1, "HOLD FOR Å FLY", "SNU TYNGDEKRAFT")), 640, 643, 19, _white, True, 1100)
        DrawSparks(g)
    End Sub

    Private Sub DrawTravelHud(g As Graphics, title As String, mode As String)
        FillPanel(g, New RectangleF(29, 23, 618, 121), Color.FromArgb(226, 15, 14, 49), _cyan, 18)
        TextAt(g, title & "  " & _s.Wave.ToString() & " → " & (_s.Wave + 1).ToString(), 51, 32, 28, _white)
        TextAt(g, "MODUS: " & mode, 52, 76, 16, _gold)
        TextAt(g, "KRASJ = -1 EVNE.  HP FORBLIR UENDRET.", 53, 107, 13, _muted)
        FillPanel(g, New RectangleF(670, 23, 579, 121), Color.FromArgb(226, 15, 14, 49), _pink, 18)
        Dim progress = Limit(_s.Portal.Distance / Math.Max(1, _s.Portal.Length), 0, 1)
        TextAt(g, "REISE " & CInt(progress * 100).ToString() & "%     FORSØK " & (_s.Portal.Attempts + 1).ToString(), 692, 38, 18, _white)
        Gauge(g, 692, 77, 531, 19, progress, _cyan)
        TextAt(g, "EVNER: " & If(_s.DashAvailable, "DASH ", "") & If(_s.DoubleJumpAvailable, "DOBBELTHOPP ", "") & If(_s.SuperAvailable, "SUPER ", "") & If(_s.OverdriveAvailable, "OD", ""), 691, 109, 12, _muted, False)
    End Sub

    Private Sub DrawPortalBoss(g As Graphics)
        DrawBackdrop(g, True)
        For i = 0 To 4
            Circle(g, 1050, 365, 125 + i * 46 + CSng(Math.Sin(_elapsed * 3)) * 8, Color.FromArgb(If(i = 0, 30, 5), _pink), If(i Mod 2 = 0, _pink, _cyan), 3)
        Next
        Dim p = _s.Portal
        Circle(g, p.BossX, p.BossY, 106 + CSng(Math.Sin(p.BossTime * 5)) * 7, Color.FromArgb(65, _pink), _pink, 5)
        For i = 0 To 7
            Dim a = p.BossTime + i * CSng(Math.PI / 4)
            Polygon(g, If(i Mod 2 = 0, _pink, _cyan), New PointF(p.BossX + CSng(Math.Cos(a)) * 95, p.BossY + CSng(Math.Sin(a)) * 95), New PointF(p.BossX + CSng(Math.Cos(a + 0.21)) * 66, p.BossY + CSng(Math.Sin(a + 0.21)) * 66), New PointF(p.BossX + CSng(Math.Cos(a - 0.21)) * 66, p.BossY + CSng(Math.Sin(a - 0.21)) * 66))
        Next
        Circle(g, p.BossX, p.BossY, 57, Color.FromArgb(31, 24, 68), _white, 3)
        Circle(g, p.BossX, p.BossY, 36, _pink, _cyan, 4)
        Circle(g, p.BossX - 9, p.BossY - 8, 6, _white)
        Circle(g, p.BossX + 9, p.BossY - 8, 6, _white)
        DrawBullets(g)
        Dim walking = IsHeld(System.Windows.Forms.Keys.A, System.Windows.Forms.Keys.D, System.Windows.Forms.Keys.W, System.Windows.Forms.Keys.S, System.Windows.Forms.Keys.Left, System.Windows.Forms.Keys.Right, System.Windows.Forms.Keys.Up, System.Windows.Forms.Keys.Down)
        DrawHero(g, p.BossPlayerX, p.BossPlayerY, walking, True)
        DrawSparks(g)
        FillPanel(g, New RectangleF(160, 23, 968, 117), Color.FromArgb(235, 18, 13, 44), _pink, 18)
        CenterText(g, "PORTAL AMBUSH  /  THE SENTINEL", 640, 34, 29, _gold)
        Gauge(g, 193, 92, 900, 23, p.BossHp / Math.Max(1, p.BossMaxHp), _pink)
        FillPanel(g, New RectangleF(160, 604, 966, 92), Color.FromArgb(220, 17, 17, 49), _cyan, 18)
        TextAt(g, "PORTALSKJOLD: " & Math.Max(0, p.BossShield).ToString() & "/3", 188, 615, 23, _cyan)
        TextAt(g, "W A S D / PILTASTER = FLYTT DEG FRITT", 536, 616, 17, _white)
        TextAt(g, "MUS ELLER F = SKYT FRA VÅPENET    |    R = LAD    |    HP ER TRYGT!", 536, 649, 14, _muted, False)
    End Sub

    Private Sub DrawHighway(g As Graphics)
        Dim p = _s.Portal
        FillGradient(g, New RectangleF(0, 0, 1280, 720), Color.FromArgb(63, 20, 105), Color.FromArgb(10, 14, 42))
        For i = 0 To 11
            Dim x = CSng((i * 145 + _elapsed * 20) Mod 1480) - 90
            Rect(g, x, 122 + i * 8, 52, 150 + i * 17, Color.FromArgb(45, 175, 87, 233))
        Next
        Circle(g, 640, 159, 100, Color.FromArgb(52, _pink), _pink, 3)
        For i = 0 To 3
            Circle(g, 640, 159, 145 + i * 41, Color.FromArgb(8, _cyan), _cyan, 1)
        Next
        Polygon(g, Color.FromArgb(25, 28, 62), New PointF(590, 182), New PointF(690, 182), New PointF(1220, 720), New PointF(60, 720))
        Line(g, 590, 182, 60, 720, _cyan, 8)
        Line(g, 690, 182, 1220, 720, _pink, 8)
        For i = 0 To 14
            Dim depth = CSng((i * 108 + p.Distance * 0.85) Mod 1500) / 1500.0F
            Dim y = 182 + depth * depth * 560
            Dim halfwidth = 42 + depth * 522
            Dim laneLeft = 640 - halfwidth / 3.0F
            Dim laneRight = 640 + halfwidth / 3.0F
            Dim dashLength = 4 + depth * 23
            Line(g, laneLeft, y, laneLeft, y + dashLength, Color.FromArgb(115, _white), 3 + depth * 5)
            Line(g, laneRight, y, laneRight, y + dashLength, Color.FromArgb(115, _white), 3 + depth * 5)
        Next
        For Each ob In p.RoadObstacles
            If ob.Resolved Then Continue For
            Dim remaining = ob.AtDistance - p.Distance
            If remaining < 0 OrElse remaining > 900 Then Continue For
            Dim near = 1 - remaining / 900.0F
            Dim y = 205 + CSng(near * near) * 465
            Dim center = 640 + (ob.Lane - 1) * (34 + near * 350)
            Dim size = 13 + near * 86
            If ob.Kind = 4 Then
                Circle(g, center, y, size * 0.35F, _gold, _white, 2)
                TextAt(g, "+50", center - 13, y - 7, 12, _navy, True, 60)
            Else
                FillPanel(g, New RectangleF(center - size / 2, y - size * 0.6F, size, size * 1.2F), If(ob.Kind Mod 2 = 0, Color.FromArgb(250, 89, 141), Color.FromArgb(67, 203, 255)), Color.FromArgb(242, 237, 252), 6, 2)
                Rect(g, center - size * 0.35F, y - size * 0.32F, size * 0.7F, size * 0.36F, Color.FromArgb(24, 23, 52))
                Rect(g, center - size * 0.3F, y + size * 0.22F, size * 0.22F, size * 0.14F, _gold)
                Rect(g, center + size * 0.08F, y + size * 0.22F, size * 0.22F, size * 0.14F, _gold)
            End If
        Next
        DrawCar(g, 640 + (p.CarLane - 1) * 191, 565, IsHeld(System.Windows.Forms.Keys.Space, System.Windows.Forms.Keys.W, System.Windows.Forms.Keys.Up) AndAlso p.Nitro > 0)
        DrawSparks(g)
        DrawTravelHud(g, "NEON HIGHWAY", "3 KJØREFELT / NITRO / MYNTER")
        FillPanel(g, New RectangleF(390, 624, 500, 76), Color.FromArgb(220, 12, 17, 48), _gold, 17)
        Gauge(g, 412, 634, 455, 17, p.Nitro, _cyan)
        CenterText(g, "A/D = BYTT FELT    ·    HOLD SPACE = NITRO", 640, 661, 16, _white)
    End Sub

    Private Sub DrawCar(g As Graphics, x As Single, y As Single, nitro As Boolean)
        Dim bounce = CSng(Math.Sin(_elapsed * 15)) * 2
        Dim tint = If(_s.Character = HeroKind.Metal, _pink, _cyan)
        If nitro Then
            Polygon(g, _gold, New PointF(x - 30, y + 40), New PointF(x - 17, y + 99 + CSng(Math.Sin(_elapsed * 30)) * 13), New PointF(x - 4, y + 40))
            Polygon(g, _cyan, New PointF(x + 4, y + 40), New PointF(x + 17, y + 99 + CSng(Math.Cos(_elapsed * 26)) * 16), New PointF(x + 30, y + 40))
        End If
        Polygon(g, Color.FromArgb(29, 27, 58), New PointF(x - 60, y + 55 + bounce), New PointF(x - 48, y - 27 + bounce), New PointF(x - 25, y - 62 + bounce), New PointF(x + 25, y - 62 + bounce), New PointF(x + 48, y - 27 + bounce), New PointF(x + 60, y + 55 + bounce))
        Polygon(g, tint, New PointF(x - 60, y + 45 + bounce), New PointF(x - 45, y - 28 + bounce), New PointF(x - 24, y - 59 + bounce), New PointF(x + 24, y - 59 + bounce), New PointF(x + 45, y - 28 + bounce), New PointF(x + 60, y + 45 + bounce))
        Polygon(g, Color.FromArgb(22, 31, 59), New PointF(x - 24, y - 41 + bounce), New PointF(x + 24, y - 41 + bounce), New PointF(x + 34, y + 3 + bounce), New PointF(x - 34, y + 3 + bounce))
        Rect(g, x - 43, y + 31 + bounce, 28, 13, _pink)
        Rect(g, x + 15, y + 31 + bounce, 28, 13, _pink)
        Rect(g, x - 12, y - 14 + bounce, 24, 29, Color.FromArgb(250, 250, 252))
    End Sub

    Private Sub DrawShopOffer(g As Graphics)
        DimBackground(g)
        FillPanel(g, New RectangleF(251, 155, 778, 420), Color.FromArgb(247, 28, 23, 62), _gold, 27, 3)
        CenterText(g, "BØLGE " & _s.Wave.ToString() & " FULLFØRT!", 640, 191, 43, _gold)
        CenterText(g, "VIL DU KJØPE NYE VÅPEN?", 640, 268, 29, _white)
        CenterText(g, "Du har tjent mynter på monstrene du har beseiret.", 640, 335, 18, _muted, False)
        CenterText(g, "DIN SALDO: " & _s.Coins.ToString("N0") & " MYNTER", 640, 375, 31, _cyan)
        Button(g, 304, 466, 312, 69, "JA, ÅPNE BUTIKK", "shop", _gold, True, True, 21)
        Button(g, 633, 466, 345, 69, "NEI, START PORTAL", "shop_no", _cyan, False, True, 19)
    End Sub

    Private Sub DrawShop(g As Graphics)
        DimBackground(g, 237)
        FillPanel(g, New RectangleF(48, 37, 1184, 649), Color.FromArgb(249, 22, 21, 57), _cyan, 23)
        TextAt(g, "THE CHAOS ARMORY", 80, 55, 40, _gold)
        TextAt(g, "DIN SALDO: " & _s.Coins.ToString("N0") & " MYNTER", 828, 70, 24, _cyan)
        TextAt(g, "KJØP VÅPEN MED MYNTER FRA MONSTRENE. EIDE VÅPEN KAN UTSTYRES.", 83, 108, 16, _muted, False)
        For i = 0 To GameRules.Weapons.Length - 1
            Dim w = GameRules.Weapons(i)
            Dim col = i Mod 4, row = i \ 4
            Dim x = 81 + col * 285, y = 157 + row * 209
            Dim owned = _s.OwnedWeapons.Contains(i)
            Dim selected = _s.EquippedWeapon = i
            FillPanel(g, New RectangleF(x, y, 264, 190), If(selected, Color.FromArgb(67, 38, 90), Color.FromArgb(34, 34, 73)), If(selected, _gold, w.Tint), 13, If(selected, 3, 1.4F))
            Rect(g, x + 14, y + 13, 9, 71, w.Tint)
            TextAt(g, (i + 1).ToString() & "  " & w.Name, x + 31, y + 17, 16, _white, True, 224)
            TextAt(g, w.Info, x + 17, y + 55, 12, _muted, False, 233)
            TextAt(g, If(owned, "EID", w.Cost.ToString("N0") & " MYNTER"), x + 17, y + 88, 22, If(owned, _cyan, _gold))
            Button(g, x + 17, y + 129, 230, 45, If(selected, "UTSTYRT", If(owned, "UTSTYR", "KJØP VÅPEN")), "buy:" & i.ToString(), w.Tint, selected, owned OrElse _s.Coins >= w.Cost, 17)
        Next
        Button(g, 436, 603, 408, 58, If(_s.PreviousScene = SceneKind.ShopOffer, "FORTSETT TIL PORTALEN", "TILBAKE TIL KAMPEN"), "shop_done", _gold, True, True, 20)
    End Sub

    Private Sub DrawUpgrade(g As Graphics)
        DimBackground(g)
        FillPanel(g, New RectangleF(65, 143, 1150, 442), Color.FromArgb(246, 25, 23, 56), _gold, 26)
        CenterText(g, "LEVEL UP  /  VELG EN FORBEDRING", 640, 185, 35, _gold)
        Dim titles = {"1  SKADE +20%", "2  FART +25", "3  RUSTNING +17 HP", "4  SKYTEFART +8%", "5  FLERE MYNTER"}
        Dim actions = {"up_damage", "up_speed", "up_armor", "up_rate", "up_coins"}
        Dim details = {"Hardere treff", "Raskere bevegelse", "Healer deg samtidig", "Kortere intervall", "+16% belønning"}
        For i = 0 To 4
            Dim x = 95 + i * 217
            FillPanel(g, New RectangleF(x, 284, 201, 200), Color.FromArgb(49, 39, 86), If(i Mod 2 = 0, _cyan, _pink), 17)
            CenterText(g, titles(i), x + 101, 328, 16, _white, True, 186)
            CenterText(g, details(i), x + 101, 384, 14, _muted, False, 188)
            Button(g, x + 15, 422, 171, 45, "VELG", actions(i), _gold, False, True, 17)
        Next
    End Sub

    Private Sub DrawPortalFail(g As Graphics)
        DimBackground(g, 219)
        FillPanel(g, New RectangleF(234, 164, 812, 410), Color.FromArgb(245, 44, 17, 54), _pink, 27, 3)
        CenterText(g, "PORTAL KRASJ!", 640, 199, 46, _pink)
        CenterText(g, "0 HP MISTET  ·  1 EVNE MISTET", 640, 274, 23, _gold)
        CenterText(g, _s.FailReason, 640, 329, 17, _white, True, 740)
        CenterText(g, "Du kan prøve igjen så mange ganger du vil.", 640, 391, 18, _muted, False)
        Button(g, 335, 460, 610, 72, "PRØV PORTALEN IGJEN  (ENTER)", "retry", _cyan, True, True, 22)
    End Sub

    Private Sub DrawPause(g As Graphics)
        DimBackground(g, 212)
        FillPanel(g, New RectangleF(382, 142, 516, 455), Color.FromArgb(246, 25, 25, 59), _cyan, 25)
        CenterText(g, "PAUSE", 640, 187, 48, _white)
        CenterText(g, "F5 lagrer også under portaler og bosskamp.", 640, 264, 15, _muted, False)
        Button(g, 451, 314, 377, 56, "FORTSETT", "pause", _gold, True, True, 21)
        Button(g, 451, 383, 377, 56, "LAGRE SPILLET (F5)", "save", _cyan, False, True, 18)
        Button(g, 451, 452, 377, 56, "HOVEDMENY", "menu", _pink, False, True, 18)
    End Sub

    Private Sub DrawEnd(g As Graphics, victory As Boolean)
        DimBackground(g, 223)
        FillPanel(g, New RectangleF(270, 142, 740, 440), Color.FromArgb(245, 28, 22, 65), If(victory, _gold, _pink), 26, 3)
        CenterText(g, If(victory, "DU REDDET PORTALEN!", "GAME OVER!"), 640, 184, 44, If(victory, _gold, _pink))
        CenterText(g, "BØLGE " & _s.Wave.ToString() & "/10     |     " & _s.Kills.ToString() & " MONSTRE", 640, 284, 23, _white)
        CenterText(g, "SCORE: " & _s.Score.ToString("N0") & "    ·    MYNTER: " & _s.TotalCoins.ToString("N0"), 640, 335, 22, _cyan)
        CenterText(g, "Du kan starte på nytt eller laste inn en lagring.", 640, 390, 17, _muted, False)
        Button(g, 310, 464, 314, 65, "SPILL IGJEN", "restart", _gold, True, True, 22)
        Button(g, 643, 464, 324, 65, "HOVEDMENY", "menu", _cyan, False, True, 20)
    End Sub
End Class
