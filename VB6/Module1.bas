Attribute VB_Name = "KinectModule"

Public Declare Function SetTimer Lib "USER32" (ByVal hwnd As Long, ByVal nIDEvent As Long, ByVal uElapse As Long, ByVal lpTimerFunc As Long) As Long
Public Declare Function KillTimer Lib "USER32" (ByVal hwnd As Long, ByVal nIDEvent As Long) As Long


Private timer_id As Long
Private timer_count As Long
Private timer_max As Long

Public kinect As KinectWrapper

Private Const IMAGE_SCALE As Integer = 2

Public cancel_flag As Boolean

Public Sub kinectCreate()
    Set kinect = New KinectWrapper
End Sub

Public Sub kinectStop()
    On Error Resume Next
    kinect.StopAndDispose
    Set kinect = Nothing
End Sub

Public Sub TimerStart(nIDEvent As Long, uElapse As Long, max As Integer)
On Error GoTo ErrorCatch:
    cancel_flag = False
    timer_count = 0
    timer_max = max
    
    If kinect Is Nothing Then Set kinect = New KinectWrapper
    Call kinect.ColorStream_Enable(ColorImageFormat_RgbResolution640x480Fps30)
    Call kinect.Start
    
    timer_id = SetTimer(0&, nIDEvent, uElapse, AddressOf TimerProc)
Exit Sub
ErrorCatch:
AddMessage (Err.Description)
End Sub

Public Sub TimerStop()
    KillTimer 0&, timer_id
End Sub


Private Sub TimerProc()
On Error GoTo ErrorCatch:
  On Error Resume Next
  AddMessage (Now)
  If timer_count > timer_max Then Call TimerStop
  timer_count = timer_count + 1
  

  If kinect.GetColorFramePixcel Then
    AddMessage ("get frame data")
    TimerStop
    Dim width As Integer, height As Integer
    Dim PixelData
    width = kinect.width
    height = kinect.height
    
    'pixcel data copy
    Call CopyPixcelData(kinect.ColorPixel, PixelData)
    kinect.StopAndDispose 'kinect stop
    
    '描画開始
    Call DrawColor(PixelData, width, height)
  End If
  
Exit Sub
ErrorCatch:
AddMessage (Err.Description)
    TimerStop
    kinectStop
End Sub


'描画開始
Private Sub CopyPixcelData(ColorPixel, ByRef PixelData)
On Error GoTo ErrorCatch:
DoEvents
    Call AddMessage("CopyPixcelData")
    Dim i
    Dim length As Long
    Dim Data
    
    length = UBound(ColorPixel)
    If length = 0 Then Exit Sub

    ReDim PixelData(length)
    For i = 0 To length
        PixelData(i) = ColorPixel(i)
    Next

    Exit Sub
ErrorCatch:
AddMessage (Err.Description)
    kinectStop
End Sub


'描画開始
Private Sub DrawColor(PixelData, width As Integer, height As Integer)
On Error GoTo ErrorCatch:
DoEvents
    Call AddMessage("Drawing sart")
    Dim length As Long
    Dim x As Integer, y As Integer
    Dim index As Long
    Dim r, g, b As Integer
    Dim bit
    
    length = UBound(PixelData)
    If length = 0 Then Exit Sub

    DrawWidth = width / IMAGE_SCALE
    DrawHeight = height / IMAGE_SCALE
'    Call CellInit(DrawWidth, DrawHeight)
    
    index = 0
    For y = 1 To DrawHeight
        For x = 1 To DrawWidth
            If cancel_flag = True Then Exit Sub
            b = PixelData(index)
            g = PixelData(index + 1)
            r = PixelData(index + 2)
'            Cells(y, x).Interior.Color = RGB(r, g, b)  ' ピクセルデータを、RGB関数を使って背景色に設定
            index = index + 4 * IMAGE_SCALE
            DoEvents
        Next
        index = index + (4 * IMAGE_SCALE) * DrawWidth
    Next
    
    Call AddMessage("Drawing complete")
    Exit Sub
ErrorCatch:
AddMessage (Err.Description)
    kinectStop
End Sub


'
'Private Sub CellInit(width, height)
'    With ActiveSheet
'          .Range(Rows(1), Rows(height)).RowHeight = 1 '画像高さ分の行高さを1にする
'          .Range(Columns(1), Columns(width)).ColumnWidth = 0.1 '画像幅分の列幅を0.1にする
'    End With
'End Sub


Public Sub AddMessage(message As String)
On Error Resume Next
    KinectForm.TextBox1.Text = KinectForm.TextBox1.Text & message & vbCrLf
    Debug.Print (message)
End Sub


