VERSION 5.00
Begin VB.Form Form1 
   Caption         =   "Form1"
   ClientHeight    =   3990
   ClientLeft      =   120
   ClientTop       =   450
   ClientWidth     =   6510
   LinkTopic       =   "Form1"
   ScaleHeight     =   3990
   ScaleWidth      =   6510
   StartUpPosition =   3  'Windows ‚ÌŠù’è’l
   Begin VB.CommandButton Command2 
      Caption         =   "Command1"
      Height          =   735
      Left            =   600
      TabIndex        =   1
      Top             =   1800
      Width           =   1695
   End
   Begin VB.CommandButton Command1 
      Caption         =   "Command1"
      Height          =   735
      Left            =   600
      TabIndex        =   0
      Top             =   600
      Width           =   1695
   End
End
Attribute VB_Name = "Form1"
Attribute VB_GlobalNameSpace = False
Attribute VB_Creatable = False
Attribute VB_PredeclaredId = True
Attribute VB_Exposed = False


Private timer_id As Long


Private Sub Form_Load()
    KinectModule.kinectCreate
    Call AddMessage("loaded")
End Sub

Private Sub Form_Unload(Cancel As Integer)
On Error Resume Next
    KinectModule.TimerStop
    KinectModule.kinectStop
End Sub


Private Sub Command1_Click()
    Dim nIDEvent As Long
    Dim uElapse As Long
    nIDEvent = 31000&
    uElapse = 250&
    Call AddMessage("kinect start")
    
    Call KinectModule.TimerStart(nIDEvent, uElapse, 10)

End Sub

Private Sub Command2_Click()
On Error Resume Next
    KinectModule.TimerStop
    cancel_flag = True
    KinectModule.kinect.StopAndDispose
    Call AddMessage("stoped")
End Sub


