' =============================================================================
' INVENTORY SYSTEM — Brick Blast WPF
' Grid-based inventory with icon rendering using asset sprites.
' Collected items (enemy drops, power-ups) accumulate per level.
'
' Usage:
'   _inventory.AddItem("gem", "UI/gem", Colors.Purple, "A shiny gem")
'   _inventory.Render(dc, mgr, x, y, slotSize, dpi)
' =============================================================================
Imports System.Globalization
Imports System.Windows
Imports System.Windows.Media

Namespace BrickBlastWPF

Public Structure InventoryItem
    Public Name As String
    Public IconKey As String
    Public IconColor As Color
    Public Quantity As Integer
    Public Description As String
End Structure

Public Class InventorySystem

    Private ReadOnly _slots As New List(Of InventoryItem)
    Private ReadOnly _maxSlots As Integer

    Public Sub New(maxSlots As Integer)
        _maxSlots = maxSlots
    End Sub

    ''' <summary>
    ''' Add one unit of an item. Stacks if already present.
    ''' </summary>
    Public Sub AddItem(name As String, iconKey As String, color As Color, description As String)
        For i = 0 To _slots.Count - 1
            If _slots(i).Name = name Then
                Dim item = _slots(i)
                item.Quantity += 1
                _slots(i) = item
                Return
            End If
        Next
        If _slots.Count < _maxSlots Then
            Dim item As InventoryItem
            item.Name = name
            item.IconKey = iconKey
            item.IconColor = color
            item.Quantity = 1
            item.Description = description
            _slots.Add(item)
        End If
    End Sub

    Public Sub RemoveItem(name As String)
        For i = _slots.Count - 1 To 0 Step -1
            If _slots(i).Name = name Then
                Dim item = _slots(i)
                item.Quantity -= 1
                If item.Quantity <= 0 Then
                    _slots.RemoveAt(i)
                Else
                    _slots(i) = item
                End If
                Return
            End If
        Next
    End Sub

    Public Function HasItem(name As String) As Boolean
        Return _slots.Any(Function(s) s.Name = name AndAlso s.Quantity > 0)
    End Function

    Public Function GetItemCount(name As String) As Integer
        Dim item = _slots.FirstOrDefault(Function(s) s.Name = name)
        Return item.Quantity
    End Function

    Public Sub Clear()
        _slots.Clear()
    End Sub

    Public ReadOnly Property Count As Integer
        Get
            Return _slots.Count
        End Get
    End Property

    ''' <summary>
    ''' Render the inventory bar: slot backgrounds + icons + quantity labels.
    ''' </summary>
    Public Sub Render(dc As DrawingContext, mgr As AssetManager, x As Single, y As Single, slotSize As Integer, dpi As Double)
        Dim gap = 2
        Dim totalWidth = _maxSlots * (slotSize + gap) - gap

        ' Bar background
        dc.DrawRoundedRectangle(
            New SolidColorBrush(Color.FromArgb(100, 0, 0, 20)), Nothing,
            New Rect(x - 4, y - 4, totalWidth + 8, slotSize + 8), 4, 4)

        For i = 0 To _maxSlots - 1
            Dim slotX = x + i * (slotSize + gap)
            Dim slotRect = New Rect(slotX, y, slotSize, slotSize)
            dc.DrawRoundedRectangle(
                New SolidColorBrush(Color.FromArgb(60, 80, 80, 120)),
                New Pen(New SolidColorBrush(Color.FromArgb(40, 150, 150, 200)), 1),
                slotRect, 3, 3)

            If i < _slots.Count Then
                Dim item = _slots(i)

                ' Try asset icon first
                Dim icon = mgr.GetSprite($"UI/{item.Name}")
                If icon IsNot Nothing Then
                    dc.DrawImage(icon, New Rect(slotX + 4, y + 4, slotSize - 8, slotSize - 8))
                Else
                    ' Fallback: colored circle
                    dc.DrawEllipse(
                        New SolidColorBrush(Color.FromArgb(180, item.IconColor.R, item.IconColor.G, item.IconColor.B)),
                        Nothing, New Point(slotX + slotSize / 2.0, y + slotSize / 2.0),
                        slotSize / 3.0, slotSize / 3.0)
                End If

                ' Quantity badge
                If item.Quantity > 1 Then
                    Dim qt = New FormattedText(item.Quantity.ToString(),
                        CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
                        New Typeface("Segoe UI"), 9, Brushes.White, dpi)
                    dc.DrawText(qt, New Point(slotX + slotSize - qt.Width - 2, y + slotSize - qt.Height - 1))
                End If
            End If
        Next
    End Sub

End Class
End Namespace
