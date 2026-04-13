' =============================================================================
' ANIMATION CONTROLLER — Brick Blast WPF
' State-machine-driven frame animation system.
' Manages named clips (Idle, Move, Attack) with configurable frame rates.
'
' Usage:
'   Dim anim As New AnimationController()
'   anim.AddClip("idle", {0, 1, 2, 1}, 8.0F)
'   anim.AddClip("attack", {3, 4, 5}, 12.0F, False)
'   anim.Play("idle")
'   anim.Update(deltaTime)
'   Dim frameIdx = anim.CurrentFrame
' =============================================================================
Namespace BrickBlastWPF

Public Structure AnimationClip
    Public Name As String
    Public FrameIndices As Integer()
    Public FrameRate As Single
    Public Looping As Boolean
End Structure

Public Class AnimationController

    Private ReadOnly _clips As New Dictionary(Of String, AnimationClip)
    Private _currentClip As String = ""
    Private _frameTimer As Single = 0
    Private _currentFrameIndex As Integer = 0
    Private _playing As Boolean = False

    ''' <summary>
    ''' Register a named animation clip with its frame sequence.
    ''' </summary>
    Public Sub AddClip(name As String, frameIndices As Integer(), frameRate As Single, Optional looping As Boolean = True)
        Dim clip As AnimationClip
        clip.Name = name
        clip.FrameIndices = frameIndices
        clip.FrameRate = frameRate
        clip.Looping = looping
        _clips(name) = clip
    End Sub

    ''' <summary>
    ''' Start playing a clip by name. No-op if already playing that clip.
    ''' </summary>
    Public Sub Play(clipName As String)
        If clipName = _currentClip AndAlso _playing Then Return
        If Not _clips.ContainsKey(clipName) Then Return
        _currentClip = clipName
        _currentFrameIndex = 0
        _frameTimer = 0
        _playing = True
    End Sub

    Public Sub [Stop]()
        _playing = False
    End Sub

    ''' <summary>
    ''' Advance the animation by deltaTime seconds.
    ''' Call once per game loop tick with 0.016 for 60fps.
    ''' </summary>
    Public Sub Update(deltaTime As Single)
        If Not _playing OrElse Not _clips.ContainsKey(_currentClip) Then Return
        Dim clip = _clips(_currentClip)
        If clip.FrameIndices Is Nothing OrElse clip.FrameIndices.Length = 0 Then Return

        _frameTimer += deltaTime
        Dim frameDuration = 1.0F / Math.Max(0.1F, clip.FrameRate)
        While _frameTimer >= frameDuration
            _frameTimer -= frameDuration
            _currentFrameIndex += 1
            If _currentFrameIndex >= clip.FrameIndices.Length Then
                If clip.Looping Then
                    _currentFrameIndex = 0
                Else
                    _currentFrameIndex = clip.FrameIndices.Length - 1
                    _playing = False
                End If
            End If
        End While
    End Sub

    ''' <summary>
    ''' The sprite-sheet frame index for the current animation state.
    ''' </summary>
    Public ReadOnly Property CurrentFrame As Integer
        Get
            If Not _clips.ContainsKey(_currentClip) Then Return 0
            Dim clip = _clips(_currentClip)
            If clip.FrameIndices Is Nothing OrElse clip.FrameIndices.Length = 0 Then Return 0
            Return clip.FrameIndices(Math.Min(_currentFrameIndex, clip.FrameIndices.Length - 1))
        End Get
    End Property

    Public ReadOnly Property CurrentClipName As String
        Get
            Return _currentClip
        End Get
    End Property

    Public ReadOnly Property IsPlaying As Boolean
        Get
            Return _playing
        End Get
    End Property

End Class
End Namespace
