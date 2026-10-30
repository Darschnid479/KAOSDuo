Imports System
Imports System.Collections.Generic
Imports System.Diagnostics
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.IO
Imports System.Linq
Imports System.Text.Json
Imports System.Windows.Forms

Public Class GameForm
    Inherits Form
    Private Const GW As Single = 1280.0F
    Private Const GH As Single = 720.0F
    Private ReadOnly loopTimer As New System.Windows.Forms.Timer() With {.Interval = 16}
    Private ReadOnly rand As New Random()
    Private ReadOnly clock As New Stopwatch()
    Private ReadOnly music As New AudioEngine()
    Private ReadOnly heldKeys As New HashSet(Of Keys)()
    Private ReadOnly foes As New List(Of Actor)()
    Private ReadOnly shots As New List(Of Shot)()
    Private ReadOnly sparks As New List(Of Spark)()
    Private ReadOnly hazards As New List(Of Hazard)()
    Private ReadOnly weapons As String() = {"PULSE BLASTER", "CHAOS SHOTGUN", "RAIL CANNON", "CHAIN ARC", "NOVA ROCKETS", "GLACIER REPEATER", "METEOR MINIGUN", "PRISM ANNIHILATOR"}
    Private ReadOnly prices As Integer() = {0, 260, 680, 780, 990, 1160, 1680, 2350}
    Private ReadOnly dmg As Single() = {15.0F, 9.0F, 70.0F, 24.0F, 50.0F, 14.0F, 8.0F, 90.0F}
    Private ReadOnly fireDelay As Single() = {0.2F, 0.5F, 0.95F, 0.3F, 0.9F, 0.1F, 0.055F, 1.25F}
    Private player As New Actor With {.X = 180, .Y = 500, .HP = 110, .Radius = 25}
    Private blueImg As Image
    Private metalImg As Image
    Private state As String = "menu"
    Private afterShop As String = "combat"
    Private chosen As String = "blue"
    Private difficulty As Integer = 1
    Private wave As Integer = 1
    Private coins As Integer = 0
    Private score As Integer = 0
    Private weaponIndex As Integer = 0
    Private unlock As Boolean() = {True, False, False, False, False, False, False, False}
    Private allowDash As Boolean = True
    Private allowDouble As Boolean = True
    Private allowSuper As Boolean = True
    Private allowOverdrive As Boolean = True
    Private portalFailures As Integer = 0
    Private killsRequired As Integer
    Private killsSpawned As Integer
    Private spawnTimer As Single
    Private transitionTime As Single
    Private phaseTime As Single
    Private globalTime As Single
    Private fxShake As Single
    Private notification As String = ""
    Private notificationTimer As Single
    Private mouseWorld As New PointF(810, 420)
    Private firing As Boolean
    Private menuCharacter As Integer
    Private menuDifficulty As Integer = 1
    Private facing As Integer = 1
    Private jumps As Integer
    Private maxHealth As Single = 110
    Private damageCooldown As Single
    Private specialCooldown As Single
    Private portalDistance As Single
    Private portalLength As Single
    Private portalScroll As Single
    Private portalCubeY As Single
    Private portalVelocity As Single
    Private portalMode As Integer
    Private portalCarLane As Integer = 1
    Private portalCourse As Integer
    Private portalHasBoss As Boolean
    Private portalBossTriggered As Boolean
    Private portalBoss As Actor
    Private aimAngle As Single
    Private volumeHint As String = ""
    Private ReadOnly savePath As String = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "KAOSDUO", "vbnext_save.json")

    Public Sub New()
        Me.Text = "KAOSDUO: VB NEXT | Native VB.NET Game Engine Preview"
        Me.ClientSize = New Size(1280, 720)
        Me.MinimumSize = New Size(960, 540)
        Me.BackColor = Color.FromArgb(11, 13, 29)
        Me.DoubleBuffered = True
        Me.KeyPreview = True
        Me.StartPosition = FormStartPosition.CenterScreen
        Try
            blueImg = Image.FromFile(Path.Combine(AppContext.BaseDirectory, "Assets", "blue.png"))
            metalImg = Image.FromFile(Path.Combine(AppContext.BaseDirectory, "Assets", "metal.png"))
        Catch
            blueImg = Nothing : metalImg = Nothing
        End Try
        AddHandler loopTimer.Tick, AddressOf TickGame
        AddHandler Me.KeyDown, AddressOf KeyPressed
        AddHandler Me.KeyUp, Sub(s, e) heldKeys.Remove(e.KeyCode)
        AddHandler Me.MouseDown, AddressOf MousePressed
        AddHandler Me.MouseUp, Sub(s, e) firing = False
        AddHandler Me.MouseMove, Sub(s, e) mouseWorld = New PointF(e.X * GW / Math.Max(1, ClientSize.Width), e.Y * GH / Math.Max(1, ClientSize.Height))
        AddHandler Me.FormClosed, Sub(s, e) music.StopMusic()
        clock.Start()
        loopTimer.Start()
    End Sub

    Private Sub TickGame(sender As Object, e As EventArgs)
        Dim dt As Single = CSng(Math.Min(0.04, clock.Elapsed.TotalSeconds))
        clock.Restart()
        globalTime += dt
        notificationTimer = Math.Max(0, notificationTimer - dt)
        fxShake = Math.Max(0, fxShake - dt * 10)
        If state = "combat" Then StepCombat(dt)
        If state = "portal" Then StepPortal(dt)
        If state = "ambush" Then StepAmbush(dt)
        If state = "road" Then StepRoad(dt)
        For i = sparks.Count - 1 To 0 Step -1
            Dim p = sparks(i)
            p.X += p.VX * dt : p.Y += p.VY * dt : p.Life -= dt
            If p.Life <= 0 Then sparks.RemoveAt(i)
        Next
        Me.Invalidate()
    End Sub

    Private Sub Notice(message As String, Optional seconds As Single = 2.5F)
        notification = message : notificationTimer = seconds
    End Sub
    Private Function Held(k As Keys) As Boolean
        Return heldKeys.Contains(k)
    End Function
    Private Sub KeyPressed(sender As Object, e As KeyEventArgs)
        Dim first = heldKeys.Add(e.KeyCode)
        If Not first Then Return
        If e.KeyCode = Keys.M Then music.Toggle(chosen)
        If state = "menu" Then
            If e.KeyCode = Keys.A OrElse e.KeyCode = Keys.Left Then menuCharacter = 0
            If e.KeyCode = Keys.D OrElse e.KeyCode = Keys.Right Then menuCharacter = 1
            If e.KeyCode >= Keys.D1 AndAlso e.KeyCode <= Keys.D4 Then menuDifficulty = CInt(e.KeyCode) - CInt(Keys.D1)
            If e.KeyCode = Keys.Enter Then BeginGame()
            If e.KeyCode = Keys.F9 Then LoadGame()
            Return
        End If
        If e.KeyCode = Keys.Escape Then
            If state = "pause" Then
                state = afterShop
            ElseIf state <> "gameover" AndAlso state <> "menu" Then
                afterShop = state : state = "pause"
            End If
            Return
        End If
        If e.KeyCode = Keys.F5 Then SaveGame()
        If e.KeyCode = Keys.F9 Then LoadGame()
        If state = "pause" Then Return
        If state = "shop" Then
            If e.KeyCode = Keys.Enter Then
                If afterShop = "travelpending" Then BeginTravel() Else state = afterShop
            End If
            If e.KeyCode >= Keys.D1 AndAlso e.KeyCode <= Keys.D8 Then Buy(CInt(e.KeyCode) - CInt(Keys.D1))
            Return
        End If
        If e.KeyCode = Keys.B AndAlso state = "combat" Then afterShop = "combat" : state = "shop" : Return
        If state = "gameover" AndAlso e.KeyCode = Keys.Enter Then state = "menu" : Return
        If state = "combat" OrElse state = "ambush" Then
            If e.KeyCode >= Keys.D1 AndAlso e.KeyCode <= Keys.D8 Then SelectGun(CInt(e.KeyCode) - CInt(Keys.D1))
            If e.KeyCode = Keys.Q Then CycleGun(-1)
            If e.KeyCode = Keys.E Then CycleGun(1)
            If e.KeyCode = Keys.ShiftKey AndAlso allowDash AndAlso specialCooldown <= 0 Then
                player.X = Math.Clamp(player.X + facing * 105.0F, 45.0F, 1220.0F)
                specialCooldown = 3 : Burst(player.X, player.Y, 24, 3)
            End If
            If e.KeyCode = Keys.Space AndAlso state = "combat" AndAlso jumps < If(allowDouble, 2, 1) Then
                player.VY = -540 : jumps += 1 : Burst(player.X, player.Y + 45, 8, 1)
            End If
        ElseIf state = "portal" Then
            If portalMode = 0 AndAlso (e.KeyCode = Keys.Space OrElse e.KeyCode = Keys.W) AndAlso portalCubeY >= 527 Then portalVelocity = -570
            If portalMode = 2 AndAlso (e.KeyCode = Keys.Space OrElse e.KeyCode = Keys.W) Then portalVelocity = -portalVelocity - 260
        ElseIf state = "road" Then
            If e.KeyCode = Keys.A OrElse e.KeyCode = Keys.Left Then portalCarLane = Math.Max(0, portalCarLane - 1)
            If e.KeyCode = Keys.D OrElse e.KeyCode = Keys.Right Then portalCarLane = Math.Min(2, portalCarLane + 1)
        End If
    End Sub
    Private Sub MousePressed(sender As Object, e As MouseEventArgs)
        mouseWorld = New PointF(e.X * GW / Math.Max(1, ClientSize.Width), e.Y * GH / Math.Max(1, ClientSize.Height))
        firing = e.Button = MouseButtons.Left
        If state = "menu" AndAlso e.Button = MouseButtons.Left Then BeginGame()
    End Sub
    Private Sub BeginGame()
        chosen = If(menuCharacter = 0, "blue", "metal")
        difficulty = menuDifficulty
        wave = 1 : score = 0 : coins = 0 : weaponIndex = 0
        unlock = {True, False, False, False, False, False, False, False}
        allowDash = True : allowDouble = True : allowSuper = True : allowOverdrive = True : portalFailures = 0
        maxHealth = CSng({150, 110, 90, 75}(difficulty))
        player = New Actor With {.X = 180, .Y = 480, .HP = maxHealth, .Radius = 25}
        music.PlayCharacterMusic(chosen)
        StartWave()
    End Sub
    Private Sub StartWave()
        state = "combat" : phaseTime = 0
        foes.Clear() : shots.Clear() : hazards.Clear()
        player.X = 180 : player.Y = 480 : player.VY = 0
        killsRequired = 5 + wave * 3 + difficulty * 2
        killsSpawned = 0 : spawnTimer = 0
        Notice($"WAVE {wave}/10  |  KJEMP DEG GJENNOM!")
        SaveGame(True)
    End Sub
    Private Sub SelectGun(id As Integer)
        If id >= 0 AndAlso id < 8 AndAlso unlock(id) Then
            weaponIndex = id : Notice("VÅPEN: " & weapons(id), 1)
        Else
            Notice("LÅST VÅPEN – KJØP I BUTIKKEN (B)", 1.8F)
        End If
    End Sub
    Private Sub CycleGun(dir As Integer)
        For i = 1 To 8
            Dim idx = (weaponIndex + i * dir + 80) Mod 8
            If unlock(idx) Then weaponIndex = idx : Exit For
        Next
    End Sub
    Private Sub Buy(idx As Integer)
        If idx < 0 OrElse idx >= 8 Then Return
        If unlock(idx) Then SelectGun(idx) : Return
        If coins < prices(idx) Then Notice("DU TRENGER FLERE MYNTER", 2) : Return
        coins -= prices(idx) : unlock(idx) = True : weaponIndex = idx
        Notice("KJØPT! " & weapons(idx), 2)
        SaveGame(True)
    End Sub
    Private Sub SpawnEnemy(Optional boss As Boolean = False)
        Dim multiplier = 1.0F + difficulty * 0.23F
        Dim e As New Actor With {
            .X = 1050 + CSng(rand.Next(175)),
            .Y = 535,
            .HP = If(boss, 490.0F + wave * 80, 35.0F + wave * 7) * multiplier,
            .Radius = If(boss, 67, 24),
            .Kind = rand.Next(3),
            .IsBoss = boss,
            .Cooldown = 1.2F
        }
        foes.Add(e)
    End Sub
    Private Sub StepCombat(dt As Single)
        phaseTime += dt
        damageCooldown = Math.Max(0, damageCooldown - dt)
        specialCooldown = Math.Max(0, specialCooldown - dt)
        player.Cooldown = Math.Max(0, player.Cooldown - dt)
        player.AttackTime = Math.Max(0, player.AttackTime - dt)
        Dim movement = CSng(If(Held(Keys.A) OrElse Held(Keys.Left), -1, 0) + If(Held(Keys.D) OrElse Held(Keys.Right), 1, 0))
        player.VX = movement * 340
        If movement <> 0 Then facing = Math.Sign(movement)
        player.X = Math.Clamp(player.X + player.VX * dt, 42.0F, 1225.0F)
        player.VY += 1400 * dt : player.Y += player.VY * dt
        If player.Y >= 535 Then player.Y = 535 : player.VY = 0 : jumps = 0
        If Held(Keys.W) AndAlso jumps = 0 AndAlso player.Y >= 534 Then player.VY = -540 : jumps = 1
        aimAngle = CSng(Math.Atan2(mouseWorld.Y - (player.Y - 95), mouseWorld.X - player.X))
        If firing OrElse Held(Keys.J) Then FireWeapon()
        If killsSpawned < killsRequired Then
            spawnTimer -= dt
            If spawnTimer <= 0 Then SpawnEnemy() : killsSpawned += 1 : spawnTimer = Math.Max(0.34F, 1.16F - wave * 0.055F - difficulty * 0.09F)
        End If
        UpdateEnemies(dt, False)
        UpdateShots(dt)
        If player.HP <= 0 Then state = "gameover" : music.StopMusic() : Return
        If killsSpawned >= killsRequired AndAlso foes.Count = 0 Then
            transitionTime += dt
            If transitionTime > 1.4F Then
                transitionTime = 0
                If wave >= 10 Then state = "victory" : music.StopMusic() : Return
                afterShop = "travelpending" : state = "shop" : Notice("KAMPEN ER OVER! KJØP VÅPEN ELLER TRYKK ENTER", 3)
                SaveGame(True)
            End If
        End If
    End Sub
    Private Sub FireWeapon()
        If player.Cooldown > 0 Then Return
        player.Cooldown = fireDelay(weaponIndex)
        player.AttackTime = 0.18F
        Dim angle = aimAngle
        Dim nx = CSng(Math.Cos(angle)), ny = CSng(Math.Sin(angle))
        ' The muzzle is projected 56 pixels from the actual hand-held weapon.
        Dim muzzleX = player.X + nx * 56
        Dim muzzleY = player.Y - 95 + ny * 56
        Dim number = If(weaponIndex = 1, 6, If(weaponIndex = 7, 3, 1))
        For i = 0 To number - 1
            Dim spread = If(number = 1, 0.0F, (i - (number - 1) / 2.0F) * If(weaponIndex = 1, 0.13F, 0.055F))
            Dim speed = CSng(If(weaponIndex = 2, 1000, 760))
            Dim a = angle + spread
            shots.Add(New Shot With {.X = muzzleX, .Y = muzzleY, .VX = CSng(Math.Cos(a)) * speed, .VY = CSng(Math.Sin(a)) * speed, .Life = 1.6F, .Damage = dmg(weaponIndex), .Friendly = True, .Pierce = If(weaponIndex = 2 OrElse weaponIndex = 7, 5, 0), .ColorId = weaponIndex})
        Next
        Burst(muzzleX, muzzleY, 7, weaponIndex)
        fxShake = Math.Max(fxShake, If(weaponIndex = 4 OrElse weaponIndex = 7, 5.0F, 1.8F))
    End Sub
    Private Sub UpdateEnemies(dt As Single, portalFight As Boolean)
        For i = foes.Count - 1 To 0 Step -1
            Dim e = foes(i)
            e.AttackTime = Math.Max(0, e.AttackTime - dt)
            Dim dx = player.X - e.X
            Dim dy = player.Y - e.Y
            Dim speed = CSng(If(e.IsBoss, 57, 80 + wave * 8 + difficulty * 15))
            If portalFight AndAlso e.IsBoss Then
                e.X = 995 + CSng(Math.Sin(globalTime * 1.35) * 75)
                e.Y = 275 + CSng(Math.Sin(globalTime * 2.1) * 80)
            Else
                e.X += Math.Sign(dx) * Math.Min(Math.Abs(dx), speed * dt)
                If e.Kind = 2 Then e.Y = 465 + CSng(Math.Sin(globalTime * 3 + i) * 43)
            End If
            e.Cooldown -= dt
            If e.Cooldown <= 0 Then
                e.Cooldown = If(e.IsBoss, 0.78F, 2.35F)
                If e.IsBoss OrElse e.Kind = 2 Then
                    Dim a = CSng(Math.Atan2(player.Y - e.Y, player.X - e.X))
                    Dim speedShot = 310.0F + difficulty * 38
                    shots.Add(New Shot With {.X = e.X, .Y = e.Y, .VX = CSng(Math.Cos(a)) * speedShot, .VY = CSng(Math.Sin(a)) * speedShot, .Life = 3.4F, .Friendly = False, .Damage = If(portalFight, 0, If(e.IsBoss, 15, 9)), .ColorId = 9})
                End If
            End If
            If Not portalFight AndAlso Math.Abs(dx) < e.Radius + 18 AndAlso Math.Abs(dy) < 85 Then TakeDamage(If(e.IsBoss, 20, 9))
            If e.HP <= 0 Then
                Burst(e.X, e.Y, If(e.IsBoss, 70, 18), If(e.IsBoss, 4, 2))
                Dim reward = If(e.IsBoss, 380, 15 + wave * 3)
                coins += reward : score += reward * 10
                foes.RemoveAt(i)
            End If
        Next
    End Sub
    Private Sub TakeDamage(amount As Single)
        If damageCooldown > 0 Then Return
        player.HP -= amount : damageCooldown = 1.05F : fxShake = 8
        Burst(player.X, player.Y - 75, 15, 9)
    End Sub
    Private Sub UpdateShots(dt As Single)
        For i = shots.Count - 1 To 0 Step -1
            Dim s = shots(i)
            s.X += s.VX * dt : s.Y += s.VY * dt : s.Life -= dt
            If s.Life <= 0 OrElse s.X < -30 OrElse s.X > 1320 OrElse s.Y < -30 OrElse s.Y > 800 Then shots.RemoveAt(i) : Continue For
            If s.Friendly Then
                For Each e In foes
                    Dim dx = e.X - s.X, dy = e.Y - s.Y
                    If dx * dx + dy * dy < (e.Radius + 8) * (e.Radius + 8) Then
                        e.HP -= s.Damage : e.AttackTime = 0.14F
                        Burst(s.X, s.Y, 5, s.ColorId)
                        If s.Pierce <= 0 Then s.Life = 0 Else s.Pierce -= 1
                        Exit For
                    End If
                Next
            Else
                If (player.X - s.X) ^ 2 + (player.Y - 75 - s.Y) ^ 2 < 1050 Then
                    If state = "ambush" Then
                        ' Portal crashes cost an ability, never hit points.
                        FailPortal() : Return
                    Else
                        TakeDamage(s.Damage)
                    End If
                    s.Life = 0
                End If
            End If
        Next
    End Sub
    Private Sub BeginTravel()
        hazards.Clear() : shots.Clear() : foes.Clear()
        portalDistance = 0 : phaseTime = 0
        portalCourse = wave
        portalHasBoss = rand.NextDouble() < 0.25
        portalBossTriggered = False
        portalMode = rand.Next(3)
        If rand.NextDouble() < 0.5 Then
            state = "road" : portalLength = 2700 + wave * 180 : portalCarLane = 1
            For i = 1 To 14 + wave
                hazards.Add(New Hazard With {.X = 300 + i * 180 + rand.Next(110), .Lane = rand.Next(3), .Kind = If(rand.NextDouble() < .22, 1, 0)})
            Next
            Notice("NEON HIGHWAY!  A / D = FILSKIFT · SHIFT = NITRO", 3)
        Else
            state = "portal" : portalLength = 2900 + wave * 175
            portalCubeY = 527 : portalVelocity = 0
            For i = 1 To 12 + wave
                hazards.Add(New Hazard With {.X = 320 + i * 210 + rand.Next(60), .Lane = 0, .Kind = rand.Next(3), .Y = CSng(If(i Mod 3 = 0, 375, 541))})
            Next
            Notice("PORTAL RUN!  HOPP / FLY / GRAVITY FLIP", 3)
        End If
        SaveGame(True)
    End Sub
    Private Sub StepPortal(dt As Single)
        phaseTime += dt
        portalDistance += dt * (380 + wave * 14)
        If portalDistance > portalLength Then FinishTravel() : Return
        If portalHasBoss AndAlso Not portalBossTriggered AndAlso portalDistance > portalLength * 0.53F Then StartAmbush() : Return
        If portalDistance > portalLength * 0.33F AndAlso portalDistance < portalLength * 0.36F Then portalMode = 1
        If portalDistance > portalLength * 0.66F Then portalMode = 2
        Dim pressed = Held(Keys.W) OrElse Held(Keys.Space)
        If portalMode = 1 Then
            portalVelocity += If(pressed, -1370.0F, 940.0F) * dt
        Else
            portalVelocity += If(portalMode = 2, -980.0F, 1210.0F) * dt
        End If
        portalVelocity = Math.Clamp(portalVelocity, -720.0F, 720.0F)
        portalCubeY += portalVelocity * dt
        If portalCubeY > 527 Then portalCubeY = 527 : portalVelocity = 0
        If portalCubeY < 120 Then portalCubeY = 120 : portalVelocity = 0
        For Each h In hazards
            Dim sx = h.X - portalDistance + 270
            If Math.Abs(sx - 270) < 34 AndAlso Math.Abs(portalCubeY - h.Y) < 53 AndAlso Not h.Collected Then FailPortal() : Return
        Next
    End Sub
    Private Sub StepRoad(dt As Single)
        phaseTime += dt
        portalDistance += dt * If(Held(Keys.ShiftKey), 610, 400)
        If portalDistance > portalLength Then FinishTravel() : Return
        For Each h In hazards
            If h.Collected Then Continue For
            Dim delta = h.X - portalDistance
            If delta < -22 Then
                h.Collected = True
            ElseIf delta < 27 AndAlso h.Lane = portalCarLane Then
                If h.Kind = 1 Then coins += 28 : score += 150 : h.Collected = True : Notice("+28 MYNTER!", 1) Else FailPortal() : Return
            End If
        Next
    End Sub
    Private Sub StartAmbush()
        portalBossTriggered = True
        state = "ambush"
        shots.Clear() : foes.Clear()
        player.X = 170 : player.Y = 350
        portalBoss = New Actor With {.X = 980, .Y = 340, .HP = 380 + wave * 65 + difficulty * 80, .Radius = 75, .IsBoss = True, .Cooldown = 1}
        foes.Add(portalBoss)
        Notice("⚠ PORTAL SENTINEL! BEVEG DEG MED WASD, SIKT OG SKYT!", 4)
    End Sub
    Private Sub StepAmbush(dt As Single)
        phaseTime += dt
        damageCooldown = Math.Max(0, damageCooldown - dt)
        player.Cooldown = Math.Max(0, player.Cooldown - dt)
        player.AttackTime = Math.Max(0, player.AttackTime - dt)
        Dim mx = CSng(If(Held(Keys.A) OrElse Held(Keys.Left), -1, 0) + If(Held(Keys.D) OrElse Held(Keys.Right), 1, 0))
        Dim my = CSng(If(Held(Keys.W) OrElse Held(Keys.Up), -1, 0) + If(Held(Keys.S) OrElse Held(Keys.Down), 1, 0))
        If mx <> 0 AndAlso my <> 0 Then mx *= 0.7071F : my *= 0.7071F
        player.VX = mx * 330 : player.VY = my * 330
        player.X = Math.Clamp(player.X + mx * 330 * dt, 60.0F, 780.0F)
        player.Y = Math.Clamp(player.Y + my * 330 * dt, 145.0F, 585.0F)
        If mx <> 0 Then facing = Math.Sign(mx)
        aimAngle = CSng(Math.Atan2(mouseWorld.Y - (player.Y - 95), mouseWorld.X - player.X))
        If firing OrElse Held(Keys.J) Then FireWeapon()
        UpdateEnemies(dt, True)
        UpdateShots(dt)
        If state <> "ambush" Then Return
        If foes.Count = 0 Then
            state = "portal" : player.Y = 480 : portalCubeY = 527 : portalVelocity = 0
            shots.Clear() : coins += 180 : score += 900
            Notice("BOSS NEDKJEMPET! PORTALEN FORTSETTER! +180 MYNTER", 4)
            SaveGame(True)
        End If
    End Sub
    Private Sub FailPortal()
        portalFailures += 1
        Dim abilities As New List(Of Integer)
        If allowDash Then abilities.Add(0)
        If allowDouble Then abilities.Add(1)
        If allowSuper Then abilities.Add(2)
        If allowOverdrive Then abilities.Add(3)
        Dim lost As String = "INGEN FLERE EVNER"
        If abilities.Count > 0 Then
            Select Case abilities(rand.Next(abilities.Count))
                Case 0 : allowDash = False : lost = "DASH"
                Case 1 : allowDouble = False : lost = "DOBBELTHOPP"
                Case 2 : allowSuper = False : lost = "SUPERANGREP"
                Case 3 : allowOverdrive = False : lost = "OVERDRIVE"
            End Select
        End If
        ' Explicitly preserve HP. Retry the same route with the penalty.
        Dim hpBefore = player.HP
        If state = "ambush" Then
            foes.Clear() : shots.Clear()
            portalBossTriggered = False
        End If
        portalDistance = 0 : phaseTime = 0
        portalCubeY = 527 : portalVelocity = 0 : portalCarLane = 1
        If state <> "road" Then state = "portal"
        player.HP = hpBefore
        Notice("PORTALKRASJ! MISTET " & lost & " · HP ER UENDRET", 4)
        SaveGame(True)
    End Sub
    Private Sub FinishTravel()
        coins += 100 + 18 * wave
        wave += 1
        StartWave()
    End Sub
    Private Sub Burst(x As Single, y As Single, count As Integer, colorId As Integer)
        For i = 1 To count
            Dim a = CSng(rand.NextDouble() * Math.PI * 2)
            Dim speed = CSng(60 + rand.NextDouble() * 210)
            Dim lifetime = CSng(0.2 + rand.NextDouble() * 0.45)
            sparks.Add(New Spark With {.X = x, .Y = y, .VX = CSng(Math.Cos(a)) * speed, .VY = CSng(Math.Sin(a)) * speed, .Life = lifetime, .MaxLife = lifetime, .ColorId = colorId})
        Next
        If sparks.Count > 500 Then sparks.RemoveRange(0, sparks.Count - 500)
    End Sub
    Private Sub SaveGame(Optional quiet As Boolean = False)
        Try
            Directory.CreateDirectory(Path.GetDirectoryName(savePath))
            Dim data As New SaveData With {
                .Character = chosen, .Difficulty = difficulty, .Wave = wave,
                .HP = player.HP, .Coins = coins, .Score = score,
                .Weapon = weaponIndex, .Unlocked = CType(unlock.Clone(), Boolean()),
                .AbilityDash = allowDash, .AbilityDouble = allowDouble,
                .AbilitySuper = allowSuper, .AbilityOverdrive = allowOverdrive,
                .PortalFailures = portalFailures
            }
            File.WriteAllText(savePath, JsonSerializer.Serialize(data, New JsonSerializerOptions With {.WriteIndented = True}))
            If Not quiet Then Notice("SPILLET ER LAGRET!", 2)
        Catch ex As Exception
            Notice("LAGRINGSFEIL: " & ex.Message, 4)
        End Try
    End Sub
    Private Sub LoadGame()
        Try
            If Not File.Exists(savePath) Then Notice("INGEN LAGRING FUNNET", 2) : Return
            Dim data = JsonSerializer.Deserialize(Of SaveData)(File.ReadAllText(savePath))
            If data Is Nothing OrElse data.Schema <> 1 Then Notice("UGYLDIG LAGRING", 3) : Return
            chosen = If(data.Character = "metal", "metal", "blue")
            difficulty = Math.Clamp(data.Difficulty, 0, 3)
            wave = Math.Clamp(data.Wave, 1, 10)
            maxHealth = CSng({150, 110, 90, 75}(difficulty))
            player = New Actor With {.X = 180, .Y = 535, .HP = Math.Clamp(data.HP, 1.0F, maxHealth), .Radius = 25}
            coins = Math.Max(0, data.Coins) : score = Math.Max(0, data.Score)
            If data.Unlocked IsNot Nothing AndAlso data.Unlocked.Length = 8 Then unlock = CType(data.Unlocked.Clone(), Boolean()) Else unlock = {True, False, False, False, False, False, False, False}
            unlock(0) = True
            weaponIndex = Math.Clamp(data.Weapon, 0, 7)
            If Not unlock(weaponIndex) Then weaponIndex = 0
            allowDash = data.AbilityDash : allowDouble = data.AbilityDouble : allowSuper = data.AbilitySuper : allowOverdrive = data.AbilityOverdrive
            portalFailures = data.PortalFailures
            music.PlayCharacterMusic(chosen)
            StartWave()
            Notice("LAGRING LASTET! BØLGE " & wave, 3)
        Catch ex As Exception
            Notice("LASTEFEIL: " & ex.Message, 4)
        End Try
    End Sub
    Protected Overrides Sub OnPaint(e As PaintEventArgs)
        MyBase.OnPaint(e)
        Dim g = e.Graphics
        g.SmoothingMode = SmoothingMode.AntiAlias
        g.InterpolationMode = InterpolationMode.NearestNeighbor
        g.ScaleTransform(ClientSize.Width / GW, ClientSize.Height / GH)
        If state = "menu" Then
            DrawMenu(g)
            Return
        End If
        If state = "road" Then
            DrawRoad(g)
        ElseIf state = "portal" Then
            DrawPortal(g)
        Else
            DrawArena(g)
            For Each eActor In foes
                DrawEnemy(g, eActor)
            Next
            For Each shot In shots
                Dim c = If(shot.Friendly, ShotColor(shot.ColorId), Color.FromArgb(255, 74, 129))
                Using b As New SolidBrush(c)
                    g.FillEllipse(b, shot.X - 7, shot.Y - 7, 14, 14)
                End Using
            Next
            DrawPlayer(g)
        End If
        For Each p In sparks
            Using b As New SolidBrush(Color.FromArgb(CInt(230 * Math.Clamp(p.Life / p.MaxLife, 0.0F, 1.0F)), ShotColor(p.ColorId)))
                g.FillRectangle(b, p.X - 3, p.Y - 3, 6, 6)
            End Using
        Next
        DrawHud(g)
        If state = "shop" Then DrawShop(g)
        If state = "pause" Then DrawOverlay(g, "PAUSE", "ESC FOR Å FORTSETTE | F5 LAGRE | F9 LASTE")
        If state = "gameover" Then DrawOverlay(g, "GAME OVER", "ENTER FOR HOVEDMENY")
        If state = "victory" Then DrawOverlay(g, "DU VANT!", "ALLE 10 BØLGENE FULLFØRT")
        If notificationTimer > 0 Then
            DrawCentered(g, notification, 690, 19, Color.FromArgb(255, 235, 125))
        End If
    End Sub
    Private Function ShotColor(id As Integer) As Color
        Select Case id
            Case 0 : Return Color.Cyan
            Case 1 : Return Color.DeepPink
            Case 2 : Return Color.LightYellow
            Case 3 : Return Color.DeepSkyBlue
            Case 4 : Return Color.OrangeRed
            Case 5 : Return Color.LightBlue
            Case 6 : Return Color.Gold
            Case 7 : Return Color.Violet
            Case Else : Return Color.Magenta
        End Select
    End Function
    Private Sub DrawText(g As Graphics, message As String, x As Single, y As Single, size As Single, color As Color, Optional bold As Boolean = True)
        Using f As New Font("Segoe UI", size, If(bold, FontStyle.Bold, FontStyle.Regular), GraphicsUnit.Pixel)
            Using b As New SolidBrush(color)
                g.DrawString(message, f, b, x, y)
            End Using
        End Using
    End Sub
    Private Sub DrawCentered(g As Graphics, message As String, y As Single, size As Single, color As Color)
        Using f As New Font("Segoe UI", size, FontStyle.Bold, GraphicsUnit.Pixel)
            Dim measure = g.MeasureString(message, f)
            Using b As New SolidBrush(color)
                g.DrawString(message, f, b, (GW - measure.Width) / 2, y - measure.Height / 2)
            End Using
        End Using
    End Sub
    Private Sub DrawMenu(g As Graphics)
        g.Clear(Color.FromArgb(10, 13, 32))
        For i = 0 To 37
            Dim x = CSng((i * 113 + Math.Sin(globalTime * 0.9 + i) * 35) Mod GW)
            Dim y = CSng(95 + ((i * 73) Mod 520))
            Using b As New SolidBrush(Color.FromArgb(90, ShotColor(i Mod 8)))
                g.FillRectangle(b, x, y, 6, 6)
            End Using
        Next
        DrawCentered(g, "KAOSDUO: VB NEXT", 83, 56, Color.White)
        DrawCentered(g, "NATIVE VISUAL BASIC .NET ENGINE • SOLO EDITION", 130, 21, Color.Cyan)
        For i = 0 To 1
            Dim card = New Rectangle(230 + i * 440, 194, 380, 370)
            Using fill As New SolidBrush(Color.FromArgb(35, 43, 78))
                g.FillRectangle(fill, card)
            End Using
            Using pen As New Pen(If(i = menuCharacter, Color.FromArgb(95, 239, 255), Color.FromArgb(100, 103, 147)), If(i = menuCharacter, 5, 2))
                g.DrawRectangle(pen, card)
            End Using
            Dim image = If(i = 0, blueImg, metalImg)
            If image IsNot Nothing Then g.DrawImage(image, 355 + i * 440, 213, 132, 300)
            DrawText(g, If(i = 0, "BLÅ", "METAL"), 368 + i * 440, 520, 25, Color.White)
        Next
        DrawCentered(g, "A / D VELG KARAKTER   •   1–4 VANSKELIGHET", 598, 19, Color.LightGray)
        DrawCentered(g, "VALGT: " & {"LETT", "MEDIUM", "VANSKELIG", "DRIT VANSKELIG"}(menuDifficulty), 629, 21, Color.Gold)
        DrawCentered(g, "ENTER / KLIKK START • F9 LOAD • M MUSIKK", 669, 20, Color.Cyan)
    End Sub
    Private Sub DrawArena(g As Graphics)
        Using back As New LinearGradientBrush(New Rectangle(0, 0, 1280, 720), Color.FromArgb(13, 17, 42), Color.FromArgb(65, 28, 94), 90.0F)
            g.FillRectangle(back, 0, 0, GW, GH)
        End Using
        For i = 0 To 13
            Dim y = 132 + i * 32
            Using p As New Pen(Color.FromArgb(24, 111, 230, 255), 2)
                g.DrawLine(p, 0, y, 1280, y)
            End Using
        Next
        For i = 0 To 16
            Dim px = ((i * 103 + globalTime * 15) Mod 1350) - 40
            Using b As New SolidBrush(Color.FromArgb(30, 60 + (i Mod 2) * 60, 220, 255))
                g.FillRectangle(b, CSng(px), 380 + (i Mod 4) * 40, 45, 220)
            End Using
        Next
        Using floor As New SolidBrush(Color.FromArgb(19, 29, 48))
            g.FillRectangle(floor, 0, 576, 1280, 144)
        End Using
        Using p As New Pen(Color.Cyan, 4)
            g.DrawLine(p, 0, 577, 1280, 577)
        End Using
        For i = 0 To 20
            Using p As New Pen(Color.FromArgb(40, 42, 225, 255), 1)
                g.DrawLine(p, i * 80, 577, i * 140 - 150, 720)
            End Using
        Next
    End Sub
    Private Sub DrawEnemy(g As Graphics, a As Actor)
        Dim r = a.Radius
        Dim c = If(a.IsBoss, Color.FromArgb(195, 52, 250), If(a.Kind = 2, Color.FromArgb(233, 77, 142), Color.FromArgb(85, 235, 140)))
        Using b As New SolidBrush(c)
            g.FillEllipse(b, a.X - r, a.Y - r, r * 2, r * 2)
        End Using
        Using p As New Pen(Color.White, If(a.IsBoss, 7, 2))
            g.DrawEllipse(p, a.X - r, a.Y - r, r * 2, r * 2)
        End Using
        Using b As New SolidBrush(Color.FromArgb(30, 15, 57))
            g.FillEllipse(b, a.X - r * 0.55F, a.Y - r * 0.18F, 9, 13)
            g.FillEllipse(b, a.X + r * 0.35F, a.Y - r * 0.18F, 9, 13)
        End Using
        If a.IsBoss Then
            DrawText(g, "PORTAL SENTINEL", a.X - 95, a.Y - r - 42, 17, Color.White)
            Using b As New SolidBrush(Color.FromArgb(95, 230, 250))
                g.FillRectangle(b, a.X - 88, a.Y - r - 16, Math.Clamp(a.HP / (380 + wave * 65 + difficulty * 80), 0.0F, 1.0F) * 175, 8)
            End Using
        End If
    End Sub
    Private Sub DrawPlayer(g As Graphics)
        Dim image = If(chosen = "metal", metalImg, blueImg)
        Dim isMoving = Math.Abs(player.VX) > 20 OrElse Math.Abs(player.VY) > 20
        Dim bob = If(isMoving, CSng(Math.Sin(globalTime * 15) * 4), 0.0F)
        Dim lean = If(isMoving, CSng(Math.Sin(globalTime * 14) * 0.045), 0.0F)
        Dim recoil = If(player.AttackTime > 0, -0.14F, 0.0F)
        g.TranslateTransform(player.X, player.Y)
        If facing < 0 Then g.ScaleTransform(-1.0F, 1.0F)
        g.RotateTransform((lean + recoil) * 180.0F / CSng(Math.PI))
        Dim flicker = damageCooldown > 0 AndAlso Math.Sin(globalTime * 35) > 0
        If image IsNot Nothing AndAlso Not flicker Then
            ' Full-body animation: walking bob / tilt, jump squash-stretch and recoil.
            Dim width = If(player.VY <> 0, 82.0F, 89.0F)
            Dim height = If(player.VY <> 0, 225.0F, 216.0F)
            g.DrawImage(image, -width / 2, -height + bob + 30, width, height)
            ' Extra native procedural animation over the sprite artwork.
            Dim stride = If(isMoving AndAlso player.VY = 0, CSng(Math.Sin(globalTime * 16) * 11), 0.0F)
            Using legPen As New Pen(If(chosen = "metal", Color.FromArgb(60, 64, 78), Color.FromArgb(44, 74, 115)), 11)
                legPen.StartCap = LineCap.Round
                legPen.EndCap = LineCap.Round
                g.DrawLine(legPen, -15, -25, -16 + stride, 9)
                g.DrawLine(legPen, 15, -25, 16 - stride, 9)
            End Using
        ElseIf image Is Nothing Then
            Using b As New SolidBrush(If(chosen = "metal", Color.HotPink, Color.LightBlue))
                g.FillRectangle(b, -33, -174, 66, 178)
            End Using
        End If
        g.ResetTransform()
        g.ScaleTransform(ClientSize.Width / GW, ClientSize.Height / GH)
        ' Draw weapon separately so it stays aimed at the pointer (not attached to body sprite rotation).
        DrawHeldGun(g)
    End Sub
    Private Sub DrawHeldGun(g As Graphics)
        Dim pivotX = player.X
        Dim pivotY = player.Y - 95
        Dim angle = aimAngle
        Dim barrel = 70.0F
        Dim muzzleX = pivotX + CSng(Math.Cos(angle)) * 56
        Dim muzzleY = pivotY + CSng(Math.Sin(angle)) * 56
        g.TranslateTransform(pivotX, pivotY)
        g.RotateTransform(angle * 180.0F / CSng(Math.PI))
        Using p As New Pen(If(chosen = "metal", Color.FromArgb(221, 170, 127), Color.FromArgb(238, 186, 146)), 15)
            g.DrawLine(p, -9, 12, 30, 6)
        End Using
        Using p As New Pen(ShotColor(weaponIndex), 10)
            g.DrawLine(p, 2, 0, barrel, 0)
        End Using
        Using p As New Pen(Color.FromArgb(20, 28, 46), 16)
            g.DrawLine(p, 18, 5, 46, 5)
        End Using
        Using b As New SolidBrush(Color.Gold)
            g.FillRectangle(b, 17, 6, 11, 20)
        End Using
        Using b As New SolidBrush(Color.WhiteSmoke)
            g.FillRectangle(b, 63, -5, 12, 10)
        End Using
        g.ResetTransform()
        g.ScaleTransform(ClientSize.Width / GW, ClientSize.Height / GH)
        If player.AttackTime > 0 Then
            Using b As New SolidBrush(Color.FromArgb(180, Color.Gold))
                g.FillEllipse(b, muzzleX - 18, muzzleY - 18, 36, 36)
            End Using
        End If
    End Sub
    Private Sub DrawPortal(g As Graphics)
        Using grad As New LinearGradientBrush(New Rectangle(0, 0, 1280, 720), Color.FromArgb(80, 15, 138), Color.FromArgb(10, 22, 75), 90.0F)
            g.FillRectangle(grad, 0, 0, 1280, 720)
        End Using
        For i = 0 To 20
            Dim x = CSng((i * 99 - portalDistance * 0.25) Mod 1400)
            Using p As New Pen(Color.FromArgb(70 + CInt(40 * Math.Sin(globalTime * 4 + i)), Color.Cyan), 2)
                g.DrawLine(p, x, 120, x + 30, 580)
            End Using
        Next
        Using f As New SolidBrush(Color.FromArgb(23, 24, 53))
            g.FillRectangle(f, 0, 582, 1280, 138)
        End Using
        Using pen As New Pen(Color.Cyan, 5)
            g.DrawLine(pen, 0, 583, 1280, 583)
        End Using
        For Each h In hazards
            Dim sx = h.X - portalDistance + 270
            If sx < -80 OrElse sx > 1320 Then Continue For
            Using b As New SolidBrush(If(h.Kind Mod 2 = 0, Color.FromArgb(253, 88, 156), Color.FromArgb(120, 240, 255)))
                Dim pts = {New PointF(sx, h.Y + 32), New PointF(sx + 23, h.Y - 29), New PointF(sx + 48, h.Y + 32)}
                g.FillPolygon(b, pts)
            End Using
        Next
        Using b As New SolidBrush(If(chosen = "metal", Color.HotPink, Color.Aqua))
            g.TranslateTransform(270, portalCubeY)
            g.RotateTransform(globalTime * 200)
            g.FillRectangle(b, -24, -24, 48, 48)
            g.ResetTransform()
            g.ScaleTransform(ClientSize.Width / GW, ClientSize.Height / GH)
        End Using
        DrawText(g, "PORTAL RUN • " & {"CUBE", "FLIGHT", "GRAVITY"}(portalMode), 28, 105, 28, Color.White)
        DrawText(g, "W / SPACE: HOPP / FLY / SNU TYNGDEKRAFT", 28, 143, 18, Color.LightCyan)
    End Sub
    Private Sub DrawRoad(g As Graphics)
        g.Clear(Color.FromArgb(13, 19, 48))
        Dim horizon = 190.0F
        Dim center = 640.0F
        For i = 0 To 13
            Dim y = horizon + i * i * 2.55F
            If y > 720 Then Exit For
            Using p As New Pen(Color.FromArgb(25 + i * 9, 0, 255, 255), 2)
                g.DrawLine(p, center - i * 52, y, center + i * 52, y)
            End Using
        Next
        Using road As New SolidBrush(Color.FromArgb(28, 35, 71))
            g.FillPolygon(road, {New PointF(565, 190), New PointF(715, 190), New PointF(1150, 720), New PointF(130, 720)})
        End Using
        Using p As New Pen(Color.Cyan, 5)
            g.DrawLine(p, 565, 190, 130, 720) : g.DrawLine(p, 715, 190, 1150, 720)
        End Using
        For lane = 1 To 2
            Using p As New Pen(Color.FromArgb(200, 255, 220, 125), 4)
                p.DashStyle = DashStyle.Dash
                Dim xNear = 130 + lane * 340
                Dim xFar = 565 + lane * 50
                g.DrawLine(p, xFar, 190, xNear, 720)
            End Using
        Next
        For Each h In hazards
            If h.Collected Then Continue For
            Dim depth = Math.Clamp(1.0F - (h.X - portalDistance) / 1200, 0.0F, 1.0F)
            If depth <= 0 Then Continue For
            Dim y = 204 + depth * depth * 505
            Dim x = center + (h.Lane - 1) * (60 + 250 * depth)
            Dim width = 16 + 68 * depth
            Using b As New SolidBrush(If(h.Kind = 1, Color.Gold, Color.FromArgb(255, 78, 122)))
                g.FillRectangle(b, x - width / 2, y - width, width, width)
            End Using
        Next
        Dim carX = center + (portalCarLane - 1) * 300
        Using b As New SolidBrush(If(chosen = "metal", Color.HotPink, Color.Cyan))
            g.FillPolygon(b, {New PointF(carX, 552), New PointF(carX - 44, 660), New PointF(carX + 44, 660)})
        End Using
        DrawText(g, "NEON HIGHWAY  |  A / D FILSKIFT  |  SHIFT NITRO", 25, 100, 26, Color.White)
    End Sub
    Private Sub DrawHud(g As Graphics)
        Using b As New SolidBrush(Color.FromArgb(218, 8, 13, 34))
            g.FillRectangle(b, 0, 0, GW, 89)
        End Using
        DrawText(g, "KAOSDUO: VB NEXT", 20, 14, 24, Color.Cyan)
        DrawText(g, "WAVE " & wave & "/10", 316, 20, 22, Color.White)
        DrawText(g, "HP " & CInt(Math.Ceiling(player.HP)) & "/" & CInt(maxHealth), 470, 20, 21, Color.LightGreen)
        DrawText(g, "COINS: " & coins, 665, 20, 22, Color.Gold)
        DrawText(g, weapons(weaponIndex), 865, 20, 20, ShotColor(weaponIndex))
        Using hpBrush As New SolidBrush(Color.FromArgb(50, 215, 110))
            g.FillRectangle(hpBrush, 474, 56, Math.Clamp(player.HP / maxHealth, 0, 1) * 150, 9)
        End Using
        If state = "portal" OrElse state = "road" Then
            DrawText(g, "REISE: " & CInt(Math.Clamp(portalDistance / portalLength, 0, 1) * 100) & "%", 1030, 105, 23, Color.Gold)
        End If
        If state = "ambush" Then DrawText(g, "BOSS-FIGHT: BEVEG DEG MED WASD + SIKT MED MUS!", 258, 100, 21, Color.Orange)
        DrawText(g, "F5 SAVE   F9 LOAD   B SHOP   M MUSIC   ESC PAUSE", 17, 705, 12, Color.LightGray, False)
    End Sub
    Private Sub DrawShop(g As Graphics)
        Using b As New SolidBrush(Color.FromArgb(242, 10, 18, 43))
            g.FillRectangle(b, 130, 80, 1020, 565)
        End Using
        DrawCentered(g, "CHAOS ARMORY — VÅPENBUTIKK", 132, 34, Color.Cyan)
        DrawCentered(g, "DU HAR " & coins & " MYNTER | TRYKK 1–8 FOR Å KJØPE | ENTER FOR Å FORTSETTE", 170, 17, Color.Gold)
        For i = 0 To 7
            Dim x = 170 + (i Mod 2) * 495
            Dim y = 208 + (i \ 2) * 91
            Using fill As New SolidBrush(Color.FromArgb(30, 44, 77))
                g.FillRectangle(fill, x, y, 459, 72)
            End Using
            Using p As New Pen(ShotColor(i), 2)
                g.DrawRectangle(p, x, y, 459, 72)
            End Using
            DrawText(g, (i + 1).ToString() & ". " & weapons(i), x + 16, y + 12, 19, Color.White)
            DrawText(g, If(unlock(i), "UNLOCKED / EQUIP", prices(i).ToString() & " COINS"), x + 17, y + 41, 15, If(unlock(i), Color.LimeGreen, Color.Gold))
        Next
    End Sub
    Private Sub DrawOverlay(g As Graphics, headline As String, subtitle As String)
        Using b As New SolidBrush(Color.FromArgb(231, 9, 12, 34))
            g.FillRectangle(b, 160, 155, 960, 402)
        End Using
        DrawCentered(g, headline, 293, 66, Color.Cyan)
        DrawCentered(g, subtitle, 385, 25, Color.White)
    End Sub
End Class
