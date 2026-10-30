Option Strict On
Option Explicit On

Imports System
Imports System.Collections.Generic
Imports System.Drawing
Imports System.Linq
Imports System.Windows.Forms

Partial Public Class GameForm
    Private Sub UpdateEffects(dt As Single)
        For i = _sparks.Count - 1 To 0 Step -1
            Dim p = _sparks(i)
            p.Life -= dt
            If p.Life <= 0 Then
                _sparks.RemoveAt(i)
                Continue For
            End If
            p.X += p.Vx * dt
            p.Y += p.Vy * dt
            p.Vy += 205.0F * dt
        Next
        If _sparks.Count > 650 Then _sparks.RemoveRange(0, _sparks.Count - 650)
    End Sub

    Private Sub Burst(x As Single, y As Single, tint As Color, count As Integer, force As Single)
        For i = 1 To count
            Dim angle = _rng.NextDouble() * Math.PI * 2
            Dim speed = CSng((0.20 + _rng.NextDouble() * 0.8) * force)
            Dim life = CSng(0.25 + _rng.NextDouble() * 0.65)
            _sparks.Add(New Spark With {
                .X = x, .Y = y, .Vx = CSng(Math.Cos(angle)) * speed,
                .Vy = CSng(Math.Sin(angle)) * speed, .Tint = tint,
                .Life = life, .FullLife = life, .Size = CSng(2.4 + _rng.NextDouble() * 6)
            })
        Next
    End Sub

    Private Function Limit(value As Single, low As Single, high As Single) As Single
        Return Math.Max(low, Math.Min(high, value))
    End Function

    Private Function DistanceSq(x1 As Single, y1 As Single, x2 As Single, y2 As Single) As Single
        Dim dx = x1 - x2, dy = y1 - y2
        Return dx * dx + dy * dy
    End Function

    Private Sub UpdateHeroTimers(dt As Single)
        Dim h = _s.Hero
        h.Invincible = Math.Max(0, h.Invincible - dt)
        h.Recoil = Math.Max(0, h.Recoil - dt * 5)
        h.MuzzleFlash = Math.Max(0, h.MuzzleFlash - dt)
        h.DashCooldown = Math.Max(0, h.DashCooldown - dt)
        h.SuperCooldown = Math.Max(0, h.SuperCooldown - dt)
        h.ShootCooldown = Math.Max(0, h.ShootCooldown - dt)
        h.ReloadRemaining = Math.Max(0, h.ReloadRemaining - dt)
        h.MotionTime += dt
        h.BlinkTime += dt
        If h.ReloadRemaining = 0 AndAlso _s.Magazine.ContainsKey(_s.EquippedWeapon) AndAlso _s.Magazine(_s.EquippedWeapon) = -1 Then
            _s.Magazine(_s.EquippedWeapon) = GameRules.Weapons(_s.EquippedWeapon).Magazine
            Notify("LADDET!", 0.7F)
        End If
        If _s.ComboTimer > 0 Then
            _s.ComboTimer = Math.Max(0, _s.ComboTimer - dt)
            If _s.ComboTimer <= 0 Then _s.Combo = 0
        End If
        If _s.FeverTime > 0 Then _s.FeverTime = Math.Max(0, _s.FeverTime - dt)
    End Sub

    Private Sub BeginJump()
        Dim h = _s.Hero
        If h.JumpCount < If(_s.DoubleJumpAvailable, 2, 1) Then
            h.Vy = If(h.JumpCount = 0, -550.0F, -500.0F)
            h.JumpCount += 1
            Burst(h.X, h.Y - 14, Color.FromArgb(135, 243, 255), 9, 175)
        End If
    End Sub

    Private Sub BeginDash()
        If Not _s.DashAvailable OrElse _s.Hero.DashCooldown > 0 Then Return
        _s.Hero.DashCooldown = 3.0F
        _s.Hero.DashLeft = 0.19F
        _s.Hero.Invincible = Math.Max(_s.Hero.Invincible, 0.3F)
        _cameraKick = 4
        Burst(_s.Hero.X, _s.Hero.Y - 74, Color.FromArgb(116, 255, 240), 16, 230)
    End Sub

    Private Sub TriggerFever()
        If _s.FeverMeter < 100 OrElse _s.FeverTime > 0 OrElse Not _s.OverdriveAvailable Then Return
        _s.FeverMeter = 0
        _s.FeverTime = 7.0F
        _cameraShake = 4
        Notify("OVERDRIVE AKTIVERT! 7 SEKUNDER!", 2.6F)
        Burst(_s.Hero.X, _s.Hero.Y - 100, Color.FromArgb(248, 121, 255), 70, 440)
    End Sub

    Private Sub TriggerSuper()
        If Not _s.SuperAvailable OrElse _s.Hero.SuperCooldown > 0 OrElse _s.Scene <> SceneKind.Combat Then Return
        _s.Hero.SuperCooldown = 13.5F
        _cameraShake = 15
        _cameraKick = 12
        Dim colour = If(_s.Character = HeroKind.Metal, Color.FromArgb(255, 99, 173), Color.FromArgb(130, 240, 255))
        Burst(_s.Hero.X + 40, _s.Hero.Y - 100, colour, 115, 600)
        For Each enemy In _s.Enemies.ToList()
            enemy.Hp -= 100 + _s.DamageLevel * 8
            enemy.Freeze = Math.Max(enemy.Freeze, 0.8F)
            If enemy.Hp <= 0 Then KillEnemy(enemy)
        Next
        Notify(If(_s.Character = HeroKind.Metal, "METAL STORM!", "GLACIER BLAST!"), 2)
    End Sub

    Private Sub Reload()
        If _s.Hero.ReloadRemaining > 0 Then Return
        Dim gun = GameRules.Weapons(_s.EquippedWeapon)
        Dim count As Integer = gun.Magazine
        If _s.Magazine.TryGetValue(gun.Id, count) AndAlso count >= gun.Magazine Then Return
        _s.Magazine(gun.Id) = -1
        _s.Hero.ReloadRemaining = gun.Reload
        Notify("LADER " & gun.Name & "...", 1.1F)
    End Sub

    Private Sub SpawnEnemy()
        Dim wave = _s.Wave
        Dim kind = _rng.Next(0, Math.Min(5, 2 + wave \ 2))
        Dim mult = GameRules.DifficultyMultiplier(_s.Difficulty)
        Dim maxHp = (24 + wave * 7 + kind * 8) * mult
        Dim fly As Boolean = (kind = 1 OrElse kind = 3)
        Dim y As Single = If(fly, CSng(330 + _rng.Next(0, 160)), GameRules.FloorY - 29)
        Dim vx As Single = (43 + wave * 5 + kind * 10) * mult
        _s.Enemies.Add(New EnemyState With {
            .Uid = _rng.Next(1, Integer.MaxValue), .X = 1350 + _rng.Next(0, 260), .Y = y, .Kind = kind,
            .Hp = maxHp, .MaxHp = maxHp, .Radius = If(kind = 3, 22, 28),
            .Vx = -vx, .FireTimer = CSng(0.6 + _rng.NextDouble() * 1.6), .Anim = CSng(_rng.NextDouble() * 6)
        })
    End Sub

    Private Sub SpawnWaveBoss()
        Dim wave = _s.Wave
        Dim hp As Single = (330 + wave * 84) * GameRules.DifficultyMultiplier(_s.Difficulty)
        _s.Enemies.Add(New EnemyState With {
            .Uid = _rng.Next(1, Integer.MaxValue), .X = 1120, .Y = 432, .Kind = 5,
            .Hp = hp, .MaxHp = hp, .Radius = 75,
            .Vx = -34, .FireTimer = 1.35F, .Boss = True
        })
        _s.WaveBossSpawned = True
        _cameraShake = 9
        Notify(If(wave = 4, "GLITCH WARDEN", If(wave = 7, "CRYSTAL TITAN", "PORTAL KING")) & " HAR ANKOMMET!", 3.4F)
    End Sub

    Private Sub UpdateCombat(dt As Single)
        Dim h = _s.Hero
        UpdateHeroTimers(dt)
        Dim axis As Integer = If(IsHeld(Keys.D, Keys.Right), 1, 0) - If(IsHeld(Keys.A, Keys.Left), 1, 0)
        Dim moveSpeed = 305 + _s.SpeedLevel * 25
        If h.DashLeft > 0 Then
            h.DashLeft = Math.Max(0, h.DashLeft - dt)
            h.X += h.Facing * (870 + _s.SpeedLevel * 22) * dt
            Burst(h.X, h.Y - 70, Color.FromArgb(92, 245, 248), 2, 65)
        Else
            h.X += axis * moveSpeed * dt
            If axis <> 0 Then h.Facing = axis
        End If
        h.X = Limit(h.X, 46, 1234)
        h.Vy += 1280 * dt
        h.Y += h.Vy * dt
        If h.Y >= GameRules.FloorY Then
            h.Y = GameRules.FloorY : h.Vy = 0 : h.JumpCount = 0
        End If
        UpdateGunAim(False)
        If _mouseHeld OrElse IsHeld(Keys.F, Keys.K, Keys.ControlKey) Then TryShoot(False)

        _s.SpawnCooldown -= dt
        If _s.SpawnRemaining > 0 AndAlso _s.SpawnCooldown <= 0 Then
            SpawnEnemy()
            _s.SpawnRemaining -= 1
            _s.SpawnCooldown = Math.Max(0.27F, 0.94F - _s.Wave * 0.035F - CInt(_s.Difficulty) * 0.05F)
        End If
        If _s.SpawnRemaining <= 0 AndAlso Not _s.WaveBossSpawned AndAlso (_s.Wave = 4 OrElse _s.Wave = 7 OrElse _s.Wave = 10) Then
            SpawnWaveBoss()
        End If

        For Each enemy In _s.Enemies.ToList()
            enemy.Anim += dt
            enemy.Freeze = Math.Max(0, enemy.Freeze - dt)
            enemy.X += enemy.Vx * dt * If(enemy.Freeze > 0, 0.18F, 1.0F)
            If enemy.Kind = 1 OrElse enemy.Kind = 3 Then enemy.Y += CSng(Math.Sin(enemy.Anim * (2.4 + enemy.Kind * 0.2))) * 25 * dt
            If enemy.X < -105 Then enemy.X = 1320
            enemy.FireTimer -= dt
            If enemy.FireTimer <= 0 Then
                If enemy.Kind >= 2 OrElse enemy.Boss Then
                    Dim salvos = If(enemy.Boss, 3, 1)
                    For i = 0 To salvos - 1
                        Dim sx = enemy.X - 20, sy = enemy.Y
                        Dim angle = CSng(Math.Atan2(h.Y - 100 - sy, h.X - sx)) + (i - (salvos - 1) * 0.5F) * 0.17F
                        EnemyShoot(sx, sy, angle, If(enemy.Boss, 200, 240))
                    Next
                End If
                enemy.FireTimer = CSng(If(enemy.Boss, 1.0, 2.2) + _rng.NextDouble() * 0.8) / GameRules.DifficultyMultiplier(_s.Difficulty)
            End If
            If DistanceSq(enemy.X, enemy.Y, h.X, h.Y - 67) < (enemy.Radius + 35) * (enemy.Radius + 35) Then
                DamageHero(If(enemy.Boss, 21, 12) * GameRules.DifficultyMultiplier(_s.Difficulty))
            End If
        Next
        UpdateProjectiles(dt, False)
        If _s.Hero.Hp <= 0 Then
            _s.Hero.Hp = 0
            _s.Scene = SceneKind.GameOver
            _cameraShake = 16
            Burst(_s.Hero.X, _s.Hero.Y - 80, Color.FromArgb(253, 112, 145), 95, 480)
            Return
        End If
        If _s.SpawnRemaining = 0 AndAlso _s.Enemies.Count = 0 Then
            If _waveDelay < 0 Then
                _waveDelay = 1.7F
                Notify("BOLGE KLART!  VAPENBUTIKKEN VENTER!", 1.8F)
            End If
            _waveDelay -= dt
            If _waveDelay <= 0 Then OfferShop()
        Else
            _waveDelay = -1
        End If
    End Sub

    Private Sub EnemyShoot(x As Single, y As Single, angle As Single, speed As Single)
        _s.Bullets.Add(New ProjectileState With {
            .X = x, .Y = y, .Vx = CSng(Math.Cos(angle)) * speed,
            .Vy = CSng(Math.Sin(angle)) * speed, .Friendly = False,
            .Radius = 8, .Damage = 11 * GameRules.DifficultyMultiplier(_s.Difficulty), .Life = 6.5F, .Weapon = -1
        })
    End Sub

    Private Sub UpdateGunAim(isPortalBoss As Boolean)
        Dim h = _s.Hero
        Dim x = If(isPortalBoss, _s.Portal.BossPlayerX, h.X)
        Dim y = If(isPortalBoss, _s.Portal.BossPlayerY, h.Y) - 102
        Dim targetX As Single = x + 140
        Dim targetY As Single = y
        If _mouseHeld Then
            targetX = _mouseWorld.X : targetY = _mouseWorld.Y
        ElseIf isPortalBoss Then
            targetX = _s.Portal.BossX : targetY = _s.Portal.BossY
        ElseIf _s.Enemies.Count > 0 Then
            Dim nearest = _s.Enemies.OrderBy(Function(en) DistanceSq(en.X, en.Y, x, y)).First()
            targetX = nearest.X : targetY = nearest.Y
        End If
        h.Aim = CSng(Math.Atan2(targetY - y, targetX - x))
        If Math.Abs(Math.Cos(h.Aim)) > 0.25 Then h.Facing = If(Math.Cos(h.Aim) >= 0, 1, -1)
    End Sub

    Private Sub TryShoot(isPortalBoss As Boolean)
        Dim h = _s.Hero
        If h.ShootCooldown > 0 OrElse h.ReloadRemaining > 0 Then Return
        Dim gun = GameRules.Weapons(_s.EquippedWeapon)
        Dim ammo As Integer = gun.Magazine
        If _s.Magazine.ContainsKey(gun.Id) Then ammo = _s.Magazine(gun.Id)
        If ammo <= 0 Then
            Reload()
            Return
        End If
        _s.Magazine(gun.Id) = ammo - 1
        h.ShootCooldown = gun.Cooldown * CSng(Math.Pow(0.92, _s.FireRateLevel)) * If(_s.FeverTime > 0, 0.52F, 1.0F)
        h.Recoil = 1
        h.MuzzleFlash = 0.11F
        Dim px = If(isPortalBoss, _s.Portal.BossPlayerX, h.X)
        Dim py = If(isPortalBoss, _s.Portal.BossPlayerY, h.Y)
        Dim mouth = GameRules.MuzzlePosition(px, py, h.Aim)
        For i = 0 To gun.Pellets - 1
            Dim angle = h.Aim + (i - (gun.Pellets - 1) / 2.0F) * gun.Spread / Math.Max(1, gun.Pellets - 1)
            If gun.Pellets = 1 AndAlso gun.Spread > 0 Then angle += CSng((_rng.NextDouble() - 0.5) * gun.Spread)
            Dim damage = gun.Damage * (1.0F + _s.DamageLevel * 0.20F) * If(_s.FeverTime > 0, 1.7F, 1.0F)
            Dim bullet As New ProjectileState With {
                .X = mouth.X, .Y = mouth.Y,
                .Vx = CSng(Math.Cos(angle)) * gun.Speed,
                .Vy = CSng(Math.Sin(angle)) * gun.Speed,
                .Damage = damage, .Friendly = True, .Weapon = gun.Id,
                .Radius = If(gun.Id = 4, 12, If(gun.Id = 2, 8, 5)),
                .Pierce = gun.Piercing, .Splash = gun.Splash, .Life = If(isPortalBoss, 2.4F, 2.8F)
            }
            _s.Bullets.Add(bullet)
        Next
        Burst(mouth.X, mouth.Y, gun.Tint, If(gun.Id = 6, 3, 8), 150)
        _cameraKick = If(gun.Id = 2 OrElse gun.Id = 4, 3.7F, 1.1F)
        If _s.Magazine(gun.Id) <= 0 Then Reload()
    End Sub

    Private Sub UpdateProjectiles(dt As Single, inPortalBoss As Boolean)
        For Each shot In _s.Bullets.ToList()
            shot.X += shot.Vx * dt
            shot.Y += shot.Vy * dt
            shot.Life -= dt
            If shot.Life <= 0 OrElse shot.X < -180 OrElse shot.X > 1450 OrElse shot.Y < -170 OrElse shot.Y > 850 Then
                _s.Bullets.Remove(shot)
                Continue For
            End If
            If shot.Friendly Then
                If inPortalBoss Then
                    Dim p = _s.Portal
                    If p.BossHp > 0 AndAlso DistanceSq(shot.X, shot.Y, p.BossX, p.BossY) < (shot.Radius + 67) * (shot.Radius + 67) Then
                        p.BossHp -= shot.Damage
                        Burst(shot.X, shot.Y, GameRules.Weapons(shot.Weapon).Tint, 7, 120)
                        _s.Bullets.Remove(shot)
                        If p.BossHp <= 0 Then DefeatPortalBoss()
                    End If
                Else
                    For Each enemy In _s.Enemies.ToList()
                        If enemy.Hp <= 0 Then Continue For
                        Dim enemyHitId As Integer = enemy.Uid
                        If shot.HitIds.Contains(enemyHitId) Then Continue For
                        If DistanceSq(shot.X, shot.Y, enemy.X, enemy.Y) < (enemy.Radius + shot.Radius) * (enemy.Radius + shot.Radius) Then
                            enemy.Hp -= shot.Damage
                            If shot.Weapon = 5 Then enemy.Freeze = Math.Max(enemy.Freeze, 1.9F)
                            Burst(shot.X, shot.Y, GameRules.Weapons(shot.Weapon).Tint, 5, 110)
                            If shot.Weapon = 3 Then ChainLightning(enemy, shot.Damage * 0.7F)
                            If shot.Splash > 0 Then ExplodeProjectile(shot)
                            If enemy.Hp <= 0 Then KillEnemy(enemy)
                            If shot.Pierce <= 0 Then
                                _s.Bullets.Remove(shot)
                            Else
                                shot.Pierce -= 1
                                shot.HitIds.Add(enemyHitId)
                            End If
                            Exit For
                        End If
                    Next
                End If
            Else
                Dim px = If(inPortalBoss, _s.Portal.BossPlayerX, _s.Hero.X)
                Dim py = If(inPortalBoss, _s.Portal.BossPlayerY, _s.Hero.Y) - 83
                If DistanceSq(shot.X, shot.Y, px, py) < 33 * 33 Then
                    _s.Bullets.Remove(shot)
                    If inPortalBoss Then
                        HitPortalShield()
                    Else
                        DamageHero(shot.Damage)
                    End If
                End If
            End If
        Next
    End Sub

    Private Sub ExplodeProjectile(shot As ProjectileState)
        Burst(shot.X, shot.Y, Color.FromArgb(255, 170, 92), 23, 260)
        _cameraShake = Math.Max(_cameraShake, 2.2F)
        For Each enemy In _s.Enemies.ToList()
            If DistanceSq(shot.X, shot.Y, enemy.X, enemy.Y) < shot.Splash * shot.Splash Then
                enemy.Hp -= shot.Damage * 0.6F
                If enemy.Hp <= 0 Then KillEnemy(enemy)
            End If
        Next
    End Sub

    Private Sub ChainLightning(firstEnemy As EnemyState, damage As Single)
        Dim nextEnemy = _s.Enemies.Where(Function(en) Not Object.ReferenceEquals(en, firstEnemy) AndAlso en.Hp > 0 AndAlso DistanceSq(en.X, en.Y, firstEnemy.X, firstEnemy.Y) < 180 * 180).OrderBy(Function(en) DistanceSq(en.X, en.Y, firstEnemy.X, firstEnemy.Y)).FirstOrDefault()
        If nextEnemy IsNot Nothing Then
            nextEnemy.Hp -= damage
            Burst(nextEnemy.X, nextEnemy.Y, Color.FromArgb(136, 255, 206), 11, 195)
            If nextEnemy.Hp <= 0 Then KillEnemy(nextEnemy)
        End If
    End Sub

    Private Sub KillEnemy(enemy As EnemyState)
        If Not _s.Enemies.Remove(enemy) Then Return
        Dim reward = If(enemy.Boss, 290, 19 + _s.Wave * 5 + enemy.Kind * 4)
        reward = CInt(reward * (1.0F + _s.MoneyLevel * 0.16F))
        _s.Coins += reward
        _s.TotalCoins += reward
        _s.Kills += 1
        _s.Combo += 1
        _s.ComboTimer = 4.2F
        _s.Score += (60 + enemy.Kind * 30 + _s.Wave * 10) * Math.Min(10, Math.Max(1, _s.Combo))
        _s.FeverMeter = Limit(_s.FeverMeter + If(enemy.Boss, 40, 5), 0, 100)
        _s.Experience += If(enemy.Boss, 150, 16 + enemy.Kind * 5)
        Burst(enemy.X, enemy.Y, If(enemy.Boss, Color.FromArgb(255, 140, 240), Color.FromArgb(125, 244, 244)), If(enemy.Boss, 85, 18), If(enemy.Boss, 470, 260))
        If enemy.Boss Then
            _cameraShake = 13
            _s.Hero.Hp = Math.Min(_s.Hero.MaxHp, _s.Hero.Hp + 30)
            Notify("BOSS KNUST! +" & reward.ToString() & " MYNTER", 3.3F)
        End If
        If _s.Experience >= _s.ExpNeeded Then
            _s.Experience -= _s.ExpNeeded
            _s.Level += 1
            _s.ExpNeeded = CInt(_s.ExpNeeded * 1.3F) + 20
            _s.UpgradesPending += 1
            If _s.Scene = SceneKind.Combat Then
                _s.PreviousScene = SceneKind.Combat
                _s.Scene = SceneKind.Upgrade
            End If
        End If
    End Sub

    Private Sub DamageHero(amount As Single)
        Dim h = _s.Hero
        If h.Invincible > 0 Then Return
        h.Invincible = 0.85F
        If h.Shield > 0 Then
            h.Shield -= 1
            Notify("SKJOLD ABSORBERTE TREFF!", 0.8F)
        Else
            h.Hp -= Math.Max(1, amount * CSng(Math.Pow(0.92, _s.ArmorLevel)))
        End If
        _cameraShake = Math.Max(_cameraShake, 6)
        Burst(h.X, h.Y - 83, Color.FromArgb(255, 91, 134), 19, 250)
    End Sub

    Private Sub StartTravel()
        Dim wave = _s.Wave
        If wave >= GameRules.TotalWaves Then
            _s.Scene = SceneKind.Victory
            Return
        End If
        Dim road = GameRules.RollHighway(_rng)
        Dim p As New PortalState With {
            .Type = If(road, "HIGHWAY", "GEOMETRY"),
            .Length = 3500 + wave * 150,
            .Distance = 0, .Elapsed = 0,
            .TriggerBoss = (Not road AndAlso GameRules.RollPortalBoss(_rng)),
            .BossPosition = CSng(0.36 + _rng.NextDouble() * 0.26) * (3500 + wave * 150),
            .StagePermutation = _rng.Next(0, 3), .Y = GameRules.FloorY - 45,
            .CarLane = 1, .Nitro = 1, .CarSpeed = 420 + wave * 13
        }
        If road Then
            Dim d As Single = 470
            While d < p.Length - 210
                p.RoadObstacles.Add(New RoadObstacle With {.AtDistance = d, .Lane = _rng.Next(0, 3), .Kind = _rng.Next(0, 5)})
                d += CSng(_rng.Next(175, 320))
            End While
        Else
            Dim d As Single = 500
            While d < p.Length - 190
                Dim mode = PortalSectionMode(p, d)
                Dim typeNumber = _rng.Next(0, 3)
                Dim oy As Single = GameRules.FloorY - 53
                Dim ow As Single = 50, oh As Single = 55
                If mode = 1 Then
                    oy = CSng(_rng.Next(238, 445))
                    ow = 58 : oh = 72
                ElseIf mode = 2 Then
                    oy = If(_rng.Next(0, 2) = 0, 168.0F, GameRules.FloorY - 84)
                    ow = 52 : oh = 78
                Else
                    ow = If(typeNumber = 2, 80.0F, 48.0F)
                    oh = If(typeNumber = 2, 74.0F, 48.0F)
                    oy = GameRules.FloorY - oh
                End If
                p.Obstacles.Add(New PortalObstacle With {.X = d, .Y = oy, .W = ow, .H = oh, .Kind = typeNumber})
                d += CSng(_rng.Next(315, 475) - wave * 3)
            End While
        End If
        _s.Portal = p
        _s.Scene = If(road, SceneKind.Highway, SceneKind.Portal)
        _s.Bullets.Clear()
        _s.Hero.Invincible = 0
        _s.Hero.ReloadRemaining = 0
        _s.Hero.ShootCooldown = 0
        Notify(If(road, "NEON HIGHWAY! KJOR MELLOM BOLGENE!", "PORTAL RUN! BYTT FORM OG UNNGA HINDRINGER!"), 3)
        SaveGame(False)
    End Sub

    Private Function PortalSectionMode(p As PortalState, distance As Single) As Integer
        Dim section = Math.Max(0, Math.Min(2, CInt(Math.Floor(distance / (p.Length / 3.0F)))))
        Return (section + p.StagePermutation) Mod 3 ' 0 = cube; 1 = jet flight; 2 = flip gravity
    End Function

    Private Sub TapPortal()
        If _s.Scene <> SceneKind.Portal Then Return
        Dim p = _s.Portal
        Dim mode = PortalSectionMode(p, p.Distance)
        If mode = 0 Then
            If p.Grounded Then
                p.Vy = -590
                p.Grounded = False
                Burst(260, p.Y + 44, Color.FromArgb(123, 253, 255), 11, 170)
            End If
        ElseIf mode = 2 Then
            p.GravitySign *= -1
            p.Grounded = False
            Burst(265, p.Y + 25, Color.FromArgb(229, 110, 255), 13, 185)
        End If
    End Sub

    Private Sub UpdatePortal(dt As Single)
        Dim p = _s.Portal
        p.Elapsed += dt
        Dim previousMode = PortalSectionMode(p, p.Distance)
        Dim speed = (365 + _s.Wave * 13) * GameRules.DifficultyMultiplier(_s.Difficulty)
        p.Distance += speed * dt
        Dim mode = PortalSectionMode(p, p.Distance)
        If mode <> previousMode Then
            p.Y = If(mode = 1, 323.0F, GameRules.FloorY - 45)
            p.Vy = 0
            p.GravitySign = 1
            p.Grounded = (mode = 0)
            Notify(If(mode = 1, "JET-MODUS! HOLD SPACE FOR A FLY!", If(mode = 2, "GRAVITASJON! TRYKK FOR A SNU!", "KUBEMODUS! TIM HOPPET!")), 1.9F)
            Burst(265, 360, Color.FromArgb(146, 255, 250), 44, 410)
        End If
        Select Case mode
            Case 0
                p.Vy += 1350 * dt
                p.Y += p.Vy * dt
                If p.Y >= GameRules.FloorY - 45 Then
                    p.Y = GameRules.FloorY - 45
                    p.Vy = 0
                    p.Grounded = True
                End If
            Case 1
                p.Vy += If(IsHeld(Keys.Space, Keys.W, Keys.Up), -1700.0F, 900.0F) * dt
                p.Vy = Limit(p.Vy, -340, 350)
                p.Y += p.Vy * dt
                If p.Y < 175 OrElse p.Y > GameRules.FloorY - 45 Then
                    FailPortal("JETEN TRAFF KANTEN")
                    Return
                End If
            Case 2
                p.Vy += 1400 * p.GravitySign * dt
                p.Vy = Limit(p.Vy, -690, 690)
                p.Y += p.Vy * dt
                If p.GravitySign < 0 AndAlso p.Y <= 164 Then p.Y = 164 : p.Vy = 0 : p.Grounded = True
                If p.GravitySign > 0 AndAlso p.Y >= GameRules.FloorY - 45 Then p.Y = GameRules.FloorY - 45 : p.Vy = 0 : p.Grounded = True
        End Select
        For Each obstacle In p.Obstacles
            Dim left = obstacle.X - p.Distance + 260
            If left < 210 OrElse left > 345 Then Continue For
            If 260 + 38 > left + 7 AndAlso 260 + 5 < left + obstacle.W - 7 AndAlso p.Y + 40 > obstacle.Y + 8 AndAlso p.Y + 4 < obstacle.Y + obstacle.H - 8 Then
                FailPortal("KUBEN BLE KNUSKET AV EN HINDRING")
                Return
            End If
        Next
        If p.TriggerBoss AndAlso Not p.BossTriggered AndAlso p.Distance >= p.BossPosition Then
            BeginPortalBoss()
            Return
        End If
        If p.Distance >= p.Length Then FinishTravel()
    End Sub

    Private Sub BeginPortalBoss()
        Dim p = _s.Portal
        p.BossTriggered = True
        p.BossDefeated = False
        p.BossMaxHp = (330 + _s.Wave * 62) * GameRules.DifficultyMultiplier(_s.Difficulty)
        p.BossHp = p.BossMaxHp
        p.BossShield = 3
        p.BossX = 1040
        p.BossY = 360
        p.BossPlayerX = 230
        p.BossPlayerY = 410
        p.BossFireCooldown = 1.25F
        p.BossTime = 0
        _s.Scene = SceneKind.PortalBoss
        _s.Bullets.Clear()
        _s.Hero.Invincible = 1.2F
        _cameraShake = 12
        Burst(935, 350, Color.FromArgb(230, 92, 240), 95, 520)
        Notify("PORTAL AMBUSH! BEVEG DEG MED WASD, SIKT OG SKYT!", 4.2F)
        SaveGame(False)
    End Sub

    Private Sub UpdatePortalBoss(dt As Single)
        Dim p = _s.Portal
        p.Elapsed += dt
        p.BossTime += dt
        UpdateHeroTimers(dt)
        Dim dx = If(IsHeld(Keys.D, Keys.Right), 1, 0) - If(IsHeld(Keys.A, Keys.Left), 1, 0)
        Dim dy = If(IsHeld(Keys.S, Keys.Down), 1, 0) - If(IsHeld(Keys.W, Keys.Up), 1, 0)
        Dim len = CSng(Math.Sqrt(dx * dx + dy * dy))
        Dim speed As Single = 325 + _s.SpeedLevel * 20
        If len > 0 Then
            p.BossPlayerX += dx / len * speed * dt
            p.BossPlayerY += dy / len * speed * dt
        End If
        If _s.Hero.DashLeft > 0 AndAlso _s.DashAvailable Then
            p.BossPlayerX += _s.Hero.Facing * 720 * dt
        End If
        p.BossPlayerX = Limit(p.BossPlayerX, 60, 810)
        p.BossPlayerY = Limit(p.BossPlayerY, 205, 565)
        p.BossX = 1025 + CSng(Math.Sin(p.BossTime * 1.15F)) * 100
        p.BossY = 360 + CSng(Math.Sin(p.BossTime * 2.1F)) * 120
        UpdateGunAim(True)
        If _mouseHeld OrElse IsHeld(Keys.F, Keys.K, Keys.ControlKey) Then TryShoot(True)
        p.BossFireCooldown -= dt
        If p.BossFireCooldown <= 0 Then
            Dim angle = CSng(Math.Atan2(p.BossPlayerY - 80 - p.BossY, p.BossPlayerX - p.BossX))
            For i = -2 To 2
                EnemyShoot(p.BossX - 36, p.BossY, angle + i * 0.18F, 235 + _s.Wave * 9)
            Next
            p.BossFireCooldown = Math.Max(0.55F, 1.5F - _s.Wave * 0.055F) / GameRules.DifficultyMultiplier(_s.Difficulty)
            Burst(p.BossX - 20, p.BossY, Color.FromArgb(245, 78, 162), 18, 240)
        End If
        If _s.Hero.DashLeft > 0 Then _s.Hero.DashLeft = Math.Max(0, _s.Hero.DashLeft - dt)
        UpdateProjectiles(dt, True)
    End Sub

    Private Sub HitPortalShield()
        If _s.Hero.Invincible > 0 Then Return
        _s.Hero.Invincible = 0.75F
        _s.Portal.BossShield -= 1
        _cameraShake = 7
        Burst(_s.Portal.BossPlayerX, _s.Portal.BossPlayerY - 80, Color.FromArgb(251, 101, 178), 23, 300)
        Notify("PORTALSKJOLD: " & Math.Max(0, _s.Portal.BossShield).ToString() & "/3", 1.0F)
        If _s.Portal.BossShield <= 0 Then FailPortal("PORTAL SENTINEL KNUSKET SKJOLDET")
    End Sub

    Private Sub DefeatPortalBoss()
        Dim p = _s.Portal
        If p.BossDefeated Then Return
        p.BossDefeated = True
        p.BossHp = 0
        Dim reward = 145 + _s.Wave * 21
        _s.Coins += reward : _s.TotalCoins += reward
        _s.Score += 1300 + _s.Wave * 125
        _s.FeverMeter = Math.Min(100, _s.FeverMeter + 40)
        Burst(p.BossX, p.BossY, Color.FromArgb(245, 172, 255), 130, 600)
        _cameraShake = 17
        _s.Bullets.Clear()
        _s.Scene = SceneKind.Portal
        Notify("PORTAL SENTINEL BESEIRET! +" & reward.ToString() & " MYNTER", 3.4F)
        SaveGame(False)
    End Sub

    Private Sub ChangeLane(amount As Integer)
        If _s.Scene <> SceneKind.Highway Then Return
        _s.Portal.CarLane = Math.Max(0, Math.Min(2, _s.Portal.CarLane + amount))
        Burst(630, 530, Color.FromArgb(255, 200, 93), 6, 80)
    End Sub

    Private Sub UpdateHighway(dt As Single)
        Dim p = _s.Portal
        p.Elapsed += dt
        Dim usingNitro As Boolean = IsHeld(Keys.Space, Keys.W, Keys.Up) AndAlso p.Nitro > 0
        Dim speed = p.CarSpeed * GameRules.DifficultyMultiplier(_s.Difficulty)
        If usingNitro Then
            speed *= 1.65F
            p.Nitro = Math.Max(0, p.Nitro - dt * 0.22F)
            If _rng.NextDouble() < 0.4 Then Burst(640, 606, Color.FromArgb(102, 247, 255), 2, 240)
        Else
            p.Nitro = Math.Min(1, p.Nitro + dt * 0.095F)
        End If
        p.Distance += speed * dt
        For Each ob In p.RoadObstacles
            If ob.Resolved OrElse ob.AtDistance > p.Distance Then Continue For
            ob.Resolved = True
            If ob.Lane = p.CarLane Then
                If ob.Kind = 4 Then
                    _s.Coins += 50
                    _s.TotalCoins += 50
                    _s.Score += 200
                    Burst(640, 580, Color.FromArgb(255, 213, 86), 24, 215)
                Else
                    FailPortal("BILEN TRAFF EN HINDRING")
                    Return
                End If
            Else
                _s.Score += 35
                If Math.Abs(ob.Lane - p.CarLane) = 1 Then _s.FeverMeter = Math.Min(100, _s.FeverMeter + 1.5F)
            End If
        Next
        If p.Distance >= p.Length Then FinishTravel()
    End Sub

    Private Sub FailPortal(reason As String)
        Dim options As New List(Of String)()
        If _s.DashAvailable Then options.Add("TURBO DASH")
        If _s.DoubleJumpAvailable Then options.Add("DOBBELTHOPP")
        If _s.SuperAvailable Then options.Add("SUPERANGREP")
        If _s.OverdriveAvailable Then options.Add("OVERDRIVE")
        Dim lost = "INGEN - ALLE EVNER ER MISTET"
        If options.Count > 0 Then
            lost = options(_rng.Next(options.Count))
            Select Case lost
                Case "TURBO DASH" : _s.DashAvailable = False
                Case "DOBBELTHOPP" : _s.DoubleJumpAvailable = False
                Case "SUPERANGREP" : _s.SuperAvailable = False
                Case "OVERDRIVE" : _s.OverdriveAvailable = False
            End Select
        End If
        _s.FailReason = reason & "  |  MISTET: " & lost & "  |  HP UENDRET!"
        _s.Portal.Attempts += 1
        _s.Scene = SceneKind.PortalFailed
        _s.Bullets.Clear()
        _cameraShake = 14
        Burst(If(_s.Portal.Type = "HIGHWAY", 640, 280), 490, Color.FromArgb(255, 76, 140), 95, 420)
        SaveGame(False)
    End Sub

    Private Sub RetryPortal()
        If _s.Scene <> SceneKind.PortalFailed Then Return
        Dim p = _s.Portal
        p.Distance = 0
        p.Elapsed = 0
        p.Y = GameRules.FloorY - 45
        p.Vy = 0
        p.GravitySign = 1
        p.Grounded = True
        p.CarLane = 1
        p.Nitro = 1
        p.BossTriggered = False
        p.BossDefeated = False
        p.BossShield = 3
        For Each ob In p.RoadObstacles
            ob.Resolved = False
        Next
        _s.Scene = If(p.Type = "HIGHWAY", SceneKind.Highway, SceneKind.Portal)
        _s.Hero.Invincible = 1.2F
        Notify("NYTT FORSOK! IKKE MIST FLERE EVNER!", 2.2F)
        SaveGame(False)
    End Sub

    Private Sub FinishTravel()
        _s.Coins += 45 + _s.Wave * 10
        _s.TotalCoins += 45 + _s.Wave * 10
        _s.Score += 490 + _s.Wave * 75
        _s.Hero.Hp = Math.Min(_s.Hero.MaxHp, _s.Hero.Hp + 9)
        Burst(645, 365, Color.FromArgb(130, 240, 255), 90, 410)
        Notify("PORTAL FULLFORT! NESTE KAMPBOLGE!", 2.0F)
        BeginWave(_s.Wave + 1)
    End Sub
End Class
