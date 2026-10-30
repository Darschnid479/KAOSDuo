Option Strict On
Option Explicit On

Imports System
Imports System.Collections.Generic
Imports System.Drawing

Public Enum SceneKind
    MainMenu = 0
    Combat = 1
    ShopOffer = 2
    Shop = 3
    Upgrade = 4
    Portal = 5
    PortalBoss = 6
    Highway = 7
    PortalFailed = 8
    Paused = 9
    GameOver = 10
    Victory = 11
End Enum

Public Enum HeroKind
    Blue = 0
    Metal = 1
End Enum

Public Enum GameDifficulty
    Easy = 0
    Medium = 1
    Hard = 2
    Insane = 3
End Enum

Public Class WeaponSpec
    Public ReadOnly Id As Integer
    Public ReadOnly Name As String
    Public ReadOnly Cost As Integer
    Public ReadOnly Cooldown As Single
    Public ReadOnly Damage As Single
    Public ReadOnly Speed As Single
    Public ReadOnly Pellets As Integer
    Public ReadOnly Spread As Single
    Public ReadOnly Piercing As Integer
    Public ReadOnly Splash As Single
    Public ReadOnly Magazine As Integer
    Public ReadOnly Reload As Single
    Public ReadOnly Tint As Color
    Public ReadOnly Info As String

    Public Sub New(idValue As Integer, nameValue As String, costValue As Integer, rate As Single, damageValue As Single, bulletSpeed As Single, count As Integer, cone As Single, pierce As Integer, blast As Single, ammo As Integer, reloadTime As Single, tintValue As Color, detail As String)
        Id = idValue : Name = nameValue : Cost = costValue
        Cooldown = rate : Damage = damageValue : Speed = bulletSpeed
        Pellets = count : Spread = cone : Piercing = pierce : Splash = blast
        Magazine = ammo : Reload = reloadTime : Tint = tintValue : Info = detail
    End Sub
End Class

Public Module GameRules
    Public Const WorldWidth As Single = 1280.0F
    Public Const WorldHeight As Single = 720.0F
    Public Const FloorY As Single = 586.0F
    Public Const TotalWaves As Integer = 10

    Public ReadOnly Weapons As WeaponSpec() = {
        New WeaponSpec(0, "PULSE BLASTER", 0, 0.24F, 13, 690, 1, 0, 0, 0, 26, 1.15F, Color.FromArgb(96, 237, 255), "Pålitelig energipistol"),
        New WeaponSpec(1, "CHAOS SHOTGUN", 260, 0.73F, 10, 645, 6, 0.42F, 0, 0, 8, 1.8F, Color.FromArgb(255, 168, 92), "Seks kuler i én salve"),
        New WeaponSpec(2, "RAIL CANNON", 680, 1.02F, 58, 990, 1, 0, 5, 0, 7, 2.2F, Color.FromArgb(191, 128, 255), "Gjennomtrengende laserskudd"),
        New WeaponSpec(3, "CHAIN ARC", 780, 0.43F, 19, 740, 1, 0, 0, 0, 18, 1.4F, Color.FromArgb(126, 252, 195), "Elektriske kjedetreff"),
        New WeaponSpec(4, "NOVA ROCKETS", 990, 0.88F, 26, 515, 1, 0, 0, 110, 9, 2.35F, Color.FromArgb(255, 105, 168), "Raketter med eksplosjon"),
        New WeaponSpec(5, "GLACIER REPEATER", 1160, 0.115F, 9, 735, 1, 0.06F, 0, 0, 42, 1.4F, Color.FromArgb(122, 214, 255), "Raske skudd som senker fiender"),
        New WeaponSpec(6, "METEOR MINIGUN", 1680, 0.066F, 7, 820, 1, 0.13F, 0, 0, 100, 3.25F, Color.FromArgb(255, 201, 82), "Voldsom skuddstorm"),
        New WeaponSpec(7, "PRISM ANNIHILATOR", 2350, 0.39F, 37, 840, 2, 0.08F, 2, 48, 25, 2.1F, Color.FromArgb(255, 126, 236), "Doble, eksplosive prismestråler")
    }

    Public Function GetDifficultyName(difficulty As GameDifficulty) As String
        Select Case difficulty
            Case GameDifficulty.Easy : Return "LETT"
            Case GameDifficulty.Medium : Return "MEDIUM"
            Case GameDifficulty.Hard : Return "VANSKELIG"
            Case Else : Return "DRIT VANSKELIG"
        End Select
    End Function

    Public Function DifficultyMultiplier(difficulty As GameDifficulty) As Single
        Select Case difficulty
            Case GameDifficulty.Easy : Return 0.78F
            Case GameDifficulty.Medium : Return 1.0F
            Case GameDifficulty.Hard : Return 1.3F
            Case Else : Return 1.7F
        End Select
    End Function

    Public Function StartingHealth(difficulty As GameDifficulty) As Single
        Select Case difficulty
            Case GameDifficulty.Easy : Return 160
            Case GameDifficulty.Medium : Return 120
            Case GameDifficulty.Hard : Return 100
            Case Else : Return 80
        End Select
    End Function

    Public Function RollHighway(rng As Random) As Boolean
        Return rng.NextDouble() < 0.5
    End Function

    Public Function RollPortalBoss(rng As Random) As Boolean
        Return rng.NextDouble() < 0.25
    End Function

    Public Function WeaponCost(id As Integer) As Integer
        If id < 0 OrElse id >= Weapons.Length Then Return Integer.MaxValue
        Return Weapons(id).Cost
    End Function

    Public Function MuzzlePosition(heroX As Single, heroFootY As Single, angle As Single) As PointF
        Dim originX As Single = heroX + CSng(Math.Cos(angle)) * 13.0F
        Dim originY As Single = heroFootY - 102.0F
        Return New PointF(originX + CSng(Math.Cos(angle)) * 63.0F,
                          originY + CSng(Math.Sin(angle)) * 63.0F)
    End Function

    Public Function CanAffordWeapon(coins As Integer, owned As IEnumerable(Of Integer), id As Integer) As Boolean
        If id < 0 OrElse id >= Weapons.Length Then Return False
        Return coins >= Weapons(id).Cost AndAlso Not (New List(Of Integer)(owned)).Contains(id)
    End Function
End Module

Public Class HeroState
    Public Property X As Single = 225.0F
    Public Property Y As Single = GameRules.FloorY
    Public Property Vx As Single = 0
    Public Property Vy As Single = 0
    Public Property Hp As Single = 120
    Public Property MaxHp As Single = 120
    Public Property Invincible As Single = 0
    Public Property Recoil As Single = 0
    Public Property MuzzleFlash As Single = 0
    Public Property JumpCount As Integer = 0
    Public Property Facing As Integer = 1
    Public Property Aim As Single = 0
    Public Property DashCooldown As Single = 0
    Public Property DashLeft As Single = 0
    Public Property SuperCooldown As Single = 0
    Public Property ShootCooldown As Single = 0
    Public Property ReloadRemaining As Single = 0
    Public Property MotionTime As Single = 0
    Public Property BlinkTime As Single = 0
    Public Property Shield As Integer = 0
End Class

Public Class EnemyState
    Public Property Uid As Integer
    Public Property X As Single
    Public Property Y As Single
    Public Property Vx As Single
    Public Property Vy As Single
    Public Property Hp As Single
    Public Property MaxHp As Single
    Public Property Kind As Integer
    Public Property Radius As Single = 26
    Public Property FireTimer As Single
    Public Property Anim As Single
    Public Property Freeze As Single
    Public Property Boss As Boolean
End Class

Public Class ProjectileState
    Public Property X As Single
    Public Property Y As Single
    Public Property Vx As Single
    Public Property Vy As Single
    Public Property Damage As Single
    Public Property Radius As Single = 6
    Public Property Life As Single = 2.2F
    Public Property Friendly As Boolean
    Public Property Weapon As Integer
    Public Property Pierce As Integer
    Public Property Splash As Single
    Public Property HitIds As New List(Of Integer)()
End Class

Public Class PortalObstacle
    Public Property X As Single
    Public Property Y As Single
    Public Property W As Single
    Public Property H As Single
    Public Property Kind As Integer
End Class

Public Class RoadObstacle
    Public Property AtDistance As Single
    Public Property Lane As Integer
    Public Property Kind As Integer
    Public Property Resolved As Boolean
End Class

Public Class PortalState
    Public Property Type As String = "GEOMETRY"
    Public Property Distance As Single = 0
    Public Property Length As Single = 3850
    Public Property Y As Single = 440
    Public Property Vy As Single = 0
    Public Property GravitySign As Integer = 1
    Public Property Grounded As Boolean = False
    Public Property Attempts As Integer = 0
    Public Property Elapsed As Single = 0
    Public Property TriggerBoss As Boolean
    Public Property BossTriggered As Boolean
    Public Property BossDefeated As Boolean
    Public Property BossPosition As Single
    Public Property BossHp As Single
    Public Property BossMaxHp As Single
    Public Property BossX As Single = 1000
    Public Property BossY As Single = 300
    Public Property BossFireCooldown As Single = 1.5F
    Public Property BossShield As Integer = 3
    Public Property BossTime As Single = 0
    Public Property BossPlayerX As Single = 250
    Public Property BossPlayerY As Single = 370
    Public Property StagePermutation As Integer = 0
    Public Property CarLane As Integer = 1
    Public Property Nitro As Single = 1
    Public Property CarSpeed As Single = 435
    Public Property Obstacles As New List(Of PortalObstacle)()
    Public Property RoadObstacles As New List(Of RoadObstacle)()
End Class

Public Class GameSave
    Public Property SaveVersion As Integer = 6
    Public Property Character As HeroKind = HeroKind.Blue
    Public Property Difficulty As GameDifficulty = GameDifficulty.Medium
    Public Property Scene As SceneKind = SceneKind.MainMenu
    Public Property PreviousScene As SceneKind = SceneKind.Combat
    Public Property Wave As Integer = 0
    Public Property Coins As Integer = 0
    Public Property TotalCoins As Integer = 0
    Public Property Score As Integer = 0
    Public Property Kills As Integer = 0
    Public Property Combo As Integer = 0
    Public Property ComboTimer As Single = 0
    Public Property FeverMeter As Single = 0
    Public Property FeverTime As Single = 0
    Public Property Level As Integer = 1
    Public Property Experience As Integer = 0
    Public Property ExpNeeded As Integer = 70
    Public Property SpawnRemaining As Integer = 0
    Public Property SpawnCooldown As Single = 1.0F
    Public Property WaveBossSpawned As Boolean = False
    Public Property Hero As New HeroState()
    Public Property Enemies As New List(Of EnemyState)()
    Public Property Bullets As New List(Of ProjectileState)()
    Public Property Portal As New PortalState()
    Public Property OwnedWeapons As New List(Of Integer) From {0}
    Public Property Magazine As New Dictionary(Of Integer, Integer) From {{0, 26}}
    Public Property EquippedWeapon As Integer = 0
    Public Property DamageLevel As Integer = 0
    Public Property SpeedLevel As Integer = 0
    Public Property ArmorLevel As Integer = 0
    Public Property FireRateLevel As Integer = 0
    Public Property MoneyLevel As Integer = 0
    Public Property DashAvailable As Boolean = True
    Public Property DoubleJumpAvailable As Boolean = True
    Public Property SuperAvailable As Boolean = True
    Public Property OverdriveAvailable As Boolean = True
    Public Property UpgradesPending As Integer = 0
    Public Property FailReason As String = ""
    Public Property MusicEnabled As Boolean = True
    Public Property SecondsPlayed As Double = 0
End Class

Public Class Spark
    Public X As Single
    Public Y As Single
    Public Vx As Single
    Public Vy As Single
    Public Life As Single
    Public FullLife As Single
    Public Size As Single
    Public Tint As Color
End Class

Public Class HitButton
    Public ReadOnly Bounds As RectangleF
    Public ReadOnly Action As String

    Public Sub New(rect As RectangleF, actionName As String)
        Bounds = rect
        Action = actionName
    End Sub
End Class
